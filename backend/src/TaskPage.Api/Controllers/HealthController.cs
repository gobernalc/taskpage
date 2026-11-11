using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPage.Infrastructure.Persistence;

namespace TaskPage.Api.Controllers;

/// <summary>
/// Exposes the API health status, including the SQL Server connectivity check.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    /// <summary>Creates the controller with the database context and a logger.</summary>
    /// <param name="context">EF Core context used to probe the database.</param>
    /// <param name="logger">Logger for the health check results.</param>
    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Returns 200 with the service status when the database responds, or 503 otherwise.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Health status of the service and the database.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var databaseAvailable = await CanConnectAsync(cancellationToken);

        if (!databaseAvailable)
        {
            _logger.LogWarning("Health check failed: the database did not respond");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { status = "degraded", database = "unavailable" });
        }

        return Ok(new { status = "ok", database = "ok" });
    }

    /// <summary>Probes the database connection, converting failures into a false result.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True when the database responds.</returns>
    private async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connectivity check threw an exception");
            return false;
        }
    }
}
