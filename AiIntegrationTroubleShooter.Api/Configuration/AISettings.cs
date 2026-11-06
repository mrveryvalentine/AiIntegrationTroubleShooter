using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Configuration;

public class AISettings
{
    [Required]
    public string Provider { get; set; } = "Fake";

    public FoundrySettings Foundry { get; set; } = new FoundrySettings();

    public bool IsValidProviderConfiguration()
    {
        if (string.Equals(Provider, "Fake", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(Provider, "Foundry", StringComparison.OrdinalIgnoreCase))
        {
            return
                !string.IsNullOrWhiteSpace(Foundry.Endpoint) &&
                !string.IsNullOrWhiteSpace(Foundry.DeploymentName) &&
                !string.IsNullOrWhiteSpace(Foundry.ApiKey);
        }

        return false;
    }
}

public class FoundrySettings
{
    public string Endpoint { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;
}