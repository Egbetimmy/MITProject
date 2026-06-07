using AIScaling.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace ApiGateway.Intelligence;

/// <summary>
/// Samples requests, runs monitoring + prediction off the hot path (parallel with downstream),
/// and attaches scale-decision headers when the response has not started.
/// </summary>
public sealed class GatewayPredictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IGatewayIntelligenceClient _intelligenceClient;
    private readonly GatewayIntelligenceOptions _options;
    private readonly ILogger<GatewayPredictionMiddleware> _logger;

    public GatewayPredictionMiddleware(
        RequestDelegate next,
        IGatewayIntelligenceClient intelligenceClient,
        IOptions<GatewayIntelligenceOptions> options,
        ILogger<GatewayPredictionMiddleware> logger)
    {
        _next = next;
        _intelligenceClient = intelligenceClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled || !ShouldSample())
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(Math.Max(1, _options.RequestTimeoutSeconds)));

        var predictionTask = _intelligenceClient.TryPredictFromLatestMetricsAsync(timeoutCts.Token);

        await _next(context).ConfigureAwait(false);

        try
        {
            var prediction = await predictionTask.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
            TryApplyHeaders(context, prediction);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Gateway intelligence timed out for {Path}.", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Gateway intelligence failed for {Path} (best-effort).", context.Request.Path);
        }
    }

    private bool ShouldSample() =>
        _options.SamplingRate >= 1.0 || Random.Shared.NextDouble() < _options.SamplingRate;

    private void TryApplyHeaders(HttpContext context, PredictionOutputDto? prediction)
    {
        if (prediction is null)
        {
            return;
        }

        if (context.Response.HasStarted)
        {
            _logger.LogDebug(
                "Response already started for {Path}; skipping scale-decision headers.",
                context.Request.Path);
            return;
        }

        context.Response.Headers[_options.ScaleDecisionHeaderName] = prediction.ScaleDecision;
        context.Response.Headers[_options.PredictedLoadHeaderName] =
            prediction.PredictedRequestLoad.ToString("F2");
    }
}
