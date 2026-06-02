using AIScaling.PredictiveMiddleware.Core.State;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Diagnostics;

/// <summary>
/// Prints predictive middleware status to the terminal every 2 seconds during stress runs.
/// </summary>
public sealed class PredictiveDiagnosticsConsoleHostedService : BackgroundService
{
    private readonly ISystemStateProvider _stateProvider;
    private readonly IPredictiveDiagnosticsCollector _diagnostics;
    private readonly ILogger<PredictiveDiagnosticsConsoleHostedService> _logger;
    private readonly TimeSpan _interval;

    public PredictiveDiagnosticsConsoleHostedService(
        ISystemStateProvider stateProvider,
        IPredictiveDiagnosticsCollector diagnostics,
        IOptions<PredictiveDiagnosticsOptions> options,
        ILogger<PredictiveDiagnosticsConsoleHostedService> logger)
    {
        _stateProvider = stateProvider;
        _diagnostics = diagnostics;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.IntervalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshot = _stateProvider.GetSnapshot();
            var metrics = _diagnostics.GetSnapshot();

            _logger.LogInformation(
                "[{Timestamp:O}] Posture: {Posture} | Current RPS: {CurrentRps:F0} | Forecasted 60s RPS: {ForecastRps:F0} | Throttled Requests: {Throttled} | P99 Internal Overhead: {P99:F2}ms",
                DateTimeOffset.UtcNow,
                snapshot.Posture,
                metrics.CurrentRequestsPerSecond,
                metrics.ForecastedRequestsPerSecond,
                metrics.ThrottledRequests,
                metrics.P99MiddlewareOverheadMs);

            await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
        }
    }
}

/// <summary>Diagnostics console cadence.</summary>
public sealed class PredictiveDiagnosticsOptions
{
    public const string SectionName = "PredictiveMiddleware:Diagnostics";

    public int IntervalSeconds { get; set; } = 2;
}
