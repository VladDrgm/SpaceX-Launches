using OneOf;
using Polly;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;

namespace SpaceXLaunchDataService.Api.Features.Launches.Services;

public class SpaceXApiService : ISpaceXApiService
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;
    private readonly ILogger<SpaceXApiService> _logger;

    public SpaceXApiService(
        HttpClient httpClient, 
        ILogger<SpaceXApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.spacexdata.com/v4/");
        
        // Initialize resilience pipeline with retry, circuit breaker, and timeout
        _resiliencePipeline = ResiliencePolicies.CreateHttpResiliencePipeline(logger);
    }

    public async Task<OneOf<List<Launch>, ServiceError>> FetchLaunchesAsync()
    {
        try
        {
            // Execute HTTP request with resilience (retry + circuit breaker + timeout)
            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _httpClient.GetAsync("launches", ct),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SpaceX API returned status code {StatusCode}: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return ServiceError.Http(
                    $"Error fetching launches from SpaceX API",
                    details: $"Status: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("SpaceX API returned empty response");
                return ServiceError.Http("Empty response from SpaceX API");
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            };

            var launches = System.Text.Json.JsonSerializer.Deserialize<List<Launch>>(content, options);

            _logger.LogInformation("Successfully fetched {Count} launches from SpaceX API", launches?.Count ?? 0);
            
            return launches ?? new List<Launch>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch launches from SpaceX API after all retry attempts");
            return ServiceError.FromException(ex, "Failed to fetch launches from SpaceX API");
        }
    }
}
