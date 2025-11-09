namespace SpaceXLaunchDataService.Api.Common.Models;

/// <summary>
/// Represents an error that occurred during a service operation
/// </summary>
public sealed record ServiceError
{
    /// <summary>
    /// The error code/type
    /// </summary>
    public required string Code { get; init; }
    
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// The underlying exception, if any
    /// </summary>
    public Exception? Exception { get; init; }
    
    /// <summary>
    /// Additional error details or context
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Creates a database error
    /// </summary>
    public static ServiceError Database(string message, Exception? exception = null, string? details = null) =>
        new()
        {
            Code = "DATABASE_ERROR",
            Message = message,
            Exception = exception,
            Details = details
        };

    /// <summary>
    /// Creates an HTTP/API error
    /// </summary>
    public static ServiceError Http(string message, Exception? exception = null, string? details = null) =>
        new()
        {
            Code = "HTTP_ERROR",
            Message = message,
            Exception = exception,
            Details = details
        };

    /// <summary>
    /// Creates a not found error
    /// </summary>
    public static ServiceError NotFound(string message, string? details = null) =>
        new()
        {
            Code = "NOT_FOUND",
            Message = message,
            Details = details
        };

    /// <summary>
    /// Creates a validation error
    /// </summary>
    public static ServiceError Validation(string message, string? details = null) =>
        new()
        {
            Code = "VALIDATION_ERROR",
            Message = message,
            Details = details
        };

    /// <summary>
    /// Creates a timeout error
    /// </summary>
    public static ServiceError Timeout(string message, Exception? exception = null) =>
        new()
        {
            Code = "TIMEOUT",
            Message = message,
            Exception = exception
        };

    /// <summary>
    /// Creates a general/unknown error
    /// </summary>
    public static ServiceError Unknown(string message, Exception? exception = null, string? details = null) =>
        new()
        {
            Code = "UNKNOWN_ERROR",
            Message = message,
            Exception = exception,
            Details = details
        };

    /// <summary>
    /// Creates an error from an exception
    /// </summary>
    public static ServiceError FromException(Exception exception, string? additionalMessage = null)
    {
        var message = string.IsNullOrEmpty(additionalMessage) 
            ? exception.Message 
            : $"{additionalMessage}: {exception.Message}";

        return exception switch
        {
            TimeoutException => Timeout(message, exception),
            HttpRequestException => Http(message, exception),
            Microsoft.Data.Sqlite.SqliteException sqliteEx => Database(message, sqliteEx, $"SQLite Error Code: {sqliteEx.SqliteErrorCode}"),
            System.Data.DataException => Database(message, exception),
            _ => Unknown(message, exception)
        };
    }

    /// <summary>
    /// Returns a simple error message for logging/display
    /// </summary>
    public override string ToString() => $"[{Code}] {Message}";
}

