namespace AIScaling.PredictiveMiddleware.Core.State;

/// <summary>
/// Immutable read model of the current protective system state.
/// </summary>
/// <param name="Posture">Active protective posture.</param>
/// <param name="LookaheadMetricScores">Latest forecast or risk scores (higher = more urgent).</param>
/// <param name="PostureChangedAtUtc">When the current posture was last assigned.</param>
/// <param name="MetricsEvaluatedAtUtc">When lookahead scores were last refreshed.</param>
public sealed record SystemStateSnapshot(
    ProtectivePosture Posture,
    IReadOnlyList<double> LookaheadMetricScores,
    DateTimeOffset PostureChangedAtUtc,
    DateTimeOffset MetricsEvaluatedAtUtc);
