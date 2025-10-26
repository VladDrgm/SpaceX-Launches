using System.Text.Json;

namespace SpaceXLaunchDataService.Common.Services.Configuration;

public static class JsonConfiguration
{
    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
}
