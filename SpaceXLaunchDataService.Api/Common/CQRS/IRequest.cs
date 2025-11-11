namespace SpaceXLaunchDataService.Api.Common.CQRS;

/// <summary>
/// Marker interface for requests that return a response
/// </summary>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Marker interface for requests with no response (void)
/// </summary>
public interface IRequest : IRequest<Unit>
{
}

/// <summary>
/// Represents a void response
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}