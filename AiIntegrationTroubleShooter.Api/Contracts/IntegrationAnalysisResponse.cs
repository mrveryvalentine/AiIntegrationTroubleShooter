using System.ComponentModel.DataAnnotations;

namespace AiIntegrationTroubleShooter.Api.Contracts;

public class IntegrationAnalysisResponse
{
    [Required]
    [StringLength(1000, MinimumLength = 5)]
    public string Summary { get; set; } = string.Empty;

    public List<PossibleCause> PossibleCauses { get; set; }
        = new List<PossibleCause>();

    public List<string> InvestigationSteps { get; set; }
        = new List<string>();

    public List<string> ImmediateActions { get; set; }
        = new List<string>();

    public List<string> Warnings { get; set; }
        = new List<string>();

    public bool RequiresHumanReview { get; set; } = true;
}

public class PossibleCause
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Cause { get; set; } = string.Empty;

    [Required]
    public string Confidence { get; set; } = "Low";

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    public string Reasoning { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    public string EvidenceToConfirm { get; set; } = string.Empty;
}