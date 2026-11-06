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
        bool isValid = settings.IsValidProviderConfiguration();

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
        bool isValid = settings.IsValidProviderConfiguration();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void FoundryProvider_WithAllCredentials_IsValid()
    {
        // Arrange
        var settings = new AISettings
        {
            Provider = "Foundry",
            Foundry = new FoundrySettings
            {
                Endpoint = "https://example.openai.azure.com/",
                DeploymentName = "test-deployment",
                ApiKey = "test-key"
            }
        };

        // Act
        bool isValid = settings.IsValidProviderConfiguration();

        // Assert
        Assert.True(isValid);
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
        bool isValid = settings.IsValidProviderConfiguration();

        // Assert
        Assert.False(isValid);
    }
}