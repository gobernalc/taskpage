using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskPage.Api.Controllers;
using TaskPage.Infrastructure.Persistence;

namespace TaskPage.Api.Tests.Controllers;

/// <summary>
/// Covers the health endpoint with the database available and unavailable.
/// </summary>
public class HealthControllerTests : TestBase
{
    /// <summary>Builds the controller with a database context and a null logger.</summary>
    /// <param name="context">Database context used to probe the connection.</param>
    /// <returns>The configured controller.</returns>
    private static HealthController CreateController(AppDbContext context) =>
        new(context, NullLogger<HealthController>.Instance);

    /// <summary>Returns 200 with the ok status when the database responds.</summary>
    [Fact]
    public async Task Get_WhenDatabaseAvailable_ReturnsOk()
    {
        await RunAsync("health check returns 200 OK when the database responds", async () =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"health-{Guid.NewGuid()}")
                .Options;
            var controller = CreateController(new AppDbContext(options));

            var result = await controller.GetAsync(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("ok", GetProperty(ok.Value, "status"));
            Assert.Equal("ok", GetProperty(ok.Value, "database"));
        });
    }

    /// <summary>Returns 503 with the degraded status when the database does not respond.</summary>
    [Fact]
    public async Task Get_WhenDatabaseUnavailable_Returns503()
    {
        await RunAsync("health check returns 503 when the database does not respond", async () =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var controller = CreateController(new AppDbContext(options));

            var result = await controller.GetAsync(CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
            Assert.Equal("degraded", GetProperty(objectResult.Value, "status"));
            Assert.Equal("unavailable", GetProperty(objectResult.Value, "database"));
        });
    }

    /// <summary>Reads a property from an anonymous object using reflection.</summary>
    /// <param name="value">Object to inspect.</param>
    /// <param name="name">Property name.</param>
    /// <returns>The property value or null.</returns>
    private static object? GetProperty(object? value, string name) =>
        value?.GetType().GetProperty(name)?.GetValue(value);
}
