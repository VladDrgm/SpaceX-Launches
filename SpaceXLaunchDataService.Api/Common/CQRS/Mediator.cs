using System.Collections.Concurrent;

namespace SpaceXLaunchDataService.Api.Common.CQRS;

/// <summary>
/// Custom MediatR-style mediator implementation
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
        }

        var handleMethod = handlerType.GetMethod("Handle");

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });

        if (result is Task<TResponse> task)
        {
            return await task;
        }

        if (result is Task taskResult)
        {
            await taskResult;
            return (TResponse)(object)Unit.Value;
        }

        return (TResponse)result!;
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        // Find the IRequest<TResponse> interface
        var requestInterface = requestType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

        if (requestInterface == null)
        {
            throw new ArgumentException($"Request type {requestType.Name} does not implement IRequest<TResponse>");
        }

        var responseType = requestInterface.GetGenericArguments()[0];
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
        }

        var handleMethod = handlerType.GetMethod("Handle");

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });

        if (result is Task task)
        {
            await task;

            // Get the result from the task
            if (task.GetType().IsGenericType)
            {
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }

            return Unit.Value;
        }

        return result;
    }
}