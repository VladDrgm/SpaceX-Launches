namespace SpaceXLaunches.Data.Types;

public static class Errors
{
    public static string NotFound(string id) => $"Launch with ID '{id}' not found";
    public static string DatabaseError(string message) => $"Database error: {message}";
    public static string ExternalApiError(string message) => $"External API error: {message}";
    public static string ValidationError(string message) => $"Validation error: {message}";
}
