using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaceXLaunchDataService.Data;
using SpaceXLaunchDataService.Features.Launches.Services;
using SpaceXLaunchDataService.Common.Filters;
using SpaceXLaunchDataService.Common.Services.Infrastructure.Database;

namespace SpaceXLaunchDataService.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register database services
            services.AddSingleton<IDatabaseConnectionFactory, SqliteDatabaseConnectionFactory>();
            services.AddScoped<ILaunchRepository, SqliteLaunchRepository>();

            // Register API services
            services.AddScoped<ISpaceXApiService, SpaceXApiService>();
            services.AddHostedService<SpaceXDataSyncService>();

            // Add HttpClient for external API calls
            services.AddHttpClient<ISpaceXApiService, SpaceXApiService>();

            return services;
        }

        public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = true;
                // Configure enums to serialize as strings instead of numbers
                options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            return services;
        }

        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SpaceX Launch Data Service API",
                    Version = "v1",
                    Description = @"A REST API for accessing SpaceX launch data with advanced filtering, sorting, and pagination capabilities.

## Date Format
All date parameters use the **yyyy-MM-dd** format:
- ✅ Correct: `2006-03-24`, `2020-05-30`, `2024-01-15`
- ❌ Incorrect: `24-03-2006`, `2006/03/24`, `March 24, 2006`

## Common Date Examples
- **2006-03-24** - First Falcon 1 launch (FalconSat)
- **2008-09-28** - First successful Falcon 1 launch  
- **2010-06-04** - First Dragon capsule launch
- **2018-02-06** - Falcon Heavy maiden flight
- **2020-05-30** - First crewed Dragon launch (Demo-2)
- **2024-01-15** - Recent launches (use current dates)",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "SpaceX Launch Data API",
                        Url = new Uri("https://github.com/VladDrgm/SpaceX-Launches")
                    }
                });

                // Include XML comments for better documentation
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add enum descriptions
                c.SchemaFilter<EnumSchemaFilter>();
            });

            return services;
        }

        public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks();

            return services;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }
    }
}
