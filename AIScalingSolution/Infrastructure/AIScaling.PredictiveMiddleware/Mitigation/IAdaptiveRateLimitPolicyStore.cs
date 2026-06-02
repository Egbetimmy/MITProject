using AIScaling.PredictiveMiddleware.Core.State;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Thread-safe store of active rate-limit profiles driven by protective posture.
/// </summary>
public interface IAdaptiveRateLimitPolicyStore
{
    PostureRateLimitProfile CurrentProfile { get; }

    ProtectivePosture CurrentPosture { get; }

    void ApplyPosture(ProtectivePosture posture);
}
