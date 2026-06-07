using System.Net.Http.Json;
using System.Text.Json;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiGateway.Intelligence;

/// <summary>Best-effort HTTP client for MonitoringService metrics and PredictionService inference.</summary>
public sealed class GatewayIntelligenceClient : IGatewayIntelligenceClient
{
    public const string MonitoringClientName = "gateway-monitoring";
    public const string PredictionClientName = "gateway-prediction";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPredictionModelReadinessMonitor _readinessMonitor;
    private readonly GatewayIntelligenceOptions _options;
    private readonly ILogger<GatewayIntelligenceClient> _logger;

    public GatewayIntelligenceClient(
        IHttpClientFactory httpClientFactory,
        IPredictionModelReadinessMonitor readinessMonitor,
        IOptions<GatewayIntelligenceOptions> options,
        ILogger<GatewayIntelligenceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _readinessMonitor = readinessMonitor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PredictionOutputDto?> TryPredictFromLatestMetricsAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (!_readinessMonitor.IsModelReady)
        {
            _logger.LogDebug("Skipping prediction — model not ready.");
            return null;
        }

        try
        {
            var input = await BuildPredictionInputAsync(cancellationToken).ConfigureAwait(false);
            if (input is null)
            {
                return null;
            }

            var predictionClient = _httpClientFactory.CreateClient(PredictionClientName);
            using var response = await predictionClient
                .PostAsJsonAsync("api/predictions/predict", input, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("PredictionService returned HTTP {StatusCode}.", (int)response.StatusCode);
                return null;
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ApiResponse<PredictionOutputDto>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            return envelope?.Data;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Gateway intelligence prediction failed (best-effort).");
            return null;
        }
    }

    private async Task<PredictionInputDto?> BuildPredictionInputAsync(CancellationToken cancellationToken)
    {
        var monitoringClient = _httpClientFactory.CreateClient(MonitoringClientName);
        using var response = await monitoringClient
            .GetAsync("api/metrics", cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("MonitoringService returned HTTP {StatusCode}.", (int)response.StatusCode);
            return null;
        }

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<IReadOnlyList<ResourceMetricDto>>>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        var metrics = envelope?.Data;
        if (metrics is null || metrics.Count == 0)
        {
            return null;
        }

        var latest = metrics
            .OrderByDescending(m => m.Timestamp)
            .Take(10)
            .ToList();

        return new PredictionInputDto
        {
            CpuUsage = latest.Average(m => m.CpuUsage),
            MemoryUsage = latest.Average(m => m.MemoryUsage),
            RequestCount = (int)latest.Average(m => m.RequestCount),
            ResponseTime = latest.Average(m => m.ResponseTime)
        };
    }
}
