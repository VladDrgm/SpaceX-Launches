using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SpaceXLaunches.Data.Types;

// DTO for the list and detail view
public class LaunchDto
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

// DTO for the paginated list response
public class PaginatedLaunchesDto
{
    [JsonPropertyName("launches")]
    public required List<LaunchDto> Launches { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
