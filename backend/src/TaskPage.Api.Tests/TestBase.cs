using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TaskPage.Api.Tests;

/// <summary>
/// Base class for tests that prints the result with ✅ or ❌ to the console.
/// </summary>
public abstract class TestBase
{
    /// <summary>Runs a test and prints ✅ when it passes or ❌ when it fails.</summary>
    /// <param name="description">Test description.</param>
    /// <param name="test">Test body to run.</param>
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
