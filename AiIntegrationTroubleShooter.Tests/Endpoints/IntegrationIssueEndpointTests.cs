using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;
using AiIntegrationTroubleShooter.Api.Exceptions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using AiIntegrationTroubleShooter.Api.Configuration;


namespace AiIntegrationTroubleShooter.Tests.Endpoints;

public class IntegrationIssueEndpointTests
{
    private const string TestApiKey ="test-api-key-for-integration-tests-32";

    [Fact]
    public async Task PostIntegrationIssue_WhenProviderFails_Returns503()
    {
        // Arrange
        await using var factory = CreateFactory(services =>
        {
            services.RemoveAll<IAIAnalysisProvider>();

            services.AddScoped<IAIAnalysisProvider, FailingAIAnalysisProvider>();
        });

        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WhenUnexpectedErrorOccurs_Returns500()
    {
        // Arrange
        await using var factory = CreateFactory(services =>
        {
            services.RemoveAll<IAIAnalysisProvider>();

            services.AddScoped<IAIAnalysisProvider, UnexpectedErrorProvider>();
        });

        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WhenRequiredFieldsAreMissing_Returns400()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WhenJsonIsMalformed_Returns400()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = CreateAuthenticatedClient(factory);

        var content = new StringContent(
            """{"integrationName": "Test integration", "errorMessage": """);

        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        // Act
        var response = await client.PostAsync(
            "/api/integration-issues",
            content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WhenRequestIsInvalid_DoesNotCallProvider()
    {
        // Arrange
        var provider = new TrackingAIAnalysisProvider();

        await using var factory = CreateFactory(services =>
        {
            services.RemoveAll<IAIAnalysisProvider>();

            services.AddSingleton<IAIAnalysisProvider>(provider);
        });

        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, provider.CallCount);
    }


    [Fact]
    public async Task PostIntegrationIssue_WithoutApiKey_Returns401()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WithInvalidApiKey_Returns401()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-API-Key",
            "incorrect-test-api-key");

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WithEmptyApiKey_Returns401()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "X-API-Key",
            string.Empty);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WithMultipleApiKeys_Returns401()
    {
        // Arrange
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-API-Key",
            new[]
            {
            TestApiKey,
            "another-test-api-key"
            });

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WithValidApiKey_Returns200()
    {
        // Arrange
        await using var factory = CreateFactory(services =>
        {
            services.RemoveAll<IAIAnalysisProvider>();
            services.AddScoped<IAIAnalysisProvider, SuccessfulAIAnalysisProvider>();
        });

        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/integration-issues",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostIntegrationIssue_WhenRateLimitIsExceeded_Returns429()
    {
        // Arrange
        await using var factory = CreateFactory(services =>
        {
            services.RemoveAll<IAIAnalysisProvider>();
            services.AddScoped<IAIAnalysisProvider, SuccessfulAIAnalysisProvider>();
        });

        using var client = CreateAuthenticatedClient(factory);

        var request = new IntegrationIssueRequest
        {
            IntegrationName = "Test integration",
            SourceSystem = "System A",
            TargetSystem = "System B",
            ErrorMessage = "Connection failed"
        };

        // Act
        HttpResponseMessage? lastResponse = null;

        for (int i = 0; i < 11; i++)
        {
            lastResponse = await client.PostAsJsonAsync(
                "/api/integration-issues",
                request);

            if (i < 10)
            {
                Assert.Equal(HttpStatusCode.OK, lastResponse.StatusCode);
            }
        }

        // Assert
        Assert.NotNull(lastResponse);
        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            lastResponse.StatusCode);
    }

    private class SuccessfulAIAnalysisProvider : IAIAnalysisProvider
    {
        public Task<IntegrationAnalysisResponse> AnalyseAsync(
            IntegrationIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new IntegrationAnalysisResponse
            {
                Summary = "The integration failed.",
                RequiresHumanReview = true
            };

            return Task.FromResult(response);
        }
    }


    private static WebApplicationFactory<Program> CreateFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        var factory = new WebApplicationFactory<Program>();

        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<ApiKeySettings>(
                    options => options.ApiKey = TestApiKey);

                configureServices?.Invoke(services);
            });
        });
    }

    private static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-API-Key", TestApiKey);

        return client;
    }

    private class UnexpectedErrorProvider : IAIAnalysisProvider
    {
        public Task<IntegrationAnalysisResponse> AnalyseAsync(
            IntegrationIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NullReferenceException("Simulated programming error.");
        }
    }

    private class FailingAIAnalysisProvider : IAIAnalysisProvider
    {
        public Task<IntegrationAnalysisResponse> AnalyseAsync(
            IntegrationIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new AIProviderException("Simulated provider failure.");
        }
    }

    private class TrackingAIAnalysisProvider : IAIAnalysisProvider
    {
        public int CallCount { get; private set; }

        public Task<IntegrationAnalysisResponse> AnalyseAsync(
            IntegrationIssueRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            throw new InvalidOperationException(
                "Provider should not be called for invalid requests.");
        }
    }
}