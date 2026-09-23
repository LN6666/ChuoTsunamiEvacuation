using System;
using UnityEngine;

[Serializable]
public class P10BPlusPlusQualityRecommendationCollection
{
    public string schemaVersion = "p10b_plus_plus.quality_recommendations.v1";
    public bool projectSettingsChangeRequired;
    public bool antiAliasingModeClaimed;
    public string antiAliasingAuditPolicy = "Runtime code may read QualitySettings.antiAliasing, but P10-C visual QA must confirm the final player AA mode.";
    public P10BPlusPlusQualityRecommendation[] presets = Array.Empty<P10BPlusPlusQualityRecommendation>();
}

[Serializable]
public class P10BPlusPlusQualityRecommendation
{
    public string presetId = "Medium";
    public string antiAliasingRecommendation = "verify_in_p10c";
    public int npcCap = 80;
    public int markerCap = 140;
    public int greenFrameCap = 140;
    public bool debugLabelsEnabled;
    public bool lightCurtainEnabled = true;
    public bool nightOverlayAllowed = true;
    public string notes = string.Empty;

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
public class P10BPlusPlusQualityRuntimeAudit
{
    public int qualityLevelIndex;
    public string qualityLevelName = string.Empty;
    public int qualitySettingsAntiAliasing;
    public int vSyncCount;
    public int targetFrameRate;
    public string antiAliasingStatus = string.Empty;
    public string notes = string.Empty;
}

public static class P10BPlusPlusQualityPresetAdvisor
{
    public static P10BPlusPlusQualityRuntimeAudit BuildRuntimeAudit()
    {
        int qualityLevel = QualitySettings.GetQualityLevel();
        string[] names = QualitySettings.names ?? Array.Empty<string>();
        string qualityName = qualityLevel >= 0 && qualityLevel < names.Length ? names[qualityLevel] : string.Empty;
        int antiAliasing = QualitySettings.antiAliasing;
        return new P10BPlusPlusQualityRuntimeAudit
        {
            qualityLevelIndex = qualityLevel,
            qualityLevelName = qualityName,
            qualitySettingsAntiAliasing = antiAliasing,
            vSyncCount = QualitySettings.vSyncCount,
            targetFrameRate = Application.targetFrameRate,
            antiAliasingStatus = DescribeAntiAliasing(antiAliasing),
            notes = "Runtime audit is informational only; P10-C must visually verify AA and measure FPS/memory in the built player."
        };
    }

    public static string DescribeAntiAliasing(int qualitySettingsAntiAliasing)
    {
        if (qualitySettingsAntiAliasing <= 0)
        {
            return "QualitySettings antiAliasing is 0; no MSAA is confirmed from this runtime setting.";
        }

        return "QualitySettings antiAliasing reports " + qualitySettingsAntiAliasing +
               "x; confirm render-pipeline and camera behavior visually in P10-C.";
    }

    public static P10BPlusPlusQualityRecommendation FindPreset(
        P10BPlusPlusQualityRecommendationCollection collection,
        string presetId)
    {
        if (collection == null || collection.presets == null)
        {
            return null;
        }

        for (int i = 0; i < collection.presets.Length; i++)
        {
            P10BPlusPlusQualityRecommendation preset = collection.presets[i];
            if (preset != null && string.Equals(preset.presetId, presetId, StringComparison.OrdinalIgnoreCase))
            {
                return preset;
            }
        }

        return null;
    }
}
