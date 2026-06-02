namespace AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

/// <summary>
/// Marker for a write-side CQRS message (command path).
/// </summary>
/// <remarks>
/// Commands represent mutations on the hot telemetry path and must remain lightweight
/// so the HTTP pipeline never waits on analytics or external stores.
/// </remarks>
public interface ICommand
{
}
