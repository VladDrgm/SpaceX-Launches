using SpaceXLaunches.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureApplicationServices(builder.Configuration);
builder.Services.ConfigureJsonSerialization();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureHealthChecks();

var app = builder.Build();

// Configure pipeline
app.ConfigurePipeline();

// Map endpoints
app.MapHealthChecks("/health");
var launches = app.MapGroup("/api/v1/launches").WithTags("Launches");
launches.MapGet("", SpaceXLaunches.Features.Launches.Endpoints.GetLaunches.HandleAsync);
launches.MapGet("{id}", SpaceXLaunches.Features.Launches.Endpoints.GetLaunchById.HandleAsync);
launches.MapPost("sync", SpaceXLaunches.Features.Launches.Endpoints.SyncLaunches.HandleAsync);

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
