using System;

[Serializable]
public class P10BStressScenarioCollection
{
    public string schemaVersion = "p10b.stress_scenarios.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public bool noThousandsOfNpcs = true;
    public P10BStressScenarioConfig[] scenarios = Array.Empty<P10BStressScenarioConfig>();
}

[Serializable]
public class P10BStressScenarioConfig
{
    public string scenarioId = string.Empty;
    public string displayName = string.Empty;
    public string qualityPreset = "Medium";
    public int npcCap;
    public int markerCap;
    public int greenFrameCap;
    public bool greenGroundFramesEnabled;
    public bool tsunamiStarted;
    public bool lightCurtainEnabled;
    public bool resultPanelLongWarningEnabled;
    public bool collapseDebrisEnabled;
    public bool debugLabelsEnabled;
    public string expectedMetricFocus = string.Empty;

    public bool IsBounded()
    {
        return npcCap >= 0 &&
            npcCap <= 250 &&
            markerCap >= 0 &&
            markerCap <= 300 &&
            greenFrameCap >= 0 &&
            greenFrameCap <= 160;
    }
}

[Serializable]
public class P10BQualityPresetCollection
{
    public string schemaVersion = "p10b.quality_presets.v1";
    public bool projectSettingsChangeRequired;
    public P10BQualityPreset[] presets = Array.Empty<P10BQualityPreset>();
}

[Serializable]
public class P10BQualityPreset
{
    public string presetId = string.Empty;
    public int npcCap;
    public int markerCap;
    public int greenFrameCap;
    public bool debugLabelsEnabled;
    public bool lightCurtainEnabled;
    public string notes = string.Empty;
}

[Serializable]
public class P10BManualPlaytestChecklist
{
    public string schemaVersion = "p10b.manual_playtest_checklist.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public bool highDetailSceneMustNotBeSavedDuringSmoke = true;
    public bool userManualPlaytestPrepared = true;
    public string[] checklist = Array.Empty<string>();
    public string[] issueLogFields = Array.Empty<string>();
}
