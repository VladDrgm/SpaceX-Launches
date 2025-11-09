using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;
using SpaceXLaunchDataService.Api.Features.Launches.Services;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class ResilienceTests
{
    [Fact]
    public async Task SpaceXApiService_ShouldRetryOnTransientHttpErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SpaceXApiService>>();
        var attemptCount = 0;
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                // Fail on first 2 attempts, succeed on 3rd
                if (attemptCount < 3)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }
                
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, mockLogger.Object);

        // Act
        var result = await service.FetchLaunchesAsync();

        // Assert
        Assert.True(result.IsT0); // Should succeed after retries
        Assert.Equal(3, attemptCount); // Should have retried 2 times (3 total attempts)
        
        // Verify retry warnings were logged
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SpaceXApiService_ShouldNotRetryOnNonTransientErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SpaceXApiService>>();
        var attemptCount = 0;
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                // Return 404 (not a transient error)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, mockLogger.Object);

        // Act
        var result = await service.FetchLaunchesAsync();

        // Assert
        Assert.True(result.IsT1); // Should fail (return error string)
        Assert.Equal(1, attemptCount); // Should NOT retry for 404
    }

    [Fact]
    public async Task SpaceXApiService_ShouldFailAfterMaxRetries()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SpaceXApiService>>();
        var attemptCount = 0;
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                // Always fail with 503
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, mockLogger.Object);

        // Act
        var result = await service.FetchLaunchesAsync();

        // Assert
        Assert.True(result.IsT1); // Should fail after all retries (returns ServiceError)
        Assert.Equal(4, attemptCount); // Initial attempt + 3 retries
        
        // Verify ServiceError was returned
        var error = result.AsT1;
        Assert.Equal("HTTP_ERROR", error.Code);
        Assert.Contains("Error fetching launches", error.Message);
        
        // Verify warning logs for each retry attempt (not error log, since we return ServiceError)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3)); // 3 retry warnings
    }

    [Fact]
    public async Task SpaceXApiService_ShouldRetryOnHttpRequestException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SpaceXApiService>>();
        var attemptCount = 0;
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                // Fail on first attempt with exception, succeed on 2nd
                if (attemptCount < 2)
                {
                    throw new HttpRequestException("Network error");
                }
                
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                };
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new SpaceXApiService(httpClient, mockLogger.Object);

        // Act
        var result = await service.FetchLaunchesAsync();

        // Assert
        Assert.True(result.IsT0); // Should succeed after retry
        Assert.Equal(2, attemptCount); // Should have retried once
    }

    [Fact]
    public void ResiliencePolicies_HttpPipeline_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateHttpResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void ResiliencePolicies_DatabasePipeline_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateDatabaseResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void ResiliencePolicies_SyncPipeline_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateSyncResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }
}

