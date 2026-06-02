namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Logging;

/// <summary>
/// Logging abstraction for the write-path telemetry handler.
/// </summary>
public interface ITrafficTickLogger
{
    void TickRecorded(string telemetryKey, string endpointRoute);

    void TickFailed(string telemetryKey, string endpointRoute, Exception exception);
}
