using Microsoft.ML.Data;

namespace AIScaling.Shared.Models;

/// <summary>ML.NET model prediction output.</summary>
public sealed class PredictionResult
{
    [ColumnName("Score")]
    public float PredictedRequestLoad { get; set; }

    public string ScaleDecision { get; set; } = "Maintain";
}
