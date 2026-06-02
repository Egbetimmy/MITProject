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
/// </remarks>
public sealed class SsaTrafficForecastEngine : IDisposable
{
    private readonly MLContext _mlContext = new(seed: 0);
    private readonly PredictiveEngineOptions _options;
    private readonly ILogger<SsaTrafficForecastEngine> _logger;
    private readonly object _sync = new();

    private TimeSeriesPredictionEngine<TrafficData, TrafficForecast>? _engine;
    private int _cycleCount;

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

            var latestObservation = normalizedSeries[^1];
            var prediction = _engine!.Predict(new TrafficData { Count = latestObservation });
            forecast = prediction.Forecast?
                .Take(_options.Horizon)
                .ToArray();

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
