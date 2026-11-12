using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TaskPage.Api.Middleware;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Api.Tests.Middleware;

/// <summary>
/// Covers the exception mapping of the global middleware and the correlation headers.
/// </summary>
public class GlobalExceptionMiddlewareTests : TestBase
{
    /// <summary>Invokes the middleware with a delegate that throws the given exception.</summary>
    /// <param name="exception">Exception to throw from the next delegate.</param>
    /// <returns>The HTTP context after the middleware wrote the response.</returns>
    private static async Task<DefaultHttpContext> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        return context;
    }

    /// <summary>Parses the JSON body written by the middleware.</summary>
    /// <param name="context">HTTP context with the response body.</param>
    /// <returns>The parsed JSON document.</returns>
    private static JsonDocument ReadBody(DefaultHttpContext context)
    {
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return JsonDocument.Parse(reader.ReadToEnd());
    }

    /// <summary>Maps a not found exception to 404 with the correlation identifiers.</summary>
    [Fact]
    public async Task Invoke_WithNotFoundException_Returns404()
    {
        await RunAsync("middleware maps not found exceptions to 404", async () =>
        {
            var context = await InvokeAsync(new NotFoundException("Task with id 1 was not found."));

            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal(context.TraceIdentifier, context.Response.Headers["X-Request-Id"].ToString());

            using var body = ReadBody(context);
            Assert.Equal("Not found", body.RootElement.GetProperty("title").GetString());
            Assert.Equal("Task with id 1 was not found.", body.RootElement.GetProperty("detail").GetString());
            Assert.Equal(context.TraceIdentifier, body.RootElement.GetProperty("traceId").GetString());
        });
    }

    /// <summary>Maps a business rule violation to 400.</summary>
    [Fact]
    public async Task Invoke_WithBusinessRuleException_Returns400()
    {
        await RunAsync("middleware maps business rule violations to 400", async () =>
        {
            var context = await InvokeAsync(new BusinessRuleException("Pending to Done is forbidden."));

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

            using var body = ReadBody(context);
            Assert.Equal("Business rule violated", body.RootElement.GetProperty("title").GetString());
        });
    }

    /// <summary>Maps a conflict to 409.</summary>
    [Fact]
    public async Task Invoke_WithConflictException_Returns409()
    {
        await RunAsync("middleware maps conflicts to 409", async () =>
        {
            var context = await InvokeAsync(new ConflictException("The email address is already registered."));

            Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

            using var body = ReadBody(context);
            Assert.Equal("Conflict", body.RootElement.GetProperty("title").GetString());
        });
    }

    /// <summary>Maps a validation exception to 400 with the joined error messages.</summary>
    [Fact]
    public async Task Invoke_WithValidationException_Returns400()
    {
        await RunAsync("middleware maps validation exceptions to 400", async () =>
        {
            var exception = new ValidationException(
            [
                new ValidationFailure("Name", "Name is required."),
                new ValidationFailure("Email", "Email is not valid.")
            ]);

            var context = await InvokeAsync(exception);

            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

            using var body = ReadBody(context);
            Assert.Equal("Validation failed", body.RootElement.GetProperty("title").GetString());
            Assert.Equal("Name is required.; Email is not valid.", body.RootElement.GetProperty("detail").GetString());
        });
    }

    /// <summary>Hides internal details from unexpected 500 responses.</summary>
    [Fact]
    public async Task Invoke_WithUnexpectedException_Returns500()
    {
        await RunAsync("middleware maps unexpected exceptions to 500 without internal details", async () =>
        {
            var context = await InvokeAsync(new InvalidOperationException("secret database detail"));

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            using var body = ReadBody(context);
            Assert.Equal("An unexpected error occurred", body.RootElement.GetProperty("title").GetString());
            Assert.Equal("The request could not be completed.", body.RootElement.GetProperty("detail").GetString());
            Assert.DoesNotContain("secret database detail", body.RootElement.GetRawText());
        });
    }
}
