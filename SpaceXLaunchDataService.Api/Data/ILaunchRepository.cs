using OneOf;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Api.Data;

public interface ILaunchRepository
{
    Task<OneOf<PaginatedLaunchesResponse, string>> GetLaunchesAsync(GetLaunchesRequest request);
    Task<OneOf<List<LaunchResponse>, string>> GetLaunchesByDateAsync(DateTime date);
    Task<OneOf<LaunchDetailsResponse, string>> GetLaunchByIdAsync(string id);
    Task<OneOf<int, string>> SaveLaunchesAsync(IEnumerable<Launch> launches);
}
