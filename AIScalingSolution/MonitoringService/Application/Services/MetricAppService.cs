using AIScaling.Shared.DTOs;
using MonitoringService.Application.Interfaces;

namespace MonitoringService.Application.Services;

public sealed class MetricAppService : IMetricAppService
{
    private readonly IMetricRepository _repository;
    public MetricAppService(IMetricRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ResourceMetricDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<IReadOnlyList<ResourceMetricDto>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default) =>
        (await _repository.GetByServiceAsync(serviceName, cancellationToken)).Select(Map).ToList();

    private static ResourceMetricDto Map(Domain.Entities.ResourceMetric m) => new()
    {
        Id = m.Id,
        ServiceName = m.ServiceName,
        Timestamp = m.Timestamp,
        CpuUsage = m.CpuUsage,
        MemoryUsage = m.MemoryUsage,
        RequestCount = m.RequestCount,
        ResponseTime = m.ResponseTime
    };
}
