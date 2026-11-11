using Microsoft.AspNetCore.Mvc;

namespace TaskPage.Api.Controllers;

/// <summary>
/// Exposes the API health status to validate that the service starts correctly.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>Returns the health status of the service.</summary>
    /// <returns>HTTP 200 response with the service status.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
