using SpaceXLaunchDataService.Data;
using SpaceXLaunchDataService.Data.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SpaceXLaunchDataService.Features.Launches.Endpoints;

/// <summary>
/// Request parameters for retrieving launches with filtering, sorting, and pagination
/// </summary>
public class GetLaunchesRequest
{
    /// <summary>
    /// Page number for pagination (starts from 1)
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (maximum 100)
    /// </summary>
    /// <example>10</example>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Field to sort by
    /// </summary>
    /// <example>DateUtc</example>
    public SortField SortBy { get; set; } = SortField.DateUtc;

    /// <summary>
    /// Sort order direction
    /// </summary>
    /// <example>Desc</example>
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;

    /// <summary>
    /// Filter by launch success status (true for successful, false for failed, null for all)
    /// </summary>
    /// <example>true</example>
    public bool? Success { get; set; }

    /// <summary>
    /// Filter launches from this date onwards (yyyy-MM-dd format)
    /// </summary>
    /// <example>2006-03-24</example>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter launches up to this date (yyyy-MM-dd format)
    /// </summary>
    /// <example>2024-12-31</example>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Search term to find in mission names or details
    /// </summary>
    /// <example>Falcon</example>
    public string? SearchTerm { get; set; }

    // Validation properties
    public int MaxPageSize { get; } = 100;
    public int ValidatedPageSize => PageSize > MaxPageSize ? MaxPageSize : PageSize < 1 ? 10 : PageSize;
    public int ValidatedPage => Page < 1 ? 1 : Page;

    // Helper property to convert enum to string for backward compatibility
    public string SortByString => SortBy.ToString().ToLower();
    public string SortOrderString => SortOrder.ToString().ToLower();
}

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

/// <summary>
/// Get launches with advanced filtering, sorting, and pagination
/// </summary>
public static class GetLaunches
{
    /// <summary>
    /// Retrieves a paginated list of SpaceX launches with filtering and sorting options
    /// </summary>
    /// <param name="page">Page number (starts from 1)</param>
    /// <param name="pageSize">Number of items per page (1-100)</param>
    /// <param name="sortBy">Field to sort by as string (DateUtc, Name, FlightNumber, Success)</param>
    /// <param name="sortOrder">Sort direction as string (Asc for ascending, Desc for descending)</param>
    /// <param name="success">Filter by success status: true=successful, false=failed, null=all</param>
    /// <param name="fromDate">Start date filter in yyyy-MM-dd format (e.g., 2006-03-24)</param>
    /// <param name="toDate">End date filter in yyyy-MM-dd format (e.g., 2024-12-31)</param>
    /// <param name="searchTerm">Search in mission names and details</param>
    /// <param name="repository">Launch repository service</param>
    /// <returns>Paginated launch results</returns>
    /// <response code="200">Successfully retrieved launches</response>
    /// <response code="400">Invalid request parameters</response>
    public static async Task<IResult> HandleAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "DateUtc",
        [FromQuery] string sortOrder = "Desc",
        [FromQuery] bool? success = null,
        [FromQuery] string? fromDate = null,
        [FromQuery] string? toDate = null,
        [FromQuery] string? searchTerm = null,
        ILaunchRepository repository = null!)
    {
        // Parse date filters if provided
        DateTime? parsedFromDate = null;
        DateTime? parsedToDate = null;

        if (!string.IsNullOrEmpty(fromDate))
        {
            if (!DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var from))
            {
                return Results.BadRequest(new { Error = "Invalid fromDate format. Use yyyy-MM-dd format (e.g., 2024-01-15)" });
            }
            parsedFromDate = from;
        }

        if (!string.IsNullOrEmpty(toDate))
        {
            if (!DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var to))
            {
                return Results.BadRequest(new { Error = "Invalid toDate format. Use yyyy-MM-dd format (e.g., 2024-01-15)" });
            }
            parsedToDate = to.AddDays(1).AddTicks(-1); // End of the day
        }

        // Parse enum parameters
        if (!Enum.TryParse<SortField>(sortBy, ignoreCase: true, out var parsedSortBy))
        {
            return Results.BadRequest(new { Error = $"Invalid sortBy value '{sortBy}'. Valid values: DateUtc, Name, FlightNumber, Success" });
        }

        if (!Enum.TryParse<SortOrder>(sortOrder, ignoreCase: true, out var parsedSortOrder))
        {
            return Results.BadRequest(new { Error = $"Invalid sortOrder value '{sortOrder}'. Valid values: Asc, Desc" });
        }

        // Validate request parameters
        if (page < 1)
        {
            return Results.BadRequest(new { Error = "Page must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Results.BadRequest(new { Error = "PageSize must be between 1 and 100" });
        }

        // Create request with parsed enum values
        var request = new GetLaunchesRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = parsedSortBy,
            SortOrder = parsedSortOrder,
            Success = success,
            FromDate = parsedFromDate,
            ToDate = parsedToDate,
            SearchTerm = searchTerm
        };

        var result = await repository.GetLaunchesAsync(request);

        return result.Match<IResult>(
            launches => Results.Ok(launches),
            error => Results.BadRequest(new { Error = error })
        );
    }
}
