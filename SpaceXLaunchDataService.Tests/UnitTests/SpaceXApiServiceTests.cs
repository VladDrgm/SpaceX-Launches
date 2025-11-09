using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SpaceXLaunchDataService.Api.Data.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class SpaceXApiServiceTests
{
    [Fact]
    public async Task FetchLaunchesAsync_ShouldReturnEmptyList_WhenApiReturnsEmptyArray()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, new Mock<ILogger<SpaceXApiService>>().Object);

        // Act
        var result = await service.FetchLaunchesAsync();

        // Assert
        result.IsT0.Should().BeTrue();
        var returnedLaunches = result.AsT0;
        returnedLaunches.Should().NotBeNull();
        returnedLaunches.Should().BeOfType<List<Launch>>();
        returnedLaunches.Should().BeEmpty();
    }

    [Fact]
    public async Task FetchLaunchesAsync_ShouldReturnSuccessfully_WhenServiceIsCalledMultipleTimes()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, new Mock<ILogger<SpaceXApiService>>().Object);

        // Act - Call the service multiple times
        var result1 = await service.FetchLaunchesAsync();
        var result2 = await service.FetchLaunchesAsync();

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
        var service = new SpaceXApiService(httpClient, new Mock<ILogger<SpaceXApiService>>().Object);

        // Assert
        service.Should().NotBeNull();
    }
}