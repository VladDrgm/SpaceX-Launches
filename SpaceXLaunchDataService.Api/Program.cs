using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpaceXLaunchDataService;
using SpaceXLaunchDataService.Extensions;
using SpaceXLaunchDataService.Common.Services.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureApplicationServices(builder.Configuration);
builder.Services.ConfigureJsonSerialization();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureHealthChecks();

var app = builder.Build();

// Initialize database on startup (async)
await InitializeDatabaseAsync(app.Services);

// Configure pipeline
app.ConfigurePipeline();

// Map endpoints
app.MapEndpoints();

// Log application URLs and important endpoints
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Use a lifecycle event to log after the server starts
app.Lifetime.ApplicationStarted.Register(() =>
{
    var baseUrl = "http://localhost:5000";
    if (app.Urls.Any())
    {
        baseUrl = app.Urls.First();
    }

    logger.LogInformation("🚀 SpaceX Launch Data Service API is running!");
    logger.LogInformation("🏥 Health Check: {BaseUrl}/health", baseUrl);

    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation("Swagger UI: {BaseUrl}/swagger", baseUrl);
        logger.LogInformation("Swagger JSON: {BaseUrl}/swagger/v1/swagger.json", baseUrl);
    }
    else
    {
        logger.LogInformation("Running in {Environment} mode - Swagger UI is disabled", app.Environment.EnvironmentName);
    }
});

app.Run();

// Initialize database asynchronously
static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var databaseFactory = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
    await databaseFactory.InitializeDatabaseAsync();
}

// Make the implicit Program class public for testing
public partial class Program { }
