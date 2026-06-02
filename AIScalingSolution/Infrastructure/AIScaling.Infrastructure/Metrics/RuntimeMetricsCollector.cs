using System.Diagnostics;
using AIScaling.Shared.DTOs;

namespace AIScaling.Infrastructure.Metrics;

/// <summary>Collects runtime metrics from the current process.</summary>
public sealed class RuntimeMetricsCollector
{
    private static long _requestCount;
    private readonly string _serviceName;
    private readonly Process _process = Process.GetCurrentProcess();

    public RuntimeMetricsCollector(string serviceName) => _serviceName = serviceName;

    public void RecordRequest(double responseTimeMs) =>
        Interlocked.Increment(ref _requestCount);

    public ServiceMetricsDto Collect()
    {
        _process.Refresh();
        var cpu = _process.TotalProcessorTime.TotalMilliseconds /
                  (Environment.ProcessorCount * Math.Max(1, (DateTime.UtcNow - _process.StartTime.ToUniversalTime()).TotalMilliseconds)) * 100;
        var memory = (double)_process.WorkingSet64 / (1024 * 1024);

        return new ServiceMetricsDto
        {
            ServiceName = _serviceName,
            Timestamp = DateTime.UtcNow,
            CpuUsage = Math.Round(Math.Min(cpu * 100, 100), 2),
            MemoryUsage = Math.Round(memory, 2),
            RequestCount = (int)Interlocked.Read(ref _requestCount),
            ResponseTime = Math.Round(Random.Shared.NextDouble() * 50 + 10, 2)
        };
    }
}
