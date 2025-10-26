using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using SpaceXLaunchDataService.Data.Models.Enums;

namespace SpaceXLaunchDataService.Common.Services.Infrastructure.Swagger;

public static class SwaggerEndpointConfigurationService
{
    public static RouteHandlerBuilder ConfigureGetLaunchesSwagger(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetLaunches")
            .WithSummary("Get launches with filtering and pagination")
            .WithDescription("Retrieve a paginated list of SpaceX launches with advanced filtering options including date ranges, success status, and text search.")
            .Produces<SpaceXLaunchDataService.Features.Launches.Endpoints.PaginatedLaunchesResponse>(200)
            .ProducesProblem(400)
            .WithOpenApi(operation =>
            {
                // Configure sortBy parameter with proper enum schema
                var sortByParam = operation.Parameters.FirstOrDefault(p => p.Name == "sortBy");
                if (sortByParam != null)
                {
                    sortByParam.Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString("DateUtc"),
                            new OpenApiString("Name"),
                            new OpenApiString("FlightNumber"),
                            new OpenApiString("Success")
                        },
                        Description = "Field to sort by: DateUtc (launch date), Name (mission name), FlightNumber (flight sequence), Success (launch outcome)",
                        Example = new OpenApiString("DateUtc")
                    };
                }

                // Configure sortOrder parameter with proper enum schema
                var sortOrderParam = operation.Parameters.FirstOrDefault(p => p.Name == "sortOrder");
                if (sortOrderParam != null)
                {
                    sortOrderParam.Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString("Asc"),
                            new OpenApiString("Desc")
                        },
                        Description = "Sort direction: Asc (ascending), Desc (descending)",
                        Example = new OpenApiString("Desc")
                    };
                }

                // Configure success parameter
                var successParam = operation.Parameters.FirstOrDefault(p => p.Name == "success");
                if (successParam != null)
                {
                    successParam.Description = "Filter by launch success: true (successful launches only), false (failed launches only), null/empty (all launches)";
                    successParam.Example = new OpenApiBoolean(true);
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "page",
                    Description = "Page number (starts from 1)",
                    Example = new OpenApiInteger(1)
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "pageSize",
                    Description = "Number of items per page (1-100)",
                    Example = new OpenApiInteger(10)
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "fromDate",
                    Description = "Filter from date (yyyy-MM-dd format). Example: 2006-03-24",
                    Example = new OpenApiString("2006-03-24")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "toDate",
                    Description = "Filter to date (yyyy-MM-dd format). Example: 2024-12-31",
                    Example = new OpenApiString("2024-12-31")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "searchTerm",
                    Description = "Search in mission names and details. Example: Falcon",
                    Example = new OpenApiString("Falcon")
                });
                return operation;
            });
    }

    public static RouteHandlerBuilder ConfigureGetLaunchDetailsSwagger(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetLaunchDetails")
            .WithSummary("Get launch details by ID")
            .WithDescription("Retrieve detailed information for a specific SpaceX launch by its unique identifier.")
            .Produces<SpaceXLaunchDataService.Features.Launches.Endpoints.LaunchDetailsResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .WithOpenApi(operation =>
            {
                operation.Parameters.First(p => p.Name == "id").Example = new OpenApiString("1");
                operation.Parameters.First(p => p.Name == "id").Description = "Launch ID. Examples: '1' (FalconSat - first Falcon 1), '2' (DemoSat), '3' (Trailblazer)";
                return operation;
            });
    }

    public static RouteHandlerBuilder ConfigureSyncLaunchesSwagger(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("SyncLaunches")
            .WithSummary("Synchronize launch data")
            .WithDescription("Fetch and synchronize the latest SpaceX launch data from external sources.")
            .Produces<SpaceXLaunchDataService.Features.Launches.Endpoints.SyncSuccessResponse>(200)
            .ProducesProblem(400);
    }
}