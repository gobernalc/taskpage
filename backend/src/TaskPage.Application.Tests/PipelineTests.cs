// ------------------------------------------------------------------
// Clase  : PipelineTests
// Fecha  : 2026-11-11
// Autor  : gobernalc
// Resumen: Prueba de humo para validar el pipeline de pruebas.
// ------------------------------------------------------------------
namespace TaskPage.Application.Tests;

public class PipelineTests : TestBase
{
    /// ------------------------------------------------------------------
    /// Método : DebeValidarElPipelineDePruebas
    /// Fecha  : 2026-11-11
    /// Autor  : gobernalc
    /// Resumen: Verifica que el runner ejecuta y reporta correctamente.
    /// ------------------------------------------------------------------
    [Fact]
    public void DebeValidarElPipelineDePruebas()
    {
        Run("suma básica para validar el pipeline", () =>
        {
            var resultado = 2 + 3;
            Assert.Equal(5, resultado);
        });
    }
}
