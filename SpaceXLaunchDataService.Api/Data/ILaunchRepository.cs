using OneOf;
using SpaceXLaunchDataService.Data.Models;
using SpaceXLaunchDataService.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Data;

public interface ILaunchRepository
{
    Task<OneOf<PaginatedLaunchesResponse, string>> GetLaunchesAsync(GetLaunchesRequest request);
    Task<OneOf<List<LaunchResponse>, string>> GetLaunchesByDateAsync(DateTime date);
    Task<OneOf<LaunchDetailsResponse, string>> GetLaunchByIdAsync(string id);
    Task<OneOf<int, string>> SaveLaunchesAsync(IEnumerable<Launch> launches);
}
