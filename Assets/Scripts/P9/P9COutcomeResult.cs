using System;

[Serializable]
public class P9COutcomeResult
{
    public bool success;
    public bool failure;
    public bool deterministic = true;
    public bool outcomeMutationApplied;
    public string selectedTargetId = string.Empty;
    public string selectedTargetType = string.Empty;
    public bool isOfficialShelter;
    public bool nonOfficialWarningRequired;
    public string nonOfficialWarningText = string.Empty;
    public string entranceStatus = string.Empty;
    public string safeFloorStatus = string.Empty;
    public float queueDelaySeconds;
    public float congestionDelaySeconds;
    public float totalDelaySeconds;
    public bool verticalEvacuationProxyCompleted;
    public bool collapseDebrisExposureTriggered;
    public bool collapseDebrisFatality;
    public float collapseDebrisFatalityProbability;
    public float hazardArrivalSeconds;
    public float playerCompletionSeconds;
    public string finalReasonCode = string.Empty;
    public string[] reasonCodes = Array.Empty<string>();
    public string summary = string.Empty;
}
