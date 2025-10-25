using SpaceXLaunches.Data;
using SpaceXLaunches.Features.Launches.Services;

namespace SpaceXLaunches.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register data services
            services.AddScoped<ILaunchRepository, LaunchRepository>();
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
            });

            return services;
        }

        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

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
