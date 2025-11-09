using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Data;

namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Resilience;

/// <summary>
/// Provides resilience policies for HTTP and database operations
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Creates a resilience pipeline for HTTP operations with retry, circuit breaker, and timeout
    /// </summary>
    public static ResiliencePipeline<HttpResponseMessage> CreateHttpResiliencePipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            // Timeout policy: 30 seconds per request
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                OnTimeout = args =>
                {
                    logger.LogWarning("HTTP request timed out after {Timeout}s", args.Timeout.TotalSeconds);
                    return default;
                }
            })
            // Retry policy: 3 attempts with exponential backoff
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true, // Adds randomness to prevent thundering herd
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => 
                        !response.IsSuccessStatusCode && 
                        (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                         response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                         (int)response.StatusCode >= 500))
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                OnRetry = args =>
                {
                    var exception = args.Outcome.Exception;
                    var statusCode = args.Outcome.Result?.StatusCode;
                    
                    logger.LogWarning(
                        exception,
                        "HTTP request failed (Attempt {Attempt}/3). Status: {StatusCode}. Retrying in {Delay}s...",
                        args.AttemptNumber + 1,
                        statusCode,
                        args.RetryDelay.TotalSeconds);
                    
                    return default;
                }
            })
            // Circuit breaker: Open after 5 consecutive failures, stay open for 30 seconds
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => !response.IsSuccessStatusCode)
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                OnOpened = _ =>
                {
                    logger.LogError(
                        "Circuit breaker OPENED after multiple failures. Will retry after {BreakDuration}s",
                        30);
                    return default;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Circuit breaker CLOSED. Normal operation resumed");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    logger.LogInformation("Circuit breaker HALF-OPEN. Testing if service recovered");
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a resilience pipeline for database operations with retry
    /// </summary>
    public static ResiliencePipeline CreateDatabaseResiliencePipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            // Retry policy: 2 attempts with short delays for transient database errors
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    // Retry on SQLite-specific transient errors
                    .Handle<Microsoft.Data.Sqlite.SqliteException>(ex =>
                        ex.SqliteErrorCode == 5 || // SQLITE_BUSY - database is locked
                        ex.SqliteErrorCode == 6 || // SQLITE_LOCKED - table is locked
                        ex.SqliteErrorCode == 10 || // SQLITE_IOERR - disk I/O error
                        ex.SqliteErrorCode == 13)   // SQLITE_FULL - database or disk is full
                    // Retry on general database timeout
                    .Handle<TimeoutException>()
                    // Retry on general data exceptions (but not schema errors)
                    .Handle<DataException>(),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Database operation failed (Attempt {Attempt}/2). Retrying in {Delay}ms...",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds);
                    
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a resilience pipeline for the entire sync operation
    /// </summary>
    public static ResiliencePipeline CreateSyncResiliencePipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            // Timeout for entire sync operation: 2 minutes
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMinutes(2),
                OnTimeout = args =>
                {
                    logger.LogError("Sync operation timed out after {Timeout} minutes", args.Timeout.TotalMinutes);
                    return default;
                }
            })
            // Retry entire sync if it fails: 1 retry with 10 second delay
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.FromSeconds(10),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Sync operation failed (Attempt {Attempt}). Retrying in {Delay}s...",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalSeconds);
                    
                    return default;
                }
            })
            .Build();
    }
}

