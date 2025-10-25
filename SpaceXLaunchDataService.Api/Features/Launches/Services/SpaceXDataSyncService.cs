using SpaceXLaunches.Data;

namespace SpaceXLaunches.Features.Launches.Services;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var spaceXService = scope.ServiceProvider.GetRequiredService<ISpaceXApiService>();
                var repository = scope.ServiceProvider.GetRequiredService<ILaunchRepository>();

                _logger.LogInformation("Starting SpaceX data synchronization");
                
                // Stub implementation
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SpaceX data synchronization");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
