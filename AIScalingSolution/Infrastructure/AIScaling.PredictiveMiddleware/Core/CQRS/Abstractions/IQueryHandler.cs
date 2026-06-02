namespace AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

/// <summary>
/// Handles a single query type on the read path.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Executes <paramref name="query"/> and returns the read model.
    /// </summary>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
