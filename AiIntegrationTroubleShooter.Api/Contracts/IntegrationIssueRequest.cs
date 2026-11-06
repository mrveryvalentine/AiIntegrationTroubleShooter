using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Contracts;

public class IntegrationIssueRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string IntegrationName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SourceSystem { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string TargetSystem { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ErrorCode { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string ErrorMessage { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Context { get; set; }
}
