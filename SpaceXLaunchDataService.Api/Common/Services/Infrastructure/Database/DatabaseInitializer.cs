namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Database;


public static class DatabaseInitializer
{
// Initialize database asynchronously and populate with data if empty
    public static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var databaseFactory = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var isNewDatabase = await databaseFactory.InitializeDatabaseAsync();

        if (isNewDatabase)
        {
            logger.LogInformation("🗄️ New database detected - populating with SpaceX launch data...");

            try
            {
                var spaceXService = scope.ServiceProvider.GetRequiredService<SpaceXLaunchDataService.Api.Features.Launches.Services.ISpaceXApiService>();
                var repository = scope.ServiceProvider.GetRequiredService<SpaceXLaunchDataService.Api.Data.ILaunchRepository>();

                var launchesResult = await spaceXService.FetchLaunchesAsync();

                await launchesResult.Match(
                    async launches =>
                    {
                        var saveResult = await repository.SaveLaunchesAsync(launches);
                        await saveResult.Match(
                            count =>
                            {
                                logger.LogInformation("✅ Successfully populated database with {Count} SpaceX launches", count);
                                return Task.CompletedTask;
                            },
                            error =>
                            {
                                logger.LogError("❌ Failed to save launches to database: {Error}", error);
                                return Task.CompletedTask;
                            });
                    },
                    async error =>
                    {
                        logger.LogError("❌ Failed to fetch launches from SpaceX API: {Error}", error);
                        await Task.CompletedTask;
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Unexpected error during database population");
            }
        }
        else
        {
            logger.LogInformation("🗄️ Database already contains data - skipping initial population");
        }
    }
}
