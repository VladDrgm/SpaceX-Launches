using OneOf;
using SpaceXLaunchDataService.Data.Models;

namespace SpaceXLaunchDataService.Features.Launches.Services;

public interface ISpaceXApiService
{
    Task<OneOf<List<Launch>, string>> FetchLaunchesAsync();
}
