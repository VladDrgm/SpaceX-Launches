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
        var response = await _httpClient.GetAsync("launches");

        if (!response.IsSuccessStatusCode)
        {
            return $"Error fetching launches: {response.ReasonPhrase}";
        }

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content))
        {
            return "Empty response from SpaceX API";
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        var launches = System.Text.Json.JsonSerializer.Deserialize<List<Launch>>(content, options);

        return launches ?? new List<Launch>();
    }
}
