using OneOf;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Queries;

namespace SpaceXLaunchDataService.Api.Data;

public interface ILaunchRepository
{
    Task<OneOf<PaginatedLaunchesResponse, ServiceError>> GetLaunchesAsync(GetLaunchesRequest request);
    Task<OneOf<List<LaunchResponse>, ServiceError>> GetLaunchesByDateAsync(DateTime date);
    Task<OneOf<LaunchDetailsResponse, ServiceError>> GetLaunchByIdAsync(string id);
    Task<OneOf<int, ServiceError>> SaveLaunchesAsync(IEnumerable<Launch> launches);
}
