using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;

namespace AiIntegrationTroubleShooter.Api.Services;

public class IntegrationAnalysisService
{
    private readonly IAIAnalysisProvider _aiAnalysisProvider;

    public IntegrationAnalysisService(IAIAnalysisProvider aiAnalysisProvider)
    {
        _aiAnalysisProvider = aiAnalysisProvider;
    }

    public async Task<IntegrationAnalysisResponse> AnalyseAsync(IntegrationIssueRequest request, CancellationToken cancellationToken = default)
    {
        return await _aiAnalysisProvider.AnalyseAsync(
            request, 
            cancellationToken);
    }
}