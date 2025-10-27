using OneOf;
using SpaceXLaunchDataService.Api.Data.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Services;

public class SpaceXApiService : ISpaceXApiService
{
    private readonly HttpClient _httpClient;

    public SpaceXApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.spacexdata.com/v4/");
    }

    public async Task<OneOf<List<Launch>, string>> FetchLaunchesAsync()
    {
        // Stub implementation - in production this would fetch from SpaceX API
        await Task.CompletedTask;
        return new List<Launch>();
    }
}
