namespace AIScaling.PredictiveMiddleware.Diagnostics;

/// <summary>
/// Thread-safe counters for console diagnostics during stress validation.
/// </summary>
public interface IPredictiveDiagnosticsCollector
{
    void RecordRequest();

    void RecordThrottledRequest();

    void RecordMiddlewareOverheadMs(double elapsedMs);

    PredictiveDiagnosticsSnapshot GetSnapshot();
}

/// <summary>Point-in-time diagnostics for terminal reporting.</summary>
public sealed record PredictiveDiagnosticsSnapshot(
    double CurrentRequestsPerSecond,
    double ForecastedRequestsPerSecond,
    long ThrottledRequests,
    double P99MiddlewareOverheadMs);
