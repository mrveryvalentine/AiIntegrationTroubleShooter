using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Configuration;

public class AISettings
{
    [Required]
    public string Provider { get; set; } = "Fake";

    public FoundrySettings Foundry { get; set; } = new FoundrySettings();
}

public class FoundrySettings
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}