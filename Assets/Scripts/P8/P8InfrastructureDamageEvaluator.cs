using System;
using UnityEngine;

[Serializable]
public class P8InfrastructureDamageEvaluationInput
{
    public string targetId = string.Empty;
    public P8InfrastructureCategory category = P8InfrastructureCategory.Road;
    public bool isHumanitarianCandidate;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public bool manualReviewNeeded;
    public int collapseSelectionRank;

    public static P8InfrastructureDamageEvaluationInput FromHazardInput(
        P8InfrastructureHazardEvaluationInput hazardInput,
        bool isHumanitarianCandidate,
        bool isOfficialShelter,
        bool nonOfficialWarningRequired,
        bool manualReviewNeeded,
        int collapseSelectionRank)
    {
        return new P8InfrastructureDamageEvaluationInput
        {
            targetId = hazardInput != null ? hazardInput.targetId : string.Empty,
            category = hazardInput != null ? hazardInput.category : P8InfrastructureCategory.Road,
            isHumanitarianCandidate = isHumanitarianCandidate,
            isOfficialShelter = isOfficialShelter,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            manualReviewNeeded = manualReviewNeeded,
            collapseSelectionRank = collapseSelectionRank
        };
    }
}

[Serializable]
public class P8InfrastructureDamageEvaluation
{
    public bool success;
    public bool failSafe;
    public bool affectsGameplaySuccessFailure;
    public bool requiresP9Systems;
    public bool isProxyOnly = true;
    public bool isStructuralEngineeringAssessment;
    public bool usesPhysicsCollapse;
    public bool usesDebrisSimulation;
    public bool usesMaxTsunamiHeightAsDepth;
    public bool usesVisualHeightAsPhysicalDepth;
    public bool manualReviewNeeded;
    public bool isHumanitarianCandidate;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public string targetId = string.Empty;
    public P8InfrastructureCategory category = P8InfrastructureCategory.Road;
    public P8InfrastructureDamageState state = P8InfrastructureDamageState.NoDamage;
    public P8InfrastructureBlockageState blockageState = P8InfrastructureBlockageState.None;
    public P8CollapseProxyState collapseProxyState = P8CollapseProxyState.None;
    public string damageStateToken = "no_damage";
    public string blockageStateToken = "none";
    public string collapseProxyStateToken = "none";
    public float arrivalTimeSeconds;
    public float inundationDepthMeters;
    public float maxTsunamiHeightMeters;
    public float visualHeightMeters;
    public float hazardIntensity;
    public float confidence;
    public string sourceMode = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string sourceBasis = string.Empty;
    public string collapseProxyReason = string.Empty;
    public string summary = string.Empty;
}

public static class P8InfrastructureDamageEvaluator
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;
    public const bool ImplementsTrueStructuralCollapse = false;
    public const bool ImplementsPhysicsCollapse = false;
    public const bool ImplementsDebrisSimulation = false;
    public const bool UsesMaxTsunamiHeightAsDepth = false;
    public const bool UsesVisualHeightAsPhysicalDepth = false;
    public const string DriverSource = "p8d_damage_blockage_proxy_from_p8c_hazard_state_v1";

    public static P8InfrastructureDamageEvaluation Evaluate(
        P8InfrastructureHazardEvaluation hazard,
        P8InfrastructureDamageConfig config,
        P8InfrastructureDamageEvaluationInput input)
    {
        config = config ?? P8InfrastructureDamageConfig.Default();
        input = input ?? new P8InfrastructureDamageEvaluationInput();

        var result = new P8InfrastructureDamageEvaluation
        {
            targetId = input.targetId ?? string.Empty,
            category = input.category,
            isHumanitarianCandidate = input.isHumanitarianCandidate || IsHumanitarianCategory(input.category),
            isOfficialShelter = input.isOfficialShelter,
            nonOfficialWarningRequired = input.nonOfficialWarningRequired,
            manualReviewNeeded = input.manualReviewNeeded,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            requiresP9Systems = RequiresP9Systems,
            isStructuralEngineeringAssessment = ImplementsTrueStructuralCollapse,
            usesPhysicsCollapse = ImplementsPhysicsCollapse,
            usesDebrisSimulation = ImplementsDebrisSimulation,
            usesMaxTsunamiHeightAsDepth = UsesMaxTsunamiHeightAsDepth,
            usesVisualHeightAsPhysicalDepth = UsesVisualHeightAsPhysicalDepth,
            sourceBasis = DriverSource
        };

        if (result.isHumanitarianCandidate && config.requireNonOfficialWarningForHumanitarianCandidates)
        {
            result.nonOfficialWarningRequired = true;
        }

        if (hazard == null || !hazard.success)
        {
            result.failSafe = true;
            result.state = result.manualReviewNeeded ? P8InfrastructureDamageState.ManualReviewRequired : P8InfrastructureDamageState.NoDamage;
            Finish(result, "Missing or fail-safe P8-C hazard state. Damage proxy remains gameplay-neutral.");
            return result;
        }

        CopyHazard(result, hazard);
        result.success = true;
        result.failSafe = false;

        P8InfrastructureDamageState state = DetermineDamageState(hazard, config, result.category);
        P8InfrastructureBlockageState blockage = DetermineBlockageState(hazard, config, result.category);

        P8CollapseProxyDecision collapse = P8CollapseProxyRule.Evaluate(
            string.IsNullOrWhiteSpace(result.targetId) ? P8InfrastructureCategoryUtility.ToToken(result.category) : result.targetId,
            config,
            result.inundationDepthMeters,
            result.hazardIntensity,
            Mathf.Max(0, input.collapseSelectionRank));

        if (collapse.showCollapsedProxyVisual && CanShowCollapseProxyVisual(result.category))
        {
            state = P8InfrastructureDamageState.CollapsedProxyVisual;
        }
        else if (state == P8InfrastructureDamageState.NoDamage && result.manualReviewNeeded)
        {
            state = P8InfrastructureDamageState.ManualReviewRequired;
        }

        result.state = state;
        result.blockageState = blockage;
        result.collapseProxyState = collapse.state;
        result.collapseProxyReason = collapse.reason;
        Finish(result, "P8-D proxy status only; no real structural collapse, debris, rigidbody destruction, official safety claim, or success/failure rule change.");
        return result;
    }

    public static bool IsGameplayNeutral()
    {
        return !AffectsGameplaySuccessFailure &&
               !RequiresP9Systems &&
               !ImplementsTrueStructuralCollapse &&
               !ImplementsPhysicsCollapse &&
               !ImplementsDebrisSimulation;
    }

    private static void CopyHazard(P8InfrastructureDamageEvaluation result, P8InfrastructureHazardEvaluation hazard)
    {
        result.targetId = string.IsNullOrWhiteSpace(result.targetId) ? hazard.targetId : result.targetId;
        result.category = hazard.category;
        result.arrivalTimeSeconds = hazard.arrivalTimeSeconds;
        result.inundationDepthMeters = Mathf.Max(0f, hazard.inundationDepthMeters);
        result.maxTsunamiHeightMeters = hazard.maxTsunamiHeightMeters;
        result.visualHeightMeters = hazard.visualHeightMeters;
        result.hazardIntensity = Mathf.Clamp01(hazard.hazardIntensity);
        result.confidence = Mathf.Clamp01(hazard.confidence);
        result.sourceMode = hazard.sourceMode ?? string.Empty;
        result.evidenceSourceId = hazard.evidenceSourceId ?? string.Empty;
    }

    private static P8InfrastructureDamageState DetermineDamageState(
        P8InfrastructureHazardEvaluation hazard,
        P8InfrastructureDamageConfig config,
        P8InfrastructureCategory category)
    {
        float depth = Mathf.Max(0f, hazard.inundationDepthMeters);
        float intensity = Mathf.Clamp01(hazard.hazardIntensity);

        if (depth >= Mathf.Max(0f, config.inaccessibleDepthMeters) ||
            intensity >= Mathf.Clamp01(config.inaccessibleIntensity))
        {
            if (category == P8InfrastructureCategory.Road)
            {
                return P8InfrastructureDamageState.RoadRestrictedProxy;
            }

            if (category == P8InfrastructureCategory.Bridge)
            {
                return P8InfrastructureDamageState.BridgeRestrictedProxy;
            }

            if (category == P8InfrastructureCategory.Underground)
            {
                return P8InfrastructureDamageState.UndergroundAvoidProxy;
            }

            if (category == P8InfrastructureCategory.Entrance)
            {
                return P8InfrastructureDamageState.EntranceBlockedProxy;
            }

            return P8InfrastructureDamageState.InaccessibleProxy;
        }

        switch (category)
        {
            case P8InfrastructureCategory.Entrance:
                if (depth >= config.entranceBlockedDepthMeters || intensity >= config.entranceBlockedIntensity)
                {
                    return P8InfrastructureDamageState.EntranceBlockedProxy;
                }
                break;
            case P8InfrastructureCategory.Road:
                if (depth >= config.roadRestrictedDepthMeters || hazard.state == P8InfrastructureHazardState.RestrictedProxy || hazard.state == P8InfrastructureHazardState.InundatedProxy)
                {
                    return P8InfrastructureDamageState.RoadRestrictedProxy;
                }
                break;
            case P8InfrastructureCategory.Bridge:
                if (depth >= config.bridgeRestrictedDepthMeters || intensity >= config.entranceBlockedIntensity)
                {
                    return P8InfrastructureDamageState.BridgeRestrictedProxy;
                }
                break;
            case P8InfrastructureCategory.Underground:
                if (depth >= config.undergroundAvoidDepthMeters || hazard.state == P8InfrastructureHazardState.AvoidProxy)
                {
                    return P8InfrastructureDamageState.UndergroundAvoidProxy;
                }
                break;
            case P8InfrastructureCategory.Building:
            case P8InfrastructureCategory.HumanitarianCandidateProxy:
            case P8InfrastructureCategory.HighriseCandidateMarker:
                if (depth >= config.buildingDamageDepthMeters || intensity >= config.buildingDamageIntensity)
                {
                    return P8InfrastructureDamageState.BuildingDamagedProxy;
                }

                if (depth >= config.lowFloorInundationDepthMeters)
                {
                    return P8InfrastructureDamageState.LowFloorInundationWarning;
                }
                break;
        }

        if (depth >= config.lowFloorInundationDepthMeters)
        {
            return P8InfrastructureDamageState.LowFloorInundationWarning;
        }

        if (depth >= config.warningDepthMeters || intensity >= config.entranceBlockedIntensity || hazard.state == P8InfrastructureHazardState.Warning)
        {
            return P8InfrastructureDamageState.Warning;
        }

        return P8InfrastructureDamageState.NoDamage;
    }

    private static P8InfrastructureBlockageState DetermineBlockageState(
        P8InfrastructureHazardEvaluation hazard,
        P8InfrastructureDamageConfig config,
        P8InfrastructureCategory category)
    {
        float depth = Mathf.Max(0f, hazard.inundationDepthMeters);
        float intensity = Mathf.Clamp01(hazard.hazardIntensity);

        switch (category)
        {
            case P8InfrastructureCategory.Entrance:
                return depth >= config.entranceBlockedDepthMeters || intensity >= config.entranceBlockedIntensity
                    ? P8InfrastructureBlockageState.EntranceBlockedProxy
                    : P8InfrastructureBlockageState.None;
            case P8InfrastructureCategory.Road:
                return depth >= config.roadRestrictedDepthMeters || hazard.state == P8InfrastructureHazardState.RestrictedProxy
                    ? P8InfrastructureBlockageState.RoadRestrictedProxy
                    : P8InfrastructureBlockageState.None;
            case P8InfrastructureCategory.Bridge:
                return depth >= config.bridgeRestrictedDepthMeters
                    ? P8InfrastructureBlockageState.BridgeRestrictedProxy
                    : P8InfrastructureBlockageState.None;
            case P8InfrastructureCategory.Underground:
                return depth >= config.undergroundAvoidDepthMeters || hazard.state == P8InfrastructureHazardState.AvoidProxy
                    ? P8InfrastructureBlockageState.UndergroundAvoidProxy
                    : P8InfrastructureBlockageState.None;
            default:
                return depth >= config.inaccessibleDepthMeters || intensity >= config.inaccessibleIntensity
                    ? P8InfrastructureBlockageState.InaccessibleProxy
                    : P8InfrastructureBlockageState.None;
        }
    }

    private static bool CanShowCollapseProxyVisual(P8InfrastructureCategory category)
    {
        return category == P8InfrastructureCategory.Building ||
               category == P8InfrastructureCategory.HumanitarianCandidateProxy ||
               category == P8InfrastructureCategory.HighriseCandidateMarker;
    }

    private static bool IsHumanitarianCategory(P8InfrastructureCategory category)
    {
        return category == P8InfrastructureCategory.HumanitarianCandidateProxy ||
               category == P8InfrastructureCategory.HighriseCandidateMarker;
    }

    private static void Finish(P8InfrastructureDamageEvaluation result, string note)
    {
        result.damageStateToken = P8InfrastructureDamageStateUtility.ToToken(result.state);
        result.blockageStateToken = P8InfrastructureDamageStateUtility.ToToken(result.blockageState);
        result.collapseProxyStateToken = P8InfrastructureDamageStateUtility.ToToken(result.collapseProxyState);
        result.summary = "P8-D " + result.damageStateToken +
                         " target=" + result.targetId +
                         " category=" + P8InfrastructureCategoryUtility.ToToken(result.category) +
                         " arrivalTimeSeconds=" + result.arrivalTimeSeconds.ToString("0.##") +
                         " inundationDepthMeters=" + result.inundationDepthMeters.ToString("0.###") +
                         " hazardIntensity=" + result.hazardIntensity.ToString("0.##") +
                         " confidence=" + result.confidence.ToString("0.##") +
                         " sourceMode=" + result.sourceMode +
                         " evidenceSourceId=" + result.evidenceSourceId +
                         ". maxTsunamiHeightMeters is metadata only; visualHeightMeters is cinematic only. " +
                         note;
    }
}
