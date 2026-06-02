using AIScaling.PredictiveMiddleware.Core.State;

namespace AIScaling.PredictiveMiddleware.Analytics;

/// <summary>
/// Maps SSA lookahead vectors to <see cref="ProtectivePosture"/> decisions.
/// </summary>
internal static class ForecastPostureEvaluator
{
    public static ProtectivePosture Evaluate(
        float[] forecast,
        PredictiveEngineOptions options,
        out double peak,
        out double acceleration)
    {
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(options);

        var effectiveLength = Math.Min(forecast.Length, options.Horizon);
        if (effectiveLength == 0)
        {
            peak = 0;
            acceleration = 0;
            return ProtectivePosture.Nominal;
        }

        peak = forecast.Take(effectiveLength).Max();
        acceleration = forecast[effectiveLength - 1] - forecast[0];

        var baseline = Math.Max(1f, options.BaselineRequestsPerBucket);
        var criticalLine = baseline * options.CriticalPeakMultiplier;
        var alertLine = baseline * options.AlertPeakMultiplier;

        // Critical: impending starvation inside the 60–300s reactive scale-lag window.
        if (peak >= criticalLine)
        {
            return ProtectivePosture.Critical;
        }

        // Alert: surge acceleration or elevated peak below critical.
        if (peak >= alertLine || acceleration >= options.AccelerationThreshold)
        {
            return ProtectivePosture.Alert;
        }

        return ProtectivePosture.Nominal;
    }
}
