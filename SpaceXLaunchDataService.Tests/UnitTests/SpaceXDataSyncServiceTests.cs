using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SpaceXLaunchDataService.Api.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class SpaceXDataSyncServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<SpaceXDataSyncService>> _mockLogger;

    public SpaceXDataSyncServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<SpaceXDataSyncService>>();
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var service = new SpaceXDataSyncService(_mockServiceProvider.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<BackgroundService>();
    }


}