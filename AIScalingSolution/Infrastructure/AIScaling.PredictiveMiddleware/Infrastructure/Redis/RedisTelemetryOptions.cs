namespace AIScaling.PredictiveMiddleware.Infrastructure.Redis;

/// <summary>
/// Configuration for Redis-backed telemetry buffering.
/// </summary>
public sealed class RedisTelemetryOptions
{
    public const string SectionName = "PredictiveMiddleware:Redis";

    /// <summary>StackExchange.Redis connection string.</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>Key namespace prefix (e.g. api:telemetry:v1).</summary>
    public string KeyPrefix { get; set; } = "api:telemetry:v1";

    /// <summary>Sliding-window TTL applied on first increment in a temporal bucket.</summary>
    public TimeSpan SlidingWindowDuration { get; set; } = TimeSpan.FromMinutes(5);
}
