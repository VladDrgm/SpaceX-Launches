using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Features.Launches.Commands;

namespace SpaceXLaunchDataService.Api.Features.Launches.Handlers;

public class SaveLaunchesHandler : IRequestHandler<SaveLaunchesCommand, OneOf<int, ServiceError>>
{
    private readonly ILaunchRepository _repository;

    public SaveLaunchesHandler(ILaunchRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<int, ServiceError>> Handle(SaveLaunchesCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SaveLaunchesAsync(request.Launches);
    }
}