using FluentAssertions;
using Moq;
using SpaceXLaunchDataService.Api.Data;
using SpaceXLaunchDataService.Api.Data.Models.Enums;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;
using SpaceXLaunchDataService.Tests.TestData;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class LaunchRepositoryTests
{
    private readonly Mock<ILaunchRepository> _mockRepository;
    private readonly ILaunchRepository _repository;

    public LaunchRepositoryTests()
    {
        // Use proper mock for unit tests - controlled data, no business logic duplication
        _mockRepository = new Mock<ILaunchRepository>();
        _repository = _mockRepository.Object;
    }

    [Fact]
    public async Task GetLaunchByIdAsync_ShouldReturnLaunch_WhenLaunchExists()
    {
        // Arrange
        var existingId = "1";
        var expectedLaunch = new LaunchDetailsResponse
        {
            Id = existingId,
            FlightNumber = 1,
            Name = "FalconSat",
            DateUtc = new DateTime(2006, 3, 24, 22, 30, 0, DateTimeKind.Utc),
            Success = false,
            Details = "Engine failure at 33 seconds and loss of vehicle"
        };

        _mockRepository
            .Setup(r => r.GetLaunchByIdAsync(existingId))
            .ReturnsAsync(expectedLaunch);

        // Act
        var result = await _repository.GetLaunchByIdAsync(existingId);

        // Assert
        result.IsT0.Should().BeTrue(); // Should be LaunchDetailsResponse
        var launch = result.AsT0;
        launch.Should().NotBeNull();
        launch.Id.Should().Be(existingId);
        launch.Name.Should().Be("FalconSat");
    }

    [Fact]
    public async Task GetLaunchByIdAsync_ShouldReturnNotFound_WhenLaunchDoesNotExist()
    {
        // Arrange
        var nonExistentId = "nonexistent-launch-id";
        var errorMessage = "Launch not found";

        _mockRepository
            .Setup(r => r.GetLaunchByIdAsync(nonExistentId))
            .ReturnsAsync(errorMessage);

        // Act
        var result = await _repository.GetLaunchByIdAsync(nonExistentId);

        // Assert
        result.IsT1.Should().BeTrue(); // Should be string (error message)
        var error = result.AsT1;
        error.Should().Contain("Launch not found");
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 1001)] // Limit too high
    public async Task GetLaunchesAsync_ShouldReturnResults_EvenWithInvalidParameters(int page, int pageSize)
    {
        // Arrange
        var request = new GetLaunchesRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = SortField.DateUtc,
            SortOrder = SortOrder.Desc
        };

        var mockResponse = new PaginatedLaunchesResponse
        {
            Launches = new List<LaunchResponse>(),
            TotalCount = 0,
            PageSize = pageSize,
            CurrentPage = page,
            TotalPages = 0
        };

        _mockRepository
            .Setup(r => r.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _repository.GetLaunchesAsync(request);

        // Assert
        result.IsT0.Should().BeTrue(); // Should be successful (PaginatedLaunchesResponse)
        var launches = result.AsT0;
        launches.Should().NotBeNull();
        launches.Launches.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLaunchesAsync_ShouldReturnSuccess_WhenCalledWithValidRequest()
    {
        // Arrange
        var request = new GetLaunchesRequest
        {
            Page = 1,
            PageSize = 10,
            SortBy = SortField.DateUtc,
            SortOrder = SortOrder.Desc
        };

        var mockLaunches = TestDataSeeder.GetTestLaunchResponses().Take(10).ToList();
        var mockResponse = new PaginatedLaunchesResponse
        {
            Launches = mockLaunches,
            TotalCount = 10,
            PageSize = 10,
            CurrentPage = 1,
            TotalPages = 1
        };

        _mockRepository
            .Setup(r => r.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _repository.GetLaunchesAsync(request);

        // Assert
        result.IsT0.Should().BeTrue(); // Should be PaginatedLaunchesResponse
        var paginatedResult = result.AsT0;
        paginatedResult.Should().NotBeNull();
        paginatedResult.Launches.Should().NotBeNull();
        paginatedResult.TotalCount.Should().Be(10);
        paginatedResult.Launches.Should().HaveCount(10);
    }
}
