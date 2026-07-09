namespace AIScaling.PredictiveMiddleware.Core.State;

/// <summary>
/// Default in-memory, thread-safe implementation of <see cref="ISystemStateProvider"/>.
/// </summary>
public sealed class SystemStateProvider : ISystemStateProvider
{
    private readonly object _sync = new();
    private ProtectivePosture _posture = ProtectivePosture.Nominal;
    private DateTimeOffset _postureChangedAtUtc = DateTimeOffset.UtcNow;
    private DateTimeOffset _metricsEvaluatedAtUtc = DateTimeOffset.UtcNow;
    private double[] _lookaheadScores = Array.Empty<double>();

    /// <inheritdoc />
    public SystemStateSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new SystemStateSnapshot(
                _posture,
                _lookaheadScores,
                _postureChangedAtUtc,
                _metricsEvaluatedAtUtc);
        }
    }

    private bool _predictiveEngineEnabled = true;

    /// <inheritdoc />
    public bool PredictiveEngineEnabled
    {
        get { lock (_sync) return _predictiveEngineEnabled; }
        set { lock (_sync) _predictiveEngineEnabled = value; }
    }

    /// <inheritdoc />
    public void SetPosture(ProtectivePosture posture, DateTimeOffset changedAtUtc)
    {
        lock (_sync)
        {
            _posture = _predictiveEngineEnabled ? posture : ProtectivePosture.Nominal;
            _postureChangedAtUtc = changedAtUtc;
        }
    }

    /// <inheritdoc />
    public void SetLookaheadMetrics(IReadOnlyList<double> scores, DateTimeOffset evaluatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(scores);

        var copy = scores.Count == 0
            ? Array.Empty<double>()
            : scores.ToArray();

        lock (_sync)
        {
            _lookaheadScores = copy;
            _metricsEvaluatedAtUtc = evaluatedAtUtc;
        }
    }
}
