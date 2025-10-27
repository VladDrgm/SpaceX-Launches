using SpaceXLaunchDataService.Api;
using SpaceXLaunchDataService.Api.Common.Extensions;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Database;

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

// Configure application startup logging
app.ConfigureApplicationStartupLogging();

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
