namespace SpaceXLaunches.Common.Services.Configuration;

public class AppConfiguration
{
    public SpaceXApiConfiguration SpaceXApi { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
}

public class SpaceXApiConfiguration
{
    public string BaseUrl { get; set; } = "https://api.spacexdata.com/v4/";
    public int TimeoutSeconds { get; set; } = 30;
}

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = "Data Source=database/launches.db";
}
