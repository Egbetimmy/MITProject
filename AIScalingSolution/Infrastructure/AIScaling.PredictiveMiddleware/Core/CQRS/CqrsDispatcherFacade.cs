using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

namespace AIScaling.PredictiveMiddleware.Core.CQRS;

/// <summary>
/// Thin wrapper that exposes <see cref="CqrsDispatcher"/> through <see cref="ICqrsDispatcher"/>.
/// </summary>
internal sealed class CqrsDispatcherFacade : ICqrsDispatcher
{
    private readonly CqrsDispatcher _dispatcher;

    public CqrsDispatcherFacade(CqrsDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
    }

    public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand =>
        _dispatcher.SendAsync(command, cancellationToken);

    public Task<TResponse> QueryAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse> =>
        _dispatcher.QueryAsync<TQuery, TResponse>(query, cancellationToken);
}
