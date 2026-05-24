using System;
using System.Collections.Generic;
using UnityEngine;

public static class P9CVerticalEvacuationProxyResolver
{
    public static P9COutcomeResult Resolve(
        P9COutcomeRulesConfig config,
        P9CVerticalEvacuationTargetRules targetRules,
        P9CEntranceCongestionRules entranceRules,
        P9CEntranceInteractionState entranceState,
        P9CSafeFloorProxyRules safeFloorRules,
        P9CCollapseDebrisFatalityConfig collapseConfig,
        P9CCollapseDebrisExposureEvent collapseEvent)
    {
        config = config ?? new P9COutcomeRulesConfig();
        var reasonCodes = new List<string>();
        P9CVerticalEvacuationTargetDecision targetDecision = P9CLifeFirstTargetSelector.Select(targetRules);
        if (!targetDecision.selected)
        {
            return CreateFailure(
                targetDecision,
                P9COutcomeReasonCode.FailedDueToNoSafeVerticalCandidate,
                reasonCodes,
                "No safe vertical evacuation target was selected.");
        }

        reasonCodes.Add(targetDecision.reasonCode);

        float travelSeconds = targetDecision.estimatedTravelTimeSeconds > 0f
            ? targetDecision.estimatedTravelTimeSeconds
            : config.baseTravelTimeSeconds;
        float remainingWindow = config.defaultHazardArrivalTimeSeconds -
            config.hazardArrivalSafetyMarginSeconds -
            travelSeconds -
            config.baseVerticalEvacuationSeconds;
        P9CEntranceCongestionResult entrance = P9CEntranceCongestionEvaluator.Evaluate(
            entranceState,
            entranceRules,
            remainingWindow);
        AddRange(reasonCodes, entrance.reasonCodes);

        P9CSafeFloorEvaluationResult safeFloor = P9CSafeFloorEvaluator.Evaluate(
            targetDecision.safeFloorStatus,
            safeFloorRules);
        reasonCodes.Add(safeFloor.reasonCode);

        P9CCollapseDebrisFatalityResult collapse = P9CCollapseDebrisFatalityEvaluator.Evaluate(
            collapseConfig,
            collapseEvent,
            0);
        if (collapse.exposureTriggered)
        {
            reasonCodes.Add(P9COutcomeReasonCode.CollapseDebrisExposureEvent);
        }

        if (!string.IsNullOrWhiteSpace(collapse.reasonCode))
        {
            reasonCodes.Add(collapse.reasonCode);
        }

        if (!string.IsNullOrWhiteSpace(collapse.secondaryReasonCode))
        {
            reasonCodes.Add(collapse.secondaryReasonCode);
        }

        if (collapse.fatality)
        {
            return CreateResult(
                targetDecision,
                entrance,
                safeFloor,
                collapse,
                null,
                false,
                P9COutcomeReasonCode.KilledByBuildingCollapseProxy,
                reasonCodes,
                "Collapse/debris exposure-event fatality proxy ended the run.");
        }

        if (config.enableEntranceBlockedFailure && entrance.failure)
        {
            return CreateResult(
                targetDecision,
                entrance,
                safeFloor,
                collapse,
                null,
                false,
                entrance.reasonCode,
                reasonCodes,
                "Entrance/congestion rule ended the run.");
        }

        if (safeFloor.failure)
        {
            return CreateResult(
                targetDecision,
                entrance,
                safeFloor,
                collapse,
                null,
                false,
                safeFloor.reasonCode,
                reasonCodes,
                "Safe-floor proxy rule ended the run.");
        }

        P9CHazardTimingOutcome hazard = P9CHazardTimingOutcomeEvaluator.Evaluate(
            travelSeconds,
            entrance.totalDelaySeconds,
            config.baseVerticalEvacuationSeconds,
            config.defaultHazardArrivalTimeSeconds,
            config.hazardArrivalSafetyMarginSeconds,
            config.enableHazardTimingFailure);
        reasonCodes.Add(hazard.reasonCode);
        reasonCodes.Add(hazard.secondaryReasonCode);

        if (config.enableCrowdDelayFailure && hazard.failure && entrance.totalDelaySeconds > 0f)
        {
            reasonCodes.Add(P9COutcomeReasonCode.FailedDueToCrowdDelay);
            return CreateResult(
                targetDecision,
                entrance,
                safeFloor,
                collapse,
                hazard,
                false,
                P9COutcomeReasonCode.FailedDueToCrowdDelay,
                reasonCodes,
                "Crowd/congestion delay exceeded the hazard arrival safety window.");
        }

        if (hazard.failure)
        {
            return CreateResult(
                targetDecision,
                entrance,
                safeFloor,
                collapse,
                hazard,
                false,
                hazard.reasonCode,
                reasonCodes,
                "Hazard timing rule ended the run.");
        }

        reasonCodes.Add(P9COutcomeReasonCode.VerticalEvacuationCompleteProxy);
        string finalReason = entrance.totalDelaySeconds > 0f
            ? P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival
            : P9COutcomeReasonCode.VerticalEvacuationCompleteProxy;
        return CreateResult(
            targetDecision,
            entrance,
            safeFloor,
            collapse,
            hazard,
            true,
            finalReason,
            reasonCodes,
            "Vertical evacuation proxy completed without a real interior scene.");
    }

    private static P9COutcomeResult CreateFailure(
        P9CVerticalEvacuationTargetDecision targetDecision,
        string finalReasonCode,
        List<string> reasonCodes,
        string summary)
    {
        reasonCodes.Add(finalReasonCode);
        return CreateResult(
            targetDecision,
            new P9CEntranceCongestionResult(),
            new P9CSafeFloorEvaluationResult(),
            new P9CCollapseDebrisFatalityResult(),
            null,
            false,
            finalReasonCode,
            reasonCodes,
            summary);
    }

    private static P9COutcomeResult CreateResult(
        P9CVerticalEvacuationTargetDecision targetDecision,
        P9CEntranceCongestionResult entrance,
        P9CSafeFloorEvaluationResult safeFloor,
        P9CCollapseDebrisFatalityResult collapse,
        P9CHazardTimingOutcome hazard,
        bool success,
        string finalReasonCode,
        List<string> reasonCodes,
        string summary)
    {
        entrance = entrance ?? new P9CEntranceCongestionResult();
        safeFloor = safeFloor ?? new P9CSafeFloorEvaluationResult();
        collapse = collapse ?? new P9CCollapseDebrisFatalityResult();
        targetDecision = targetDecision ?? new P9CVerticalEvacuationTargetDecision();

        return new P9COutcomeResult
        {
            success = success,
            failure = !success,
            deterministic = true,
            outcomeMutationApplied = P9RuntimePolicy.P9COutcomeMutationProxyEnabled,
            selectedTargetId = targetDecision.selectedTargetId ?? string.Empty,
            selectedTargetType = targetDecision.selectedTargetType ?? string.Empty,
            isOfficialShelter = targetDecision.isOfficialShelter,
            nonOfficialWarningRequired = targetDecision.nonOfficialWarningRequired,
            nonOfficialWarningText = targetDecision.warningText ?? string.Empty,
            entranceStatus = entrance.entranceStatus ?? string.Empty,
            safeFloorStatus = safeFloor.safeFloorStatus ?? targetDecision.safeFloorStatus ?? string.Empty,
            queueDelaySeconds = Mathf.Max(0f, entrance.queueDelaySeconds),
            congestionDelaySeconds = Mathf.Max(0f, entrance.congestionDelaySeconds),
            totalDelaySeconds = Mathf.Max(0f, entrance.totalDelaySeconds),
            verticalEvacuationProxyCompleted = success && safeFloor.verticalEvacuationProxyCompleted,
            collapseDebrisExposureTriggered = collapse.exposureTriggered,
            collapseDebrisFatality = collapse.fatality,
            collapseDebrisFatalityProbability = collapse.configuredFatalityProbability,
            hazardArrivalSeconds = hazard == null ? 0f : hazard.hazardArrivalSeconds,
            playerCompletionSeconds = hazard == null ? 0f : hazard.playerCompletionSeconds,
            finalReasonCode = finalReasonCode ?? string.Empty,
            reasonCodes = Unique(reasonCodes).ToArray(),
            summary = summary ?? string.Empty
        };
    }

    private static void AddRange(List<string> target, string[] values)
    {
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            target.Add(values[i]);
        }
    }

    private static List<string> Unique(List<string> values)
    {
        var unique = new List<string>();
        if (values == null)
        {
            return unique;
        }

        for (int i = 0; i < values.Count; i++)
        {
            string value = values[i];
            if (!string.IsNullOrWhiteSpace(value) && !unique.Contains(value))
            {
                unique.Add(value);
            }
        }

        return unique;
    }
}
