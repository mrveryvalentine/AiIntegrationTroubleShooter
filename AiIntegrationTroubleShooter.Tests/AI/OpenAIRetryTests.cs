
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
        var handler = new RetryTestHandler(alwaysReturnError: true);

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

    [Fact]
    public async Task ChatClient_WhenFirstRequestReturns429_RetriesAndSucceeds()
    {
        // Arrange
        var handler = new RetryTestHandler(
            errorStatusCode: HttpStatusCode.TooManyRequests);

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
    public async Task ChatClient_WhenAllRequestsReturn429_ThrowsException()
    {
        // Arrange
        var handler = new RetryTestHandler(
            alwaysReturnError: true,
            errorStatusCode: HttpStatusCode.TooManyRequests);

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

    [Fact]
    public async Task ChatClient_When429IncludesRetryAfter_WaitsBeforeRetrying()
    {
        // Arrange
        var handler = new RetryTestHandler(
            errorStatusCode: HttpStatusCode.TooManyRequests,
            retryAfterSeconds: 2);

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

        Assert.Equal(
            "Hello from test",
            result.Value.Content[0].Text);

        var delay = handler.RequestTimes[1] - handler.RequestTimes[0];

        Assert.True(
            delay >= TimeSpan.FromSeconds(1.8),
            $"Expected at least 1.8 seconds between requests, but was {delay.TotalSeconds:F2} seconds.");
    }

    private class RetryTestHandler : HttpMessageHandler
    {

        private readonly bool _alwaysReturnError;
        private readonly HttpStatusCode _errorStatusCode;
        private readonly int _retryAfterSeconds;

        public RetryTestHandler(
            bool alwaysReturnError = false,
            HttpStatusCode errorStatusCode = HttpStatusCode.ServiceUnavailable,
            int retryAfterSeconds = 0)
        {
            _alwaysReturnError = alwaysReturnError;
            _errorStatusCode = errorStatusCode;
            _retryAfterSeconds = retryAfterSeconds;
        }

        public int RequestCount { get; private set; }

        public List<DateTimeOffset> RequestTimes { get; }
            = new List<DateTimeOffset>();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            RequestTimes.Add(DateTimeOffset.UtcNow);

            if (_alwaysReturnError || RequestCount == 1)
            {
                var response = new HttpResponseMessage(_errorStatusCode)
                {
                    Content = new StringContent(
                        """{"error":{"message":"Temporary service failure"}}""",
                        Encoding.UTF8,
                        "application/json")
                };

                response.Headers.RetryAfter =
                    new RetryConditionHeaderValue(
                        TimeSpan.FromSeconds(_retryAfterSeconds));

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
