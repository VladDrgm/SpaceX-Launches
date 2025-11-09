using Dapper;
using OneOf;
using Polly;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Data.Models.Enums;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Database;

public class SqliteLaunchRepository : ILaunchRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<SqliteLaunchRepository> _logger;

    public SqliteLaunchRepository(
        IDatabaseConnectionFactory connectionFactory,
        ILogger<SqliteLaunchRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        
        // Initialize resilience pipeline for database operations
        _resiliencePipeline = ResiliencePolicies.CreateDatabaseResiliencePipeline(logger);
    }

    public async Task<OneOf<PaginatedLaunchesResponse, ServiceError>> GetLaunchesAsync(GetLaunchesRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            // Build dynamic query with filters
            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            // Apply filters
            if (request.Success.HasValue)
            {
                whereConditions.Add("Success = @Success");
                parameters.Add("Success", request.Success.Value ? 1 : 0);
            }

            if (request.FromDate.HasValue)
            {
                whereConditions.Add("DateUtc >= @FromDate");
                parameters.Add("FromDate", request.FromDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (request.ToDate.HasValue)
            {
                whereConditions.Add("DateUtc <= @ToDate");
                parameters.Add("ToDate", request.ToDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                whereConditions.Add("(Name LIKE @SearchTerm OR Details LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{request.SearchTerm}%");
            }

            // Build WHERE clause
            var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

            // Build ORDER BY clause
            var orderByColumn = request.SortBy switch
            {
                SortField.DateUtc => "DateUtc",
                SortField.Name => "Name",
                SortField.FlightNumber => "FlightNumber",
                SortField.Success => "Success",
                _ => "DateUtc"
            };

            var orderDirection = request.SortOrder == SortOrder.Desc ? "DESC" : "ASC";
            var orderByClause = $"ORDER BY {orderByColumn} {orderDirection}";

            // Get total count
            var countSql = $"SELECT COUNT(*) FROM Launches {whereClause}";
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // Calculate pagination
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var offset = (request.Page - 1) * request.PageSize;

            // Get paginated results
            var dataSql = $"""
                SELECT Id, FlightNumber, Name, DateUtc, Success, Details
                FROM Launches 
                {whereClause}
                {orderByClause}
                LIMIT @PageSize OFFSET @Offset
                """;

            parameters.Add("PageSize", request.PageSize);
            parameters.Add("Offset", offset);

            var entities = await connection.QueryAsync<LaunchEntity>(dataSql, parameters);

            // Convert to response DTOs
            var launches = entities.Select(entity => new LaunchResponse
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,
                Name = entity.Name,
                DateUtc = entity.DateUtc,
                Success = entity.Success,
                Details = entity.Details
            }).ToList();

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
            _logger.LogError(ex, "Error retrieving launches from database");
            return ServiceError.Database("Failed to retrieve launches from database", ex);
        }
    }

    public async Task<OneOf<List<LaunchResponse>, ServiceError>> GetLaunchesByDateAsync(DateTime date)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = """
                SELECT Id, FlightNumber, Name, DateUtc, Success, Details
                FROM Launches 
                WHERE DATE(DateUtc) = DATE(@Date)
                ORDER BY DateUtc
                """;

            var entities = await connection.QueryAsync<LaunchEntity>(sql, new { Date = date.ToString("yyyy-MM-dd") });

            var launches = entities.Select(entity => new LaunchResponse
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,
                Name = entity.Name,
                DateUtc = entity.DateUtc,
                Success = entity.Success,
                Details = entity.Details
            }).ToList();

            return launches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving launches by date from database for date {Date}", date);
            return ServiceError.Database("Failed to retrieve launches by date from database", ex);
        }
    }

    public async Task<OneOf<LaunchDetailsResponse, ServiceError>> GetLaunchByIdAsync(string id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = """
                SELECT Id, FlightNumber, Name, DateUtc, Success, Details
                FROM Launches 
                WHERE Id = @Id
                """;

            var entity = await connection.QuerySingleOrDefaultAsync<LaunchEntity>(sql, new { Id = id });

            if (entity == null)
            {
                _logger.LogWarning("Launch with ID {Id} not found", id);
                return ServiceError.NotFound($"Launch with ID '{id}' not found");
            }

            var launchDetails = new LaunchDetailsResponse
            {
                Id = entity.Id,
                FlightNumber = entity.FlightNumber,
                Name = entity.Name,
                DateUtc = entity.DateUtc,
                Success = entity.Success,
                Details = entity.Details
            };

            return launchDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving launch by ID {Id} from database", id);
            return ServiceError.Database("Failed to retrieve launch from database", ex);
        }
    }

    public async Task<OneOf<int, ServiceError>> SaveLaunchesAsync(IEnumerable<Launch> launches)
    {
        try
        {
            // Execute database operation with resilience (retry on transient errors)
            var affectedRows = await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                using var connection = _connectionFactory.CreateConnection();

                var upsertSql = """
                    INSERT INTO Launches (Id, FlightNumber, Name, DateUtc, Success, Details, CreatedAt, UpdatedAt)
                    VALUES (@Id, @FlightNumber, @Name, @DateUtc, @Success, @Details, @CreatedAt, @UpdatedAt)
                    ON CONFLICT(Id) DO UPDATE SET
                        FlightNumber = excluded.FlightNumber,
                        Name = excluded.Name,
                        DateUtc = excluded.DateUtc,
                        Success = excluded.Success,
                        Details = excluded.Details,
                        UpdatedAt = excluded.UpdatedAt
                    """;

                var entities = launches.Select(launch => new
                {
                    launch.Id,
                    launch.FlightNumber,
                    launch.Name,
                    DateUtc = launch.DateUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                    Success = launch.Success.HasValue ? (launch.Success.Value ? 1 : 0) : (int?)null,
                    Details = launch.Details ?? "",
                    CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                });

                return await connection.ExecuteAsync(upsertSql, entities);
            }, CancellationToken.None);
            
            _logger.LogInformation("Successfully saved {Count} launches to database", affectedRows);
            return affectedRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save launches to database after all retry attempts");
            return ServiceError.Database("Failed to save launches to database", ex);
        }
    }
}

