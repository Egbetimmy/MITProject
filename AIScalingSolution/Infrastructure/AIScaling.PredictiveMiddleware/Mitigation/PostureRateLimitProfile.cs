using AIScaling.PredictiveMiddleware.Core.State;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Immutable per-posture token-bucket limits (requests per minute).
/// </summary>
public readonly record struct PostureRateLimitProfile(
    int AuthenticatedRequestsPerMinute,
    int UnauthenticatedRequestsPerMinute)
{
    public static PostureRateLimitProfile FromOptions(
        ProtectivePosture posture,
        AdaptiveMitigationOptions options)
    {
        return posture switch
        {
            ProtectivePosture.Alert => new PostureRateLimitProfile(
                options.AlertAuthenticatedRequestsPerMinute,
                options.AlertUnauthenticatedRequestsPerMinute),
            ProtectivePosture.Critical => new PostureRateLimitProfile(
                options.CriticalAuthenticatedRequestsPerMinute,
                options.CriticalUnauthenticatedRequestsPerMinute),
            _ => new PostureRateLimitProfile(
                options.NominalAuthenticatedRequestsPerMinute,
                options.NominalUnauthenticatedRequestsPerMinute),
        };
    }
}
