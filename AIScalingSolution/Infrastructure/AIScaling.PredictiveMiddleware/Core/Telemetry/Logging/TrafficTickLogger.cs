using Microsoft.Extensions.Logging;

namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Logging;

/// <summary>
/// Default <see cref="ITrafficTickLogger"/> backed by <see cref="ILogger"/>.
/// </summary>
public sealed class TrafficTickLogger : ITrafficTickLogger
{
    private readonly ILogger<TrafficTickLogger> _logger;

    public TrafficTickLogger(ILogger<TrafficTickLogger> logger)
    {
        _logger = logger;
    }

    public void TickRecorded(string telemetryKey, string endpointRoute) =>
        _logger.LogDebug(
            "Telemetry tick recorded for {EndpointRoute} (key={TelemetryKey})",
            endpointRoute,
            telemetryKey);

    public void TickFailed(string telemetryKey, string endpointRoute, Exception exception) =>
        _logger.LogWarning(
            exception,
            "Telemetry tick failed for {EndpointRoute} (key={TelemetryKey})",
            endpointRoute,
            telemetryKey);
}
