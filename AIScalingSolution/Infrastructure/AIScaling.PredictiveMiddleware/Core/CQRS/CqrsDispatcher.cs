using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AIScaling.PredictiveMiddleware.Core.CQRS;

/// <summary>
/// Intra-process CQRS dispatcher that resolves handlers from <see cref="IServiceProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// Handlers are resolved per invocation (singleton handlers are still a single instance).
/// This avoids MediatR-style pipeline overhead while keeping registration explicit.
/// </para>
/// <para>
/// <see cref="SendAsync{TCommand}"/> serves the write path; <see cref="QueryAsync{TQuery,TResponse}"/>
/// serves the read path—preserving CQRS separation at the call site.
/// </para>
/// </remarks>
internal sealed class CqrsDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CqrsDispatcher(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches a command to its registered <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// Dispatches a query to its registered <see cref="IQueryHandler{TQuery,TResponse}"/>.
    /// </summary>
    public Task<TResponse> QueryAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return handler.HandleAsync(query, cancellationToken);
    }
}
