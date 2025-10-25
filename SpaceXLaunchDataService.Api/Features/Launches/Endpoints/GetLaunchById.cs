using SpaceXLaunches.Data;

namespace SpaceXLaunches.Features.Launches.Endpoints;

public static class GetLaunchById
{
    public static async Task<IResult> HandleAsync(
        string id,
        ILaunchRepository repository)
    {
        var result = await repository.GetLaunchByIdAsync(id);

        return result.Match<IResult>(
            launch => Results.Ok(launch),
            error => Results.NotFound(new { Error = error })
        );
    }
}
