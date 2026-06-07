using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiGateway.Intelligence;

/// <summary>Polls PredictionService /health/model so the gateway skips predict calls when the model is unavailable.</summary>
public sealed class PredictionModelReadinessMonitor : BackgroundService, IPredictionModelReadinessMonitor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GatewayIntelligenceOptions _options;
    private readonly ILogger<PredictionModelReadinessMonitor> _logger;
    private volatile bool _isModelReady;
    private DateTimeOffset? _lastCheckedUtc;

    public PredictionModelReadinessMonitor(
        IHttpClientFactory httpClientFactory,
        IOptions<GatewayIntelligenceOptions> options,
        ILogger<PredictionModelReadinessMonitor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsModelReady => _isModelReady;

    public DateTimeOffset? LastCheckedUtc => _lastCheckedUtc;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.ModelReadinessCacheSeconds));
        await RefreshAsync(stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
            await RefreshAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(GatewayIntelligenceClient.PredictionClientName);
            using var response = await client.GetAsync("health/model", cancellationToken).ConfigureAwait(false);
            _isModelReady = response.IsSuccessStatusCode;
            _lastCheckedUtc = DateTimeOffset.UtcNow;

            if (!_isModelReady)
            {
                _logger.LogDebug("Prediction model not ready (HTTP {StatusCode}).", (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _isModelReady = false;
            _lastCheckedUtc = DateTimeOffset.UtcNow;
            _logger.LogDebug(ex, "Prediction model readiness check failed.");
        }
    }
}
