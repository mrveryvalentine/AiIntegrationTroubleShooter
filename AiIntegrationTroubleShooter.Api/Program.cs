using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Configuration;
using AiIntegrationTroubleShooter.Api.Endpoints;
using AiIntegrationTroubleShooter.Api.Services;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel.Primitives;

var builder = WebApplication.CreateBuilder(args);
// Load local environment variables from .env
var envFile = Path.Combine(
    builder.Environment.ContentRootPath,
    ".env");

if (File.Exists(envFile))
{
    DotNetEnv.Env.Load(envFile);
}

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();


// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<AISettings>()
    .Bind(builder.Configuration.GetSection("AI"))
    .ValidateDataAnnotations()
    .Validate(
        settings => settings.IsValidProviderConfiguration(),
        "Provider must be Fake or Foundry. Foundry requires Endpoint, DeploymentName and ApiKey.")
    .ValidateOnStart();

var aiSettings = builder.Configuration
    .GetSection("AI")
    .Get<AISettings>() ?? new AISettings();

if (aiSettings.Provider.Equals("Fake", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IAIAnalysisProvider, FakeAIAnalysisProvider>();
}
else if (aiSettings.Provider.Equals("Foundry", StringComparison.OrdinalIgnoreCase))
{
    var foundrySettings = aiSettings.Foundry;

    builder.Services.AddSingleton<IChatClient>(serviceProvider =>
    {
        var clientOptions = new AzureOpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(maxRetries: 2)
        };

        var azureClient = new AzureOpenAIClient(
            new Uri(foundrySettings.Endpoint),
            new Azure.AzureKeyCredential(foundrySettings.ApiKey),
            clientOptions);

        return azureClient
            .GetChatClient(foundrySettings.DeploymentName)
            .AsIChatClient();
    });

    builder.Services.AddScoped<IAIAnalysisProvider, FoundryAIAnalysisProvider>();
}
else
{
    throw new InvalidOperationException(
        $"Unknown AI provider: {aiSettings.Provider}");
}

builder.Services.AddScoped<IntegrationAnalysisService>();



var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIntegrationIssueEndpoints();

app.Run();

public partial class Program { }