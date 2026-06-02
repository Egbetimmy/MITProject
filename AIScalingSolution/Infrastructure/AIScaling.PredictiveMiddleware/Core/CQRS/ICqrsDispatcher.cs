using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

namespace AIScaling.PredictiveMiddleware.Core.CQRS;

/// <summary>
/// Public façade for dispatching CQRS messages without exposing the internal resolver.
/// </summary>
public interface ICqrsDispatcher
{
    /// <summary>
    /// Dispatches a command on the write path.
    /// </summary>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand;

    /// <summary>
    /// Dispatches a query on the read path.
    /// </summary>
    Task<TResponse> QueryAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>;
}
