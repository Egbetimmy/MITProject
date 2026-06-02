using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using PredictionService.Application.Interfaces;

namespace PredictionService.Api.Controllers;

/// <summary>ML.NET training and prediction endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PredictionsController : ControllerBase
{
    private readonly IPredictionAppService _predictionService;

    public PredictionsController(IPredictionAppService predictionService) =>
        _predictionService = predictionService;

    /// <summary>Train ML.NET model from historical metrics.</summary>
    [HttpPost("train")]
    public async Task<ActionResult<ApiResponse<string>>> Train(CancellationToken cancellationToken)
    {
        try
        {
            var message = await _predictionService.TrainModelAsync(cancellationToken);
            return Ok(ApiResponse<string>.Ok(message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    /// <summary>Predict future workload and scaling decision.</summary>
    [HttpPost("predict")]
    public async Task<ActionResult<ApiResponse<PredictionOutputDto>>> Predict(
        [FromBody] PredictionInputDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _predictionService.PredictAsync(input, cancellationToken);
            return Ok(ApiResponse<PredictionOutputDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PredictionOutputDto>.Fail(ex.Message));
        }
    }
}
