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
                CancellationToken cancellationToken) =>
            {
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);

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
                    throw;
                }
                catch (AIProviderException ex)
                {
                    app.Logger.LogError(
                        ex,
                        "AI provider failed to analyse integration issue.");

                    return Results.Problem(
                        title: "AI analysis unavailable",
                        detail: "The AI analysis service could not process your request.",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            });
    }
}