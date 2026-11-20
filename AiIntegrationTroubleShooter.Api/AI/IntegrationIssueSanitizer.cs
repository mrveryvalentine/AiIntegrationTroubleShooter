
using AiIntegrationTroubleShooter.Api.Contracts;
using System.Text.RegularExpressions;

namespace AiIntegrationTroubleShooter.Api.AI;

public class IntegrationIssueSanitizer
{
    private static readonly Regex[] SensitivePatterns =
    {
        // Authorization bearer tokens
        new Regex(
            @"(?i)(Authorization\s*[:=]\s*Bearer\s+)[^\s,;]+",
            RegexOptions.Compiled),

        // Password and secret assignments
        new Regex(
            @"(?i)\b(password|passwd|pwd|client_secret|clientsecret|api_key|apikey|access_token|refresh_token|secret)(\s*=\s*)(""[^""]*""|'[^']*'|[^;\s,]+)",
            RegexOptions.Compiled),

        // Common JSON-style secret assignments
        new Regex(
            @"(?i)(""?(?:password|client_secret|clientsecret|api_key|apikey|access_token|refresh_token|secret)""?)(\s*:\s*)(""[^""]*""|'[^']*'|[^,\s}]+)",
            RegexOptions.Compiled)
    };

    public IntegrationIssueRequest Sanitize(
        IntegrationIssueRequest request)
    {
        return new IntegrationIssueRequest
        {
            IntegrationName = SanitizeRequiredText(request.IntegrationName),
            SourceSystem = SanitizeRequiredText(request.SourceSystem),
            TargetSystem = SanitizeRequiredText(request.TargetSystem),
            ErrorCode = SanitizeText(request.ErrorCode),
            ErrorMessage = SanitizeRequiredText(request.ErrorMessage),
            Context = SanitizeText(request.Context)
        };
    }

    private static string SanitizeRequiredText(string value)
    {
        return SanitizeText(value) ?? string.Empty;
    }

    private static string? SanitizeText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        foreach (var pattern in SensitivePatterns)
        {
            value = pattern.Replace(value, match =>
            {
                if (pattern == SensitivePatterns[0])
                {
                    return match.Groups[1].Value + "[REDACTED]";
                }

                return match.Groups[1].Value
                    + match.Groups[2].Value
                    + "[REDACTED]";
            });
        }

        return value;
    }
}
