namespace AIScaling.PredictiveMiddleware.Pipeline;

/// <summary>
/// Middleware behavior for protective posture gating and traffic shedding during load tests.
/// </summary>
public sealed class PredictiveMiddlewareOptions
{
    public const string SectionName = "PredictiveMiddleware";

    /// <summary>
    /// Route path prefixes shed with HTTP 429 when posture is <see cref="Core.State.ProtectivePosture.Critical"/>.
    /// </summary>
    public IReadOnlyList<string> NonCriticalRoutePrefixes { get; set; } =
    [
        "/api/v1/promotions",
        "/api/promotions",
        "/api/widgets",
        "/api/search/index",
        "/health/ui",
    ];

    /// <summary>Response header signaling elevated risk during Alert posture.</summary>
    public string AlertStatusHeaderName { get; set; } = "X-System-Status";

    /// <summary>Header value written when posture is Alert.</summary>
    public string AlertStatusHeaderValue { get; set; } = "Alert-Mode";
}
