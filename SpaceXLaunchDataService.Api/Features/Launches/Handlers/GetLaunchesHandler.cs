using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Features.Launches.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Queries;

namespace SpaceXLaunchDataService.Api.Features.Launches.Handlers;

public class GetLaunchesHandler : IRequestHandler<GetLaunchesQuery, OneOf<PaginatedLaunchesResponse, ServiceError>>
{
    private readonly ILaunchRepository _repository;

    public GetLaunchesHandler(ILaunchRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<PaginatedLaunchesResponse, ServiceError>> Handle(GetLaunchesQuery request, CancellationToken cancellationToken)
    {
        // Convert query to the repository request format
        var repoRequest = new GetLaunchesRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            Success = request.Success,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            SearchTerm = request.SearchTerm
        };

        return await _repository.GetLaunchesAsync(repoRequest);
    }
}