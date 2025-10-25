using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types.Requests;

namespace SpaceXLaunches.Features.Launches.Endpoints;

public static class GetLaunches
{
    public static async Task<IResult> HandleAsync(
        int page = 1,
        int pageSize = 10,
        ILaunchRepository repository = null!)
    {
        var request = new GetLaunchesRequest { Page = page, PageSize = pageSize };
        var result = await repository.GetLaunchesAsync(request);

        return result.Match<IResult>(
            launches => Results.Ok(launches),
            error => Results.BadRequest(new { Error = error })
        );
    }
}
