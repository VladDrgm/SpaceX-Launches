using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Commands;

public record SyncLaunchesFromExternalApiCommand : Command<OneOf<int, ServiceError>>;