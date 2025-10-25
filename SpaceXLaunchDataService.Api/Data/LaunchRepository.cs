using OneOf;
using SpaceXLaunches.Data.Types;
using SpaceXLaunches.Data.Types.Requests;

namespace SpaceXLaunches.Data;

public class LaunchRepository : ILaunchRepository
{
    public async Task<OneOf<PaginatedLaunchesDto, string>> GetLaunchesAsync(GetLaunchesRequest request)
    {
        // Stub implementation
        await Task.CompletedTask;
        return new PaginatedLaunchesDto
        {
            Launches = new List<LaunchDto>(),
            TotalCount = 0,
            PageSize = request.PageSize,
            CurrentPage = request.Page,
            TotalPages = 0
        };
    }

    public async Task<OneOf<LaunchDto, string>> GetLaunchByIdAsync(string id)
    {
        // Stub implementation
        await Task.CompletedTask;
        return "Not found";
    }

    public async Task<OneOf<int, string>> SaveLaunchesAsync(IEnumerable<Launch> launches)
    {
        // Stub implementation
        await Task.CompletedTask;
        return 0;
    }
}
