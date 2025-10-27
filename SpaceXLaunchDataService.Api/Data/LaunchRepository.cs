using OneOf;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Data.Models.Enums;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Api.Data;

public class LaunchRepository : ILaunchRepository
{
    // In-memory collection for demonstration - replace with actual data source
    private readonly List<LaunchResponse> _launches = new()
    {
        new LaunchResponse
        {
            Id = "1",
            Name = "FalconSat",
            DateUtc = new DateTime(2006, 3, 24, 22, 30, 0, DateTimeKind.Utc),
            FlightNumber = 1,
            Success = false,
            Details = "Engine failure at 33 seconds and loss of vehicle"
        },
        new LaunchResponse
        {
            Id = "2",
            Name = "DemoSat",
            DateUtc = new DateTime(2007, 3, 21, 1, 10, 0, DateTimeKind.Utc),
            FlightNumber = 2,
            Success = false,
            Details = "Successful first stage burn and transition to second stage, maximum altitude 289 km, Premature engine shutdown at T+7 min 30 s, Failed to reach orbit, Failed to recover first stage"
        },
        new LaunchResponse
        {
            Id = "3",
            Name = "Trailblazer",
            DateUtc = new DateTime(2008, 8, 2, 3, 34, 0, DateTimeKind.Utc),
            FlightNumber = 3,
            Success = false,
            Details = "Residual stage 1 thrust led to collision between stage 1 and stage 2"
        }
    };

    public async Task<OneOf<PaginatedLaunchesResponse, string>> GetLaunchesAsync(GetLaunchesRequest request)
    {
        await Task.CompletedTask;

        try
        {
            IEnumerable<LaunchResponse> query = _launches;

            // Apply filters
            if (request.Success.HasValue)
            {
                query = query.Where(l => l.Success == request.Success.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(l => l.DateUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(l => l.DateUtc <= request.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLowerInvariant();
                query = query.Where(l =>
                    l.Name.ToLowerInvariant().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(l.Details) && l.Details.ToLowerInvariant().Contains(searchTerm)));
            }

            // Apply sorting using enum values
            var isDescending = request.SortOrder == SortOrder.Desc;

            query = request.SortBy switch
            {
                SortField.DateUtc => isDescending
                    ? query.OrderByDescending(l => l.DateUtc)
                    : query.OrderBy(l => l.DateUtc),
                SortField.Name => isDescending
                    ? query.OrderByDescending(l => l.Name)
                    : query.OrderBy(l => l.Name),
                SortField.FlightNumber => isDescending
                    ? query.OrderByDescending(l => l.FlightNumber)
                    : query.OrderBy(l => l.FlightNumber),
                SortField.Success => isDescending
                    ? query.OrderByDescending(l => l.Success)
                    : query.OrderBy(l => l.Success),
                _ => query.OrderBy(l => l.DateUtc) // default sort
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // Apply pagination
            var launches = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedLaunchesResponse
            {
                Launches = launches,
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            return $"Error retrieving launches: {ex.Message}";
        }
    }

    public async Task<OneOf<List<LaunchResponse>, string>> GetLaunchesByDateAsync(DateTime date)
    {
        await Task.CompletedTask;

        try
        {
            var launches = _launches.Where(l => l.DateUtc.Date == date.Date).ToList();
            return launches;
        }
        catch (Exception ex)
        {
            return $"Error retrieving launches by date: {ex.Message}";
        }
    }

    public async Task<OneOf<LaunchDetailsResponse, string>> GetLaunchByIdAsync(string id)
    {
        await Task.CompletedTask;

        try
        {
            var launch = _launches.FirstOrDefault(l => l.Id == id);
            if (launch == null)
            {
                return "Launch not found";
            }

            // Convert to LaunchDetailsResponse
            var launchDetails = new LaunchDetailsResponse
            {
                Id = launch.Id,
                FlightNumber = launch.FlightNumber,
                Name = launch.Name,
                DateUtc = launch.DateUtc,
                Success = launch.Success,
                Details = launch.Details
            };

            return launchDetails;
        }
        catch (Exception ex)
        {
            return $"Error retrieving launch: {ex.Message}";
        }
    }

    public async Task<OneOf<int, string>> SaveLaunchesAsync(IEnumerable<Launch> launches)
    {
        await Task.CompletedTask;

        try
        {
            var count = 0;
            foreach (var launch in launches)
            {
                var dto = new LaunchResponse
                {
                    Id = launch.Id,
                    Name = launch.Name,
                    DateUtc = launch.DateUtc,
                    FlightNumber = launch.FlightNumber,
                    Success = launch.Success,
                    Details = launch.Details
                };

                var existingIndex = _launches.FindIndex(l => l.Id == dto.Id);
                if (existingIndex >= 0)
                {
                    _launches[existingIndex] = dto;
                }
                else
                {
                    _launches.Add(dto);
                }
                count++;
            }

            return count;
        }
        catch (Exception ex)
        {
            return $"Error saving launches: {ex.Message}";
        }
    }
}
