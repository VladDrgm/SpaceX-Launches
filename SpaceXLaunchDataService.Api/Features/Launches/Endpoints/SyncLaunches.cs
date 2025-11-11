using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Features.Launches.Commands;

namespace SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

public static class SyncLaunches
{
    /// <summary>
    /// Triggers synchronization of launch data from the SpaceX API
    /// </summary>
    /// <param name="mediator">CQRS mediator for dispatching commands</param>
    /// <returns>Synchronization result</returns>
    /// <response code="200">Successfully synchronized launch data</response>
    /// <response code="400">Synchronization failed</response>
    public static async Task<IResult> HandleAsync(IMediator mediator)
    {
        var command = new SyncLaunchesFromExternalApiCommand();
        var result = await mediator.Send(command);

        return result.Match<IResult>(
            count => Results.Ok(new SyncSuccessResponse($"Successfully synchronized {count} launches", count)),
            error => Results.BadRequest(new SyncErrorResponse(error.Message, error.Code, error.Details))
        );
    }
}

// Response Models
public record SyncSuccessResponse(string Message, int Count);
public record SyncErrorResponse(string Error, string Code, string? Details);
