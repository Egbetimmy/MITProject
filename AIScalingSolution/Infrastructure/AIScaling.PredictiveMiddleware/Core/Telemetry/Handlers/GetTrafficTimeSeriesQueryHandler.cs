using AIScaling.PredictiveMiddleware.Analytics;
using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using AIScaling.PredictiveMiddleware.Core.Storage;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Queries;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Handlers;

/// <summary>
/// Read-path handler: materializes Redis sliding-window buckets for ML.NET ingestion.
/// </summary>
/// <remarks>
/// Executes on background threads only. Zero-valued buckets represent empty temporal slots so the SSA
/// pipeline receives a fixed-length, normalized vector without null gaps.
/// </remarks>
public sealed class GetTrafficTimeSeriesQueryHandler : IQueryHandler<GetTrafficTimeSeriesQuery, TrafficTimeSeriesResult>
{
    private readonly ITelemetryBufferStore _bufferStore;
    private readonly PredictiveEngineOptions _engineOptions;

    public GetTrafficTimeSeriesQueryHandler(
        ITelemetryBufferStore bufferStore,
        IOptions<PredictiveEngineOptions> engineOptions)
    {
        _bufferStore = bufferStore;
        engineOptions = engineOptions ?? throw new ArgumentNullException(nameof(engineOptions));
        _engineOptions = engineOptions.Value;
    }

    /// <inheritdoc />
    public async Task<TrafficTimeSeriesResult> HandleAsync(
        GetTrafficTimeSeriesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var bucketSeconds = Math.Max(1, query.BucketIntervalSeconds);
        var totalBuckets = Math.Max(1, query.WindowSizeSeconds / bucketSeconds);

        var raw = await _bufferStore
            .GetSeriesAsync(
                _engineOptions.AggregateSeriesKey,
                totalBuckets,
                TimeSpan.FromSeconds(bucketSeconds),
                cancellationToken)
            .ConfigureAwait(false);

        var volumes = NormalizeSeries(raw, totalBuckets);
        return new TrafficTimeSeriesResult(volumes);
    }

    /// <summary>
    /// Pads or truncates to a fixed bucket count; missing Redis slots remain zero.
    /// </summary>
    internal static int[] NormalizeSeries(long[] raw, int totalBuckets)
    {
        var normalized = new int[totalBuckets];

        if (raw.Length == 0)
        {
            return normalized;
        }

        var copyLength = Math.Min(raw.Length, totalBuckets);
        for (var i = 0; i < copyLength; i++)
        {
            normalized[i] = (int)Math.Min(raw[i], int.MaxValue);
        }

        return normalized;
    }
}
