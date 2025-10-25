using FluentAssertions;
using SpaceXLaunches.Common.Services.Configuration;
using SpaceXLaunches.Data.Types;
using System.Text.Json;
using Xunit;

namespace SpaceXLaunches.Tests.UnitTests;

public class JsonSerializationTests
{
    [Fact]
    public void LaunchDto_ShouldSerializeToCorrectJson_UsingSystemTextJson()
    {
        // Arrange
        var launchDto = new LaunchDto
        {
            Id = "test-launch-id",
            FlightNumber = 123,
            MissionName = "Test Mission",
            LaunchDateUtc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            WasSuccessful = true,
            Summary = "Test launch summary",
            IsUpcoming = false
        };

        // Act
        var json = JsonSerializer.Serialize(launchDto, JsonConfiguration.ApiOptions);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"missionName\":");
        json.Should().Contain("\"launchDateUtc\":");
        json.Should().Contain("\"wasSuccessful\": true");
        json.Should().Contain("\"Test Mission\"");
        json.Should().NotContain("null"); // Should ignore null values if configured
    }

    [Fact]
    public void LaunchDto_ShouldDeserializeFromJson_UsingSystemTextJson()
    {
        // Arrange
        var json = """
        {
            "id": "deserialize-test-id",
            "flightNumber": 456,
            "missionName": "Deserialization Test",
            "launchDateUtc": "2024-02-20T14:45:00Z",
            "wasSuccessful": false,
            "summary": "Test deserialization",
            "isUpcoming": true
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<LaunchDto>(json, JsonConfiguration.TestOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("deserialize-test-id");
        result.FlightNumber.Should().Be(456);
        result.MissionName.Should().Be("Deserialization Test");
        result.WasSuccessful.Should().BeFalse();
        result.IsUpcoming.Should().BeTrue();
    }

    [Fact]
    public void PaginatedLaunchesDto_ShouldSerializeCorrectly_WithCamelCaseNaming()
    {
        // Arrange
        var paginatedDto = new PaginatedLaunchesDto
        {
            Page = 1,
            Limit = 10,
            TotalPages = 5,
            TotalLaunches = 50,
            HasNextPage = true,
            Launches = new[]
            {
                new LaunchDto
                {
                    Id = "launch-1",
                    FlightNumber = 1,
                    MissionName = "First Launch",
                    LaunchDateUtc = DateTime.UtcNow,
                    WasSuccessful = true,
                    Summary = "First test launch",
                    IsUpcoming = false
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(paginatedDto, JsonConfiguration.ApiOptions);

        // Assert
        json.Should().Contain("\"page\": 1");
        json.Should().Contain("\"totalPages\": 5");
        json.Should().Contain("\"hasNextPage\": true");
        json.Should().Contain("\"launches\":");
        json.Should().Contain("\"First Launch\"");
    }

    [Fact]
    public void Launch_DomainModel_ShouldSerializeWithSnakeCaseForExternalApi()
    {
        // Arrange
        var launch = new Launch
        {
            Id = "external-api-test",
            FlightNumber = 789,
            Name = "External API Test",
            DateUtc = new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc),
            Success = true,
            Details = "Testing external API serialization"
        };

        // Act
        var json = JsonSerializer.Serialize(launch, JsonConfiguration.ExternalApiOptions);

        // Assert - Should use snake_case property names for external API
        json.Should().Contain("\"flight_number\":");
        json.Should().Contain("\"date_utc\":");
        json.Should().Contain("\"success\":");
        json.Should().Contain("\"details\":");
    }

    [Fact]
    public void JsonConfiguration_ApiOptions_ShouldHandleNullValues()
    {
        // Arrange
        var launchDto = new LaunchDto
        {
            Id = "null-test",
            FlightNumber = 100,
            MissionName = "Null Test",
            LaunchDateUtc = DateTime.UtcNow,
            WasSuccessful = null, // This should be ignored in serialization
            Summary = "Testing null handling",
            IsUpcoming = false
        };

        // Act
        var json = JsonSerializer.Serialize(launchDto, JsonConfiguration.ApiOptions);

        // Assert - wasSuccessful should not appear in JSON when null due to DefaultIgnoreCondition
        json.Should().NotContain("\"wasSuccessful\": null");
    }

    [Fact]
    public void JsonConfiguration_TestOptions_ShouldBeCompact()
    {
        // Arrange
        var simple = new { test = "value", number = 42 };

        // Act
        var compactJson = JsonSerializer.Serialize(simple, JsonConfiguration.TestOptions);
        var prettyJson = JsonSerializer.Serialize(simple, JsonConfiguration.ApiOptions);

        // Assert
        compactJson.Should().NotContain("\n"); // No line breaks
        compactJson.Should().NotContain("  ");  // No indentation
        prettyJson.Should().Contain("\n");      // Should have line breaks
    }
}
