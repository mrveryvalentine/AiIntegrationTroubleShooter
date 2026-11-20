
using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Configuration;

public class ApiKeySettings
{
    [Required]
    [MinLength(32)]
    public string ApiKey { get; set; } = string.Empty;
}
