
using AiIntegrationTroubleShooter.Api.Contracts;
using AiIntegrationTroubleShooter.Api.Exceptions;
using AiIntegrationTroubleShooter.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Endpoints;

public static class IntegrationIssueEndpoints
{
    public static void MapIntegrationIssueEndpoints(this WebApplication app)
    {
        app.MapPost("/api/integration-issues",
            async (
                IntegrationIssueRequest request,
                IntegrationAnalysisService analysisService,
                ILoggerFactory loggerFactory,
                CancellationToken cancellationToken) =>
            {
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                var logger = loggerFactory.CreateLogger("IntegrationIssueEndpoints");

                bool isValid = Validator.TryValidateObject(
                    request,
                    validationContext,
                    validationResults,
                    true);

                if (!isValid)
                {
                    return Results.ValidationProblem(
                        validationResults
                            .GroupBy(x => x.MemberNames.FirstOrDefault() ?? "")
                            .ToDictionary(
                                x => x.Key,
                                x => x.Select(v => v.ErrorMessage ?? "Invalid value").ToArray()
                            )
                    );
                }

                try
                {
                    var response = await analysisService.AnalyseAsync(
                        request,
                        cancellationToken);

                    return Results.Ok(response);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug(
                        "Integration analysis request was cancelled by the client.");

                    throw;
                }
                catch (AIProviderException ex)
                {
                    logger.LogError(
                        ex,
                        "AI provider failed to analyse an integration request.");

                    return Results.Problem(
                        title: "AI analysis unavailable",
                        detail: "The AI analysis service could not process your request.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            })
            .RequireAuthorization()
            .RequireRateLimiting("AIAnalysis");
    }
}
