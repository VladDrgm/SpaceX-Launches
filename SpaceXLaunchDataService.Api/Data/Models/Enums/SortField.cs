using System.ComponentModel;

namespace SpaceXLaunchDataService.Api.Data.Models.Enums;

/// <summary>
/// Available fields for sorting launch results
/// </summary>
public enum SortField
{
    /// <summary>
    /// Sort by launch date (UTC)
    /// </summary>
    [Description("Sort by launch date (UTC)")]
    DateUtc,

    /// <summary>
    /// Sort by mission name
    /// </summary>
    [Description("Sort by mission name")]
    Name,

    /// <summary>
    /// Sort by flight number
    /// </summary>
    [Description("Sort by flight number")]
    FlightNumber,

    /// <summary>
    /// Sort by launch success status
    /// </summary>
    [Description("Sort by launch success status")]
    Success
}