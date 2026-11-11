namespace TaskPage.Api.Tests;

/// <summary>Prueba de humo para validar el pipeline de pruebas.</summary>
public class PipelineTests : TestBase
{
    /// <summary>Verifica que el runner ejecuta y reporta correctamente.</summary>
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
