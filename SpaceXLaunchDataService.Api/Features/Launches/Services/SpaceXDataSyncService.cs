using Polly;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;
using SpaceXLaunchDataService.Api.Features.Launches.Commands;

namespace SpaceXLaunchDataService.Api.Features.Launches.Services;

public class SpaceXDataSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SpaceXDataSyncService> _logger;
    private readonly ResiliencePipeline _syncResiliencePipeline;

    public SpaceXDataSyncService(IServiceProvider serviceProvider, ILogger<SpaceXDataSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Initialize resilience pipeline for sync operations
        _syncResiliencePipeline = ResiliencePolicies.CreateSyncResiliencePipeline(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run initial synchronization immediately
        await SynchronizeDataWithResilienceAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait 5 minutes before next synchronization
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await SynchronizeDataWithResilienceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during SpaceX data synchronization cycle");
                // Continue with shorter delay on error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task SynchronizeDataWithResilienceAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Execute synchronization with resilience (timeout + retry for entire operation)
            await _syncResiliencePipeline.ExecuteAsync(async ct =>
            {
                await SynchronizeDataAsync(ct);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronization failed after all retry attempts");
        }
    }

    private async Task SynchronizeDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedMediator = new ScopedMediator(scope.ServiceProvider);

        _logger.LogInformation("Starting SpaceX data synchronization");

        var command = new SyncLaunchesFromExternalApiCommand();
        var result = await scopedMediator.Send(command);

        await result.Match(
            count =>
            {
                _logger.LogInformation("Successfully synchronized {Count} launches", count);
                return Task.CompletedTask;
            },
            error =>
            {
                _logger.LogError(error.Exception, "Failed to synchronize launches: [{Code}] {Message}", error.Code, error.Message);
                throw new InvalidOperationException($"Failed to synchronize launches: {error.Message}", error.Exception);
            });
    }
}
