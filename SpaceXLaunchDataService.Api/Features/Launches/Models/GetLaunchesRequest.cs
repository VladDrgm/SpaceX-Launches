using SpaceXLaunchDataService.Api.Data.Models.Enums;

namespace SpaceXLaunchDataService.Api.Features.Launches.Models;

/// <summary>
/// Request parameters for retrieving launches with filtering, sorting, and pagination
/// </summary>
public class GetLaunchesRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SortField SortBy { get; set; } = SortField.DateUtc;
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
    public bool? Success { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
}