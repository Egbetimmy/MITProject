using AIScaling.PredictiveMiddleware.Analytics.ML;
using AIScaling.PredictiveMiddleware.Core.CQRS;
using AIScaling.PredictiveMiddleware.Core.State;
using AIScaling.PredictiveMiddleware.Core.Telemetry.Queries;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Analytics;

/// <summary>
/// Background worker: queries Redis via CQRS, runs in-process ML.NET SSA, updates <see cref="ISystemStateProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Hosted services run off the Kestrel thread pool, so matrix operations inside SSA never block HTTP
/// request processing—even under concurrent JMeter/k6 burst scripts.
/// </para>
/// <para>
/// SSA operates purely on the live Redis sliding window; no CSV/log files are required. Each second (configurable)
/// the worker pulls the latest bucket vector, projects 1–5 minutes forward, and adjusts protective posture
/// ahead of reactive scale lag.
/// </para>
/// </remarks>
public sealed class PredictiveEngineHostedService : BackgroundService
{
    private readonly ICqrsDispatcher _dispatcher;
    private readonly ISystemStateProvider _stateProvider;
    private readonly SsaTrafficForecastEngine _forecastEngine;
    private readonly PredictiveEngineOptions _options;
    private readonly ILogger<PredictiveEngineHostedService> _logger;

    public PredictiveEngineHostedService(
        ICqrsDispatcher dispatcher,
        ISystemStateProvider stateProvider,
        SsaTrafficForecastEngine forecastEngine,
        IOptions<PredictiveEngineOptions> options,
        ILogger<PredictiveEngineHostedService> logger)
    {
        _dispatcher = dispatcher;
        _stateProvider = stateProvider;
        _forecastEngine = forecastEngine;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(Math.Max(250, _options.EvaluationIntervalMilliseconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateForecastAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Predictive engine evaluation cycle failed.");
            }

            await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task EvaluateForecastAsync(CancellationToken cancellationToken)
    {
        var query = new GetTrafficTimeSeriesQuery(
            _options.WindowSizeSeconds,
            _options.BucketIntervalSeconds);

        var seriesResult = await _dispatcher
            .QueryAsync<GetTrafficTimeSeriesQuery, TrafficTimeSeriesResult>(query, cancellationToken)
            .ConfigureAwait(false);

        var normalized = seriesResult.RequestVolumes
            .Select(v => (float)Math.Max(0, v))
            .ToArray();

        if (!_forecastEngine.TryForecast(normalized, out var forecast) || forecast is null)
        {
            _logger.LogDebug(
                "Insufficient telemetry for SSA (have {Count}, need {Required}).",
                normalized.Length,
                _options.SeriesLength);
            return;
        }

        var posture = ForecastPostureEvaluator.Evaluate(
            forecast,
            _options,
            out var peak,
            out var acceleration);

        var evaluatedAt = DateTimeOffset.UtcNow;
        var lookaheadScores = forecast
            .Select(f => (double)f)
            .ToArray();

        _stateProvider.SetLookaheadMetrics(lookaheadScores, evaluatedAt);
        _stateProvider.SetPosture(posture, evaluatedAt);

        _logger.LogInformation(
            "SSA forecast evaluated: posture={Posture}, peak={Peak:F1}, acceleration={Acceleration:F1}, horizon={Horizon}.",
            posture,
            peak,
            acceleration,
            _options.Horizon);
    }
}
