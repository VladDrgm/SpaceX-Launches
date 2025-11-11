namespace SpaceXLaunchDataService.Api.Common.CQRS;

/// <summary>
/// Handler for requests that return a response
/// </summary>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handler for requests with no response
/// </summary>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}