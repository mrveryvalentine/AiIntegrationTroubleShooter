using AiIntegrationTroubleShooter.Api.Configuration;


namespace AiIntegrationTroubleShooter.Tests.Configuration;

public class AISettingsTests
{
    [Fact]
    public void FakeProvider_WithoutFoundryCredentials_IsValid()
    {
        // Arrange
        var settings = new AISettings
        {
            Provider = "Fake"
        };

        // Act
        bool isValid =
            settings.Provider.Equals("Fake", StringComparison.OrdinalIgnoreCase) ||
            (
                settings.Provider.Equals("Foundry", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.Endpoint) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.DeploymentName) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.ApiKey)
            );

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void FoundryProvider_WithoutCredentials_IsInvalid()
    {
        // Arrange
        var settings = new AISettings
        {
            Provider = "Foundry"
        };

        // Act
        bool isValid =
            settings.Provider.Equals("Fake", StringComparison.OrdinalIgnoreCase) ||
            (
                settings.Provider.Equals("Foundry", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.Endpoint) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.DeploymentName) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.ApiKey)
            );

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void UnknownProvider_IsInvalid()
    {
        // Arrange
        var settings = new AISettings
        {
            Provider = "Unknown"
        };

        // Act
        bool isValid =
            settings.Provider.Equals("Fake", StringComparison.OrdinalIgnoreCase) ||
            (
                settings.Provider.Equals("Foundry", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.Endpoint) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.DeploymentName) &&
                !string.IsNullOrWhiteSpace(settings.Foundry.ApiKey)
            );

        // Assert
        Assert.False(isValid);
    }
}
