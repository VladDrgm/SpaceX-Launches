using SpaceXLaunchDataService.Api.Data;

namespace SpaceXLaunchDataService.Api.Features.Launches.Services;

public class SpaceXDataSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SpaceXDataSyncService> _logger;

    public SpaceXDataSyncService(IServiceProvider serviceProvider, ILogger<SpaceXDataSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run initial synchronization immediately
        await SynchronizeDataAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait 5 minutes before next synchronization
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await SynchronizeDataAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SpaceX data synchronization");
                // Continue with shorter delay on error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task SynchronizeDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var spaceXService = scope.ServiceProvider.GetRequiredService<ISpaceXApiService>();
        var repository = scope.ServiceProvider.GetRequiredService<ILaunchRepository>();

        _logger.LogInformation("Starting SpaceX data synchronization");

        var launchesResult = await spaceXService.FetchLaunchesAsync();

        await launchesResult.Match(
            async launches =>
            {
                var saveResult = await repository.SaveLaunchesAsync(launches);
                await saveResult.Match(
                    count =>
                    {
                        _logger.LogInformation("Successfully synchronized {Count} launches", count);
                        return Task.CompletedTask;
                    },
                    error =>
                    {
                        _logger.LogError("Failed to save launches: {Error}", error);
                        return Task.CompletedTask;
                    });
            },
            async error =>
            {
                _logger.LogError("Failed to fetch launches from SpaceX API: {Error}", error);
                await Task.CompletedTask;
            });
    }
}
