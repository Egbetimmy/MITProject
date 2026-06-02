namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Mock cache pre-warming: simulates DB reads and injects results into Redis before traffic spikes.
/// </summary>
public interface ICachePreWarmingService
{
    Task WarmHighFrequencyKeysAsync(CancellationToken cancellationToken);
}
