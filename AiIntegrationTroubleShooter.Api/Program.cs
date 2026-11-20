using AiIntegrationTroubleShooter.Api.AI;
using AiIntegrationTroubleShooter.Api.Configuration;
using AiIntegrationTroubleShooter.Api.Endpoints;
using AiIntegrationTroubleShooter.Api.Security;
using AiIntegrationTroubleShooter.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi;
using System.ClientModel.Primitives;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 64 * 1024;
});

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

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Description = "Enter your API key."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
        });
});

builder.Services
    .AddOptions<AISettings>()
    .Bind(builder.Configuration.GetSection("AI"))
    .ValidateDataAnnotations()
    .Validate(
        settings => settings.IsValidProviderConfiguration(),
        "Provider must be Fake or Foundry. Foundry requires Endpoint, DeploymentName and ApiKey.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ApiKeySettings>()
    .Bind(builder.Configuration.GetSection("Security"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", 
        options => { });

builder.Services.AddAuthorization();

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
        var chatClient = new OpenAI.Chat.ChatClient(
            foundrySettings.DeploymentName,
            new System.ClientModel.ApiKeyCredential(foundrySettings.ApiKey),
            new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri(foundrySettings.Endpoint),
                RetryPolicy = new ClientRetryPolicy(
                    maxRetries: 1)
            });

        return chatClient.AsIChatClient();
    });

    builder.Services.AddScoped<IAIAnalysisProvider, FoundryAIAnalysisProvider>();
}
else
{
    throw new InvalidOperationException(
        $"Unknown AI provider: {aiSettings.Provider}");
}

builder.Services.AddSingleton<IntegrationIssueSanitizer>();
builder.Services.AddScoped<IntegrationAnalysisService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("AIAnalysis", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.AutoReplenishment = true;
    });
});


var app = builder.Build();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapIntegrationIssueEndpoints();

app.Run();

public partial class Program { }