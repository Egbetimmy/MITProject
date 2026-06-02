using AIScaling.PredictiveMiddleware.Core.State;
using Microsoft.Extensions.Options;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Thread-safe snapshot of rate limits — safe for concurrent reads from Kestrel threads.
/// </summary>
public sealed class AdaptiveRateLimitPolicyStore : IAdaptiveRateLimitPolicyStore
{
    private readonly AdaptiveMitigationOptions _options;
    private readonly object _sync = new();
    private PostureRateLimitProfile _profile;
    private ProtectivePosture _posture;

    public AdaptiveRateLimitPolicyStore(IOptions<AdaptiveMitigationOptions> options)
    {
        _options = options.Value;
        _posture = ProtectivePosture.Nominal;
        _profile = PostureRateLimitProfile.FromOptions(_posture, _options);
    }

    public PostureRateLimitProfile CurrentProfile
    {
        get
        {
            lock (_sync)
            {
                return _profile;
            }
        }
    }

    public ProtectivePosture CurrentPosture
    {
        get
        {
            lock (_sync)
            {
                return _posture;
            }
        }
    }

    public void ApplyPosture(ProtectivePosture posture)
    {
        var profile = PostureRateLimitProfile.FromOptions(posture, _options);
        lock (_sync)
        {
            _posture = posture;
            _profile = profile;
        }
    }
}
