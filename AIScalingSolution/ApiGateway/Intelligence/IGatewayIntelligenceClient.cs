using AIScaling.Shared.DTOs;

namespace ApiGateway.Intelligence;

public interface IGatewayIntelligenceClient
{
    Task<PredictionOutputDto?> TryPredictFromLatestMetricsAsync(CancellationToken cancellationToken);
}
