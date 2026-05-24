using System;
using UnityEngine;

[Serializable]
public class P10BGreenGroundFrameConfig
{
    public string schemaVersion = "p10b.green_ground_frame_config.v1";
    public bool featureEnabled = true;
    public bool tsunamiStartTriggered = true;
    public bool debugPreviewBeforeTsunamiStart;
    public bool runtimeGenerated = true;
    public bool mutatesPlateauAssets;
    public bool mutatesHighDetailScene;
    public bool claimsExactBuildingFootprint;
    public bool claimsOfficialApprovalForHumanitarianCandidates;
    public int maxFrameCount = 140;
    public float defaultProxyFootprintWidthMeters = 24f;
    public float defaultProxyFootprintDepthMeters = 18f;
    public float officialShelterProxyWidthMeters = 28f;
    public float officialShelterProxyDepthMeters = 20f;
    public float minFootprintSizeMeters = 4f;
    public float maxFootprintSizeMeters = 80f;
    public float groundYOffsetMeters = 0.04f;
    public float lineWidthMeters = 0.18f;
    public bool transparentFillEnabled;
    public bool debugLabelsEnabled;
    public bool rejectInvalidCoordinates = true;
    public bool rejectZeroProxyPosition = true;
    public string frameMeaning = "evacuation_related_building_marker";
    public string humanitarianWarningText = P9CVerticalEvacuationTargetDecision.NonOfficialWarningText;
    public Color frameColor = new Color(0f, 1f, 0.12f, 0.95f);

    public bool IsRuntimeSafe()
    {
        return featureEnabled &&
            tsunamiStartTriggered &&
            runtimeGenerated &&
            !mutatesPlateauAssets &&
            !mutatesHighDetailScene &&
            !claimsExactBuildingFootprint &&
            !claimsOfficialApprovalForHumanitarianCandidates &&
            maxFrameCount > 0 &&
            defaultProxyFootprintWidthMeters > 0f &&
            defaultProxyFootprintDepthMeters > 0f;
    }
}

[Serializable]
public class P10BGreenGroundFrameTarget
{
    public string targetId = string.Empty;
    public string targetType = string.Empty;
    public string displayName = string.Empty;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool nonOfficialWarningRequired;
    public bool safeApprovedByDefault;
    public bool hasCoordinate;
    public float latitude;
    public float longitude;
    public Vector3 proxyCenter;
    public bool hasRendererBounds;
    public Vector3 rendererBoundsCenter;
    public Vector3 rendererBoundsSize;
    public bool exactFootprintProven;
    public bool coordinateDerivedProxy = true;
    public float footprintWidthMeters;
    public float footprintDepthMeters;
    public string anchorStatus = "coordinate_proxy";
    public string confidence = "medium";
    public string warningText = string.Empty;
    public string fallbackReason = "coordinate_derived_rectangle_proxy";

    public bool IsEvacuationRelatedTarget => isOfficialShelter || isHumanitarianCandidate;
    public bool RequiresNonOfficialWarning => isHumanitarianCandidate;
    public bool PreservesHumanitarianSemantics => !isHumanitarianCandidate || (!isOfficialShelter && nonOfficialWarningRequired && !safeApprovedByDefault);
    public bool UsesProxyRectangle => coordinateDerivedProxy || !exactFootprintProven;

    public Vector3 GetFrameCenter()
    {
        return hasRendererBounds ? rendererBoundsCenter : proxyCenter;
    }

    public float GetWidth(P10BGreenGroundFrameConfig config)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        float fallback = isOfficialShelter ? config.officialShelterProxyWidthMeters : config.defaultProxyFootprintWidthMeters;
        float width = footprintWidthMeters > 0f ? footprintWidthMeters : fallback;
        return Mathf.Clamp(width, config.minFootprintSizeMeters, config.maxFootprintSizeMeters);
    }

    public float GetDepth(P10BGreenGroundFrameConfig config)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        float fallback = isOfficialShelter ? config.officialShelterProxyDepthMeters : config.defaultProxyFootprintDepthMeters;
        float depth = footprintDepthMeters > 0f ? footprintDepthMeters : fallback;
        return Mathf.Clamp(depth, config.minFootprintSizeMeters, config.maxFootprintSizeMeters);
    }

    public bool IsValidForFrame(P10BGreenGroundFrameConfig config)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        if (!IsEvacuationRelatedTarget || !PreservesHumanitarianSemantics)
        {
            return false;
        }

        Vector3 center = GetFrameCenter();
        if (!IsFinite(center))
        {
            return false;
        }

        if (config.rejectInvalidCoordinates && hasCoordinate && (!IsFinite(latitude) || !IsFinite(longitude)))
        {
            return false;
        }

        if (config.rejectZeroProxyPosition && !hasRendererBounds && !hasCoordinate && center == Vector3.zero)
        {
            return false;
        }

        return GetWidth(config) > 0f && GetDepth(config) > 0f;
    }

    public static P10BGreenGroundFrameTarget FromAnchoringResult(P9DCoordinateAnchoringResult result, P10BGreenGroundFrameConfig config)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        result = result ?? new P9DCoordinateAnchoringResult();
        bool official = result.isOfficialShelter && !result.isHumanitarianCandidate;
        bool humanitarian = result.isHumanitarianCandidate;
        return new P10BGreenGroundFrameTarget
        {
            targetId = string.IsNullOrWhiteSpace(result.sourceId) ? result.anchorId : result.sourceId,
            targetType = official ? "official_evacuation_building" : "non_official_humanitarian_vertical_candidate",
            displayName = string.IsNullOrWhiteSpace(result.anchorId) ? result.sourceId : result.anchorId,
            isOfficialShelter = official,
            isHumanitarianCandidate = humanitarian,
            nonOfficialWarningRequired = humanitarian || result.nonOfficialWarningRequired,
            safeApprovedByDefault = false,
            hasCoordinate = result.coordinateValid,
            latitude = result.latitude,
            longitude = result.longitude,
            proxyCenter = result.unityPosition,
            exactFootprintProven = false,
            coordinateDerivedProxy = true,
            footprintWidthMeters = official ? config.officialShelterProxyWidthMeters : config.defaultProxyFootprintWidthMeters,
            footprintDepthMeters = official ? config.officialShelterProxyDepthMeters : config.defaultProxyFootprintDepthMeters,
            anchorStatus = result.anchoringStatus ?? string.Empty,
            confidence = result.confidence ?? string.Empty,
            warningText = humanitarian ? config.humanitarianWarningText : string.Empty,
            fallbackReason = "coordinate/proxy anchoring only; exact building footprint not proven"
        };
    }

    private static bool IsFinite(Vector3 value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

[Serializable]
public class P10BGreenGroundFrameTargetCollection
{
    public string schemaVersion = "p10b.green_ground_frame_targets.v1";
    public string sourcePolicy = "coordinate_derived_rectangle_proxy";
    public bool exactFootprintsProven;
    public bool officialRouteClaimed;
    public bool humanitarianWarningsPreserved = true;
    public P10BGreenGroundFrameTarget[] targets = Array.Empty<P10BGreenGroundFrameTarget>();
}

[Serializable]
public class P10BGreenGroundFrameMetrics
{
    public bool featureEnabled;
    public bool tsunamiStarted;
    public bool hiddenBeforeTsunamiStart;
    public bool debugPreviewEnabled;
    public int requestedTargetCount;
    public int validTargetCount;
    public int rejectedTargetCount;
    public int officialShelterFrameCount;
    public int humanitarianCandidateFrameCount;
    public int generatedFrameCount;
    public int activeFrameCount;
    public int createdPoolObjectCount;
    public bool allHumanitarianCandidatesRemainNonOfficial;
    public bool allHumanitarianWarningsPreserved;
    public bool noPerFrameObjectCreationRequired = true;
    public string summary = string.Empty;
}
