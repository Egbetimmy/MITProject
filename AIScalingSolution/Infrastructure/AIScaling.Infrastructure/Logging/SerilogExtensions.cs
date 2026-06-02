using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace AIScaling.Infrastructure.Logging;

/// <summary>Serilog configuration extensions for all microservices.</summary>
public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder, string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: $"logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}
