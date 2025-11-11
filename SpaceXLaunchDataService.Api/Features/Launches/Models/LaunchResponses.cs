using System.Text.Json.Serialization;

namespace SpaceXLaunchDataService.Api.Features.Launches.Models;

// Response model for launch list items
public class LaunchResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("flightNumber")]
    public int FlightNumber { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("dateUtc")]
    public DateTime DateUtc { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("details")]
    public required string Details { get; set; }
}

// Response model for paginated launch list
public class PaginatedLaunchesResponse
{
    [JsonPropertyName("launches")]
    public required List<LaunchResponse> Launches { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

// Response model for launch details
public class LaunchDetailsResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("flightNumber")]
    public int FlightNumber { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("dateUtc")]
    public DateTime DateUtc { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("details")]
    public required string Details { get; set; }
}