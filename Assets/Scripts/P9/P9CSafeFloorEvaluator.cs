using System;

public static class P9CSafeFloorEvaluator
{
    public static P9CSafeFloorEvaluationResult Evaluate(string safeFloorStatus, P9CSafeFloorProxyRules rules)
    {
        rules = rules ?? new P9CSafeFloorProxyRules();
        string status = string.IsNullOrWhiteSpace(safeFloorStatus) ? "unknown" : safeFloorStatus;
        var result = new P9CSafeFloorEvaluationResult
        {
            safeFloorStatus = status,
            verticalEvacuationProxyEvaluated = true,
            canFailPlayerInP9C = P9RuntimePolicy.CanCausePlayerFailureInP9C
        };

        if (string.Equals(status, "available", StringComparison.OrdinalIgnoreCase))
        {
            result.success = true;
            result.verticalEvacuationProxyCompleted = true;
            result.reasonCode = P9COutcomeReasonCode.SafeFloorAvailableSuccess;
        }
        else if (string.Equals(status, "unavailable", StringComparison.OrdinalIgnoreCase))
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.SafeFloorUnavailableFailure;
        }
        else if (string.Equals(status, "below_required_height", StringComparison.OrdinalIgnoreCase))
        {
            result.failure = true;
            result.reasonCode = P9COutcomeReasonCode.SafeFloorBelowRequiredHeightFailure;
        }
        else if (string.Equals(status, "crowd_over_capacity", StringComparison.OrdinalIgnoreCase))
        {
            result.failure = rules.failWhenCrowdOverCapacity;
            result.success = !result.failure;
            result.verticalEvacuationProxyCompleted = !result.failure;
            result.reasonCode = result.failure
                ? P9COutcomeReasonCode.FailedDueToCrowdDelay
                : P9COutcomeReasonCode.SafeFloorUnknownWarning;
        }
        else if (string.Equals(status, "hazard_warning", StringComparison.OrdinalIgnoreCase))
        {
            result.failure = rules.failWhenHazardWarning;
            result.success = !result.failure;
            result.verticalEvacuationProxyCompleted = !result.failure;
            result.warning = !result.failure;
            result.reasonCode = result.failure
                ? P9COutcomeReasonCode.FailedDueToHazardReachingEntrance
                : P9COutcomeReasonCode.SafeFloorUnknownWarning;
        }
        else
        {
            result.failure = rules.failWhenUnknown;
            result.success = !result.failure;
            result.verticalEvacuationProxyCompleted = !result.failure;
            result.warning = !result.failure;
            result.reasonCode = result.failure
                ? P9COutcomeReasonCode.SafeFloorUnavailableFailure
                : P9COutcomeReasonCode.SafeFloorUnknownWarning;
        }

        result.summary = "P9-C safe-floor evaluation: status=" + result.safeFloorStatus +
                         ", completed=" + result.verticalEvacuationProxyCompleted +
                         ", reason=" + result.reasonCode + ".";
        return result;
    }

    public static bool IsTerminalFailureStatus(string safeFloorStatus)
    {
        return string.Equals(safeFloorStatus, "unavailable", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(safeFloorStatus, "below_required_height", StringComparison.OrdinalIgnoreCase);
    }
}

[Serializable]
public class P9CSafeFloorProxyRules
{
    public string schemaVersion = string.Empty;
    public bool failWhenUnavailable = true;
    public bool failWhenBelowRequiredHeight = true;
    public bool failWhenUnknown;
    public bool failWhenHazardWarning;
    public bool failWhenCrowdOverCapacity = true;
    public string[] supportedStatuses =
    {
        "available",
        "unavailable",
        "unknown",
        "below_required_height",
        "hazard_warning",
        "crowd_over_capacity"
    };
}

[Serializable]
public class P9CSafeFloorEvaluationResult
{
    public bool success;
    public bool failure;
    public bool warning;
    public bool verticalEvacuationProxyEvaluated;
    public bool verticalEvacuationProxyCompleted;
    public bool canFailPlayerInP9C;
    public string safeFloorStatus = string.Empty;
    public string reasonCode = string.Empty;
    public string summary = string.Empty;
}
