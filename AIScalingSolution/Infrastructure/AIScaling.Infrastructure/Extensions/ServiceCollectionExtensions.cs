using AIScaling.Infrastructure.Logging;
using AIScaling.Infrastructure.Metrics;
using AIScaling.Infrastructure.Middleware;
using AIScaling.Infrastructure.Telemetry;
using AIScaling.Shared.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIScaling.Infrastructure.Extensions;

/// <summary>Common service registration for microservice APIs.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        services.AddSingleton<IScalingLogger, ScalingLogger>();
        services.AddSingleton(new RuntimeMetricsCollector(serviceName));
        services.AddAppOpenTelemetry(serviceName, configuration["OpenTelemetry:OtlpEndpoint"]);
        return services;
    }

    public static WebApplication UseCommonInfrastructure(this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
