using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;
using AIScaling.PredictiveMiddleware.Core.State;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIScaling.PredictiveMiddleware.Core.CQRS;

/// <summary>
/// Registration blueprint for intra-process CQRS handlers and the internal dispatcher.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CQRS dispatcher and default system state provider.
    /// </summary>
    /// <remarks>
    /// Register command and query handlers before the host starts, e.g.:
    /// <c>services.AddCommandHandler&lt;RecordTrafficTickCommand, RecordTrafficTickCommandHandler&gt;();</c>
    /// </remarks>
    public static IServiceCollection AddPredictiveMiddlewareCqrs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<CqrsDispatcher>();
        services.TryAddSingleton<ICqrsDispatcher, CqrsDispatcherFacade>();
        services.TryAddSingleton<ISystemStateProvider, SystemStateProvider>();

        return services;
    }

    /// <summary>
    /// Registers a singleton command handler for the write path.
    /// </summary>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ICommandHandler<TCommand>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a singleton query handler for the read path.
    /// </summary>
    public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IQueryHandler<TQuery, TResponse>, THandler>();
        return services;
    }
}
