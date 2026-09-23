using System;
using UnityEngine;

public static class P8InfrastructureHazardEvaluator
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;
    public const bool ImplementsCollapseProxy = false;
    public const string DriverSource = "hazard_layer_arrival_depth_boundary_infrastructure_v1";

    private const float ContactWindowSeconds = 60f;
    private const float WatchDepthMeters = 0.05f;
    private const float WarningDepthMeters = 0.3f;
    private const float InundatedDepthMeters = 1.0f;
    private const float WarningIntensity = 0.35f;
    private const float RestrictedIntensity = 0.65f;

    public static P8InfrastructureHazardEvaluation Evaluate(
        P8InfrastructureHazardTarget target,
        P8HazardLayerData hazardLayer,
        float simulationTimeSeconds)
    {
        if (target == null)
        {
            return CreateFailSafe("Target is null.");
        }

        return Evaluate(target.CreateEvaluationInput(), hazardLayer, simulationTimeSeconds);
    }

    public static P8InfrastructureHazardEvaluation Evaluate(
        P8InfrastructureHazardEvaluationInput input,
        P8HazardLayerData hazardLayer,
        float simulationTimeSeconds)
    {
        if (input == null)
        {
            return CreateFailSafe("Evaluation input is null.");
        }

        var result = new P8InfrastructureHazardEvaluation
        {
            success = false,
            failSafe = true,
            proxyBased = input.isProxy,
            targetId = input.targetId ?? string.Empty,
            category = input.category,
            simulationTimeSeconds = Mathf.Max(0f, simulationTimeSeconds),
            sourceBasis = DriverSource
        };

        if (hazardLayer == null || hazardLayer.features == null || hazardLayer.features.Length == 0)
        {
            result.state = P8InfrastructureHazardState.Safe;
            result.summary = "Missing P8 hazard layer. Infrastructure hazard evaluation remains fail-safe and gameplay-neutral.";
            return result;
        }

        P8HazardFeature feature = P8RiskFrontCurveGenerator.SelectFeatureForTime(hazardLayer, result.simulationTimeSeconds);
        if (feature == null)
        {
            result.state = P8InfrastructureHazardState.Safe;
            result.summary = "No usable P8 hazard feature. Infrastructure hazard evaluation remains fail-safe.";
            return result;
        }

        CopyFeatureMetadata(result, feature);
        result.contactPhase = CalculateContactPhase(result.simulationTimeSeconds, feature.arrivalTimeSeconds);
        result.secondsUntilArrival = feature.arrivalTimeSeconds - result.simulationTimeSeconds;

        bool categoryEnabled = P8InfrastructureCategoryUtility.IsCategoryEnabled(feature.affectedInfrastructureTypes, input.category);
        SpatialEvaluation spatial = EvaluateSpatialValues(input, feature);
        result.missingBoundary = spatial.missingBoundary;
        result.missingSpatialDepth = spatial.missingSpatialDepth;
        result.usedFeatureFallback = spatial.usedFeatureFallback;
        result.usedSpatialSample = spatial.usedSpatialSample;
        result.targetInsideBoundary = spatial.targetInsideBoundary;
        result.distanceToSpatialSampleMeters = spatial.distanceToSpatialSampleMeters;
        result.inundationDepthMeters = Mathf.Max(0f, spatial.inundationDepthMeters);
        result.hazardIntensity = Mathf.Clamp01(feature.hazardIntensity);
        result.depthIntensityAffected = result.inundationDepthMeters >= WatchDepthMeters ||
                                        result.hazardIntensity >= WarningIntensity;

        bool spatialIsPrototypeOrPending = !IsEvidenceBasedExtracted(feature) ||
                                           result.missingBoundary ||
                                           result.missingSpatialDepth ||
                                           result.usedFeatureFallback ||
                                           input.isProxy;
        result.proxyBased = result.proxyBased || spatialIsPrototypeOrPending;
        result.sourceModeForState = spatialIsPrototypeOrPending ? "prototype_or_pending" : result.sourceMode;

        if (!categoryEnabled)
        {
            result.state = P8InfrastructureHazardState.Watch;
            result.proxyBased = true;
            result.summary = CreateSummary(result, "category not explicitly enabled by affectedInfrastructureTypes");
            result.success = true;
            result.failSafe = false;
            return result;
        }

        result.state = DetermineState(input.category, result, input.conservativeFallbackWhenSpatialDataMissing);
        result.success = true;
        result.failSafe = false;
        result.summary = CreateSummary(result, spatialIsPrototypeOrPending ? "proxy/prototype spatial basis" : "evidence-backed spatial basis");
        return result;
    }

    public static bool IsGameplayNeutral()
    {
        return !AffectsGameplaySuccessFailure && !ImplementsCollapseProxy && !RequiresP9Systems;
    }

    private static void CopyFeatureMetadata(P8InfrastructureHazardEvaluation result, P8HazardFeature feature)
    {
        result.selectedFeatureId = feature.featureId ?? string.Empty;
        result.arrivalTimeSeconds = feature.arrivalTimeSeconds;
        result.maxTsunamiHeightMeters = feature.maxTsunamiHeightMeters;
        result.visualHeightMeters = feature.visualHeightMeters;
        result.confidence = Mathf.Clamp01(feature.confidence);
        result.sourceMode = feature.sourceMode ?? string.Empty;
        result.sourceCategory = feature.sourceCategory ?? string.Empty;
        result.evidenceSourceId = feature.evidenceSourceId ?? string.Empty;
        result.boundaryStatus = feature.boundaryStatus ?? string.Empty;
        result.spatialExtractionStatus = feature.spatialExtractionStatus ?? string.Empty;
        result.boundaryIsEvidenceBasedOrPrototype = feature.boundaryIsEvidenceBasedOrPrototype ?? string.Empty;
    }

    private static P8RiskFrontContactPhase CalculateContactPhase(float simulationTimeSeconds, float arrivalTimeSeconds)
    {
        if (simulationTimeSeconds < arrivalTimeSeconds - ContactWindowSeconds)
        {
            return P8RiskFrontContactPhase.BeforeFrontArrival;
        }

        if (Mathf.Abs(simulationTimeSeconds - arrivalTimeSeconds) <= ContactWindowSeconds)
        {
            return P8RiskFrontContactPhase.AtRiskFrontContact;
        }

        return P8RiskFrontContactPhase.AfterFrontArrival;
    }

    private static P8InfrastructureHazardState DetermineState(
        P8InfrastructureCategory category,
        P8InfrastructureHazardEvaluation result,
        bool conservativeFallbackWhenSpatialDataMissing)
    {
        if (result.contactPhase == P8RiskFrontContactPhase.BeforeFrontArrival)
        {
            return result.depthIntensityAffected ? P8InfrastructureHazardState.Watch : P8InfrastructureHazardState.Safe;
        }

        if ((result.missingBoundary || result.missingSpatialDepth) && conservativeFallbackWhenSpatialDataMissing)
        {
            return ConservativeProxyState(category, result.contactPhase);
        }

        if (result.inundationDepthMeters >= InundatedDepthMeters || result.hazardIntensity >= RestrictedIntensity)
        {
            return SevereStateForCategory(category);
        }

        if (result.inundationDepthMeters >= WarningDepthMeters || result.hazardIntensity >= WarningIntensity)
        {
            return ModerateStateForCategory(category);
        }

        if (result.inundationDepthMeters >= WatchDepthMeters)
        {
            return P8InfrastructureHazardState.Warning;
        }

        return result.contactPhase == P8RiskFrontContactPhase.AtRiskFrontContact
            ? P8InfrastructureHazardState.Watch
            : P8InfrastructureHazardState.Safe;
    }

    private static P8InfrastructureHazardState ConservativeProxyState(
        P8InfrastructureCategory category,
        P8RiskFrontContactPhase phase)
    {
        if (phase == P8RiskFrontContactPhase.BeforeFrontArrival)
        {
            return P8InfrastructureHazardState.Watch;
        }

        switch (category)
        {
            case P8InfrastructureCategory.Underground:
            case P8InfrastructureCategory.Waterfront:
            case P8InfrastructureCategory.NavigationTargetProxy:
            case P8InfrastructureCategory.HumanitarianCandidateProxy:
            case P8InfrastructureCategory.HighriseCandidateMarker:
                return P8InfrastructureHazardState.AvoidProxy;
            case P8InfrastructureCategory.Road:
            case P8InfrastructureCategory.Bridge:
            case P8InfrastructureCategory.Entrance:
            case P8InfrastructureCategory.OpenSpace:
            case P8InfrastructureCategory.ShelterProxy:
                return P8InfrastructureHazardState.RestrictedProxy;
            default:
                return P8InfrastructureHazardState.Warning;
        }
    }

    private static P8InfrastructureHazardState SevereStateForCategory(P8InfrastructureCategory category)
    {
        switch (category)
        {
            case P8InfrastructureCategory.Underground:
            case P8InfrastructureCategory.Waterfront:
            case P8InfrastructureCategory.NavigationTargetProxy:
            case P8InfrastructureCategory.HumanitarianCandidateProxy:
            case P8InfrastructureCategory.HighriseCandidateMarker:
                return P8InfrastructureHazardState.AvoidProxy;
            case P8InfrastructureCategory.Road:
            case P8InfrastructureCategory.Bridge:
            case P8InfrastructureCategory.Entrance:
            case P8InfrastructureCategory.OpenSpace:
            case P8InfrastructureCategory.ShelterProxy:
                return P8InfrastructureHazardState.RestrictedProxy;
            default:
                return P8InfrastructureHazardState.InundatedProxy;
        }
    }

    private static P8InfrastructureHazardState ModerateStateForCategory(P8InfrastructureCategory category)
    {
        switch (category)
        {
            case P8InfrastructureCategory.Underground:
            case P8InfrastructureCategory.Waterfront:
                return P8InfrastructureHazardState.AvoidProxy;
            case P8InfrastructureCategory.Road:
            case P8InfrastructureCategory.Bridge:
            case P8InfrastructureCategory.Entrance:
            case P8InfrastructureCategory.NavigationTargetProxy:
            case P8InfrastructureCategory.HumanitarianCandidateProxy:
            case P8InfrastructureCategory.HighriseCandidateMarker:
                return P8InfrastructureHazardState.RestrictedProxy;
            default:
                return P8InfrastructureHazardState.Warning;
        }
    }

    private static SpatialEvaluation EvaluateSpatialValues(
        P8InfrastructureHazardEvaluationInput input,
        P8HazardFeature feature)
    {
        var spatial = new SpatialEvaluation
        {
            inundationDepthMeters = Mathf.Max(0f, feature.inundationDepthMeters),
            targetInsideBoundary = true,
            distanceToSpatialSampleMeters = -1f
        };

        float x = input.hasExplicitHazardCoordinates ? input.hazardXMeters : input.worldPosition.x;
        float y = input.hasExplicitHazardCoordinates ? input.hazardYMeters : input.worldPosition.z;
        bool inputLooksWgs84 = CoordinatesLookLikeWgs84(x, y);
        bool boundaryLooksWgs84 = BoundaryLooksLikeWgs84(feature.inundationBoundary);
        P8HazardSpatialSample nearest = null;
        float nearestDistance = -1f;

        if (feature.spatialSamples != null && feature.spatialSamples.Length > 0)
        {
            nearest = FindNearestSample(feature.spatialSamples, x, y, inputLooksWgs84, out nearestDistance);
        }

        float boundaryX = x;
        float boundaryY = y;
        if (nearest != null && boundaryLooksWgs84 && !inputLooksWgs84)
        {
            boundaryX = nearest.longitude;
            boundaryY = nearest.latitude;
        }
        else if (nearest != null && !boundaryLooksWgs84 && inputLooksWgs84)
        {
            boundaryX = nearest.xMeters;
            boundaryY = nearest.yMeters;
        }

        if (feature.inundationBoundary == null || feature.inundationBoundary.Length < 3)
        {
            spatial.missingBoundary = true;
            spatial.usedFeatureFallback = true;
        }
        else
        {
            spatial.targetInsideBoundary = ContainsPoint(feature.inundationBoundary, boundaryX, boundaryY);
            if (!spatial.targetInsideBoundary)
            {
                spatial.inundationDepthMeters = 0f;
                return spatial;
            }
        }

        if (nearest != null)
        {
            spatial.inundationDepthMeters = Mathf.Max(0f, nearest.inundationDepthMeters);
            spatial.usedSpatialSample = true;
            spatial.distanceToSpatialSampleMeters = nearestDistance;
            return spatial;
        }

        if (feature.inundationDepthMeters <= 0f)
        {
            spatial.missingSpatialDepth = true;
        }

        spatial.usedFeatureFallback = true;
        return spatial;
    }

    private static P8HazardSpatialSample FindNearestSample(
        P8HazardSpatialSample[] samples,
        float x,
        float y,
        bool useWgs84,
        out float distanceMeters)
    {
        P8HazardSpatialSample nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < samples.Length; i++)
        {
            P8HazardSpatialSample sample = samples[i];
            if (sample == null)
            {
                continue;
            }

            float sampleX = useWgs84 ? sample.longitude : sample.xMeters;
            float sampleY = useWgs84 ? sample.latitude : sample.yMeters;
            float dx = sampleX - x;
            float dy = sampleY - y;
            float sqr = dx * dx + dy * dy;
            if (sqr < nearestSqr)
            {
                nearest = sample;
                nearestSqr = sqr;
            }
        }

        distanceMeters = nearest == null ? -1f : Mathf.Sqrt(nearestSqr) * (useWgs84 ? 111000f : 1f);
        return nearest;
    }

    private static bool BoundaryLooksLikeWgs84(P8BoundaryPoint[] boundary)
    {
        if (boundary == null || boundary.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < boundary.Length; i++)
        {
            if (!CoordinatesLookLikeWgs84(boundary[i].x, boundary[i].y))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CoordinatesLookLikeWgs84(float x, float y)
    {
        return Mathf.Abs(x) <= 180f && Mathf.Abs(y) <= 90f;
    }

    private static bool ContainsPoint(P8BoundaryPoint[] polygon, float x, float y)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            float xi = polygon[i].x;
            float yi = polygon[i].y;
            float xj = polygon[j].x;
            float yj = polygon[j].y;

            float denominator = yj - yi;
            if (Mathf.Abs(denominator) < 0.000001f)
            {
                denominator = denominator < 0f ? -0.000001f : 0.000001f;
            }

            bool intersects = ((yi > y) != (yj > y)) &&
                              (x < (xj - xi) * (y - yi) / denominator + xi);
            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static bool IsEvidenceBasedExtracted(P8HazardFeature feature)
    {
        return feature != null &&
               string.Equals(feature.boundaryIsEvidenceBasedOrPrototype, "evidence_based", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(feature.spatialExtractionStatus, "extracted", StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(feature.evidenceSourceId);
    }

    private static string CreateSummary(P8InfrastructureHazardEvaluation result, string basis)
    {
        return "P8-C infrastructure hazard state " + result.state +
               " for " + P8InfrastructureCategoryUtility.ToToken(result.category) +
               " target=" + result.targetId +
               " phase=" + result.contactPhase +
               " driver=" + DriverSource +
               " arrivalTimeSeconds=" + result.arrivalTimeSeconds.ToString("0.##") +
               " inundationDepthMeters=" + result.inundationDepthMeters.ToString("0.###") +
               " hazardIntensity=" + result.hazardIntensity.ToString("0.##") +
               " confidence=" + result.confidence.ToString("0.##") +
               " sourceMode=" + result.sourceMode +
               " evidenceSourceId=" + result.evidenceSourceId +
               " boundary=" + result.boundaryIsEvidenceBasedOrPrototype +
               " sourceBasis=" + basis +
               ". maxTsunamiHeightMeters is reference metadata only; visualHeightMeters is cinematic only; no gameplay success/failure rule changes.";
    }

    private static P8InfrastructureHazardEvaluation CreateFailSafe(string reason)
    {
        return new P8InfrastructureHazardEvaluation
        {
            failSafe = true,
            state = P8InfrastructureHazardState.Safe,
            sourceBasis = DriverSource,
            summary = reason ?? "P8-C infrastructure hazard evaluation failed safe."
        };
    }

    private class SpatialEvaluation
    {
        public bool missingSpatialDepth;
        public bool missingBoundary;
        public bool usedFeatureFallback;
        public bool usedSpatialSample;
        public bool targetInsideBoundary;
        public float inundationDepthMeters;
        public float distanceToSpatialSampleMeters;
    }
}
