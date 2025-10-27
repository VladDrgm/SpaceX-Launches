namespace SpaceXLaunchDataService.Api.Features.Launches.Endpoints;

public interface IEndpoint
{
    static abstract Task<IResult> HandleAsync();
}
