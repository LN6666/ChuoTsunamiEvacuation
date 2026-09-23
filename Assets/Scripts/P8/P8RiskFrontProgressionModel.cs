using System;
using System.IO;
using UnityEngine;

[Serializable]
public class P8RiskFrontProgressionConfig
{
    public string configId = "p8e_risk_front_progression_model_v1";
    public float baseOnshoreSpeedMetersPerSecond = 2.2f;
    public float minOnshoreSpeedMetersPerSecond = 0.4f;
    public float maxOnshoreSpeedMetersPerSecond = 6f;
    public bool shallowDepthSlowdownEnabled = true;
    public float shallowDepthThresholdMeters = 0.3f;
    public float depthSpeedFactor = 0.45f;
    public bool arrivalTimeOverridePriority = true;
    public bool useArrivalTimeWhenAvailable = true;
    public bool useDerivedBoundaryWhenOfficialContourMissing = true;
    public bool visualHeightIsCinematicOnlyRequired = true;
    public bool affectsGameplaySuccessFailure = false;
    public bool implementsP9Gameplay = false;
    public string notes = string.Empty;

    public static P8RiskFrontProgressionConfig Default()
    {
        return new P8RiskFrontProgressionConfig();
    }

    public static P8RiskFrontProgressionConfig FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Default();
        }

        P8RiskFrontProgressionConfig config = JsonUtility.FromJson<P8RiskFrontProgressionConfig>(json);
        return config ?? Default();
    }

    public static P8RiskFrontProgressionConfig LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Default();
        }

        return FromJson(File.ReadAllText(path));
    }
}

[Serializable]
public class P8RiskFrontProgressionInput
{
    public float simulationTimeSeconds;
    public float timeOriginSeconds;
    public float arrivalTimeSeconds;
    public bool hasArrivalTime;
    public float fallbackDistanceMeters = 100f;
    public float inundationDepthMeters;
    public float hazardIntensity;
    public float visualHeightMeters;
    public float maxTsunamiHeightMeters;
    public bool officialContourAvailable;
    public bool derivedBoundaryAvailable = true;
}

[Serializable]
public class P8RiskFrontProgressionResult
{
    public bool success;
    public bool failSafe;
    public bool usedArrivalTime;
    public bool usedFallbackSpeed;
    public bool shallowSlowdownApplied;
    public bool usesVisualHeightAsPhysicalDepth;
    public bool usesMaxTsunamiHeightAsDepth;
    public bool affectsGameplaySuccessFailure;
    public bool implementsP9Gameplay;
    public bool usesDerivedBoundary;
    public float progress01;
    public float selectedSpeedMetersPerSecond;
    public float visualRiskStrength01;
    public string warningLevel = "safe";
    public string summary = string.Empty;
}

public static class P8RiskFrontProgressionModel
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool ImplementsP9Gameplay = false;
    public const bool UsesVisualHeightAsPhysicalDepth = false;
    public const bool UsesMaxTsunamiHeightAsDepth = false;
    public const string DriverSource = "p8e_arrival_front_progression_model_v1";

    public static P8RiskFrontProgressionResult Evaluate(
        P8RiskFrontProgressionInput input,
        P8RiskFrontProgressionConfig config)
    {
        config = config ?? P8RiskFrontProgressionConfig.Default();
        input = input ?? new P8RiskFrontProgressionInput();

        var result = new P8RiskFrontProgressionResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            implementsP9Gameplay = ImplementsP9Gameplay,
            usesVisualHeightAsPhysicalDepth = UsesVisualHeightAsPhysicalDepth,
            usesMaxTsunamiHeightAsDepth = UsesMaxTsunamiHeightAsDepth,
            usesDerivedBoundary = !input.officialContourAvailable &&
                                  input.derivedBoundaryAvailable &&
                                  config.useDerivedBoundaryWhenOfficialContourMissing
        };

        if (config.visualHeightIsCinematicOnlyRequired &&
            !P8RiskFrontSpeedModel.VisualHeightIsCinematicOnly)
        {
            result.failSafe = true;
            result.summary = "Invalid P8-E risk-front configuration. visualHeightMeters must remain cinematic only.";
            return result;
        }

        bool canUseArrival = config.useArrivalTimeWhenAvailable &&
                             config.arrivalTimeOverridePriority &&
                             input.hasArrivalTime &&
                             input.arrivalTimeSeconds > input.timeOriginSeconds;

        if (canUseArrival)
        {
            float denominator = Mathf.Max(0.001f, input.arrivalTimeSeconds - input.timeOriginSeconds);
            result.progress01 = Mathf.Clamp01((input.simulationTimeSeconds - input.timeOriginSeconds) / denominator);
            result.usedArrivalTime = true;
            result.selectedSpeedMetersPerSecond = 0f;
        }
        else
        {
            bool slowdown;
            float speed = P8RiskFrontSpeedModel.CalculateFallbackSpeed(
                config,
                input.inundationDepthMeters,
                out slowdown);

            float distance = Mathf.Max(0.001f, input.fallbackDistanceMeters);
            float elapsed = Mathf.Max(0f, input.simulationTimeSeconds - input.timeOriginSeconds);
            result.progress01 = Mathf.Clamp01(elapsed * speed / distance);
            result.usedFallbackSpeed = true;
            result.shallowSlowdownApplied = slowdown;
            result.selectedSpeedMetersPerSecond = speed;
        }

        result.visualRiskStrength01 = P8RiskFrontSpeedModel.CalculateVisualRiskStrength(
            input.inundationDepthMeters,
            input.hazardIntensity);
        result.warningLevel = P8RiskFrontSpeedModel.CalculateWarningLevel(result.visualRiskStrength01);
        result.success = true;
        result.failSafe = false;
        result.summary = "P8-E risk front progression uses arrivalTimeSeconds when available, " +
                         "falls back to onshore speed with shallow-depth slowdown, and keeps " +
                         "visualHeightMeters cinematic-only and maxTsunamiHeightMeters metadata-only.";
        return result;
    }
}
