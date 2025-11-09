using Microsoft.Extensions.Logging;
using Moq;
using SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;
using Xunit;

namespace SpaceXLaunchDataService.Tests.IntegrationTests;

/// <summary>
/// Integration tests to verify resilience policies are working correctly
/// </summary>
public class ResilienceIntegrationTests
{
    [Fact]
    public void HttpResiliencePipeline_ShouldBeCreated_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateHttpResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void DatabaseResiliencePipeline_ShouldBeCreated_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateDatabaseResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void SyncResiliencePipeline_ShouldBeCreated_Successfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var pipeline = ResiliencePolicies.CreateSyncResiliencePipeline(mockLogger.Object);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task HttpResiliencePipeline_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var pipeline = ResiliencePolicies.CreateHttpResiliencePipeline(mockLogger.Object);
        var expectedResult = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        // Act
        var result = await pipeline.ExecuteAsync(async ct =>
        {
            await Task.Delay(10, ct);
            return expectedResult;
        });

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DatabaseResiliencePipeline_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var pipeline = ResiliencePolicies.CreateDatabaseResiliencePipeline(mockLogger.Object);
        var expectedResult = 42;

        // Act
        var result = await pipeline.ExecuteAsync(async ct =>
        {
            await Task.Delay(10, ct);
            return expectedResult;
        });

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task SyncResiliencePipeline_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var pipeline = ResiliencePolicies.CreateSyncResiliencePipeline(mockLogger.Object);

        // Act & Assert
        await pipeline.ExecuteAsync(async ct =>
        {
            await Task.Delay(10, ct);
        });
    }
}

