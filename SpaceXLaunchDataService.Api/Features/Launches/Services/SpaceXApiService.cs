using OneOf;
using SpaceXLaunches.Data.Types;

namespace SpaceXLaunches.Features.Launches.Services;

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
        try
        {
            // Stub implementation
            await Task.CompletedTask;
            return new List<Launch>();
        }
        catch (Exception ex)
        {
            return $"Error fetching launches: {ex.Message}";
        }
    }
}
