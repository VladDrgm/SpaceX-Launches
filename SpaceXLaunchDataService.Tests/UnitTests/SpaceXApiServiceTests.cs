using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using SpaceXLaunches.Features.Launches.Services;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests;

public class SpaceXApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<SpaceXApiService>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly SpaceXApiService _service;

    public SpaceXApiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<SpaceXApiService>>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.spacexdata.com/v5/")
        };
        _service = new SpaceXApiService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task FetchAllLaunchesAsync_ShouldReturnLaunches_WhenApiReturnsValidData()
    {
        // Arrange
        var launches = new List<Launch>
        {
            new() { Id = "1", Name = "Test Launch", Success = true, DateUtc = DateTime.UtcNow, Details = "Test details", FlightNumber = 1 },
            new() { Id = "2", Name = "Another Launch", Success = false, DateUtc = DateTime.UtcNow.AddDays(-1), Details = "Another test", FlightNumber = 2 }
        };
        var json = JsonSerializer.Serialize(launches);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchAllLaunchesAsync();

        // Assert
        result.IsT0.Should().BeTrue();
        var returnedLaunches = result.AsT0;
        returnedLaunches.Should().HaveCount(2);
        returnedLaunches.First().Name.Should().Be("Test Launch");
    }

    [Fact]
    public async Task FetchAllLaunchesAsync_ShouldReturnApiError_WhenHttpRequestFails()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.FetchAllLaunchesAsync();

        // Assert
        result.IsT1.Should().BeTrue();
        var apiError = result.AsT1;
        apiError.Message.Should().Contain("Network error");
    }

    [Fact]
    public async Task FetchAllLaunchesAsync_ShouldReturnApiError_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchAllLaunchesAsync();

        // Assert
        result.IsT1.Should().BeTrue();
        var apiError = result.AsT1;
        apiError.Message.Should().Contain("InternalServerError");
    }

    [Fact]
    public async Task FetchAllLaunchesAsync_ShouldReturnApiError_WhenResponseContainsInvalidJson()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchAllLaunchesAsync();

        // Assert
        result.IsT1.Should().BeTrue();
        var apiError = result.AsT1;
        apiError.Message.Should().Contain("Failed to parse API response");
    }
}
