using FluentAssertions;
using SpaceXLaunchDataService.Tests.Infrastructure;
using System.Net;
using System.Text.Json;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;
using Xunit;

namespace SpaceXLaunchDataService.Tests.IntegrationTests;

public class LaunchesApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LaunchesApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Seed test data
        _factory.SeedTestDataAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetLaunches_ShouldReturnOk_WithPaginatedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var result = JsonSerializer.Deserialize<PaginatedLaunchesResponse>(content, options);

        result.Should().NotBeNull();
        result!.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.Launches.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLaunches_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLaunchDetails_ShouldReturnOk_WithValidId()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var result = JsonSerializer.Deserialize<LaunchDetailsResponse>(content, options);

        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
        result.Name.Should().NotBeNullOrEmpty();
        result.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetLaunchDetails_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SyncLaunches_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/launches/sync", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
