using MonitoringService.Domain.Entities;

namespace MonitoringService.Application.Interfaces;

public interface IMetricRepository
{
    Task SaveAsync(ResourceMetric metric, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResourceMetric>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResourceMetric>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default);
}
