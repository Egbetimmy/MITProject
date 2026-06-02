namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Queries;

/// <summary>
/// Read-model returned by <see cref="GetTrafficTimeSeriesQuery"/>.
/// </summary>
/// <param name="RequestVolumes">
/// Request counts per bucket, ordered from oldest to newest bucket in the window.
/// </param>
public sealed record TrafficTimeSeriesResult(int[] RequestVolumes);
