using AIScaling.PredictiveMiddleware.Core.CQRS.Abstractions;

namespace AIScaling.PredictiveMiddleware.Core.Telemetry.Commands;

/// <summary>
/// Write-path command: records one HTTP request tick into the sliding telemetry window.
/// </summary>
/// <remarks>
/// Dispatched from middleware on the hot path. Handlers must complete quickly—typically a
/// single in-memory or Redis increment—so ingress never blocks on forecasting.
/// </remarks>
/// <param name="EndpointRoute">Normalized route template or path.</param>
/// <param name="Timestamp">Observation time (UTC recommended).</param>
/// <param name="ClientIdentifier">Optional client or correlation identifier.</param>
public sealed record RecordTrafficTickCommand(
    string EndpointRoute,
    DateTimeOffset Timestamp,
    string? ClientIdentifier) : ICommand;
