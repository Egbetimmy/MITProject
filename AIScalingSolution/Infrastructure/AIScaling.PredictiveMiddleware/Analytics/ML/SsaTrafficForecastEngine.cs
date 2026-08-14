using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace AIScaling.PredictiveMiddleware.Analytics.ML;

/// <summary>
/// In-process SSA model holder. Caches <see cref="TimeSeriesPredictionEngine{TSrc,TDst}"/> across evaluation cycles.
/// </summary>
/// <remarks>
/// <para>
/// Singular Spectrum Analysis decomposes the in-memory Redis sliding window into trend, seasonal, and noise
/// components without reading historical log files from disk—ideal for short-horizon burst forecasting during
/// JMeter/k6 runs and live production traffic.
/// </para>
/// <para>
/// The engine is refit periodically (<see cref="PredictiveEngineOptions.RefitEveryNCycles"/>), not on every
/// one-second tick, to keep CPU on background threads and off the Kestrel pool.
/// </para>
/// <para>
/// <b>Validation Status (Hypothesis 1):</b>
/// Hypothesis 1 proposed that a non-parametric Singular Spectrum Analysis (SSA) forecasting model would generate 
/// short-horizon traffic projections with a Mean Absolute Percentage Error (MAPE) below 10%, translating to improved 
/// p99 latency stability and SLA attainment under burst conditions.
/// The SSA forecasting engine was integrated into the framework's analytics layer and demonstrated to operate correctly 
/// during live load testing, detecting traffic surges and triggering posture transitions within one second.
/// However, formal offline validation of the model's forecast accuracy against the specific MAPE &lt; 10% threshold 
/// was not completed within the scope of this project's testing phase. No systematic comparison of predicted versus 
/// realised request-rate values was logged during evaluation runs. Consequently, H1 is considered partially validated: 
/// the underlying forecasting mechanism was implemented and shown to function correctly in an operational sense, 
/// but the specific quantitative accuracy criterion has not been empirically confirmed.
/// </para>
/// </remarks>
public sealed class SsaTrafficForecastEngine : IDisposable
{
    private readonly MLContext _mlContext = new(seed: 0);
    private readonly PredictiveEngineOptions _options;
    private readonly ILogger<SsaTrafficForecastEngine> _logger;
    private readonly object _sync = new();

    private TimeSeriesPredictionEngine<TrafficData, TrafficForecast>? _engine;
    private int _cycleCount;

    // Online accuracy tracking buffers
    private readonly Queue<(int cycle, float forecast)> _pendingForecasts = new();
    private readonly List<float> _absolutePercentageErrors = new();
    private readonly List<float> _squaredErrors = new();

    public SsaTrafficForecastEngine(
        IOptions<PredictiveEngineOptions> options,
        ILogger<SsaTrafficForecastEngine> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Produces a lookahead vector from normalized bucket counts (zeros preserved for empty slots).
    /// </summary>
    public bool TryForecast(IReadOnlyList<float> normalizedSeries, out float[]? forecast)
    {
        forecast = null;

        if (normalizedSeries.Count < _options.SeriesLength)
        {
            return false;
        }

        lock (_sync)
        {
            var latestObservation = normalizedSeries[^1];

            if (_options.DisableForecasting)
            {
                forecast = Enumerable.Repeat(latestObservation, _options.Horizon).ToArray();
                _cycleCount++;
                return true;
            }

            var requiresRefit = _engine is null ||
                                _cycleCount == 0 ||
                                (_options.RefitEveryNCycles > 0 &&
                                 _cycleCount % _options.RefitEveryNCycles == 0);

            if (requiresRefit)
            {
                if (!TryRefitEngine(normalizedSeries))
                {
                    return false;
                }
            }

            // Evaluate previous lookahead predictions against the actual observed load
            while (_pendingForecasts.Count > 0 && _pendingForecasts.Peek().cycle <= _cycleCount)
            {
                var past = _pendingForecasts.Dequeue();
                if (past.cycle == _cycleCount)
                {
                    var actualVal = latestObservation;
                    var absErr = Math.Abs(actualVal - past.forecast);
                    var ape = actualVal > 0.1f ? (absErr / actualVal) * 100f : 0f;
                    var se = absErr * absErr;

                    _absolutePercentageErrors.Add(ape);
                    _squaredErrors.Add(se);

                    // Maintain a sliding window of the last 120 cycles
                    if (_absolutePercentageErrors.Count > 120)
                    {
                        _absolutePercentageErrors.RemoveAt(0);
                        _squaredErrors.RemoveAt(0);
                    }

                    var mape = _absolutePercentageErrors.Average();
                    var rmse = Math.Sqrt(_squaredErrors.Average());

                    _logger.LogInformation(
                        "Online accuracy telemetry: n={Count}, running_MAPE={Mape:F2}%, running_RMSE={Rmse:F2} RPS",
                        _absolutePercentageErrors.Count,
                        mape,
                        rmse);
                }
            }

            var prediction = _engine!.Predict(new TrafficData { Count = latestObservation });
            forecast = prediction.Forecast?
                .Take(_options.Horizon)
                .ToArray();

            // Schedule the lookahead prediction to be evaluated after the horizon duration
            if (forecast is { Length: > 0 } && forecast.Length >= _options.Horizon)
            {
                var targetCycle = _cycleCount + _options.Horizon;
                var forecastedValue = forecast[_options.Horizon - 1];
                _pendingForecasts.Enqueue((targetCycle, forecastedValue));
            }

            _cycleCount++;
            return forecast is { Length: > 0 };
        }
    }

    private bool TryRefitEngine(IReadOnlyList<float> normalizedSeries)
    {
        try
        {
            var trainCount = Math.Min(normalizedSeries.Count, _options.TrainSize);
            var trainingSlice = normalizedSeries
                .Skip(normalizedSeries.Count - trainCount)
                .Select(v => new TrafficData { Count = v })
                .ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingSlice);

            // SSA: spectral decomposition + recurrent forecasting on the sliding Redis window only.
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(TrafficForecast.Forecast),
                inputColumnName: nameof(TrafficData.Count),
                windowSize: _options.WindowSize,
                seriesLength: _options.SeriesLength,
                trainSize: _options.TrainSize,
                horizon: _options.Horizon);

            _engine?.Dispose();
            var model = pipeline.Fit(dataView);
            _engine = model.CreateTimeSeriesEngine<TrafficData, TrafficForecast>(_mlContext);

            _logger.LogDebug(
                "SSA engine refit complete (window={Window}, series={Series}, horizon={Horizon}).",
                _options.WindowSize,
                _options.SeriesLength,
                _options.Horizon);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSA engine refit failed; forecast skipped this cycle.");
            _engine?.Dispose();
            _engine = null;
            return false;
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}
