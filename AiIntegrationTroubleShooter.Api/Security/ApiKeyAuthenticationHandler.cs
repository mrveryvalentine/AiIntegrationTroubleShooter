
using AiIntegrationTroubleShooter.Api.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace AiIntegrationTroubleShooter.Api.Security;

public class ApiKeyAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeySettings _apiKeySettings;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ApiKeySettings> apiKeySettings)
        : base(options, logger, encoder)
    {
        _apiKeySettings = apiKeySettings.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string headerName = "X-API-Key";

        if (!Request.Headers.TryGetValue(headerName, out var suppliedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Only accept exactly one non-empty API key.
        if (suppliedApiKey.Count != 1 ||
            string.IsNullOrWhiteSpace(suppliedApiKey[0]))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid API key."));
        }

        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedApiKey[0]!);
        var configuredBytes = Encoding.UTF8.GetBytes(_apiKeySettings.ApiKey);

        bool isValid = CryptographicOperations.FixedTimeEquals(
            suppliedBytes,
            configuredBytes);

        if (!isValid)
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyClient")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
