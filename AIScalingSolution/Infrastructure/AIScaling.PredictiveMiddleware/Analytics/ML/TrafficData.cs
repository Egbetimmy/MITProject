namespace AIScaling.PredictiveMiddleware.Analytics.ML;

/// <summary>
/// ML.NET input schema: one observation of request volume per temporal bucket.
/// </summary>
public sealed class TrafficData
{
    public float Count { get; set; }
}
