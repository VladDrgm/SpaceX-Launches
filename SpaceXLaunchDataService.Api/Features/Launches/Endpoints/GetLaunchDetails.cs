using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SpaceXLaunchDataService.Api.Data;

namespace SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

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

/// <summary>
/// Get launch details by ID
/// </summary>
public static class GetLaunchDetails
{
    /// <summary>
    /// Retrieves detailed information for a specific SpaceX launch by its ID
    /// </summary>
    /// <param name="id">Launch ID. Examples: "1" (FalconSat), "2" (DemoSat), "3" (Trailblazer)</param>
    /// <param name="repository">Launch repository service</param>
    /// <returns>Detailed launch information</returns>
    /// <response code="200">Successfully retrieved launch details</response>
    /// <response code="400">Invalid launch ID provided</response>
    /// <response code="404">Launch not found with the specified ID</response>
    public static async Task<IResult> HandleAsync(
        [FromRoute] string id,
        ILaunchRepository repository)
    {
        // Validate that ID is not null or empty
        if (string.IsNullOrWhiteSpace(id))
        {
            return Results.BadRequest(new { Error = "Launch ID is required. Use a valid launch ID (e.g., '1', '2', '3')" });
        }

        var result = await repository.GetLaunchByIdAsync(id);

        return result.Match<IResult>(
            launch => Results.Ok(launch),
            error => error.Code switch
            {
                "NOT_FOUND" => Results.NotFound(new { Error = error.Message, Code = error.Code }),
                _ => Results.BadRequest(new { Error = error.Message, Code = error.Code, Details = error.Details })
            }
        );
    }
}
