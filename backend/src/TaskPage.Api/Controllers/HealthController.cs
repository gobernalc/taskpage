/// ------------------------------------------------------------------
/// Clase  : HealthController
/// Fecha  : 2026-11-11
/// Autor  : gobernalc
/// Resumen: Expone el estado de salud de la API para validar el arranque.
/// ------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;

namespace TaskPage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// ------------------------------------------------------------------
    /// Método : Get
    /// Fecha  : 2026-11-11
    /// Autor  : gobernalc
    /// Resumen: Devuelve el estado de salud del servicio.
    /// ------------------------------------------------------------------
    /// <returns>Respuesta HTTP 200 con el estado del servicio.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
