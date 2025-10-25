using OneOf;
using SpaceXLaunches.Data.Types;
using SpaceXLaunches.Data.Types.Requests;

namespace SpaceXLaunches.Data;

public interface ILaunchRepository
{
    Task<OneOf<PaginatedLaunchesDto, string>> GetLaunchesAsync(GetLaunchesRequest request);
    Task<OneOf<LaunchDto, string>> GetLaunchByIdAsync(string id);
    Task<OneOf<int, string>> SaveLaunchesAsync(IEnumerable<Launch> launches);
}
