using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Pipeline.Ingestion;

/// <summary>
/// Background fire-and-forget loop draining telemetry ticks to Redis via the command handler.
/// </summary>
/// <remarks>
/// Decouples JMeter/k6 request threads from network latency: HTTP returns while this service
/// performs batched handler invocations on dedicated pool threads.
/// </remarks>
public sealed class TelemetryIngestionBackgroundService : BackgroundService
{
    private readonly TelemetryIngestionQueue _queue;
    private readonly ICommandHandler<RecordTrafficTickCommand> _commandHandler;
    private readonly ILogger<TelemetryIngestionBackgroundService> _logger;
    private readonly int _consumerCount;

    public TelemetryIngestionBackgroundService(
        TelemetryIngestionQueue queue,
        ICommandHandler<RecordTrafficTickCommand> commandHandler,
        IOptions<TelemetryIngestionOptions> options,
        ILogger<TelemetryIngestionBackgroundService> logger)
    {
        _queue = queue;
        _commandHandler = commandHandler;
        _logger = logger;
        _consumerCount = Math.Max(1, options.Value.ConsumerCount);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumers = new Task[_consumerCount];
        for (var i = 0; i < _consumerCount; i++)
        {
            consumers[i] = ConsumeAsync(stoppingToken);
        }

        return Task.WhenAll(consumers);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _commandHandler.HandleAsync(command, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Telemetry ingestion consumer fault (swallowed to protect HTTP path).");
            }
        }
    }
}
