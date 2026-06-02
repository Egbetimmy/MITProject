using System.Threading.Channels;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Pipeline.Ingestion;

/// <summary>
/// Bounded in-process queue isolating HTTP threads from Redis I/O during load-test bursts.
/// </summary>
/// <remarks>
/// Under JMeter/k6 scripts issuing thousands of requests per second, per-request
/// thread-pool dispatch can exhaust workers and inflate latency.
/// A bounded <see cref="Channel{T}"/> provides a fixed-memory fire-and-forget handoff.
/// </remarks>
public sealed class TelemetryIngestionQueue : ITelemetryIngestionQueue
{
    private readonly Channel<RecordTrafficTickCommand> _channel;
    private long _droppedTickCount;

    public TelemetryIngestionQueue(IOptions<TelemetryIngestionOptions> options)
    {
        var capacity = Math.Max(1024, options.Value.QueueCapacity);
        _channel = Channel.CreateBounded<RecordTrafficTickCommand>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = false,
                SingleWriter = false,
            });
    }

    internal ChannelReader<RecordTrafficTickCommand> Reader => _channel.Reader;

    public long DroppedTickCount => Interlocked.Read(ref _droppedTickCount);

    public bool TryEnqueue(RecordTrafficTickCommand command)
    {
        if (!_channel.Writer.TryWrite(command))
        {
            Interlocked.Increment(ref _droppedTickCount);
            return false;
        }

        return true;
    }
}
