// API entry point: controllers, Serilog, Swagger and the HTTP pipeline.

using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using TaskPage.Api.Middleware;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Application.Tasks.Services;
using TaskPage.Application.Tasks.Validators;
using TaskPage.Application.Users.DTOs;
using TaskPage.Application.Users.Services;
using TaskPage.Application.Users.Validators;
using TaskPage.Infrastructure;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

// Configure Serilog before the host so the earliest failures are captured too.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/taskpage-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    // Keep validation messages in English regardless of the host culture.
    ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("en-US");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Register controllers and map invalid model state to ProblemDetails with
    // the same correlation identifier used by the exception middleware.
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var traceId = context.HttpContext.TraceIdentifier;

                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed"
                };
                problem.Extensions["traceId"] = traceId;
                context.HttpContext.Response.Headers["X-Request-Id"] = traceId;

                return new ObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            };
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "TaskPage API",
            Version = "v1",
            Description = "TaskPage technical test services."
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    });

    // Persistence, repositories and application services.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<TaskService>();

    // FluentValidation validators used by the controllers.
    builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
    builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
    builder.Services.AddScoped<IValidator<CreateTaskRequest>, CreateTaskRequestValidator>();
    builder.Services.AddScoped<IValidator<UpdateTaskRequest>, UpdateTaskRequestValidator>();
    builder.Services.AddScoped<IValidator<UpdateTaskStatusRequest>, UpdateTaskStatusRequestValidator>();

    var app = builder.Build();

    // Global boundary: logs the request, converts exceptions to ProblemDetails and
    // correlates every event with the request TraceIdentifier.
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Swagger is available at /swagger (UI) and /swagger/v1/swagger.json (JSON).
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskPage API v1");
    });

    app.UseHttpsRedirection();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
