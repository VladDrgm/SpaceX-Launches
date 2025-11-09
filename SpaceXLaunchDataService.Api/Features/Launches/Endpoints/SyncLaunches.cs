using SpaceXLaunchDataService.Api.Features.Launches.Services;

namespace SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

public static class SyncLaunches
{
    public static async Task<IResult> HandleAsync(ISpaceXApiService spaceXService)
    {
        var result = await spaceXService.FetchLaunchesAsync();

        return result.Match<IResult>(
            launches => Results.Ok(new SyncSuccessResponse($"Synced {launches.Count} launches", launches.Count)),
            error => Results.BadRequest(new SyncErrorResponse(error.Message, error.Code, error.Details))
        );
    }
}

// Response Models
public record SyncSuccessResponse(string Message, int Count);
public record SyncErrorResponse(string Error, string Code, string? Details);
