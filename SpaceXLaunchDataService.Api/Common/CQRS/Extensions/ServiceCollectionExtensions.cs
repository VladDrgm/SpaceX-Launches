using System.Reflection;

namespace SpaceXLaunchDataService.Api.Common.CQRS.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CQRS mediator and all request handlers from the specified assemblies
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

        // Get all assemblies to scan
        var assembliesToScan = assemblies.Length != 0
            ? assemblies
            : new[] { Assembly.GetExecutingAssembly() };

        // Register all request handlers
        foreach (var assembly in assembliesToScan)
        {
            RegisterHandlers(services, assembly);
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(IsHandlerInterface))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(IsHandlerInterface)
                .ToList();

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, handlerType);
            }
        }
    }

    private static bool IsHandlerInterface(Type type)
    {
        if (!type.IsGenericType) return false;

        var genericType = type.GetGenericTypeDefinition();
        return genericType == typeof(IRequestHandler<,>) ||
               genericType == typeof(IRequestHandler<>);
    }
}