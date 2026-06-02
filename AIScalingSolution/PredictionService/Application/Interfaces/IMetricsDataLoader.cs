using AIScaling.Shared.Models;

namespace PredictionService.Application.Interfaces;

public interface IMetricsDataLoader
{
    Task<IReadOnlyList<MetricData>> LoadHistoricalMetricsAsync(CancellationToken cancellationToken = default);
}
