namespace AIScaling.PredictiveMiddleware.Core.State;

/// <summary>
/// Thread-safe, process-wide provider for protective posture and lookahead metrics.
/// </summary>
public interface ISystemStateProvider
{
    /// <summary>
    /// Returns a consistent snapshot of the current system state.
    /// </summary>
    SystemStateSnapshot GetSnapshot();

    /// <summary>
    /// Updates the active protective posture.
    /// </summary>
    void SetPosture(ProtectivePosture posture, DateTimeOffset changedAtUtc);

    /// <summary>
    /// Replaces lookahead metric scores from the analytics engine.
    /// </summary>
    void SetLookaheadMetrics(IReadOnlyList<double> scores, DateTimeOffset evaluatedAtUtc);
}
