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

public class SpaceXDataSyncServiceTests
{
    private readonly Mock<ISpaceXApiService> _mockApiService;
    private readonly Mock<ILaunchRepository> _mockRepository;
    private readonly Mock<ILogger<SpaceXDataSyncService>> _mockLogger;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;

    public SpaceXDataSyncServiceTests()
    {
        _mockApiService = new Mock<ISpaceXApiService>();
        _mockRepository = new Mock<ILaunchRepository>();
        _mockLogger = new Mock<ILogger<SpaceXDataSyncService>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

        // Setup service provider chain
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);

        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockServiceScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ISpaceXApiService)))
            .Returns(_mockApiService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ILaunchRepository)))
            .Returns(_mockRepository.Object);
    }

    [Fact]
    public async Task DoWork_ShouldSaveLaunches_WhenApiFetchSucceeds()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new() { Id = "1", Name = "Test Launch", Success = true, DateUtc = DateTime.UtcNow, Details = "Test details", FlightNumber = 1 }
        };

        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT0(launches));

        _mockRepository.Setup(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()))
            .ReturnsAsync(OneOf<int, DatabaseError>.FromT0(1));

        var service = new SpaceXDataSyncService(_mockServiceProvider.Object, _mockLogger.Object);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background service to run
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockApiService.Verify(x => x.FetchAllLaunchesAsync(), Times.AtLeastOnce);
        _mockRepository.Verify(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DoWork_ShouldLogError_WhenApiFetchFails()
    {
        // Arrange
        var apiError = new ApiError("API is down");
        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT1(apiError));

        var service = new SpaceXDataSyncService(_mockServiceProvider.Object, _mockLogger.Object);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background service to run
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockApiService.Verify(x => x.FetchAllLaunchesAsync(), Times.AtLeastOnce);
        _mockRepository.Verify(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()), Times.Never);

        // Verify error logging
        VerifyLogContains(LogLevel.Error, "Failed to fetch launches from SpaceX API");
    }

    [Fact]
    public async Task DoWork_ShouldLogError_WhenSaveFails()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new() { Id = "1", Name = "Test Launch", Success = true, DateUtc = DateTime.UtcNow, Details = "Test details", FlightNumber = 1 }
        };
        var dbError = new DatabaseError("Database connection failed");

        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT0(launches));

        _mockRepository.Setup(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()))
            .ReturnsAsync(OneOf<int, DatabaseError>.FromT1(dbError));

        var service = new SpaceXDataSyncService(_mockServiceProvider.Object, _mockLogger.Object);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background service to run
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockApiService.Verify(x => x.FetchAllLaunchesAsync(), Times.AtLeastOnce);
        _mockRepository.Verify(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()), Times.AtLeastOnce);

        // Verify error logging
        VerifyLogContains(LogLevel.Error, "Failed to save launches to database");
    }

    private void VerifyLogContains(LogLevel level, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
