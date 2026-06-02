namespace AIScaling.Shared.Constants;

/// <summary>Auto-scaling decision constants.</summary>
public static class ScaleDecisions
{
    public const string ScaleUp = "ScaleUp";
    public const string ScaleDown = "ScaleDown";
    public const string Maintain = "Maintain";

    public const float ScaleUpThreshold = 1000f;
    public const float ScaleDownThreshold = 200f;

    public static string Resolve(float predictedLoad) =>
        predictedLoad > ScaleUpThreshold ? ScaleUp :
        predictedLoad < ScaleDownThreshold ? ScaleDown : Maintain;
}
