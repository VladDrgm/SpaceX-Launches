using FluentAssertions;
using SpaceXLaunchDataService.Data;
using SpaceXLaunchDataService.Data.Models.Enums;
using SpaceXLaunchDataService.Features.Launches.Endpoints;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class LaunchRepositoryTests
{
    private readonly LaunchRepository _repository;

    public LaunchRepositoryTests()
    {
        _repository = new LaunchRepository();
    }

    [Fact]
    public async Task GetLaunchByIdAsync_ShouldReturnLaunch_WhenLaunchExists()
    {
        // Arrange
        var existingId = "1"; // This exists in the in-memory data

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

        // Act
        var result = await _repository.GetLaunchByIdAsync(nonExistentId);

        // Assert
        result.IsT1.Should().BeTrue(); // Should be string (error message)
        var errorMessage = result.AsT1;
        errorMessage.Should().Contain("Launch not found");
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 1001)] // Limit too high
    public async Task GetLaunchesAsync_ShouldReturnResults_EvenWithInvalidParameters(int page, int pageSize)
    {
        // Arrange - Repository doesn't validate, it just processes the request
        var request = new GetLaunchesRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = SortField.DateUtc,
            SortOrder = SortOrder.Desc
        };

        // Act
        var result = await _repository.GetLaunchesAsync(request);

        // Assert - Repository returns results regardless of parameters
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

        // Act
        var result = await _repository.GetLaunchesAsync(request);

        // Assert
        result.IsT0.Should().BeTrue(); // Should be PaginatedLaunchesResponse
        var paginatedResult = result.AsT0;
        paginatedResult.Should().NotBeNull();
        paginatedResult.Launches.Should().NotBeEmpty();
    }
}
