namespace AIScaling.PredictiveMiddleware.Core.State;

/// <summary>
/// System protective posture used to communicate scaling urgency to operators and policies.
/// </summary>
public enum ProtectivePosture
{
    /// <summary>Traffic and forecasts are within expected bounds.</summary>
    Nominal = 0,

    /// <summary>Elevated risk; proactive scaling should be evaluated.</summary>
    Alert = 1,

    /// <summary>Imminent overload; immediate protective action is warranted.</summary>
    Critical = 2,
}
