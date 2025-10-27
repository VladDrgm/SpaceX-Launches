using System.Text.Json.Serialization;

namespace SpaceXLaunchDataService.Api.Data.Models;

public class Launch
{
    // Primary Key from SpaceX API
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("flight_number")]
    public int FlightNumber { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("date_utc")]
    public DateTime DateUtc { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; } // Nullable: true/false/null

    [JsonPropertyName("details")]
    public required string Details { get; set; }
}