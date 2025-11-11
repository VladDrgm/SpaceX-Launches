namespace SpaceXLaunchDataService.Api.Common.CQRS;

/// <summary>
/// Base class for commands that modify state and return a response
/// </summary>
public abstract record Command<TResponse> : IRequest<TResponse>;

/// <summary>
/// Base class for commands that modify state without returning a response
/// </summary>
public abstract record Command : IRequest;

/// <summary>
/// Base class for queries that read data and return a response
/// </summary>
public abstract record Query<TResponse> : IRequest<TResponse>;