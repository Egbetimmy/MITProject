using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Queries;

/// <summary>
/// Read-path query: retrieves request-volume buckets for forecasting evaluation.
/// </summary>
/// <remarks>
/// Executed by background analytics—not per HTTP request—so time-series assembly stays off
/// the middleware critical path and reactive scale lag is reduced.
/// </remarks>
/// <param name="WindowSizeSeconds">Total sliding window duration in seconds.</param>
/// <param name="BucketIntervalSeconds">Width of each bucket in seconds.</param>
public sealed record GetTrafficTimeSeriesQuery(
    int WindowSizeSeconds,
    int BucketIntervalSeconds) : IQuery<TrafficTimeSeriesResult>;
