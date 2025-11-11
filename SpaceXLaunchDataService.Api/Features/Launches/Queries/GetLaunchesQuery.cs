using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data.Models.Enums;
using SpaceXLaunchDataService.Api.Features.Launches.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Queries;

public record GetLaunchesQuery(
    int Page = 1,
    int PageSize = 10,
    SortField SortBy = SortField.DateUtc,
    SortOrder SortOrder = SortOrder.Desc,
    bool? Success = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null
) : Query<OneOf<PaginatedLaunchesResponse, ServiceError>>;