using System;
using System.IO;
using UnityEngine;

[Serializable]
public class P10BPlusPlusOptimizationConfig
{
    public string schemaVersion = "p10b_plus_plus.optimization_config.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public bool noAddressablesOrPackagesAdded = true;
    public bool projectSettingsChangeRequired;
    public bool mutatesPlateauAssets;
    public bool mutatesHighDetailScene;
    public bool createsReleaseOrArchiveArtifacts;
    public string defaultQualityPreset = "Medium";
    public bool debugLayersEnabledByDefault;
    public bool markerLayerEnabledByDefault = true;
    public bool crowdLayerEnabledByDefault = true;
    public bool lightCurtainEnabledByDefault = true;
    public int maxNpcCount = 120;
    public int maxMarkerCount = 140;
    public int maxGreenFrameCount = 140;
    public int greenFramePoolWarmupCount = 64;
    public int activationBudgetPerFrame = 32;
    public int metricsRingBufferCapacity = 600;
    public float frameSpikeThresholdMs = 50f;
    public float uiRefreshMinIntervalSeconds = 0.25f;
    public float memorySampleIntervalSeconds = 1f;
    public bool applyTargetFrameRate;
    public int targetFrameRate = 60;
    public string notes = "P10-B++ final optimization hardening uses runtime toggles, bounded metrics, and checklists only. P10-C remains responsible for the Windows EXE build and authoritative profiling.";

    public bool IsSafeFinalOptimizationConfig()
    {
        return finalWindowsExeBuildDeferredToP10C &&
            noAddressablesOrPackagesAdded &&
            !projectSettingsChangeRequired &&
            !mutatesPlateauAssets &&
            !mutatesHighDetailScene &&
            !createsReleaseOrArchiveArtifacts &&
            !debugLayersEnabledByDefault &&
            maxNpcCount >= 0 &&
            maxNpcCount <= 250 &&
            maxMarkerCount >= 0 &&
            maxMarkerCount <= 300 &&
            maxGreenFrameCount >= 0 &&
            maxGreenFrameCount <= 160 &&
            greenFramePoolWarmupCount >= 0 &&
            activationBudgetPerFrame > 0 &&
            metricsRingBufferCapacity >= 60 &&
            frameSpikeThresholdMs >= 16f;
    }
}

[Serializable]
public class P10BPlusPlusPerformanceAuditSample
{
    public string schemaVersion = "p10b_plus_plus.performance_audit_sample.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public string scenarioId = "p10b_plus_plus_high_detail_manual_profile";
    public string qualityPreset = "Medium";
    public string weatherMode = "clear_day";
    public string language = "en";
    public bool nightOverlayEnabled;
    public bool lightCurtainEnabled;
    public bool resultPanelActive;
    public bool sprintActive;
    public int npcCount;
    public int markerCount;
    public int greenFrameCount;
    public float loadingTimeSeconds;
    public float averageFps;
    public float onePercentLowFps;
    public float minFrameTimeMs;
    public float averageFrameTimeMs;
    public float maxFrameTimeMs;
    public float frameSpikeThresholdMs = 50f;
    public int frameSpikeCountOverThreshold;
    public float managedHeapMb;
    public float profilerAllocatedMemoryMb;
    public int gcCollectionCount0;
    public int gcCollectionCount1;
    public int gcCollectionCount2;
    public int playerLogWarningCount;
    public int playerLogErrorCount;
    public string cpuMeasurementPolicy = "Unity runtime CPU is approximated by frame-time and spike counts; P10-C must record OS-level CPU manually.";
    public string diskPagingMeasurementPolicy = "Disk paging is checked manually in P10-C with Task Manager, Resource Monitor, and PowerShell observations.";
}

public static class P10BPlusPlusDataLoader
{
    public const string OptimizationConfigFileName = "p10b_plus_plus_optimization_config.json";
    public const string PerformanceAuditSampleFileName = "p10b_plus_plus_performance_audit_sample.json";
    public const string QualityRecommendationsFileName = "p10b_plus_plus_quality_recommendations.json";

    public static P9BLoadResult<P10BPlusPlusOptimizationConfig> LoadOptimizationConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusPlusOptimizationConfig>(
            P10DataPath(OptimizationConfigFileName),
            "P10-B++ optimization config");
    }

    public static P9BLoadResult<P10BPlusPlusPerformanceAuditSample> LoadPerformanceAuditSample()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusPlusPerformanceAuditSample>(
            P10DataPath(PerformanceAuditSampleFileName),
            "P10-B++ performance audit sample");
    }

    public static P9BLoadResult<P10BPlusPlusQualityRecommendationCollection> LoadQualityRecommendations()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusPlusQualityRecommendationCollection>(
            P10DataPath(QualityRecommendationsFileName),
            "P10-B++ quality recommendations");
    }

    private static string P10DataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", "P10", fileName);
    }
}
