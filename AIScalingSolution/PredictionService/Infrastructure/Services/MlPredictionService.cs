using AIScaling.Shared.Constants;
using AIScaling.Shared.DTOs;
using AIScaling.Shared.Logging;
using AIScaling.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using PredictionService.Application.Interfaces;

namespace PredictionService.Infrastructure.Services;

/// <summary>ML.NET training and prediction pipeline.</summary>
public sealed class MlPredictionService : IPredictionAppService
{
    private readonly IMetricsDataLoader _dataLoader;
    private readonly IScalingLogger _scalingLogger;
    private readonly string _modelPath;
    private readonly MLContext _mlContext = new(seed: 0);
    private ITransformer? _model;
    private PredictionEngine<MetricData, PredictionResult>? _predictionEngine;
    private readonly object _lock = new();

    public bool IsModelReady
    {
        get
        {
            if (_predictionEngine is not null) return true;
            TryLoadModel();
            return _predictionEngine is not null;
        }
    }

    public MlPredictionService(
        IMetricsDataLoader dataLoader,
        IScalingLogger scalingLogger,
        IConfiguration configuration)
    {
        _dataLoader = dataLoader;
        _scalingLogger = scalingLogger;
        _modelPath = configuration["MlNet:ModelPath"] ?? Path.Combine(AppContext.BaseDirectory, "model.zip");
        TryLoadModel();
    }

    public async Task<string> TrainModelAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dataLoader.LoadHistoricalMetricsAsync(cancellationToken);
        if (data.Count < 10)
            throw new InvalidOperationException("Insufficient historical metrics for training (minimum 10 records).");

        var trainData = _mlContext.Data.LoadFromEnumerable(data);
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(MetricData.CpuUsage),
                nameof(MetricData.MemoryUsage),
                nameof(MetricData.RequestCount),
                nameof(MetricData.ResponseTime))
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: nameof(MetricData.PredictedRequestLoad),
                featureColumnName: "Features"));

        _model = pipeline.Fit(trainData);
        _mlContext.Model.Save(_model, trainData.Schema, _modelPath);

        lock (_lock)
        {
            _predictionEngine?.Dispose();
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<MetricData, PredictionResult>(_model);
        }

        return $"Model trained on {data.Count} records and saved to {_modelPath}";
    }

    public Task<PredictionOutputDto> PredictAsync(PredictionInputDto input, CancellationToken cancellationToken = default)
    {
        if (_predictionEngine is null)
            TryLoadModel();

        if (_predictionEngine is null)
            throw new InvalidOperationException("Model is not trained. Call POST /api/predictions/train first.");

        var prediction = _predictionEngine.Predict(new MetricData
        {
            CpuUsage = (float)input.CpuUsage,
            MemoryUsage = (float)input.MemoryUsage,
            RequestCount = input.RequestCount,
            ResponseTime = (float)input.ResponseTime
        });

        var decision = ScaleDecisions.Resolve(prediction.PredictedRequestLoad);
        prediction.ScaleDecision = decision;
        _scalingLogger.LogScalingDecision("PredictionService", decision, prediction.PredictedRequestLoad);

        return Task.FromResult(new PredictionOutputDto
        {
            PredictedRequestLoad = prediction.PredictedRequestLoad,
            ScaleDecision = decision
        });
    }

    private void TryLoadModel()
    {
        if (!File.Exists(_modelPath)) return;

        lock (_lock)
        {
            _model = _mlContext.Model.Load(_modelPath, out _);
            _predictionEngine?.Dispose();
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<MetricData, PredictionResult>(_model);
        }
    }
}
