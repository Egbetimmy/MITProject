using AIScaling.PredictiveMiddleware.Core.CQRS;
using AIScaling.PredictiveMiddleware.Core.Storage;
using AIScaling.PredictiveMiddleware.Diagnostics;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Handlers;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Logging;
using AIScaling.PredictiveMiddleware.Infrastructure.Redis;
using AIScaling.PredictiveMiddleware.Pipeline;
using AIScaling.PredictiveMiddleware.Pipeline.Ingestion;
using AIScaling.PredictiveMiddleware.Pipeline.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace AIScaling.PredictiveMiddleware.Extensions;

/// <summary>
/// Registers the Part 2 command/write pipeline: Redis store, handlers, and middleware.
/// </summary>
public static class PredictiveCommandPipelineExtensions
{
    /// <summary>
    /// Adds Redis telemetry buffering, CQRS command handlers, and supporting services.
    /// </summary>
    /// <param name="services">Application services.</param>
    /// <param name="configuration">Configuration root (reads PredictiveMiddleware sections).</param>
    public static IServiceCollection AddPredictiveCommandPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPredictiveMiddlewareCqrs();

        services
            .AddOptions<RedisTelemetryOptions>()
            .Bind(configuration.GetSection(RedisTelemetryOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                "PredictiveMiddleware:Redis:ConnectionString is required.")
            .ValidateOnStart();

        services
            .AddOptions<PredictiveMiddlewareOptions>()
            .Bind(configuration.GetSection(PredictiveMiddlewareOptions.SectionName));

        services
            .AddOptions<TelemetryIngestionOptions>()
            .Bind(configuration.GetSection("PredictiveMiddleware:Ingestion"));

        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisTelemetryOptions>>().Value;
            var config = ConfigurationOptions.Parse(redisOptions.ConnectionString);
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });

        services.TryAddSingleton<ITelemetryBufferStore, RedisTelemetryBufferStore>();
        services.TryAddSingleton<ITrafficTickLogger, TrafficTickLogger>();
        services.TryAddSingleton<IPredictiveDiagnosticsCollector, PredictiveDiagnosticsCollector>();
        services.AddCommandHandler<RecordTrafficTickCommand, RecordTrafficTickCommandHandler>();

        services.AddSingleton<TelemetryIngestionQueue>();
        services.AddSingleton<ITelemetryIngestionQueue>(sp => sp.GetRequiredService<TelemetryIngestionQueue>());
        services.AddHostedService<TelemetryIngestionBackgroundService>();

        return services;
    }

    /// <summary>
    /// Inserts <see cref="PredictiveTrafficMiddleware"/> into the HTTP pipeline.
    /// Place early (before routing/endpoints) so all requests are observed and gated.
    /// </summary>
    public static IApplicationBuilder UsePredictiveCommandPipeline(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<PredictiveTrafficMiddleware>();
    }
}
