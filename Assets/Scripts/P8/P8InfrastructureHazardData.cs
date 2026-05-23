using System;
using UnityEngine;

public enum P8InfrastructureCategory
{
    Road,
    Building,
    Bridge,
    Underground,
    Entrance,
    Waterfront,
    OpenSpace,
    ShelterProxy,
    NavigationTargetProxy
}

[Serializable]
public class P8InfrastructureHazardEvaluationInput
{
    public string targetId = string.Empty;
    public P8InfrastructureCategory category = P8InfrastructureCategory.Road;
    public Vector3 worldPosition;
    public bool isProxy = true;
    public bool hasExplicitHazardCoordinates;
    public float hazardXMeters;
    public float hazardYMeters;
    public bool conservativeFallbackWhenSpatialDataMissing = true;

    public static P8InfrastructureHazardEvaluationInput FromWorldPosition(
        string targetId,
        P8InfrastructureCategory category,
        Vector3 worldPosition,
        bool isProxy)
    {
        return new P8InfrastructureHazardEvaluationInput
        {
            targetId = targetId ?? string.Empty,
            category = category,
            worldPosition = worldPosition,
            isProxy = isProxy,
            hasExplicitHazardCoordinates = false,
            hazardXMeters = worldPosition.x,
            hazardYMeters = worldPosition.z
        };
    }

    public static P8InfrastructureHazardEvaluationInput FromHazardCoordinates(
        string targetId,
        P8InfrastructureCategory category,
        float hazardXMeters,
        float hazardYMeters,
        bool isProxy)
    {
        return new P8InfrastructureHazardEvaluationInput
        {
            targetId = targetId ?? string.Empty,
            category = category,
            isProxy = isProxy,
            hasExplicitHazardCoordinates = true,
            hazardXMeters = hazardXMeters,
            hazardYMeters = hazardYMeters,
            worldPosition = new Vector3(hazardXMeters, 0f, hazardYMeters)
        };
    }
}

[Serializable]
public class P8InfrastructureHazardEvaluation
{
    public bool success;
    public bool failSafe;
    public bool proxyBased;
    public bool missingSpatialDepth;
    public bool missingBoundary;
    public bool usedFeatureFallback;
    public bool usedSpatialSample;
    public bool targetInsideBoundary;
    public bool depthIntensityAffected;
    public string targetId = string.Empty;
    public P8InfrastructureCategory category;
    public P8InfrastructureHazardState state = P8InfrastructureHazardState.Safe;
    public P8RiskFrontContactPhase contactPhase = P8RiskFrontContactPhase.Unknown;
    public float simulationTimeSeconds;
    public float arrivalTimeSeconds;
    public float secondsUntilArrival;
    public float inundationDepthMeters;
    public float maxTsunamiHeightMeters;
    public float visualHeightMeters;
    public float hazardIntensity;
    public float confidence;
    public float distanceToSpatialSampleMeters = -1f;
    public string selectedFeatureId = string.Empty;
    public string sourceMode = string.Empty;
    public string sourceCategory = string.Empty;
    public string sourceModeForState = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string boundaryStatus = string.Empty;
    public string spatialExtractionStatus = string.Empty;
    public string boundaryIsEvidenceBasedOrPrototype = string.Empty;
    public string sourceBasis = string.Empty;
    public string summary = string.Empty;
}

public static class P8InfrastructureCategoryUtility
{
    public static string ToToken(P8InfrastructureCategory category)
    {
        switch (category)
        {
            case P8InfrastructureCategory.Road:
                return "road";
            case P8InfrastructureCategory.Building:
                return "building";
            case P8InfrastructureCategory.Bridge:
                return "bridge";
            case P8InfrastructureCategory.Underground:
                return "underground";
            case P8InfrastructureCategory.Entrance:
                return "entrance";
            case P8InfrastructureCategory.Waterfront:
                return "waterfront";
            case P8InfrastructureCategory.OpenSpace:
                return "open_space";
            case P8InfrastructureCategory.ShelterProxy:
                return "shelter_proxy";
            case P8InfrastructureCategory.NavigationTargetProxy:
                return "navigation_target_proxy";
            default:
                return "unknown";
        }
    }

    public static bool IsCategoryEnabled(P8AffectedInfrastructureTypes affected, P8InfrastructureCategory category)
    {
        if (affected == null)
        {
            return false;
        }

        switch (category)
        {
            case P8InfrastructureCategory.Road:
                return affected.roads;
            case P8InfrastructureCategory.Building:
                return affected.buildings;
            case P8InfrastructureCategory.Bridge:
                return affected.bridges;
            case P8InfrastructureCategory.Underground:
                return affected.underground;
            case P8InfrastructureCategory.Entrance:
                return affected.entrances;
            case P8InfrastructureCategory.Waterfront:
                return affected.waterfront;
            case P8InfrastructureCategory.OpenSpace:
                return affected.open_space;
            case P8InfrastructureCategory.ShelterProxy:
                return affected.shelter_proxy;
            case P8InfrastructureCategory.NavigationTargetProxy:
                return affected.navigation_target_proxy;
            default:
                return false;
        }
    }
}
