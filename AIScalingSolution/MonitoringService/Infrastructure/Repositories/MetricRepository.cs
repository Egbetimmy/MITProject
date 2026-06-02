using Microsoft.EntityFrameworkCore;
using MonitoringService.Application.Interfaces;
using MonitoringService.Domain.Entities;
using MonitoringService.Infrastructure.Persistence;

namespace MonitoringService.Infrastructure.Repositories;

public sealed class MetricRepository : IMetricRepository
{
    private readonly MonitoringDbContext _context;
    public MetricRepository(MonitoringDbContext context) => _context = context;

    public async Task SaveAsync(ResourceMetric metric, CancellationToken cancellationToken = default)
    {
        _context.ResourceMetrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceMetric>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.ResourceMetrics.AsNoTracking()
            .OrderByDescending(m => m.Timestamp).Take(1000).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ResourceMetric>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default) =>
        await _context.ResourceMetrics.AsNoTracking()
            .Where(m => m.ServiceName == serviceName)
            .OrderByDescending(m => m.Timestamp).Take(500).ToListAsync(cancellationToken);
}
