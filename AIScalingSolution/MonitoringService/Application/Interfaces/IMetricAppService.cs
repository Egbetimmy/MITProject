using AIScaling.Shared.DTOs;

namespace MonitoringService.Application.Interfaces;

public interface IMetricAppService
{
    Task<IReadOnlyList<ResourceMetricDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResourceMetricDto>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default);
}
