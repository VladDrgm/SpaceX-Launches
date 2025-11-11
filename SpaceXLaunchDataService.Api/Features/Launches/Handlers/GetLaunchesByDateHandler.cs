using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Features.Launches.Queries;
using SpaceXLaunchDataService.Api.Features.Launches.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Handlers;

public class GetLaunchesByDateHandler : IRequestHandler<GetLaunchesByDateQuery, OneOf<List<LaunchResponse>, ServiceError>>
{
    private readonly ILaunchRepository _repository;

    public GetLaunchesByDateHandler(ILaunchRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<List<LaunchResponse>, ServiceError>> Handle(GetLaunchesByDateQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetLaunchesByDateAsync(request.Date);
    }
}