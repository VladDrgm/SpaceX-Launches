using SpaceXLaunchDataService.Data.Models;
using SpaceXLaunchDataService.Data;

namespace SpaceXLaunchDataService.Tests.TestData;

public static class TestDataSeeder
{
    public static async Task SeedTestDataAsync(ILaunchRepository repository)
    {
        var testLaunches = new List<Launch>
        {
            new Launch
            {
                Id = "1",
                FlightNumber = 1,
                Name = "FalconSat",
                DateUtc = new DateTime(2006, 3, 24, 22, 30, 0, DateTimeKind.Utc),
                Success = false,
                Details = "Engine failure at 33 seconds and loss of vehicle"
            },
            new Launch
            {
                Id = "2",
                FlightNumber = 2,
                Name = "DemoSat",
                DateUtc = new DateTime(2007, 3, 21, 1, 10, 0, DateTimeKind.Utc),
                Success = false,
                Details = "Successful first stage burn and transition to second stage, maximum altitude 289 km, Premature engine shutdown at T+7 min 30 s, Failed to reach orbit, Failed to recover first stage"
            },
            new Launch
            {
                Id = "3",
                FlightNumber = 3,
                Name = "Trailblazer",
                DateUtc = new DateTime(2008, 8, 3, 3, 34, 0, DateTimeKind.Utc),
                Success = false,
                Details = "Residual stage-1 thrust led to collision between stage 1 and stage 2"
            },
            new Launch
            {
                Id = "4",
                FlightNumber = 4,
                Name = "RatSat",
                DateUtc = new DateTime(2008, 9, 28, 23, 15, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Ratsat was a aluminum honeycomb pot with a mass of 165 kg"
            },
            new Launch
            {
                Id = "5",
                FlightNumber = 5,
                Name = "RazakSat",
                DateUtc = new DateTime(2009, 7, 13, 3, 35, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Success! First successful orbital launch of Falcon 9"
            },
            new Launch
            {
                Id = "6",
                FlightNumber = 6,
                Name = "Falcon 9 Test Flight",
                DateUtc = new DateTime(2010, 6, 4, 18, 45, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Falcon 9 maiden flight. Dragon Qualification Unit"
            },
            new Launch
            {
                Id = "7",
                FlightNumber = 7,
                Name = "COTS 1",
                DateUtc = new DateTime(2010, 12, 8, 15, 43, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Dragon 2.5 orbit demonstration mission"
            },
            new Launch
            {
                Id = "8",
                FlightNumber = 8,
                Name = "COTS 2",
                DateUtc = new DateTime(2012, 5, 22, 7, 44, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Dragon mission to ISS"
            },
            new Launch
            {
                Id = "9",
                FlightNumber = 9,
                Name = "CRS-1",
                DateUtc = new DateTime(2012, 10, 7, 0, 35, 0, DateTimeKind.Utc),
                Success = true,
                Details = "First cargo delivery to the ISS"
            },
            new Launch
            {
                Id = "10",
                FlightNumber = 10,
                Name = "CRS-2",
                DateUtc = new DateTime(2013, 3, 1, 19, 10, 0, DateTimeKind.Utc),
                Success = true,
                Details = "Second cargo delivery to the ISS"
            }
        };

        await repository.SaveLaunchesAsync(testLaunches);
    }
}