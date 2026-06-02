using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AIScaling.Infrastructure.Telemetry;

/// <summary>OpenTelemetry distributed tracing configuration.</summary>
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddAppOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        string? otlpEndpoint = null)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
