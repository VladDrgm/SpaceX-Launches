using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Features.Launches.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Queries;

public record GetLaunchesByDateQuery(DateTime Date) : Query<OneOf<List<LaunchResponse>, ServiceError>>;