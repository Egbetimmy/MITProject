using AIScaling.PredictiveMiddleware.Core.State;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIScaling.PredictiveMiddleware.Mitigation;

/// <summary>
/// Monitors <see cref="ISystemStateProvider"/> and applies adaptive mitigations on posture transitions.
/// </summary>
public sealed class AdaptiveMitigationCoordinator : BackgroundService
{
    private readonly ISystemStateProvider _stateProvider;
    private readonly IAdaptiveRateLimitPolicyStore _rateLimitPolicyStore;
    private readonly ICachePreWarmingService _cachePreWarmingService;
    private readonly ILogger<AdaptiveMitigationCoordinator> _logger;
    private ProtectivePosture _lastObservedPosture = ProtectivePosture.Nominal;

    public AdaptiveMitigationCoordinator(
        ISystemStateProvider stateProvider,
        IAdaptiveRateLimitPolicyStore rateLimitPolicyStore,
        ICachePreWarmingService cachePreWarmingService,
        ILogger<AdaptiveMitigationCoordinator> logger)
    {
        _stateProvider = stateProvider;
        _rateLimitPolicyStore = rateLimitPolicyStore;
        _cachePreWarmingService = cachePreWarmingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ApplyIfChanged(_stateProvider.GetSnapshot().Posture, force: true);

        while (!stoppingToken.IsCancellationRequested)
        {
            var posture = _stateProvider.GetSnapshot().Posture;
            ApplyIfChanged(posture, force: false);
            await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken).ConfigureAwait(false);
        }
    }

    private void ApplyIfChanged(ProtectivePosture posture, bool force)
    {
        if (!force && posture == _lastObservedPosture)
        {
            return;
        }

        _lastObservedPosture = posture;
        _rateLimitPolicyStore.ApplyPosture(posture);

        _logger.LogInformation("Adaptive mitigation applied for posture {Posture}.", posture);

        if (posture == ProtectivePosture.Alert)
        {
            _ = Task.Run(
                async () =>
                {
                    try
                    {
                        await _cachePreWarmingService
                            .WarmHighFrequencyKeysAsync(CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cache pre-warm failed during Alert transition.");
                    }
                },
                CancellationToken.None);
        }
    }
}
