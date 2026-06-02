using Microsoft.ML.Data;

namespace AIScaling.Shared.Models;

/// <summary>ML.NET training and prediction input features.</summary>
public sealed class MetricData
{
    [LoadColumn(0)]
    public float CpuUsage { get; set; }

    [LoadColumn(1)]
    public float MemoryUsage { get; set; }

    [LoadColumn(2)]
    public float RequestCount { get; set; }

    [LoadColumn(3)]
    public float ResponseTime { get; set; }

    [LoadColumn(4)]
    [ColumnName("Label")]
    public float PredictedRequestLoad { get; set; }
}
