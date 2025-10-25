namespace SpaceXLaunches.Data.Types.Requests;

public class GetLaunchesRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
