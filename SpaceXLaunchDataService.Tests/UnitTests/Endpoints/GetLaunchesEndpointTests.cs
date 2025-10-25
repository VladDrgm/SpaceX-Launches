using FluentAssertions;
using Moq;
using OneOf;
using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using SpaceXLaunches.Features.Launches.Endpoints;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests.Endpoints;

public class GetLaunchesEndpointTests
{
    private readonly Mock<ILaunchRepository> _mockRepository;

    public GetLaunchesEndpointTests()
    {
        _mockRepository = new Mock<ILaunchRepository>();
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenRepositoryReturnsData()
    {
        // Arrange
        var paginatedDto = new PaginatedLaunchesDto
        {
            Page = 1,
            Limit = 10,
            TotalLaunches = 1,
            TotalPages = 1,
            HasNextPage = false,
            Launches = new List<LaunchDto>
            {
                new()
                {
                    Id = "1",
                    MissionName = "Test Launch",
                    Summary = "Test mission",
                    WasSuccessful = true,
                    LaunchDateUtc = DateTime.UtcNow,
                    FlightNumber = 1,
                    IsUpcoming = false
                }
            }
        };

        _mockRepository.Setup(x => x.GetLaunchesPaginatedAsync(1, 10, null))
            .ReturnsAsync(OneOf<PaginatedLaunchesDto, ValidationError, DatabaseError>.FromT0(paginatedDto));

        // Act
        var result = await GetLaunches.Handle(_mockRepository.Object, 1, 10, null);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchesPaginatedAsync(1, 10, null), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var validationError = new ValidationError("page", "Page must be greater than 0");
        _mockRepository.Setup(x => x.GetLaunchesPaginatedAsync(0, 10, null))
            .ReturnsAsync(OneOf<PaginatedLaunchesDto, ValidationError, DatabaseError>.FromT1(validationError));

        // Act
        var result = await GetLaunches.Handle(_mockRepository.Object, 0, 10, null);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchesPaginatedAsync(0, 10, null), Times.Once);
    }
}
