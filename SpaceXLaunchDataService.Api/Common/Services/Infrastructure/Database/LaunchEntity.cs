namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Database;

/// <summary>
/// Database entity for Launch data
/// </summary>
public class LaunchEntity
{
    public required string Id { get; set; }
    public int FlightNumber { get; set; }
    public required string Name { get; set; }
    public DateTime DateUtc { get; set; }
    public bool? Success { get; set; }
    public required string Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}