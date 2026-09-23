using System;

public static class P9CHazardTimingOutcomeEvaluator
{
    public static P9CHazardTimingOutcome Evaluate(
        float travelSeconds,
        float entranceDelaySeconds,
        float verticalEvacuationSeconds,
        float hazardArrivalSeconds,
        float safetyMarginSeconds,
        bool failureEnabled)
    {
        var outcome = new P9CHazardTimingOutcome
        {
            travelSeconds = Math.Max(0f, travelSeconds),
            entranceDelaySeconds = Math.Max(0f, entranceDelaySeconds),
            verticalEvacuationSeconds = Math.Max(0f, verticalEvacuationSeconds),
            hazardArrivalSeconds = Math.Max(0f, hazardArrivalSeconds),
            safetyMarginSeconds = Math.Max(0f, safetyMarginSeconds)
        };

        outcome.playerCompletionSeconds =
            outcome.travelSeconds +
            outcome.entranceDelaySeconds +
            outcome.verticalEvacuationSeconds;
        outcome.remainingSafetyWindowSeconds =
            outcome.hazardArrivalSeconds -
            outcome.safetyMarginSeconds -
            outcome.playerCompletionSeconds;

        if (failureEnabled && outcome.hazardArrivalSeconds > 0f && outcome.remainingSafetyWindowSeconds < 0f)
        {
            outcome.failure = true;
            outcome.reasonCode = P9COutcomeReasonCode.FailedDueToTsunamiArrival;
            outcome.secondaryReasonCode = P9COutcomeReasonCode.VerticalEvacuationFailedAfterArrival;
        }
        else if (outcome.entranceDelaySeconds > 0f)
        {
            outcome.success = true;
            outcome.reasonCode = P9COutcomeReasonCode.DelayedSuccessBeforeHazardArrival;
            outcome.secondaryReasonCode = P9COutcomeReasonCode.VerticalEvacuationCompletedBeforeArrival;
        }
        else
        {
            outcome.success = true;
            outcome.reasonCode = P9COutcomeReasonCode.VerticalEvacuationCompletedBeforeArrival;
            outcome.secondaryReasonCode = P9COutcomeReasonCode.VerticalEvacuationCompleteProxy;
        }

        return outcome;
    }
}

[Serializable]
public class P9CHazardTimingOutcome
{
    public bool success;
    public bool failure;
    public float travelSeconds;
    public float entranceDelaySeconds;
    public float verticalEvacuationSeconds;
    public float hazardArrivalSeconds;
    public float safetyMarginSeconds;
    public float playerCompletionSeconds;
    public float remainingSafetyWindowSeconds;
    public string reasonCode = string.Empty;
    public string secondaryReasonCode = string.Empty;
}
