using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TaskPage.Application.Tests;

/// <summary>
/// Base class for application layer tests with ✅/❌ output.
/// </summary>
public abstract class TestBase
{
    /// <summary>Runs a test and prints ✅ when it passes or ❌ when it fails.</summary>
    /// <param name="description">English description of the test.</param>
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

    /// <summary>Runs an asynchronous test and prints ✅ when it passes or ❌ when it fails.</summary>
    /// <param name="description">English description of the test.</param>
    /// <param name="test">Async test body to run.</param>
    protected async Task RunAsync(string description, Func<Task> test)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine($"▶ {description}");
        try
        {
            await test();
            Console.WriteLine($"   ✅ {description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ {description} — {ex.Message}");
            throw;
        }
    }
}
