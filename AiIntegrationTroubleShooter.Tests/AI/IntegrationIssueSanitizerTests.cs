
using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Contracts;

namespace AiIntegrationTroubleShooter.Tests.AI;

public class IntegrationIssueSanitizerTests
{
    private readonly IntegrationIssueSanitizer _sanitizer = new();

    [Fact]
    public void Sanitize_PasswordInErrorMessage_RedactsPassword()
    {
        var request = new IntegrationIssueRequest
        {
            ErrorMessage = "Connection failed. Password=MySecret123"
        };

        var result = _sanitizer.Sanitize(request);

        Assert.Contains("Password=[REDACTED]", result.ErrorMessage);
        Assert.DoesNotContain("MySecret123", result.ErrorMessage);
    }

    [Fact]
    public void Sanitize_BearerTokenInContext_RedactsToken()
    {
        var request = new IntegrationIssueRequest
        {
            Context = "Authorization: Bearer abc123token"
        };

        var result = _sanitizer.Sanitize(request);

        Assert.Contains("Bearer [REDACTED]", result.Context);
        Assert.DoesNotContain("abc123token", result.Context);
    }

    [Fact]
    public void Sanitize_DoesNotModifyOriginalRequest()
    {
        var request = new IntegrationIssueRequest
        {
            ErrorMessage = "Password=MySecret123"
        };

        var result = _sanitizer.Sanitize(request);

        Assert.Contains("MySecret123", request.ErrorMessage);
        Assert.Contains("[REDACTED]", result.ErrorMessage);
    }

    [Fact]
    public void Sanitize_NullContext_RemainsNull()
    {
        var request = new IntegrationIssueRequest
        {
            Context = null
        };

        var result = _sanitizer.Sanitize(request);

        Assert.Null(result.Context);
    }
}
