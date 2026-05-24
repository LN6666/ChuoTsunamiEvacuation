using System;

[Serializable]
public class P9CVerticalEvacuationTargetRules
{
    public string schemaVersion = string.Empty;
    public bool allowLifeFirstCandidates = true;
    public bool officialShelterAccessUnsafeOrUnavailable;
    public bool rejectLowFloorWarning = true;
    public bool requireSafeFloorProxy = true;
    public bool requireNonOfficialWarning = true;
    public bool requireRouteGuidanceProxy = false;
    public string nonOfficialWarningText = P9CVerticalEvacuationTargetDecision.NonOfficialWarningText;
    public string lifeFirstUseText = P9CVerticalEvacuationTargetDecision.LifeFirstUseText;
    public P9CVerticalEvacuationTargetRecord[] targets = Array.Empty<P9CVerticalEvacuationTargetRecord>();
}

[Serializable]
public class P9CVerticalEvacuationTargetRecord
{
    public string targetId = string.Empty;
    public string proxyId = string.Empty;
    public string displayName = string.Empty;
    public string targetType = string.Empty;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool nonOfficialWarningRequired = true;
    public bool safeApprovedByDefault;
    public bool hasSafeFloorProxy = true;
    public string safeFloorStatus = "unknown";
    public string entranceStatus = "open";
    public bool lowFloorInundationWarning;
    public string hazardState = "clear";
    public string buildingDamageState = "unknown";
    public bool routeGuidanceProxyAvailable = true;
    public bool routeIsOfficial;
    public bool routeIsEstimatedPrototypeGuidance = true;
    public float distanceMeters;
    public float estimatedTravelTimeSeconds;
    public int crowdQueueLength;
    public float scenarioPriorityBonus;
    public string selectionNotes = string.Empty;
}

[Serializable]
public class P9CVerticalEvacuationTargetDecision
{
    public const string NonOfficialWarningText = "Non-official humanitarian vertical evacuation candidate. Not an official evacuation shelter.";
    public const string LifeFirstUseText = "Life-first candidate. Use only when official shelter access is unsafe or unavailable.";

    public bool selected;
    public string selectedTargetId = string.Empty;
    public string selectedProxyId = string.Empty;
    public string selectedDisplayName = string.Empty;
    public string selectedTargetType = string.Empty;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool nonOfficialWarningRequired;
    public bool remainsNonOfficialAndWarningRequired;
    public bool safeApprovedByDefault;
    public string safeFloorStatus = string.Empty;
    public string entranceStatus = string.Empty;
    public float estimatedTravelTimeSeconds;
    public string routeGuidanceStatus = string.Empty;
    public bool routeIsOfficial;
    public bool routeIsEstimatedPrototypeGuidance = true;
    public string reasonCode = string.Empty;
    public string warningText = string.Empty;
    public P9CRejectedTargetReason[] rejectedTargets = Array.Empty<P9CRejectedTargetReason>();

    public bool IsEthicallyWarningSafe()
    {
        if (isOfficialShelter)
        {
            return !nonOfficialWarningRequired;
        }

        return isHumanitarianCandidate &&
            nonOfficialWarningRequired &&
            remainsNonOfficialAndWarningRequired &&
            !safeApprovedByDefault &&
            warningText.Contains("Not an official evacuation shelter");
    }
}

[Serializable]
public class P9CRejectedTargetReason
{
    public string targetId = string.Empty;
    public string reasonCode = string.Empty;
    public string message = string.Empty;
}
