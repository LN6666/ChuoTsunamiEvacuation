using System;
using UnityEngine;

[Serializable]
public class P9CRunLogRecord
{
    public string selectedTargetId = string.Empty;
    public string selectedTargetType = string.Empty;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public string entranceStatus = string.Empty;
    public float queueDelaySeconds;
    public float congestionDelaySeconds;
    public string safeFloorStatus = string.Empty;
    public bool verticalEvacuationProxyCompleted;
    public bool collapseDebrisExposureTriggered;
    public bool collapseDebrisFatality;
    public float collapseDebrisFatalityProbability;
    public float hazardArrivalTimeSeconds;
    public float playerCompletionTimeSeconds;
    public string finalOutcome = string.Empty;
    public string finalReasonCode = string.Empty;
    public string[] reasonCodes = Array.Empty<string>();

    public static P9CRunLogRecord FromOutcome(P9COutcomeResult result)
    {
        result = result ?? new P9COutcomeResult();
        return new P9CRunLogRecord
        {
            selectedTargetId = result.selectedTargetId ?? string.Empty,
            selectedTargetType = result.selectedTargetType ?? string.Empty,
            isOfficialShelter = result.isOfficialShelter,
            nonOfficialWarningRequired = result.nonOfficialWarningRequired,
            entranceStatus = result.entranceStatus ?? string.Empty,
            queueDelaySeconds = result.queueDelaySeconds,
            congestionDelaySeconds = result.congestionDelaySeconds,
            safeFloorStatus = result.safeFloorStatus ?? string.Empty,
            verticalEvacuationProxyCompleted = result.verticalEvacuationProxyCompleted,
            collapseDebrisExposureTriggered = result.collapseDebrisExposureTriggered,
            collapseDebrisFatality = result.collapseDebrisFatality,
            collapseDebrisFatalityProbability = result.collapseDebrisFatalityProbability,
            hazardArrivalTimeSeconds = result.hazardArrivalSeconds,
            playerCompletionTimeSeconds = result.playerCompletionSeconds,
            finalOutcome = result.success ? "success" : "failure",
            finalReasonCode = result.finalReasonCode ?? string.Empty,
            reasonCodes = result.reasonCodes ?? Array.Empty<string>()
        };
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}
