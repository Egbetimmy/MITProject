namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Rate-limit and cache warm-up tuning per protective posture.
/// </summary>
public sealed class AdaptiveMitigationOptions
{
    public const string SectionName = "PredictiveMiddleware:Mitigation";

    public int NominalAuthenticatedRequestsPerMinute { get; set; } = 1000;
    public int NominalUnauthenticatedRequestsPerMinute { get; set; } = 1000;

    public int AlertAuthenticatedRequestsPerMinute { get; set; } = 400;
    public int AlertUnauthenticatedRequestsPerMinute { get; set; } = 400;

    public int CriticalAuthenticatedRequestsPerMinute { get; set; } = 200;
    public int CriticalUnauthenticatedRequestsPerMinute { get; set; } = 20;

    /// <summary>Redis keys pre-loaded when posture transitions to Alert.</summary>
    public IReadOnlyList<string> WarmupCacheKeys { get; set; } =
    [
        "catalog:featured",
        "catalog:bestsellers",
        "promotions:homepage",
    ];
}
