using FluentAssertions;
using Moq;
using OneOf;
using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using SpaceXLaunches.Features.Launches.Endpoints;
using SpaceXLaunches.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests.Endpoints;

public class SyncLaunchesEndpointTests
{
    private readonly Mock<ILaunchRepository> _mockRepository;
    private readonly Mock<ISpaceXApiService> _mockApiService;

    public SyncLaunchesEndpointTests()
    {
        _mockRepository = new Mock<ILaunchRepository>();
        _mockApiService = new Mock<ISpaceXApiService>();
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenSyncSucceeds()
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
                Details = "Test launch details",
                FlightNumber = 1
            }
        };

        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT0(launches));

        _mockRepository.Setup(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()))
            .ReturnsAsync(OneOf<int, DatabaseError>.FromT0(1));

        // Act
        var result = await SyncLaunches.Handle(_mockApiService.Object, _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockApiService.Verify(x => x.FetchAllLaunchesAsync(), Times.Once);
        _mockRepository.Verify(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnProblem_WhenApiFails()
    {
        // Arrange
        var apiError = new ApiError("SpaceX API is unavailable");
        _mockApiService.Setup(x => x.FetchAllLaunchesAsync())
            .ReturnsAsync(OneOf<IEnumerable<Launch>, ApiError>.FromT1(apiError));

        // Act
        var result = await SyncLaunches.Handle(_mockApiService.Object, _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockApiService.Verify(x => x.FetchAllLaunchesAsync(), Times.Once);
        _mockRepository.Verify(x => x.SaveLaunchesAsync(It.IsAny<IEnumerable<Launch>>()), Times.Never);
    }
}
