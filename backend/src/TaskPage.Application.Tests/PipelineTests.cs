namespace TaskPage.Application.Tests;

/// <summary>Smoke test that validates the test pipeline.</summary>
public class PipelineTests : TestBase
{
    /// <summary>Verifies that the runner executes and reports correctly.</summary>
    [Fact]
    public void ShouldValidateTestingPipeline()
    {
        Run("basic sum to validate the pipeline", () =>
        {
            var result = 2 + 3;
            Assert.Equal(5, result);
        });
    }
}
