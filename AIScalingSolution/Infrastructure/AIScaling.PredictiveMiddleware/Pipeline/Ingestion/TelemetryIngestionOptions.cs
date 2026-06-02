namespace AIScaling.PredictiveMiddleware.Pipeline.Ingestion;

/// <summary>
/// Tuning for high-density ingestion (e.g. Apache JMeter, Grafana k6 burst scripts).
/// </summary>
public sealed class TelemetryIngestionOptions
{
    /// <summary>
    /// Bounded queue capacity before ticks are dropped rather than blocking HTTP threads.
    /// Sized for short burst windows at 10k+ RPS without unbounded memory growth.
    /// </summary>
    public int QueueCapacity { get; set; } = 65_536;

    /// <summary>Number of background consumers draining the ingestion queue.</summary>
    public int ConsumerCount { get; set; } = 2;
}
