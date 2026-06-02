using Microsoft.Extensions.DependencyInjection;
using PredictionService.Application.Interfaces;
using PredictionService.Infrastructure.Services;

namespace PredictionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPredictionInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IMetricsDataLoader, MetricsDataLoader>();
        services.AddSingleton<IPredictionAppService, MlPredictionService>();
        return services;
    }
}
