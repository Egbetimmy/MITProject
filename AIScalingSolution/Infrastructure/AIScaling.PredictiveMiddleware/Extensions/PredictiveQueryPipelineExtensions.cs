using AIScaling.PredictiveMiddleware.Analytics;
using AIScaling.PredictiveMiddleware.Analytics.ML;
using AIScaling.PredictiveMiddleware.Core.CQRS;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Handlers;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIScaling.PredictiveMiddleware.Extensions;

/// <summary>
/// Registers Part 3 query/read pipeline: time-series handler and SSA background worker.
/// </summary>
public static class PredictiveQueryPipelineExtensions
{
    /// <summary>
    /// Adds CQRS query handlers, ML.NET SSA engine, and <see cref="PredictiveEngineHostedService"/>.
    /// </summary>
    /// <remarks>Call <see cref="PredictiveCommandPipelineExtensions.AddPredictiveCommandPipeline"/> for the write path.</remarks>
    public static IServiceCollection AddPredictiveQueryPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPredictiveMiddlewareCqrs();

        services
            .AddOptions<PredictiveEngineOptions>()
            .Bind(configuration.GetSection(PredictiveEngineOptions.SectionName))
            .Validate(
                o => o.Horizon is >= 1 and <= 300,
                "PredictiveMiddleware:Engine:Horizon must be between 1 and 300.")
            .Validate(
                o => o.SeriesLength >= o.WindowSize,
                "SeriesLength must be >= WindowSize for SSA.")
            .ValidateOnStart();

        services.TryAddSingleton<SsaTrafficForecastEngine>();
        services.AddQueryHandler<GetTrafficTimeSeriesQuery, TrafficTimeSeriesResult, GetTrafficTimeSeriesQueryHandler>();
        services.AddHostedService<PredictiveEngineHostedService>();

        return services;
    }

    /// <summary>
    /// Registers Part 2 (write) and Part 3 (read/forecast) pipelines together.
    /// </summary>
    public static IServiceCollection AddPredictiveMiddleware(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPredictiveCommandPipeline(configuration);
        services.AddPredictiveQueryPipeline(configuration);
        return services;
    }
}
