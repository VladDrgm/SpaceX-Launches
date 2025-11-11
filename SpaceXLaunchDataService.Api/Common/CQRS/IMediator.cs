namespace SpaceXLaunchDataService.Api.Common.CQRS;

/// <summary>
/// Mediator interface for dispatching requests to their handlers
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task<object?> Send(object request, CancellationToken cancellationToken = default);
}