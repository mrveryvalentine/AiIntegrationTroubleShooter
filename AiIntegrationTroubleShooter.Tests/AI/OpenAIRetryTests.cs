
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using OpenAI;
using OpenAI.Chat;

namespace AiIntegrationTroubleShooter.Tests.AI;

public class OpenAIRetryTests
{
    [Fact]
    public async Task ChatClient_WhenFirstRequestReturns503_RetriesAndSucceeds()
    {
        // Arrange
        var handler = new RetryTestHandler();

        using var httpClient = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://localhost/openai/v1"),
            Transport = new HttpClientPipelineTransport(httpClient),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 1)
        };

        var chatClient = new ChatClient(
            "test-deployment",
            new ApiKeyCredential("test-api-key"),
            options);

        // Act
        var result = await chatClient.CompleteChatAsync("Say hello.");

        // Assert
        Assert.Equal(2, handler.RequestCount);
        Assert.Equal("Hello from test", result.Value.Content[0].Text);
    }


    [Fact]
    public async Task ChatClient_WhenAllRequestsReturn503_ThrowsException()
    {
        // Arrange
        var handler = new RetryTestHandler(alwaysReturn503: true);

        using var httpClient = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://localhost/openai/v1"),
            Transport = new HttpClientPipelineTransport(httpClient),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 1)
        };

        var chatClient = new ChatClient(
            "test-deployment",
            new ApiKeyCredential("test-api-key"),
            options);

        // Act and Assert
        await Assert.ThrowsAsync<ClientResultException>(
            async () => await chatClient.CompleteChatAsync("Say hello."));

        Assert.Equal(2, handler.RequestCount);
    }


    private class RetryTestHandler : HttpMessageHandler
    {
        private readonly bool _alwaysReturn503;

        public RetryTestHandler(bool alwaysReturn503 = false)
        {
            _alwaysReturn503 = alwaysReturn503;
        }

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            if (_alwaysReturn503 || RequestCount == 1)
            {
                var response = new HttpResponseMessage(
                    HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(
                        """{"error":{"message":"Temporary service failure"}}""",
                        Encoding.UTF8,
                        "application/json")
                };

                response.Headers.RetryAfter =
                    new RetryConditionHeaderValue(TimeSpan.Zero);

                return Task.FromResult(response);
            }

            var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                        "id": "chatcmpl-test",
                        "object": "chat.completion",
                        "created": 1730000000,
                        "model": "test-deployment",
                        "choices": [
                            {
                                "index": 0,
                                "message": {
                                    "role": "assistant",
                                    "content": "Hello from test"
                                },
                                "finish_reason": "stop"
                            }
                        ],
                        "usage": {
                            "prompt_tokens": 1,
                            "completion_tokens": 1,
                            "total_tokens": 2
                        }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };

            return Task.FromResult(successResponse);
        }
    }
}
