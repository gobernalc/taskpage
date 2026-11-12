using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Api.Middleware;

/// <summary>
/// Central exception boundary of the HTTP pipeline. Maps domain and validation
/// failures to ProblemDetails, adds the X-Request-Id header and correlates every
/// log event with the request TraceIdentifier.
/// </summary>
public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>Creates the middleware with the next delegate and a logger.</summary>
    /// <param name="next">Next component in the pipeline.</param>
    /// <param name="logger">Logger used to record requests and errors.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Logs the request, invokes the pipeline and converts exceptions to ProblemDetails.</summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var startedAt = Stopwatch.GetTimestamp();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceIdentifier"] = traceId
        });

        _logger.LogInformation(
            "Request started {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

            _logger.LogError(
                exception,
                "Request failed {Method} {Path} after {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path,
                elapsedMs);

            await WriteErrorAsync(context, exception, traceId);
        }
        finally
        {
            var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

            _logger.LogInformation(
                "Request completed {Method} {Path} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path,
                elapsedMs);
        }
    }

    /// <summary>Writes the ProblemDetails response with the correlation identifiers.</summary>
    /// <param name="context">Current HTTP context.</param>
    /// <param name="exception">Exception that interrupted the pipeline.</param>
    /// <param name="traceId">Request correlation identifier.</param>
    private static async Task WriteErrorAsync(HttpContext context, Exception exception, string traceId)
    {
        var (statusCode, title) = Map(exception);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = GetDetail(exception, statusCode),
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["X-Request-Id"] = traceId;

        await context.Response.WriteAsJsonAsync(
            problem,
            JsonOptions,
            "application/problem+json",
            context.RequestAborted);
    }

    /// <summary>Maps an exception to its HTTP status and ProblemDetails title.</summary>
    /// <param name="exception">Exception to classify.</param>
    /// <returns>Status code and title pair.</returns>
    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
        BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
        NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
    };

    /// <summary>Builds the detail text, hiding internal information from 500 responses.</summary>
    /// <param name="exception">Exception to describe.</param>
    /// <param name="statusCode">Mapped HTTP status code.</param>
    /// <returns>Safe detail message.</returns>
    private static string GetDetail(Exception exception, int statusCode) => exception switch
    {
        ValidationException validation => string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
        DomainException domain => domain.Message,
        _ when statusCode == StatusCodes.Status500InternalServerError => "The request could not be completed.",
        _ => exception.Message
    };
}
