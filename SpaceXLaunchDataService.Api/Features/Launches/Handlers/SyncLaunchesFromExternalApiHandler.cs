using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Features.Launches.Commands;
using SpaceXLaunchDataService.Api.Features.Launches.Services;

namespace SpaceXLaunchDataService.Api.Features.Launches.Handlers;

public class SyncLaunchesFromExternalApiHandler : IRequestHandler<SyncLaunchesFromExternalApiCommand, OneOf<int, ServiceError>>
{
    private readonly ISpaceXApiService _spaceXApiService;
    private readonly ILaunchRepository _repository;
    private readonly ILogger<SyncLaunchesFromExternalApiHandler> _logger;

    public SyncLaunchesFromExternalApiHandler(
        ISpaceXApiService spaceXApiService,
        ILaunchRepository repository,
        ILogger<SyncLaunchesFromExternalApiHandler> logger)
    {
        _spaceXApiService = spaceXApiService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<OneOf<int, ServiceError>> Handle(SyncLaunchesFromExternalApiCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting synchronization of launches from SpaceX API");

        var launchesResult = await _spaceXApiService.FetchLaunchesAsync();

        return await launchesResult.Match(
            async launches =>
            {
                var saveResult = await _repository.SaveLaunchesAsync(launches);

                return await saveResult.Match<Task<OneOf<int, ServiceError>>>(
                    async count =>
                    {
                        _logger.LogInformation("Successfully synchronized {Count} launches", count);
                        return count;
                    },
                    async error =>
                    {
                        _logger.LogError("Failed to save launches: {Error}", error.Message);
                        return error;
                    });
            },
            async error =>
            {
                _logger.LogError("Failed to fetch launches from SpaceX API: {Error}", error);
                var serviceError = error.Exception != null
                    ? ServiceError.FromException(error.Exception, "Failed to fetch launches from SpaceX API")
                    : ServiceError.Http($"Failed to fetch launches from SpaceX API: {error.Message}");
                return await Task.FromResult<OneOf<int, ServiceError>>(serviceError);
            });
    }
}