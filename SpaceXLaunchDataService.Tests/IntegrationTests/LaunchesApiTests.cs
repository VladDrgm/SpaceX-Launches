using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SpaceXLaunches.Data.Types;
using SpaceXLaunches.Common.Services.Configuration;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SpaceXLaunches.Tests.IntegrationTests;

public class LaunchesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LaunchesApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetLaunches_ShouldReturnOk_WithPaginatedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches?page=1&limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedLaunchesDto>(content, JsonConfiguration.TestOptions);

        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.Limit.Should().Be(5);
    }

    [Fact]
    public async Task GetLaunches_ShouldReturnBadRequest_WhenPageIsInvalid()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches?page=0&limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLaunchById_ShouldReturnNotFound_WhenLaunchDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/launches/nonexistent-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
