using AiIntegrationTroubleShooter.Api.Contracts;

namespace AiIntegrationTroubleShooter.Api.AI;

public interface IAIAnalysisProvider
{
    Task<IntegrationAnalysisResponse> AnalyseAsync(
        IntegrationIssueRequest request, 
        CancellationToken cancellationToken = default);
}
