using System.Collections.Concurrent;
using AIScaling.PredictiveMiddleware.Core.State;

namespace AIScaling.PredictiveMiddleware.Diagnostics;

/// <summary>
/// Lock-free diagnostics aggregation for load-test observability.
/// </summary>
public sealed class PredictiveDiagnosticsCollector : IPredictiveDiagnosticsCollector
{
    private readonly ISystemStateProvider _stateProvider;
    private long _windowRequestCount;
    private long _windowStartTicks = Environment.TickCount64;
    private long _throttledRequests;
    private readonly ConcurrentQueue<double> _overheadSamples = new();

    public PredictiveDiagnosticsCollector(ISystemStateProvider stateProvider)
    {
        _stateProvider = stateProvider;
    }

    public void RecordRequest() => Interlocked.Increment(ref _windowRequestCount);

    public void RecordThrottledRequest() => Interlocked.Increment(ref _throttledRequests);

    public void RecordMiddlewareOverheadMs(double elapsedMs)
    {
        _overheadSamples.Enqueue(elapsedMs);
        while (_overheadSamples.Count > 512 && _overheadSamples.TryDequeue(out _))
        {
        }
    }

    public PredictiveDiagnosticsSnapshot GetSnapshot()
    {
        var nowTicks = Environment.TickCount64;
        var elapsedMs = Math.Max(1, nowTicks - Interlocked.Read(ref _windowStartTicks));
        var requests = Interlocked.Exchange(ref _windowRequestCount, 0);
        Interlocked.Exchange(ref _windowStartTicks, nowTicks);

        var rps = requests * 1000d / elapsedMs;

        var state = _stateProvider.GetSnapshot();
        var forecasted = state.LookaheadMetricScores.Count > 0
            ? state.LookaheadMetricScores.Max()
            : 0d;

        return new PredictiveDiagnosticsSnapshot(
            rps,
            forecasted,
            Interlocked.Read(ref _throttledRequests),
            CalculateP99());
    }

    private double CalculateP99()
    {
        var values = _overheadSamples.ToArray();
        if (values.Length == 0)
        {
            return 0d;
        }

        Array.Sort(values);
        var index = (int)Math.Ceiling(values.Length * 0.99) - 1;
        index = Math.Clamp(index, 0, values.Length - 1);
        return values[index];
    }
}
