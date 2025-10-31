using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Swagger;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Api;

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
        var launches = app.MapGroup("/api/v1/launches")
            .WithTags("Launches")
            .WithDescription("SpaceX launch data endpoints with filtering, sorting, and pagination");

        launches.MapGet("", GetLaunches.HandleAsync)
            .ConfigureGetLaunchesSwagger();

        launches.MapGet("{id}", GetLaunchDetails.HandleAsync)
            .ConfigureGetLaunchDetailsSwagger();

        launches.MapPost("sync", SyncLaunches.HandleAsync)
            .ConfigureSyncLaunchesSwagger();
    }

    private static void MapRocketsEndpoints(WebApplication app)
    {
        //BLABLA    
    }
    
    //OTHER ENDPOINTS  
}
