namespace ApiGateway.Intelligence;

/// <summary>Gateway-side monitoring + batch ML prediction integration (best-effort, non-blocking).</summary>
public sealed class GatewayIntelligenceOptions
{
    public const string SectionName = "GatewayIntelligence";

    public bool Enabled { get; set; } = true;

    public string MonitoringBaseUrl { get; set; } = "http://monitoringservice:8080";

    public string PredictionBaseUrl { get; set; } = "http://predictionservice:8080";

    /// <summary>Fraction of requests that trigger monitoring/prediction (0.0-1.0).</summary>
    public double SamplingRate { get; set; } = 0.1;

    /// <summary>Per-request timeout budget after the downstream handler completes.</summary>
    public int RequestTimeoutSeconds { get; set; } = 2;

    /// <summary>How often to refresh /health/model from PredictionService.</summary>
    public int ModelReadinessCacheSeconds { get; set; } = 30;

    public int RetryCount { get; set; } = 2;

    public string ScaleDecisionHeaderName { get; set; } = "X-Scale-Decision";

    public string PredictedLoadHeaderName { get; set; } = "X-Predicted-Load";
}
