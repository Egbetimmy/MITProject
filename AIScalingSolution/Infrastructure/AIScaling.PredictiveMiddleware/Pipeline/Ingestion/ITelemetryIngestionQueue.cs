using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;

namespace AIScaling.PredictiveMiddleware.Pipeline.Ingestion;

/// <summary>
/// Non-blocking enqueue surface for the write path under synthetic load bursts.
/// </summary>
public interface ITelemetryIngestionQueue
{
    /// <summary>
    /// Attempts to enqueue a telemetry tick without blocking the caller.
    /// </summary>
    /// <returns><c>false</c> when the bounded queue is saturated (tick dropped).</returns>
    bool TryEnqueue(RecordTrafficTickCommand command);

    /// <summary>Total ticks dropped because the queue was full (JMeter/k6 overload).</summary>
    long DroppedTickCount { get; }
}
