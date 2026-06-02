namespace AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

/// <summary>
/// Handles a single command type on the write path.
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Processes <paramref name="command"/> asynchronously.
    /// </summary>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}
