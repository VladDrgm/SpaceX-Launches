using SpaceXLaunches.Features.Launches.Endpoints;

namespace SpaceXLaunches;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        MapHealthCheckEndpoints(app);
        MapLaunchEndpoints(app);
    }

    private static void MapHealthCheckEndpoints(WebApplication app)
    {
        app.MapHealthChecks("/health");
    }

    private static void MapLaunchEndpoints(WebApplication app)
    {
        var launches = app.MapGroup("/api/v1/launches").WithTags("Launches");

        launches.MapGet("", GetLaunches.HandleAsync);
        launches.MapGet("{id}", GetLaunchById.HandleAsync);
        launches.MapPost("sync", SyncLaunches.HandleAsync);
    }
}
