using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonitoringService.Application.Interfaces;
using MonitoringService.Application.Services;
using MonitoringService.Infrastructure.BackgroundServices;
using MonitoringService.Infrastructure.Persistence;
using MonitoringService.Infrastructure.Repositories;
using MonitoringService.Infrastructure.Seed;

namespace MonitoringService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMonitoringInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MonitoringDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")).ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        services.AddHttpClient("metrics", client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddScoped<IMetricRepository, MetricRepository>();
        services.AddScoped<IMetricAppService, MetricAppService>();
        services.AddHostedService<MetricsPollingBackgroundService>();

        return services;
    }

    public static async Task MigrateAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MonitoringDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await MonitoringDbSeed.SeedAsync(context);
    }
}
