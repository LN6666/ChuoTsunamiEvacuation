using System;
using System.Collections.Generic;
using UnityEngine;

public static class P9CEntranceCongestionEvaluator
{
    public static P9CEntranceCongestionResult Evaluate(
        P9CEntranceInteractionState state,
        P9CEntranceCongestionRules rules,
        float remainingSafetyWindowSeconds)
    {
        var result = new P9CEntranceCongestionResult
        {
            canFailPlayerInP9C = P9RuntimePolicy.CanCausePlayerFailureInP9C,
            deterministic = true
        };

        if (state == null)
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.FailedDueToNoAvailableEntrance;
            result.summary = "P9-C entrance state missing.";
            return result;
        }

        rules = rules ?? new P9CEntranceCongestionRules();
        result.entranceProxyId = state.entranceProxyId ?? string.Empty;
        result.entranceStatus = P9CEntranceInteractionState.ToToken(state.ParsedStatus);
        result.queueLength = Mathf.Max(0, state.queueLength);
        result.crowdDensity01 = Mathf.Clamp01(state.crowdDensity01);

        var reasonCodes = new List<string>();
        if (state.ParsedStatus == P9CEntranceStatus.Open)
        {
            reasonCodes.Add(P9COutcomeReasonCode.EntranceOpen);
        }

        if (state.ParsedStatus == P9CEntranceStatus.Crowded || result.queueLength >= rules.crowdedQueueThreshold)
        {
            reasonCodes.Add(P9COutcomeReasonCode.EntranceCrowdedDelay);
        }

        if (state.hazardAffected || state.ParsedStatus == P9CEntranceStatus.HazardAffected)
        {
            reasonCodes.Add(P9COutcomeReasonCode.EntranceHazardAffected);
        }

        result.queueDelaySeconds = result.queueLength * Mathf.Max(0f, rules.queueDelaySecondsPerPerson);
        if (result.queueDelaySeconds > 0f)
        {
            reasonCodes.Add(P9COutcomeReasonCode.QueueDelayApplied);
            reasonCodes.Add(P9COutcomeReasonCode.VerticalEvacuationDelayedByQueue);
        }

        result.congestionDelaySeconds =
            Mathf.Clamp01(state.crowdDensity01) * Mathf.Max(0f, rules.crowdDensityDelaySeconds) +
            Mathf.Clamp01(state.routeCongestionScore01) * Mathf.Max(0f, rules.routeCongestionDelaySeconds);
        if (result.congestionDelaySeconds > 0f)
        {
            reasonCodes.Add(P9COutcomeReasonCode.CrowdCongestionDelay);
            reasonCodes.Add(P9COutcomeReasonCode.DelayedByCrowdCongestion);
            reasonCodes.Add(P9COutcomeReasonCode.VerticalEvacuationDelayedByCrowd);
        }

        float uncapped = Mathf.Max(0f, state.baseInteractionDelaySeconds) +
            result.queueDelaySeconds +
            result.congestionDelaySeconds;
        float cap = rules.maxDelayCapSeconds <= 0f ? uncapped : rules.maxDelayCapSeconds;
        result.totalDelaySeconds = Mathf.Min(uncapped, cap);
        result.delayCapped = uncapped > result.totalDelaySeconds;

        if ((state.ParsedStatus == P9CEntranceStatus.Blocked || state.ParsedStatus == P9CEntranceStatus.Closed) &&
            rules.failWhenBlocked)
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.EntranceBlockedFailure;
            reasonCodes.Add(P9COutcomeReasonCode.EntranceBlockedFailure);
            reasonCodes.Add(P9COutcomeReasonCode.FailedDueToNoAvailableEntrance);
        }
        else if ((state.hazardAffected || state.ParsedStatus == P9CEntranceStatus.HazardAffected) &&
            rules.failWhenHazardAffected)
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.FailedDueToHazardReachingEntrance;
            reasonCodes.Add(P9COutcomeReasonCode.FailedDueToHazardReachingEntrance);
        }
        else if (rules.failWhenDelayExceedsSafetyWindow &&
            remainingSafetyWindowSeconds > 0f &&
            result.totalDelaySeconds > remainingSafetyWindowSeconds)
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.FailedDueToCrowdDelay;
            reasonCodes.Add(P9COutcomeReasonCode.FailedDueToCrowdDelay);
        }
        else
        {
            result.reasonCode = result.totalDelaySeconds > 0f
                ? P9COutcomeReasonCode.DelayedByCrowdCongestion
                : P9COutcomeReasonCode.EntranceOpen;
        }

        result.success = !result.failure;
        result.reasonCodes = Unique(reasonCodes).ToArray();
        result.summary = "P9-C entrance evaluation: status=" + result.entranceStatus +
                         ", queueDelay=" + result.queueDelaySeconds.ToString("0.0") +
                         ", congestionDelay=" + result.congestionDelaySeconds.ToString("0.0") +
                         ", failure=" + result.failure + ".";
        return result;
    }

    private static List<string> Unique(List<string> values)
    {
        var unique = new List<string>();
        for (int i = 0; i < values.Count; i++)
        {
            if (!unique.Contains(values[i]))
            {
                unique.Add(values[i]);
            }
        }

        return unique;
    }
}

[Serializable]
public class P9CEntranceCongestionRules
{
    public string schemaVersion = string.Empty;
    public int deterministicSeed = 9702;
    public int crowdedQueueThreshold = 6;
    public int blockedQueueThreshold = 18;
    public float queueDelaySecondsPerPerson = 4f;
    public float crowdDensityDelaySeconds = 30f;
    public float routeCongestionDelaySeconds = 24f;
    public float maxDelayCapSeconds = 120f;
    public bool failWhenBlocked = true;
    public bool failWhenHazardAffected = true;
    public bool failWhenDelayExceedsSafetyWindow = true;
    public P9CEntranceInteractionState[] entrances = Array.Empty<P9CEntranceInteractionState>();
}

[Serializable]
public class P9CEntranceCongestionResult
{
    public bool success;
    public bool failure;
    public bool deterministic;
    public bool canFailPlayerInP9C;
    public string entranceProxyId = string.Empty;
    public string entranceStatus = string.Empty;
    public int queueLength;
    public float crowdDensity01;
    public float queueDelaySeconds;
    public float congestionDelaySeconds;
    public float totalDelaySeconds;
    public bool delayCapped;
    public string reasonCode = string.Empty;
    public string[] reasonCodes = Array.Empty<string>();
    public string summary = string.Empty;
}
