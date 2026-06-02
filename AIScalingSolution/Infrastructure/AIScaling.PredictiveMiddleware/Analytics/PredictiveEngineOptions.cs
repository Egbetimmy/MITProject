namespace AIScaling.PredictiveMiddleware.Analytics;

/// <summary>
/// Configuration for the background SSA forecasting worker (Part 3 read path).
/// </summary>
public sealed class PredictiveEngineOptions
{
    public const string SectionName = "PredictiveMiddleware:Engine";

    /// <summary>How often the engine evaluates Redis metrics and updates posture (milliseconds).</summary>
    public int EvaluationIntervalMilliseconds { get; set; } = 1000;

    /// <summary>Redis series key for aggregate request velocity (all endpoints).</summary>
    public string AggregateSeriesKey { get; set; } = "global";

    /// <summary>Historical window pulled from Redis per evaluation cycle (seconds).</summary>
    public int WindowSizeSeconds { get; set; } = 120;

    /// <summary>Width of each temporal bucket (seconds).</summary>
    public int BucketIntervalSeconds { get; set; } = 1;

    /// <summary>SSA local sub-series length (decomposition window).</summary>
    public int WindowSize { get; set; } = 30;

    /// <summary>Total points supplied to SSA (e.g. last 120 one-second buckets).</summary>
    public int SeriesLength { get; set; } = 120;

    /// <summary>Active sliding training set size (must be &gt;= <see cref="SeriesLength"/>).</summary>
    public int TrainSize { get; set; } = 120;

    /// <summary>
    /// Forecast horizon in buckets (60–300 => 1–5 minutes ahead at 1s buckets).
    /// </summary>
    public int Horizon { get; set; } = 60;

    /// <summary>Refit the cached SSA engine every N evaluation cycles (not every tick).</summary>
    public int RefitEveryNCycles { get; set; } = 60;

    /// <summary>Expected steady-state requests per bucket for capacity comparison.</summary>
    public float BaselineRequestsPerBucket { get; set; } = 100f;

    /// <summary>Peak / baseline ratio that elevates posture to Alert.</summary>
    public float AlertPeakMultiplier { get; set; } = 1.75f;

    /// <summary>Peak / baseline ratio that elevates posture to Critical.</summary>
    public float CriticalPeakMultiplier { get; set; } = 2.5f;

    /// <summary>Minimum forecast slope (last − first) across the horizon to flag acceleration.</summary>
    public float AccelerationThreshold { get; set; } = 50f;
}
