namespace AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

/// <summary>
/// Marker for a read-side CQRS message (query path) that returns <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>
/// Queries are consumed by background analytics—not by per-request middleware—so time-series
/// retrieval and forecasting evaluation stay off the request critical path.
/// </remarks>
public interface IQuery<TResponse>
{
}
