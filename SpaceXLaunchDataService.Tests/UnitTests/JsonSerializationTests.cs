using FluentAssertions;
using SpaceXLaunchDataService.Data.Models;
using SpaceXLaunchDataService.Features.Launches.Endpoints;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SpaceXLaunchDataService.Tests.UnitTests;

public class JsonSerializationTests
{
    [Fact]
    public void LaunchResponse_ShouldSerializeToCorrectJson_UsingSystemTextJson()
    {
        // Arrange
        var launchResponse = new LaunchResponse
        {
            Id = "test-launch-id",
            FlightNumber = 123,
            Name = "Test Mission",
            DateUtc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            Success = true,
            Details = "Test launch summary"
        };

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(launchResponse, options);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"dateUtc\":");
        json.Should().Contain("\"success\": true");
        json.Should().Contain("\"Test Mission\"");
    }

    [Fact]
    public void LaunchResponse_ShouldDeserializeFromJson_UsingSystemTextJson()
    {
        // Arrange
        var json = """
        {
            "id": "deserialize-test-id",
            "flightNumber": 456,
            "name": "Deserialization Test",
            "dateUtc": "2024-02-20T14:45:00Z",
            "success": false,
            "details": "Test deserialization"
        }
        """;

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var result = JsonSerializer.Deserialize<LaunchResponse>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("deserialize-test-id");
        result.FlightNumber.Should().Be(456);
        result.Name.Should().Be("Deserialization Test");
        result.Success.Should().BeFalse();
        result.Details.Should().Be("Test deserialization");
    }

    [Fact]
    public void PaginatedLaunchesResponse_ShouldSerializeCorrectly_WithCamelCaseNaming()
    {
        // Arrange
        var paginatedResponse = new PaginatedLaunchesResponse
        {
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 5,
            TotalCount = 50,
            Launches = new List<LaunchResponse>
            {
                new LaunchResponse
                {
                    Id = "launch-1",
                    FlightNumber = 1,
                    Name = "First Launch",
                    DateUtc = DateTime.UtcNow,
                    Success = true,
                    Details = "First test launch"
                }
            }
        };

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(paginatedResponse, options);

        // Assert
        json.Should().Contain("\"currentPage\": 1");
        json.Should().Contain("\"totalPages\": 5");
        json.Should().Contain("\"totalCount\": 50");
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
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var json = JsonSerializer.Serialize(launch, options);

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
        var launchResponse = new LaunchResponse
        {
            Id = "null-test",
            FlightNumber = 100,
            Name = "Null Test",
            DateUtc = DateTime.UtcNow,
            Success = null, // This should be ignored in serialization
            Details = "Testing null handling"
        };

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(launchResponse, options);

        // Assert - success should not appear in JSON when null due to DefaultIgnoreCondition
        json.Should().NotContain("\"success\": null");
    }

    [Fact]
    public void JsonConfiguration_TestOptions_ShouldBeCompact()
    {
        // Arrange
        var simple = new { test = "value", number = 42 };

        // Act
        var compactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        var prettyOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var compactJson = JsonSerializer.Serialize(simple, compactOptions);
        var prettyJson = JsonSerializer.Serialize(simple, prettyOptions);

        // Assert
        compactJson.Should().NotContain("\n"); // No line breaks
        compactJson.Should().NotContain("  ");  // No indentation
        prettyJson.Should().Contain("\n");      // Should have line breaks
    }
}
