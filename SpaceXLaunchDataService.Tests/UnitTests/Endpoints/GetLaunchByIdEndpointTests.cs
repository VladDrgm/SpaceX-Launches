using FluentAssertions;
using Moq;
using OneOf;
using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using SpaceXLaunches.Features.Launches.Endpoints;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests.Endpoints;

public class GetLaunchByIdEndpointTests
{
    private readonly Mock<ILaunchRepository> _mockRepository;

    public GetLaunchByIdEndpointTests()
    {
        _mockRepository = new Mock<ILaunchRepository>();
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenLaunchExists()
    {
        // Arrange
        var launchDto = new LaunchDto
        {
            Id = "test-id",
            MissionName = "Test Launch",
            Summary = "Test mission summary",
            WasSuccessful = true,
            LaunchDateUtc = DateTime.UtcNow,
            FlightNumber = 1,
            IsUpcoming = false
        };

        _mockRepository.Setup(x => x.GetLaunchByIdAsync("test-id"))
            .ReturnsAsync(OneOf<LaunchDto, NotFoundError, ValidationError, DatabaseError>.FromT0(launchDto));

        // Act
        var result = await GetLaunchById.Handle("test-id", _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchByIdAsync("test-id"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenLaunchDoesNotExist()
    {
        // Arrange
        var notFoundError = new NotFoundError("Launch with ID 'nonexistent' not found");
        _mockRepository.Setup(x => x.GetLaunchByIdAsync("nonexistent"))
            .ReturnsAsync(OneOf<LaunchDto, NotFoundError, ValidationError, DatabaseError>.FromT1(notFoundError));

        // Act
        var result = await GetLaunchById.Handle("nonexistent", _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchByIdAsync("nonexistent"), Times.Once);
    }
}
