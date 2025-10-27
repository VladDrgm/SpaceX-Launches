using FluentAssertions;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class SpaceXApiServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly SpaceXApiService _service;

    public SpaceXApiServiceTests()
    {
        _httpClient = new HttpClient();
        _service = new SpaceXApiService(_httpClient);
    }

    [Fact]
    public async Task FetchLaunchesAsync_ShouldReturnEmptyList_WithStubImplementation()
    {
        // Act - Testing the current stub implementation
        var result = await _service.FetchLaunchesAsync();

        // Assert
        result.IsT0.Should().BeTrue();
        var returnedLaunches = result.AsT0;
        returnedLaunches.Should().NotBeNull();
        returnedLaunches.Should().BeOfType<List<Launch>>();
        returnedLaunches.Should().BeEmpty(); // Stub returns empty list
    }

    [Fact]
    public async Task FetchLaunchesAsync_ShouldReturnSuccessfully_WhenServiceIsCalledMultipleTimes()
    {
        // Act - Call the service multiple times
        var result1 = await _service.FetchLaunchesAsync();
        var result2 = await _service.FetchLaunchesAsync();

        // Assert - Both calls should succeed with empty lists
        result1.IsT0.Should().BeTrue();
        result2.IsT0.Should().BeTrue();

        var launches1 = result1.AsT0;
        var launches2 = result2.AsT0;

        launches1.Should().BeEmpty();
        launches2.Should().BeEmpty();
    }

    [Fact]
    public void SpaceXApiService_ShouldCreateInstance_WithValidHttpClient()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();
        var service = new SpaceXApiService(httpClient);

        // Assert
        service.Should().NotBeNull();
    }
}