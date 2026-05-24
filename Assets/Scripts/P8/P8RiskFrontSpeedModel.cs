using UnityEngine;

public static class P8RiskFrontSpeedModel
{
    public const bool VisualHeightIsCinematicOnly = true;
    public const bool UsesVisualHeightAsPhysicalDepth = false;
    public const bool UsesMaxTsunamiHeightAsDepth = false;

    public static float CalculateFallbackSpeed(
        P8RiskFrontProgressionConfig config,
        float inundationDepthMeters,
        out bool shallowSlowdownApplied)
    {
        config = config ?? P8RiskFrontProgressionConfig.Default();
        shallowSlowdownApplied = false;

        float baseSpeed = Mathf.Max(0.001f, config.baseOnshoreSpeedMetersPerSecond);
        float selectedSpeed = baseSpeed;
        float depth = Mathf.Max(0f, inundationDepthMeters);

        if (config.shallowDepthSlowdownEnabled &&
            config.shallowDepthThresholdMeters > 0f &&
            depth < config.shallowDepthThresholdMeters)
        {
            float shallow01 = Mathf.Clamp01(depth / config.shallowDepthThresholdMeters);
            float minFactor = Mathf.Clamp(config.depthSpeedFactor, 0.05f, 1f);
            float speedFactor = Mathf.Lerp(minFactor, 1f, shallow01);
            selectedSpeed *= speedFactor;
            shallowSlowdownApplied = true;
        }

        float minSpeed = Mathf.Max(0.001f, config.minOnshoreSpeedMetersPerSecond);
        float maxSpeed = Mathf.Max(minSpeed, config.maxOnshoreSpeedMetersPerSecond);
        return Mathf.Clamp(selectedSpeed, minSpeed, maxSpeed);
    }

    public static float CalculateVisualRiskStrength(float inundationDepthMeters, float hazardIntensity)
    {
        float depthStrength = Mathf.Clamp01(Mathf.Max(0f, inundationDepthMeters) / 2.5f);
        float intensity = Mathf.Clamp01(hazardIntensity);
        return Mathf.Clamp01(Mathf.Max(depthStrength, intensity));
    }

    public static string CalculateWarningLevel(float visualRiskStrength01)
    {
        float value = Mathf.Clamp01(visualRiskStrength01);
        if (value >= 0.8f)
        {
            return "high";
        }

        if (value >= 0.45f)
        {
            return "warning";
        }

        if (value >= 0.15f)
        {
            return "watch";
        }

        return "safe";
    }
}
