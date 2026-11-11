// ------------------------------------------------------------------
// Clase  : TestBase
// Fecha  : 2026-11-11
// Autor  : gobernalc
// Resumen: Clase base para pruebas que imprime el resultado con emojis.
// ------------------------------------------------------------------
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TaskPage.Api.Tests;

public abstract class TestBase
{
    /// ------------------------------------------------------------------
    /// Método : Run
    /// Fecha  : 2026-11-11
    /// Autor  : gobernalc
    /// Resumen: Ejecuta una prueba e imprime ✅ si pasa o ❌ si falla.
    /// ------------------------------------------------------------------
    /// <param name="description">Descripción de la prueba.</param>
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
