using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Features.Launches.Queries;
using SpaceXLaunchDataService.Api.Features.Launches.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Handlers;

public class GetLaunchByIdHandler : IRequestHandler<GetLaunchByIdQuery, OneOf<LaunchDetailsResponse, ServiceError>>
{
    private readonly ILaunchRepository _repository;

    public GetLaunchByIdHandler(ILaunchRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<LaunchDetailsResponse, ServiceError>> Handle(GetLaunchByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetLaunchByIdAsync(request.Id);
    }
}