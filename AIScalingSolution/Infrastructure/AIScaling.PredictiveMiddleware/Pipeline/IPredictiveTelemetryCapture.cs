namespace AIScaling.PredictiveMiddleware.Pipeline;

/// <summary>
/// Abstraction used by middleware to emit a traffic tick without coupling to CQRS wiring.
/// </summary>
/// <remarks>
/// Implementations should enqueue or fire-and-forget a command dispatch so the ASP.NET
/// thread returns immediately—preserving non-blocking ingress for high-frequency traffic.
/// </remarks>
public interface IPredictiveTelemetryCapture
{
    /// <summary>
    /// Records one HTTP request observation for the predictive sliding window.
    /// </summary>
    ValueTask CaptureAsync(
        string endpointRoute,
        DateTimeOffset timestamp,
        string? clientIdentifier,
        CancellationToken cancellationToken);
}
