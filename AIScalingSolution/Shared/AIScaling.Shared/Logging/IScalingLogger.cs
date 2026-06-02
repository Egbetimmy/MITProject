namespace AIScaling.Shared.Logging;

/// <summary>Abstraction for scaling-related operational logging.</summary>
public interface IScalingLogger
{
    void LogScalingDecision(string serviceName, string decision, float predictedLoad);
    void LogRequest(string method, string path);
    void LogResponse(string method, string path, int statusCode, double elapsedMs);
    void LogException(Exception exception, string context);
}
