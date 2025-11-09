using OneOf;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Services;

public interface ISpaceXApiService
{
    Task<OneOf<List<Launch>, ServiceError>> FetchLaunchesAsync();
}
