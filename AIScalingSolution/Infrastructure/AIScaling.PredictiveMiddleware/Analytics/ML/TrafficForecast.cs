using Microsoft.ML.Data;

namespace AIScaling.PredictiveMiddleware.Analytics.ML;

/// <summary>
/// ML.NET output schema: SSA lookahead vector (horizon buckets into the future).
/// </summary>
public sealed class TrafficForecast
{
    /// <summary>
    /// Forecasted request counts per future bucket. Length matches configured horizon (max 300).
    /// </summary>
    [VectorType(300)]
    public float[]? Forecast { get; set; }
}
