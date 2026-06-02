using AIScaling.PredictiveMiddleware.Diagnostics;
using AIScaling.PredictiveMiddleware.Mitigation;
using AIScaling.PredictiveMiddleware.Pipeline.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIScaling.PredictiveMiddleware.Extensions;

/// <summary>
/// Registers Part 4 adaptive mitigations, diagnostics, and middleware integrations.
/// </summary>
public static class PredictiveMitigationExtensions
{
    /// <summary>
    /// Adds posture-driven rate limiting, cache pre-warming, and console diagnostics.
    /// </summary>
    public static IServiceCollection AddPredictiveMitigationPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AdaptiveMitigationOptions>()
            .Bind(configuration.GetSection(AdaptiveMitigationOptions.SectionName));

        services
            .AddOptions<PredictiveDiagnosticsOptions>()
            .Bind(configuration.GetSection(PredictiveDiagnosticsOptions.SectionName));

        services.TryAddSingleton<IAdaptiveRateLimitPolicyStore, AdaptiveRateLimitPolicyStore>();
        services.TryAddSingleton<AdaptiveClientRateLimiter>();
        services.TryAddSingleton<ICachePreWarmingService, RedisCachePreWarmingService>();
        services.TryAddSingleton<IPredictiveDiagnosticsCollector, PredictiveDiagnosticsCollector>();

        services.AddHostedService<AdaptiveMitigationCoordinator>();
        services.AddHostedService<PredictiveDiagnosticsConsoleHostedService>();

        return services;
    }

    /// <summary>
    /// Full predictive stack: write path, read/forecast path, and adaptive mitigations.
    /// </summary>
    public static IServiceCollection AddFullPredictiveMiddleware(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPredictiveMiddleware(configuration);
        services.AddPredictiveMitigationPipeline(configuration);
        return services;
    }

    /// <summary>
    /// HTTP pipeline ordering for stress validation and production ingress.
    /// </summary>
    public static WebApplication UseFullPredictiveMiddleware(this WebApplication app)
    {
        app.UsePredictiveCommandPipeline();
        app.UseAdaptiveRateLimiting();
        return app;
    }

    /// <summary>
    /// Inserts adaptive token-bucket limiting after telemetry capture middleware.
    /// </summary>
    public static IApplicationBuilder UseAdaptiveRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AdaptiveRateLimitingMiddleware>();
    }
}
