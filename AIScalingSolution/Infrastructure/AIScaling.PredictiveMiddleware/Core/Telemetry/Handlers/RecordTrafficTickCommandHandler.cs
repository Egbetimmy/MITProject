using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using AIScaling.PredictiveMiddleware.Core.Storage;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Logging;
using AIScaling.PredictiveMiddleware.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Handlers;

/// <summary>
/// Write-path handler translating <see cref="RecordTrafficTickCommand"/> into Redis increments.
/// </summary>
/// <remarks>
/// Invoked by <see cref="Pipeline.Ingestion.TelemetryIngestionBackgroundService"/> off the HTTP
/// thread. Failures are logged and swallowed so Redis latency during JMeter/k6 bursts never
/// propagates to live clients or distorts load-test latency measurements.
/// </remarks>
public sealed class RecordTrafficTickCommandHandler : ICommandHandler<RecordTrafficTickCommand>
{
    private readonly ITelemetryBufferStore _bufferStore;
    private readonly ITrafficTickLogger _logger;
    private readonly TimeSpan _slidingWindowDuration;

    public RecordTrafficTickCommandHandler(
        ITelemetryBufferStore bufferStore,
        ITrafficTickLogger logger,
        IOptions<RedisTelemetryOptions> redisOptions)
    {
        ArgumentNullException.ThrowIfNull(bufferStore);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(redisOptions);
        _bufferStore = bufferStore;
        _logger = logger;
        _slidingWindowDuration = redisOptions.Value.SlidingWindowDuration;
    }

    /// <inheritdoc />
    public async Task HandleAsync(RecordTrafficTickCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var telemetryKey = TelemetryKeyFactory.FromEndpoint(command.EndpointRoute);

        try
        {
            await _bufferStore
                .IncrementMetricAsync(telemetryKey, _slidingWindowDuration, cancellationToken)
                .ConfigureAwait(false);

            // Aggregate series consumed by Part 3 SSA read path.
            await _bufferStore
                .IncrementMetricAsync(TelemetryKeyFactory.GlobalAggregateKey, _slidingWindowDuration, cancellationToken)
                .ConfigureAwait(false);

            _logger.TickRecorded(telemetryKey, command.EndpointRoute);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.TickFailed(telemetryKey, command.EndpointRoute, ex);
        }
    }
}

/// <summary>Normalizes endpoint routes into stable Redis series keys.</summary>
internal static class TelemetryKeyFactory
{
    public const string GlobalAggregateKey = "global";

    public static string FromEndpoint(string endpointRoute)
    {
        if (string.IsNullOrWhiteSpace(endpointRoute))
        {
            return "unknown";
        }

        return endpointRoute.Trim().TrimEnd('/').ToLowerInvariant();
    }
}
