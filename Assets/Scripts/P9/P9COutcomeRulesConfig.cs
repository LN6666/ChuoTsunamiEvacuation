using System;

[Serializable]
public class P9COutcomeRulesConfig
{
    public string schemaVersion = string.Empty;
    public string scenarioPresetId = "p9c_default";
    public int deterministicSeed = 9701;
    public bool enableOutcomeMutationProxy = true;
    public bool enableEntranceBlockedFailure = true;
    public bool enableCrowdDelayFailure = true;
    public bool enableHazardTimingFailure = true;
    public float baseTravelTimeSeconds = 120f;
    public float baseVerticalEvacuationSeconds = 45f;
    public float defaultHazardArrivalTimeSeconds = 420f;
    public float hazardArrivalSafetyMarginSeconds = 15f;
    public float maxTotalDelaySeconds = 180f;
    public bool claimsOfficialRouteStatus;
    public bool importsExternalCrowdPackage;
    public bool reimplementsP8HazardModel;
    public string notes = string.Empty;

    public bool IsGameplayProxySafe()
    {
        return enableOutcomeMutationProxy &&
            !claimsOfficialRouteStatus &&
            !importsExternalCrowdPackage &&
            !reimplementsP8HazardModel;
    }
}

[Serializable]
public class P9CScenarioFailurePresetCollection
{
    public string schemaVersion = string.Empty;
    public P9CScenarioFailurePreset[] presets = Array.Empty<P9CScenarioFailurePreset>();
}

[Serializable]
public class P9CScenarioFailurePreset
{
    public string presetId = string.Empty;
    public string displayName = string.Empty;
    public bool enableEntranceBlockedFailure;
    public bool enableCrowdDelayFailure;
    public bool enableCollapseDebrisFatalityProxy;
    public bool enableHazardTimingFailure;
    public float hazardArrivalTimeSeconds;
    public string notes = string.Empty;
}

[Serializable]
public class P9CReasonCodeCatalog
{
    public string schemaVersion = string.Empty;
    public P9CReasonCodeCatalogRecord[] reasonCodes = Array.Empty<P9CReasonCodeCatalogRecord>();
}

[Serializable]
public class P9CReasonCodeCatalogRecord
{
    public string reasonCode = string.Empty;
    public string category = string.Empty;
    public string description = string.Empty;
    public bool canEndRun;
    public bool warningRequired;
}
