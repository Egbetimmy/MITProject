using Microsoft.EntityFrameworkCore;
using MonitoringService.Domain.Entities;
using MonitoringService.Infrastructure.Persistence;

namespace MonitoringService.Infrastructure.Seed;

/// <summary>Seeds synthetic historical metrics for ML training.</summary>
public static class MonitoringDbSeed
{
    public static async Task SeedAsync(MonitoringDbContext context)
    {
        if (await context.ResourceMetrics.AnyAsync()) return;

        var services = new[] { "UserService", "ProductService", "OrderService" };
        var random = new Random(42);
        var now = DateTime.UtcNow;

        for (var day = 7; day >= 0; day--)
        {
            for (var hour = 0; hour < 24; hour++)
            {
                foreach (var service in services)
                {
                    var requestCount = random.Next(50, 800);
                    context.ResourceMetrics.Add(new ResourceMetric
                    {
                        ServiceName = service,
                        Timestamp = now.AddDays(-day).AddHours(-hour),
                        CpuUsage = random.NextDouble() * 80 + 10,
                        MemoryUsage = random.NextDouble() * 512 + 128,
                        RequestCount = requestCount,
                        ResponseTime = random.NextDouble() * 100 + 20
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
