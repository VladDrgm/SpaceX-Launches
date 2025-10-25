using OneOf;
using SpaceXLaunches.Data.Types;

namespace SpaceXLaunches.Features.Launches.Services;

public interface ISpaceXApiService
{
    Task<OneOf<List<Launch>, string>> FetchLaunchesAsync();
}
