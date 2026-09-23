using System;
using UnityEngine;

public static class P9RuntimePolicy
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool CanCausePlayerFailureInP9A = false;
    public const bool CanCausePlayerFailureInP9B = false;
    public const bool ImplementsFinalFailureGameplay = false;
    public const bool ImplementsIndoorSceneGameplay = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP7HighDetailScene = false;
    public const bool RequiresP8DEFinalHandoff = false;
    public const bool ClaimsOfficialRoutes = false;
    public const bool ClaimsOfficialHumanitarianCandidateShelters = false;
    public const bool P9BRuntimePrototypeOnly = true;
}

public enum P9VerticalEvacuationStatus
{
    Available,
    Warning,
    Restricted,
    BlockedProxy,
    Unknown
}

public enum P9EntranceCongestionState
{
    Clear,
    Queueing,
    CongestedProxy,
    BlockedProxy,
    Unknown
}

[Serializable]
public class P9EvacuationProxyState
{
    public bool success;
    public bool failSafe = true;
    public bool affectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public bool finalFailureGameplayEnabled = P9RuntimePolicy.ImplementsFinalFailureGameplay;
    public string proxyId = string.Empty;
    public P9VerticalEvacuationStatus verticalEvacuationStatus = P9VerticalEvacuationStatus.Unknown;
    public P9EntranceCongestionState congestionState = P9EntranceCongestionState.Unknown;
    public int queueLength;
    public int spawnedAgentCount;
    public int activeCrowdCount;
    public int failedOrBlockedEntranceCount;
    public float averageEvacuationProxyTimeSeconds;
    public string congestionHotspotProxy = string.Empty;
    public string delayReason = string.Empty;
    public string evacuationFailureReasonDraft = string.Empty;
    public string summary = string.Empty;

    public static P9EvacuationProxyState CreateNoEffect(string proxyId, string reason)
    {
        return new P9EvacuationProxyState
        {
            success = true,
            failSafe = false,
            proxyId = proxyId ?? string.Empty,
            verticalEvacuationStatus = P9VerticalEvacuationStatus.Unknown,
            congestionState = P9EntranceCongestionState.Unknown,
            summary = string.IsNullOrWhiteSpace(reason)
                ? "P9-A no-effect evacuation proxy state."
                : reason
        };
    }

    public static P9EvacuationProxyState FromQueueSnapshot(
        string proxyId,
        P9VerticalEvacuationStatus verticalStatus,
        int queueLength,
        int activeCrowdCount)
    {
        var state = CreateNoEffect(proxyId, "P9-A queue snapshot is metrics-only and has no gameplay failure effect.");
        state.verticalEvacuationStatus = verticalStatus;
        state.queueLength = Mathf.Max(0, queueLength);
        state.activeCrowdCount = Mathf.Max(0, activeCrowdCount);
        state.congestionState = DetermineCongestionState(state.queueLength, verticalStatus);
        return state;
    }

    private static P9EntranceCongestionState DetermineCongestionState(int queueLength, P9VerticalEvacuationStatus verticalStatus)
    {
        if (verticalStatus == P9VerticalEvacuationStatus.BlockedProxy)
        {
            return P9EntranceCongestionState.BlockedProxy;
        }

        if (queueLength >= 10)
        {
            return P9EntranceCongestionState.CongestedProxy;
        }

        if (queueLength > 0)
        {
            return P9EntranceCongestionState.Queueing;
        }

        return P9EntranceCongestionState.Clear;
    }

    public static P9VerticalEvacuationStatus ParseVerticalStatus(string value)
    {
        if (string.Equals(value, "available", StringComparison.OrdinalIgnoreCase))
        {
            return P9VerticalEvacuationStatus.Available;
        }

        if (string.Equals(value, "warning", StringComparison.OrdinalIgnoreCase))
        {
            return P9VerticalEvacuationStatus.Warning;
        }

        if (string.Equals(value, "restricted", StringComparison.OrdinalIgnoreCase))
        {
            return P9VerticalEvacuationStatus.Restricted;
        }

        if (string.Equals(value, "blocked_proxy", StringComparison.OrdinalIgnoreCase))
        {
            return P9VerticalEvacuationStatus.BlockedProxy;
        }

        return P9VerticalEvacuationStatus.Unknown;
    }

    public static string ToStatusToken(P9VerticalEvacuationStatus status)
    {
        switch (status)
        {
            case P9VerticalEvacuationStatus.Available:
                return "available";
            case P9VerticalEvacuationStatus.Warning:
                return "warning";
            case P9VerticalEvacuationStatus.Restricted:
                return "restricted";
            case P9VerticalEvacuationStatus.BlockedProxy:
                return "blocked_proxy";
            default:
                return "unknown";
        }
    }
}
