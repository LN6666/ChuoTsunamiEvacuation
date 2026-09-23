using System;

public static class P9CCrowdDelayEvaluator
{
    public static P9CCrowdDelayOutcome Evaluate(
        P9CEntranceCongestionResult entrance,
        float remainingSafetyWindowSeconds,
        bool failureEnabled)
    {
        var outcome = new P9CCrowdDelayOutcome
        {
            deterministic = true
        };

        if (entrance == null)
        {
            outcome.reasonCode = P9COutcomeReasonCode.EntranceOpen;
            return outcome;
        }

        outcome.queueDelaySeconds = Math.Max(0f, entrance.queueDelaySeconds);
        outcome.congestionDelaySeconds = Math.Max(0f, entrance.congestionDelaySeconds);
        outcome.totalDelaySeconds = Math.Max(0f, entrance.totalDelaySeconds);
        outcome.hasCrowdDelay = outcome.totalDelaySeconds > 0f;
        outcome.reasonCode = outcome.hasCrowdDelay
            ? P9COutcomeReasonCode.DelayedByCrowdCongestion
            : P9COutcomeReasonCode.EntranceOpen;

        if (failureEnabled &&
            remainingSafetyWindowSeconds > 0f &&
            outcome.totalDelaySeconds > remainingSafetyWindowSeconds)
        {
            outcome.failure = true;
            outcome.reasonCode = P9COutcomeReasonCode.FailedDueToCrowdDelay;
        }

        return outcome;
    }
}

[Serializable]
public class P9CCrowdDelayOutcome
{
    public bool deterministic;
    public bool hasCrowdDelay;
    public bool failure;
    public float queueDelaySeconds;
    public float congestionDelaySeconds;
    public float totalDelaySeconds;
    public string reasonCode = string.Empty;
}
