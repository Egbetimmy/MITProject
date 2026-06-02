using AIScaling.Shared.DTOs;

namespace PredictionService.Application.Interfaces;

public interface IPredictionAppService
{
    Task<string> TrainModelAsync(CancellationToken cancellationToken = default);
    Task<PredictionOutputDto> PredictAsync(PredictionInputDto input, CancellationToken cancellationToken = default);
}
