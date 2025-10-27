using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Tests.TestData;

namespace SpaceXLaunchDataService.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private bool _dataSeeded = false;
    private readonly object _seedLock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use test environment
        builder.UseEnvironment("Test");
    }

    public async Task SeedTestDataAsync()
    {
        if (_dataSeeded) return;

        lock (_seedLock)
        {
            if (_dataSeeded) return;

            using var scope = Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ILaunchRepository>();
            TestDataSeeder.SeedTestDataAsync(repository).GetAwaiter().GetResult();

            _dataSeeded = true;
        }
    }
}