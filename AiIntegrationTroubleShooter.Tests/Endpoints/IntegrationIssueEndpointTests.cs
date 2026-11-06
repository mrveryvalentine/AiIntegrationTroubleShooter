using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;
using AiIntegrationTroubleShooter.Api.Exceptions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;

namespace AiIntegrationTroubleShooter.Tests.Endpoints;

public class IntegrationIssueEndpointTests
{
    [Fact]
    public async Task PostIntegrationIssue_WhenProviderFails_Returns503()
    {
        // Arrange
        await using var factory = new WebApplicationFactory<Program>();

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAIAnalysisProvider>();

                services.AddScoped<IAIAnalysisProvider, FailingAIAnalysisProvider>();
            });
        }).CreateClient();

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
        await using var factory = new WebApplicationFactory<Program>();

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAIAnalysisProvider>();

                services.AddScoped<IAIAnalysisProvider, UnexpectedErrorProvider>();
            });
        }).CreateClient();

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
}