using AIScaling.Shared.Logging;
using Serilog;

namespace AIScaling.Infrastructure.Logging;

/// <summary>Serilog-backed scaling and request logger.</summary>
public sealed class ScalingLogger : IScalingLogger
{
    public void LogScalingDecision(string serviceName, string decision, float predictedLoad) =>
        Log.Information("Scaling decision for {ServiceName}: {Decision} (predicted load: {PredictedLoad})",
            serviceName, decision, predictedLoad);

    public void LogRequest(string method, string path) =>
        Log.Information("HTTP {Method} {Path}", method, path);

    public void LogResponse(string method, string path, int statusCode, double elapsedMs) =>
        Log.Information("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            method, path, statusCode, elapsedMs);

    public void LogException(Exception exception, string context) =>
        Log.Error(exception, "Exception in {Context}", context);
}
