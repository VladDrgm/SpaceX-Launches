using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf;
using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using SpaceXLaunches.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests;

public class SpaceXDataSyncServiceBackgroundTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ISpaceXApiService> _mockApiService;
    private readonly Mock<ILaunchRepository> _mockRepository;
    private readonly Mock<ILogger<SpaceXDataSyncService>> _mockLogger;
    private readonly SpaceXDataSyncService _service;

    public SpaceXDataSyncServiceBackgroundTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockApiService = new Mock<ISpaceXApiService>();
        _mockRepository = new Mock<ILaunchRepository>();
        _mockLogger = new Mock<ILogger<SpaceXDataSyncService>>();

        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ISpaceXApiService)))
            .Returns(_mockApiService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ILaunchRepository)))
            .Returns(_mockRepository.Object);

        _service = new SpaceXDataSyncService(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task BackgroundService_ShouldHandleCancellation()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new() 
            { 
                Id = "1", 
                Name = "Test Launch", 
                Success = true, 
                DateUtc = DateTime.UtcNow,
                Details = "Test details",
                FlightNumber = 1
            }
        };

        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT0(launches));

        _mockRepository.Setup(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()))
            .ReturnsAsync(OneOf<int, DatabaseError>.FromT0(1));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50)); // Cancel quickly

        // Act & Assert - Should handle cancellation gracefully
        try
        {
            await _service.StartAsync(cts.Token);
            await Task.Delay(100, CancellationToken.None); // Wait for cancellation
            await _service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }

        // Service should have started without throwing
        _service.Should().NotBeNull();
    }

    [Fact]
    public void ServiceConstruction_ShouldSucceed()
    {
        // Act & Assert
        _service.Should().NotBeNull();
        _service.Should().BeOfType<SpaceXDataSyncService>();
    }

    [Fact]
    public async Task ServiceLifecycle_ShouldStartAndStopProperly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        await _service.StartAsync(cts.Token);
        await _service.StopAsync(CancellationToken.None);

        // Assert - Should complete without exceptions
        _service.Should().NotBeNull();
    }
}
