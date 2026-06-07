namespace ApiGateway.Intelligence;

/// <summary>Cached view of PredictionService /health/model.</summary>
public interface IPredictionModelReadinessMonitor
{
    bool IsModelReady { get; }

    DateTimeOffset? LastCheckedUtc { get; }
}
