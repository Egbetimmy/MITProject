using System.Collections.Concurrent;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Per-client token bucket with hot-swappable limits when posture changes.
/// </summary>
public sealed class AdaptiveClientRateLimiter
{
    private readonly IAdaptiveRateLimitPolicyStore _policyStore;
    private readonly ConcurrentDictionary<string, ClientBucketState> _buckets = new();

    public AdaptiveClientRateLimiter(IAdaptiveRateLimitPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    public bool TryAcquire(string clientKey, bool isAuthenticated, out TimeSpan retryAfter)
    {
        var profile = _policyStore.CurrentProfile;
        var limitPerMinute = isAuthenticated
            ? profile.AuthenticatedRequestsPerMinute
            : profile.UnauthenticatedRequestsPerMinute;

        var bucket = _buckets.GetOrAdd(clientKey, static _ => new ClientBucketState());
        return bucket.TryConsume(limitPerMinute, out retryAfter);
    }

    private sealed class ClientBucketState
    {
        private readonly object _sync = new();
        private double _tokens = -1d;
        private long _lastRefillTicks = Environment.TickCount64;

        public bool TryConsume(int limitPerMinute, out TimeSpan retryAfter)
        {
            lock (_sync)
            {
                if (_tokens < 0d)
                {
                    _tokens = limitPerMinute;
                }

                Refill(limitPerMinute);

                if (_tokens >= 1d)
                {
                    _tokens -= 1d;
                    retryAfter = TimeSpan.Zero;
                    return true;
                }

                var deficit = 1d - _tokens;
                var secondsUntilToken = deficit / (limitPerMinute / 60d);
                retryAfter = TimeSpan.FromSeconds(Math.Max(1d, secondsUntilToken));
                return false;
            }
        }

        private void Refill(int limitPerMinute)
        {
            var now = Environment.TickCount64;
            var elapsedMs = now - _lastRefillTicks;
            if (elapsedMs <= 0)
            {
                return;
            }

            _lastRefillTicks = now;
            var tokensToAdd = limitPerMinute * (elapsedMs / 60_000d);
            _tokens = Math.Min(limitPerMinute, _tokens + tokensToAdd);
        }
    }
}
