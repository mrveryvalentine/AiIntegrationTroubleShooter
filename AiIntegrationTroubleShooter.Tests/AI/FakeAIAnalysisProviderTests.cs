using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;

namespace AiIntegrationTroubleShooter.Tests.AI;

public class FakeAIAnalysisProviderTests
{
    [Fact]
    public async Task AnalyseAsync_ReturnsESimulatedAnalysis()
    {
        // Arrange
        var provider = new FakeAIAnalysisProvider();

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test Integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var result = await provider.AnalyseAsync(request);

        // Assert
        Assert.NotEmpty(result.Summary);
        Assert.NotEmpty(result.PossibleCauses);
        Assert.NotEmpty(result.InvestigationSteps);
        Assert.NotEmpty(result.Warnings);
        Assert.True(result.RequiresHumanReview);

        Assert.Contains(
            result.Warnings, 
            warning => warning.Contains("simulated", StringComparison.OrdinalIgnoreCase));
    }
}
