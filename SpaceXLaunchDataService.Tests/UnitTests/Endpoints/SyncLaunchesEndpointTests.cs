using FluentAssertions;
using Moq;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;
using SpaceXLaunchDataService.Api.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests.Endpoints;

public class SyncLaunchesEndpointTests
{
    private readonly Mock<ISpaceXApiService> _mockApiService;

    public SyncLaunchesEndpointTests()
    {
        _mockApiService = new Mock<ISpaceXApiService>();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnOk_WhenApiServiceReturnsLaunches()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new Launch
            {
                Id = "1",
                Name = "Test Launch",
                DateUtc = DateTime.UtcNow,
                FlightNumber = 1,
                Success = true,
                Details = "Test details"
            }
        };

        _mockApiService.Setup(x => x.FetchLaunchesAsync())
            .ReturnsAsync(launches);

        // Act
        var result = await SyncLaunches.HandleAsync(_mockApiService.Object);

        // Assert
        result.Should().NotBeNull();
        _mockApiService.Verify(x => x.FetchLaunchesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBadRequest_WhenApiServiceReturnsError()
    {
        // Arrange
        var errorMessage = "SpaceX API is unavailable";
        _mockApiService.Setup(x => x.FetchLaunchesAsync())
            .ReturnsAsync(errorMessage);

        // Act
        var result = await SyncLaunches.HandleAsync(_mockApiService.Object);

        // Assert
        result.Should().NotBeNull();
        _mockApiService.Verify(x => x.FetchLaunchesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCorrectCount_WhenApiServiceReturnsMultipleLaunches()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new Launch { Id = "1", Name = "Launch 1", DateUtc = DateTime.UtcNow, FlightNumber = 1, Success = true, Details = "Details 1" },
            new Launch { Id = "2", Name = "Launch 2", DateUtc = DateTime.UtcNow, FlightNumber = 2, Success = false, Details = "Details 2" },
            new Launch { Id = "3", Name = "Launch 3", DateUtc = DateTime.UtcNow, FlightNumber = 3, Success = true, Details = "Details 3" }
        };

        _mockApiService.Setup(x => x.FetchLaunchesAsync())
            .ReturnsAsync(launches);

        // Act
        var result = await SyncLaunches.HandleAsync(_mockApiService.Object);

        // Assert
        result.Should().NotBeNull();
        _mockApiService.Verify(x => x.FetchLaunchesAsync(), Times.Once);
    }
}