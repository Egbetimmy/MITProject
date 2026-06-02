namespace MonitoringService.Domain.Entities;

/// <summary>Persisted runtime resource metrics.</summary>
public sealed class ResourceMetric
{
    public long Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RequestCount { get; set; }
    public double ResponseTime { get; set; }
}
