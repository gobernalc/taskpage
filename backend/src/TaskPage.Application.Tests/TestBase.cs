using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TaskPage.Application.Tests;

/// <summary>
/// Clase base para las pruebas de la capa de aplicación con salida ✅/❌.
/// </summary>
public abstract class TestBase
{
    /// <summary>Ejecuta una prueba e imprime ✅ si pasa o ❌ si falla.</summary>
    /// <param name="description">Descripción en español de la prueba.</param>
    /// <param name="test">Cuerpo de la prueba a ejecutar.</param>
    protected void Run(string description, Action test)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine($"▶ {description}");
        try
        {
            test();
            Console.WriteLine($"   ✅ {description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ {description} — {ex.Message}");
            throw;
        }
    }
}
