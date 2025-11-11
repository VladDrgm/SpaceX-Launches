using OneOf;
using SpaceXLaunchDataService.Api.Common.CQRS;
using SpaceXLaunchDataService.Api.Common.Models;
using SpaceXLaunchDataService.Api.Data.Models;

namespace SpaceXLaunchDataService.Api.Features.Launches.Commands;

public record SaveLaunchesCommand(IEnumerable<Launch> Launches) : Command<OneOf<int, ServiceError>>;