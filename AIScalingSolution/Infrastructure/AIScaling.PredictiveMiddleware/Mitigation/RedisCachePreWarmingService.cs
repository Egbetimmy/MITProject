using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Mock pre-warming engine: loads synthetic datasets into Redis ahead of forecasted surges.
/// </summary>
public sealed class RedisCachePreWarmingService : ICachePreWarmingService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly AdaptiveMitigationOptions _options;
    private readonly ILogger<RedisCachePreWarmingService> _logger;
    private int _isWarming;

    public RedisCachePreWarmingService(
        IConnectionMultiplexer multiplexer,
        IOptions<AdaptiveMitigationOptions> options,
        ILogger<RedisCachePreWarmingService> logger)
    {
        _multiplexer = multiplexer;
        _options = options.Value;
        _logger = logger;
    }

    public async Task WarmHighFrequencyKeysAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _isWarming, 1, 0) != 0)
        {
            return;
        }

        try
        {
            var database = _multiplexer.GetDatabase();

            foreach (var key in _options.WarmupCacheKeys)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Mock "database query" latency.
                await Task.Delay(15, cancellationToken).ConfigureAwait(false);

                var payload = $"{{\"key\":\"{key}\",\"warmedAt\":\"{DateTimeOffset.UtcNow:O}\",\"source\":\"mock-db\"}}";
                await database.StringSetAsync(
                        $"prewarm:{key}",
                        payload,
                        TimeSpan.FromMinutes(10))
                    .ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Cache pre-warm complete for {Count} high-frequency keys (Alert posture).",
                _options.WarmupCacheKeys.Count);
        }
        finally
        {
            Interlocked.Exchange(ref _isWarming, 0);
        }
    }
}
