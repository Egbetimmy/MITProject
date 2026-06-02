namespace AIScaling.PredictiveMiddleware.Core.Storage;

/// <summary>
/// Abstract sliding-window telemetry buffer (Redis or in-memory adapters in later parts).
/// </summary>
public interface ITelemetryBufferStore
{
    /// <summary>
    /// Increments a metric within a sliding window keyed by <paramref name="key"/>.
    /// </summary>
    Task IncrementMetricAsync(
        string key,
        TimeSpan slidingWindowDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns bucketed request volumes for <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Series identifier (e.g. route or aggregate key).</param>
    /// <param name="totalBuckets">Number of buckets to return.</param>
    /// <param name="bucketSize">Duration of each bucket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bucket counts ordered oldest to newest.</returns>
    Task<long[]> GetSeriesAsync(
        string key,
        int totalBuckets,
        TimeSpan bucketSize,
        CancellationToken cancellationToken);
}
