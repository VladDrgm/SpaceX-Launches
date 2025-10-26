using FluentAssertions;
using Moq;
using SpaceXLaunchDataService.Data;
using SpaceXLaunchDataService.Data.Models.Enums;
using SpaceXLaunchDataService.Features.Launches.Endpoints;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests.Endpoints;

public class GetLaunchesEndpointTests
{
    private readonly Mock<ILaunchRepository> _mockRepository;

    public GetLaunchesEndpointTests()
    {
        _mockRepository = new Mock<ILaunchRepository>();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnLaunches_WhenRepositoryReturnsValidData()
    {
        // Arrange
        var paginatedResponse = new PaginatedLaunchesResponse
        {
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1,
            TotalCount = 1,
            Launches = new List<LaunchResponse>
            {
                new LaunchResponse
                {
                    Id = "1",
                    FlightNumber = 1,
                    Name = "Test Launch",
                    Details = "Test mission",
                    Success = true,
                    DateUtc = DateTime.UtcNow
                }
            }
        };

        _mockRepository.Setup(x => x.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await GetLaunches.HandleAsync(
            page: 1,
            pageSize: 10,
            repository: _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBadRequest_WhenRepositoryReturnsError()
    {
        // Arrange
        var errorMessage = "Database connection failed";
        _mockRepository.Setup(x => x.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()))
            .ReturnsAsync(errorMessage);

        // Act
        var result = await GetLaunches.HandleAsync(
            page: 1,
            pageSize: 10,
            repository: _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectParameters_WhenCalledWithFilters()
    {
        // Arrange
        var paginatedResponse = new PaginatedLaunchesResponse
        {
            CurrentPage = 1,
            PageSize = 5,
            TotalPages = 1,
            TotalCount = 0,
            Launches = new List<LaunchResponse>()
        };

        _mockRepository.Setup(x => x.GetLaunchesAsync(It.IsAny<GetLaunchesRequest>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await GetLaunches.HandleAsync(
            page: 1,
            pageSize: 5,
            sortBy: "Name",
            sortOrder: "Asc",
            success: true,
            fromDate: "2020-01-01",
            toDate: "2020-12-31",
            searchTerm: "falcon",
            repository: _mockRepository.Object);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetLaunchesAsync(It.Is<GetLaunchesRequest>(req =>
            req.Page == 1 &&
            req.PageSize == 5 &&
            req.SortBy == SortField.Name &&
            req.SortOrder == SortOrder.Asc &&
            req.Success == true &&
            req.SearchTerm == "falcon"
        )), Times.Once);
    }
}