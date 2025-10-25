using SpaceXLaunches.Features.Launches.Services;

namespace SpaceXLaunches.Features.Launches.Endpoints;

public static class SyncLaunches
{
    public static async Task<IResult> HandleAsync(ISpaceXApiService spaceXService)
    {
        var result = await spaceXService.FetchLaunchesAsync();

        return result.Match<IResult>(
            launches => Results.Ok(new { Message = $"Synced {launches.Count} launches", Count = launches.Count }),
            error => Results.BadRequest(new { Error = error })
        );
    }
}
