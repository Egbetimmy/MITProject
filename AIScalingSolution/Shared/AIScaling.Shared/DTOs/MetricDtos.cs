namespace AIScaling.Shared.DTOs;

/// <summary>Runtime metrics snapshot for a microservice.</summary>
public sealed class ServiceMetricsDto
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RequestCount { get; set; }
    public double ResponseTime { get; set; }
}

/// <summary>Stored resource metrics record.</summary>
public sealed class ResourceMetricDto
{
    public long Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RequestCount { get; set; }
    public double ResponseTime { get; set; }
}

/// <summary>Input for ML.NET prediction.</summary>
public sealed class PredictionInputDto
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RequestCount { get; set; }
    public double ResponseTime { get; set; }
}

/// <summary>ML.NET prediction output.</summary>
public sealed class PredictionOutputDto
{
    public float PredictedRequestLoad { get; set; }
    public string ScaleDecision { get; set; } = string.Empty;
}
