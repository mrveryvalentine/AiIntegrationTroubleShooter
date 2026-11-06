using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;
using AiIntegrationTroubleShooter.Api.Services;

namespace AiIntegrationTroubleShooter.Tests.Services;

public class IntegrationAnalysisServiceTests
{
    [Fact]
    public async Task AnalyseAsync_ReturnsResponseFromProvider()
    {
        // Arrange
        var expectedResponse = new IntegrationAnalysisResponse
        {
            Summary = "Test analysis"
        };
        var fakeProvider = new TestAIAnalysisProvider(expectedResponse);
        var service = new IntegrationAnalysisService(fakeProvider);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test Integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var result = await service.AnalyseAsync(request);

        // Assert
        Assert.Equal(expectedResponse, result);
    }


    [Fact]
    public async Task AnalyseAsync_PassesCancellationTokenToProvider()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var expectedToken = cancellationTokenSource.Token;
        var fakeProvider = new TestAIAnalysisProvider(
            new IntegrationAnalysisResponse());

        var service = new IntegrationAnalysisService(fakeProvider);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test Integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        await service.AnalyseAsync(request, expectedToken);

        // Assert
        Assert.Equal(expectedToken, fakeProvider.ReceivedCancellationToken);
        
    }



    private class TestAIAnalysisProvider : IAIAnalysisProvider
    {
        private readonly IntegrationAnalysisResponse _response;
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public TestAIAnalysisProvider(IntegrationAnalysisResponse response)
        {
            _response = response;
        }

        public Task<IntegrationAnalysisResponse> AnalyseAsync(
            IntegrationIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(_response);
        }
    }

}
