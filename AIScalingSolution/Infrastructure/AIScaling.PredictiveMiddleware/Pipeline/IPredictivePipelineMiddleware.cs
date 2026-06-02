namespace AIScaling.PredictiveMiddleware.Pipeline;

/// <summary>
/// Contract for a composable stage in the predictive HTTP middleware pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The write path (command side) should perform only O(1) work—typically dispatching a
/// <see cref="Core.Telemetry.Commands.RecordTrafficTickCommand"/>—and must not block on
/// forecasting, Redis I/O, or ML evaluation. Those concerns belong on the read/query path
/// executed by background workers.
/// </para>
/// <para>
/// CQRS isolation keeps request threads from awaiting analytics: commands mutate the hot
/// telemetry buffer; queries read time-series slices on a separate schedule.
/// </para>
/// </remarks>
public interface IPredictivePipelineMiddleware
{
    /// <summary>
    /// Executes this pipeline stage and optionally invokes <paramref name="next"/>.
    /// </summary>
    ValueTask InvokeAsync(
        HttpContext context,
        PredictivePipelineDelegate next,
        CancellationToken cancellationToken);
}
