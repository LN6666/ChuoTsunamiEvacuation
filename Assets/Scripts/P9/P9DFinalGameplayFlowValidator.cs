using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class P9DFinalGameplayFlowValidator
{
    public static P9DFinalGameplayFlowSummary Validate(
        P9DFinalGameplayScenarioCollection scenarios,
        P9DAnchoringReport anchoringReport)
    {
        scenarios = scenarios ?? new P9DFinalGameplayScenarioCollection();
        var results = new List<P9DFinalGameplayScenarioResult>();
        P9BP8HandoffAvailability handoffAvailability = P9BDataLoader.VerifyP8HandoffAvailability();

        if (scenarios.scenarios != null)
        {
            for (int i = 0; i < scenarios.scenarios.Length; i++)
            {
                results.Add(RunScenario(scenarios.scenarios[i]));
            }
        }

        var summary = new P9DFinalGameplayFlowSummary
        {
            success = results.Count > 0 && results.All(result => result.success),
            p8HandoffConsumed = handoffAvailability.allExpectedFilesPresent,
            anchoringReportPresent = anchoringReport != null && anchoringReport.successfulAnchors > 0,
            humanitarianCandidateTotal = anchoringReport == null ? 0 : anchoringReport.humanitarianCandidateTotal,
            nonOfficialWarningsPreserved = anchoringReport != null && anchoringReport.allHumanitarianCandidatesRequireWarning,
            routesRemainEstimatedPrototypeGuidance = anchoringReport == null || anchoringReport.routesRemainEstimatedPrototypeGuidance,
            exactPlateauObjectIdentityClaimed = anchoringReport != null && anchoringReport.exactPlateauObjectIdentityClaimed,
            resultPanelFeedbackAvailable = results.All(result => !string.IsNullOrWhiteSpace(result.resultPanelFeedback)),
            scenarioResults = results.ToArray()
        };
        summary.scenarioCount = summary.scenarioResults.Length;
        summary.passedScenarioCount = summary.scenarioResults.Count(result => result.success);
        summary.summary = "P9-D final gameplay flow validation: passed=" + summary.passedScenarioCount +
                          "/" + summary.scenarioCount +
                          ", P8 handoff consumed=" + summary.p8HandoffConsumed +
                          ", anchoring report present=" + summary.anchoringReportPresent + ".";
        return summary;
    }

    public static P9DFinalGameplayScenarioResult RunScenario(P9DFinalGameplayScenarioRecord scenario)
    {
        scenario = scenario ?? new P9DFinalGameplayScenarioRecord();
        P9COutcomeRulesConfig outcomeConfig = P9CDataLoader.LoadOutcomeRulesConfig().data;
        P9CVerticalEvacuationTargetRules targetRules = P9CDataLoader.LoadVerticalEvacuationTargetRules().data;
        P9CEntranceCongestionRules entranceRules = P9CDataLoader.LoadEntranceCongestionRules().data;
        P9CSafeFloorProxyRules safeFloorRules = P9CDataLoader.LoadSafeFloorProxyRules().data;
        P9CCollapseDebrisFatalityConfig collapseConfig = P9CDataLoader.LoadCollapseDebrisFatalityConfig().data;

        ApplyScenarioOverrides(scenario, outcomeConfig, targetRules, safeFloorRules, collapseConfig);
        P9CEntranceInteractionState entranceState = SelectEntranceState(entranceRules, scenario);
        P9CCollapseDebrisExposureEvent exposureEvent = scenario.triggerCollapseDebrisExposure
            ? CreateExposureEvent(scenario)
            : null;
        if (scenario.forceFatalCollapseSeed)
        {
            collapseConfig.deterministicSeed = FindFatalSeed(collapseConfig, exposureEvent);
        }

        P9COutcomeResult outcome = P9CVerticalEvacuationProxyResolver.Resolve(
            outcomeConfig,
            targetRules,
            entranceRules,
            entranceState,
            safeFloorRules,
            collapseConfig,
            exposureEvent);

        string feedback = P9CResultPanelFeedbackFormatter.Format(outcome);
        P9CRunLogRecord log = P9CRunLogRecord.FromOutcome(outcome);
        bool expectedOutcomeMatches = string.Equals(
            scenario.expectedOutcome,
            outcome.success ? "success" : "failure",
            StringComparison.OrdinalIgnoreCase);
        bool expectedReasonMatches = string.IsNullOrWhiteSpace(scenario.expectedFinalReasonCode) ||
            string.Equals(scenario.expectedFinalReasonCode, outcome.finalReasonCode, StringComparison.Ordinal);

        return new P9DFinalGameplayScenarioResult
        {
            scenarioId = scenario.scenarioId ?? string.Empty,
            success = expectedOutcomeMatches && expectedReasonMatches,
            outcomeSuccess = outcome.success,
            finalReasonCode = outcome.finalReasonCode ?? string.Empty,
            selectedTargetType = outcome.selectedTargetType ?? string.Empty,
            nonOfficialWarningRequired = outcome.nonOfficialWarningRequired,
            queueDelaySeconds = outcome.queueDelaySeconds,
            congestionDelaySeconds = outcome.congestionDelaySeconds,
            collapseDebrisExposureTriggered = outcome.collapseDebrisExposureTriggered,
            collapseDebrisFatality = outcome.collapseDebrisFatality,
            resultPanelFeedback = feedback,
            runLogJson = log.ToJson(),
            summary = "P9-D scenario " + scenario.scenarioId + " outcome=" +
                      (outcome.success ? "success" : "failure") +
                      ", reason=" + outcome.finalReasonCode + "."
        };
    }

    private static void ApplyScenarioOverrides(
        P9DFinalGameplayScenarioRecord scenario,
        P9COutcomeRulesConfig outcomeConfig,
        P9CVerticalEvacuationTargetRules targetRules,
        P9CSafeFloorProxyRules safeFloorRules,
        P9CCollapseDebrisFatalityConfig collapseConfig)
    {
        if (outcomeConfig != null && scenario.hazardArrivalTimeSeconds > 0f)
        {
            outcomeConfig.defaultHazardArrivalTimeSeconds = scenario.hazardArrivalTimeSeconds;
        }

        if (targetRules != null && targetRules.targets != null && !string.IsNullOrWhiteSpace(scenario.safeFloorStatusOverride))
        {
            for (int i = 0; i < targetRules.targets.Length; i++)
            {
                if (targetRules.targets[i].targetId == "p8_plateau_highrise_candidate_001")
                {
                    targetRules.targets[i].safeFloorStatus = scenario.safeFloorStatusOverride;
                }
            }
        }

        if (safeFloorRules != null)
        {
            safeFloorRules.failWhenUnknown = scenario.failWhenSafeFloorUnknown;
            safeFloorRules.failWhenHazardWarning = scenario.failWhenSafeFloorHazardWarning;
        }

        if (collapseConfig != null)
        {
            collapseConfig.enableCollapseDebrisFatalityProxy = scenario.enableCollapseDebrisFatalityProxy;
            if (scenario.collapseDebrisFatalityProbability >= 0f)
            {
                collapseConfig.collapseDebrisExposureFatalityProbability = scenario.collapseDebrisFatalityProbability;
            }
            if (scenario.deterministicSeed > 0)
            {
                collapseConfig.deterministicSeed = scenario.deterministicSeed;
            }
        }
    }

    private static P9CEntranceInteractionState SelectEntranceState(
        P9CEntranceCongestionRules rules,
        P9DFinalGameplayScenarioRecord scenario)
    {
        if (rules != null && rules.entrances != null)
        {
            for (int i = 0; i < rules.entrances.Length; i++)
            {
                if (string.Equals(rules.entrances[i].entranceStatus, scenario.entranceStatus, StringComparison.OrdinalIgnoreCase))
                {
                    return rules.entrances[i];
                }
            }
        }

        return new P9CEntranceInteractionState
        {
            entranceProxyId = "p9d_default_open_entrance",
            entranceStatus = "open"
        };
    }

    private static P9CCollapseDebrisExposureEvent CreateExposureEvent(P9DFinalGameplayScenarioRecord scenario)
    {
        return new P9CCollapseDebrisExposureEvent
        {
            eventId = string.IsNullOrWhiteSpace(scenario.collapseDebrisEventId)
                ? "p9d_final_flow_collapse_exposure"
                : scenario.collapseDebrisEventId,
            zoneId = "p9b_collapse_proxy_tower_edge_001",
            riskZoneCategory = "collapse_debris_proxy",
            routeSegmentId = "p9d_final_flow_route_segment",
            exposurePosition = new Vector3(30f, 0f, 40f),
            exposureTriggered = true,
            damagedBuildingState = true,
            exposureDistanceMeters = 3f
        };
    }

    private static int FindFatalSeed(
        P9CCollapseDebrisFatalityConfig config,
        P9CCollapseDebrisExposureEvent exposureEvent)
    {
        if (exposureEvent == null)
        {
            return config == null ? 1 : config.deterministicSeed;
        }

        for (int seed = 1; seed <= 300; seed++)
        {
            config.deterministicSeed = seed;
            if (P9CCollapseDebrisFatalityEvaluator.Evaluate(config, exposureEvent, 0).fatality)
            {
                return seed;
            }
        }

        return config.deterministicSeed;
    }
}

[Serializable]
public class P9DFinalGameplayScenarioCollection
{
    public string schemaVersion = string.Empty;
    public bool p8HandoffConsumed = true;
    public bool fullGameplayChainRequired = true;
    public P9DFinalGameplayScenarioRecord[] scenarios = Array.Empty<P9DFinalGameplayScenarioRecord>();
}

[Serializable]
public class P9DFinalGameplayScenarioRecord
{
    public string scenarioId = string.Empty;
    public string expectedOutcome = string.Empty;
    public string expectedFinalReasonCode = string.Empty;
    public string entranceStatus = "open";
    public string safeFloorStatusOverride = string.Empty;
    public bool failWhenSafeFloorUnknown;
    public bool failWhenSafeFloorHazardWarning;
    public float hazardArrivalTimeSeconds = 420f;
    public bool enableCollapseDebrisFatalityProxy;
    public bool triggerCollapseDebrisExposure;
    public bool forceFatalCollapseSeed;
    public int deterministicSeed;
    public float collapseDebrisFatalityProbability = 0.35f;
    public string collapseDebrisEventId = string.Empty;
}

[Serializable]
public class P9DFinalGameplayFlowSummary
{
    public bool success;
    public int scenarioCount;
    public int passedScenarioCount;
    public bool p8HandoffConsumed;
    public bool anchoringReportPresent;
    public int humanitarianCandidateTotal;
    public bool nonOfficialWarningsPreserved;
    public bool routesRemainEstimatedPrototypeGuidance;
    public bool exactPlateauObjectIdentityClaimed;
    public bool resultPanelFeedbackAvailable;
    public P9DFinalGameplayScenarioResult[] scenarioResults = Array.Empty<P9DFinalGameplayScenarioResult>();
    public string summary = string.Empty;
}

[Serializable]
public class P9DFinalGameplayScenarioResult
{
    public string scenarioId = string.Empty;
    public bool success;
    public bool outcomeSuccess;
    public string finalReasonCode = string.Empty;
    public string selectedTargetType = string.Empty;
    public bool nonOfficialWarningRequired;
    public float queueDelaySeconds;
    public float congestionDelaySeconds;
    public bool collapseDebrisExposureTriggered;
    public bool collapseDebrisFatality;
    public string resultPanelFeedback = string.Empty;
    public string runLogJson = string.Empty;
    public string summary = string.Empty;
}
