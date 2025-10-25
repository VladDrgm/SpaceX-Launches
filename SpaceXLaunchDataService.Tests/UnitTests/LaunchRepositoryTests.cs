using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OneOf;
using SpaceXLaunches.Common.Services.Configuration;
using SpaceXLaunches.Data;
using SpaceXLaunches.Data.Types;
using static SpaceXLaunches.Data.Types.Errors;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests;

public class LaunchRepositoryTests : IDisposable
{
    private readonly LaunchRepository _repository;
    private readonly string _testDbPath;

    public LaunchRepositoryTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_spacex_{Guid.NewGuid():N}.db");

        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Data Source={_testDbPath}"
        };

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(configData);

        var config = new AppConfiguration(configBuilder.Build());
        _repository = new LaunchRepository(config);
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
            File.Delete(_testDbPath);
    }

    [Fact]
    public async Task GetLaunchByIdAsync_ShouldReturnNotFound_WhenLaunchDoesNotExist()
    {
        // Arrange
        await _repository.InitializeDbAsync(); // Ensure DB is initialized
        var nonExistentId = "nonexistent-launch-id";

        // Act
        var result = await _repository.GetLaunchByIdAsync(nonExistentId);

        // Assert
        result.IsT1.Should().BeTrue(); // Should be NotFoundError
        var notFoundError = result.AsT1;
        notFoundError.Message.Should().Contain("Launch");
        notFoundError.Message.Should().Contain(nonExistentId);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(1, 1001)] // Limit too high
    public async Task GetLaunchesPaginatedAsync_ShouldReturnValidationError_WhenParametersInvalid(int page, int limit)
    {
        // Arrange
        await _repository.InitializeDbAsync(); // Ensure DB is initialized

        // Act
        var result = await _repository.GetLaunchesPaginatedAsync(page, limit, null);

        // Assert
        result.IsT1.Should().BeTrue(); // Should be ValidationError

        var validationError = result.AsT1;
        if (page <= 0)
        {
            validationError.Message.Should().Contain("page is out of range. Expected: must be greater than 0");
        }
        else if (limit > 1000)
        {
            validationError.Message.Should().Contain("limit is out of range. Expected: must be between 1 and 1000");
        }
    }

    [Fact]
    public async Task InitializeDbAsync_ShouldReturnSuccess_WhenCalledOnNewDatabase()
    {
        // Act
        var result = await _repository.InitializeDbAsync();

        // Assert
        result.IsT0.Should().BeTrue(); // Should be Success
    }
}
