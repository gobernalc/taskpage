using Microsoft.AspNetCore.Mvc;

namespace TaskPage.Api.Controllers;

/// <summary>
/// Expone el estado de salud de la API para validar el arranque del servicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>Devuelve el estado de salud del servicio.</summary>
    /// <returns>Respuesta HTTP 200 con el estado del servicio.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
