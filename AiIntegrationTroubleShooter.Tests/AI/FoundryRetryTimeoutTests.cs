
using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Configuration;
using AiIntegrationTroubleShooter.Api.Contracts;
using AiIntegrationTroubleShooter.Api.Exceptions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Moq;

namespace AiIntegrationTroubleShooter.Tests.AI;

public class FoundryRetryTimeoutTests
{
    [Fact]
    public async Task AnalyseAsync_WhenRetryDelayExceedsTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var handler = new RateLimitHandler();

        using var httpClient = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://localhost/openai/v1"),
            Transport = new HttpClientPipelineTransport(httpClient),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 1)
        };

        var chatClient = new ChatClient(
            "test-deployment",
            new ApiKeyCredential("test-api-key"),
            clientOptions);

        IChatClient aiChatClient = chatClient.AsIChatClient();

        var settings = new AISettings
        {
            Foundry = new FoundrySettings
            {
                TimeoutSeconds = 1
            }
        };

        var sanitizer = new IntegrationIssueSanitizer();
        var provider = new FoundryAIAnalysisProvider(
            aiChatClient,
            Options.Create(settings),
            Mock.Of<ILogger<FoundryAIAnalysisProvider>>(),
            sanitizer);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var exception = await Assert.ThrowsAsync<AIProviderException>(
            async () => await provider.AnalyseAsync(request));

        // Assert
        Assert.Contains("timed out", exception.Message);
        Assert.Equal(1, handler.RequestCount);
    }

    private class RateLimitHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            var response = new HttpResponseMessage(
                HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent(
                    """{"error":{"message":"Rate limit exceeded"}}""",
                    Encoding.UTF8,
                    "application/json")
            };

            response.Headers.RetryAfter =
                new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));

            return Task.FromResult(response);
        }
    }
}
