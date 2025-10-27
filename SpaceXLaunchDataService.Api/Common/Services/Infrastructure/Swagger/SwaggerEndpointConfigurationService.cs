using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Swagger;

public static class SwaggerEndpointConfigurationService
{
    public static RouteHandlerBuilder ConfigureGetLaunchesSwagger(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetLaunches")
            .WithSummary("Get launches with filtering and pagination")
            .WithDescription("Retrieve a paginated list of SpaceX launches with advanced filtering options including date ranges, success status, and text search.")
            .Produces<PaginatedLaunchesResponse>(200)
            .ProducesProblem(400)
            .WithOpenApi(operation =>
            {
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
                        Description = "Field to sort by:\n• DateUtc - Sort by launch date\n• Name - Sort by mission name\n• FlightNumber - Sort by flight sequence number\n• Success - Sort by launch outcome",
                        Example = new OpenApiString("DateUtc"),
                        Default = new OpenApiString("DateUtc")
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
                        Description = "Sort direction:\n• Asc - Ascending order (oldest/lowest first)\n• Desc - Descending order (newest/highest first)",
                        Example = new OpenApiString("Desc"),
                        Default = new OpenApiString("Desc")
                    };
                }

                var successParam = operation.Parameters.FirstOrDefault(p => p.Name == "success");
                if (successParam != null)
                {
                    successParam.Description = "Filter by launch success:\n• true - Successful launches only\n• false - Failed launches only\n• null/empty - All launches";
                    successParam.Example = new OpenApiBoolean(true);
                }

                var pageParam = operation.Parameters.FirstOrDefault(p => p.Name == "page");
                if (pageParam != null)
                {
                    pageParam.Description = "Page number (starts from 1)";
                    pageParam.Example = new OpenApiInteger(1);
                }

                var pageSizeParam = operation.Parameters.FirstOrDefault(p => p.Name == "pageSize");
                if (pageSizeParam != null)
                {
                    pageSizeParam.Description = "Number of items per page (1-100)";
                    pageSizeParam.Example = new OpenApiInteger(10);
                }

                var fromDateParam = operation.Parameters.FirstOrDefault(p => p.Name == "fromDate");
                if (fromDateParam != null)
                {
                    fromDateParam.Description = "Filter from date (yyyy-MM-dd format). Example: 2006-03-24 for first Falcon 1 launch";
                    fromDateParam.Example = new OpenApiString("2006-03-24");
                }

                var toDateParam = operation.Parameters.FirstOrDefault(p => p.Name == "toDate");
                if (toDateParam != null)
                {
                    toDateParam.Description = "Filter to date (yyyy-MM-dd format). Example: 2024-12-31";
                    toDateParam.Example = new OpenApiString("2024-12-31");
                }

                var searchTermParam = operation.Parameters.FirstOrDefault(p => p.Name == "searchTerm");
                if (searchTermParam != null)
                {
                    searchTermParam.Description = "Search in mission names and details. Examples: 'Falcon', 'Dragon', 'ISS'";
                    searchTermParam.Example = new OpenApiString("Falcon");
                }
                return operation;
            });
    }

    public static RouteHandlerBuilder ConfigureGetLaunchDetailsSwagger(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetLaunchDetails")
            .WithSummary("Get launch details by ID")
            .WithDescription("Retrieve detailed information for a specific SpaceX launch by its unique identifier.")
            .Produces<LaunchDetailsResponse>(200)
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
            .Produces<SyncSuccessResponse>(200)
            .ProducesProblem(400);
    }
}