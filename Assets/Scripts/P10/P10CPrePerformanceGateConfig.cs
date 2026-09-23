using System;
using System.IO;
using UnityEngine;

[Serializable]
public class P10CPrePerformanceGateConfig
{
    public string schemaVersion = "p10c_pre.performance_gate_config.v1";
    public bool p10cPreIsPerformanceGate = true;
    public bool finalReleaseBuildDeferredToP10C = true;
    public bool finalReleasePackageDeferredToP10C = true;
    public bool finalArchiveDeferredToP10C = true;
    public bool temporaryWindowsProfilingBuildAllowed = true;
    public bool buildArtifactsMustStayUncommitted = true;
    public bool noP10EFG = true;
    public bool noProjectSettingsChangeRequired = true;
    public bool noPackagesChangeRequired = true;
    public bool noPlateauAssetMutation = true;
    public bool noHighDetailSceneMutation = true;
    public bool noAddressablesOrAssetBundlesAdded = true;
    public bool trueChunkStreamingImplemented;
    public string temporaryBuildOutputPath = @"D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe";
    public string defaultBuildScene = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public string defaultQualityProfile = "Low";
    public string ordinaryPcTargetResolution = "1920x1080";
    public float lowQualityTargetAverageFps = 30f;
    public float lowQualityMinimumOnePercentLowFps = 20f;
    public float passWithLimitationsMinimumAverageFps = 24f;
    public float quickFixMinimumAverageFps = 15f;
    public float maxFrameSpikeThresholdMs = 50f;
    public float maxAllowedFrameTimeMs = 250f;
    public int maxFrameSpikeCountOverThreshold = 30;
    public int maxPlayerLogErrors = 0;
    public int maxPlayerLogWarnings = 100;
    public float maxShortRunMemoryGrowthMb = 256f;
    public string[] readinessClassifications = new[]
    {
        "pass_for_p10c",
        "pass_with_limitations",
        "needs_quick_fix",
        "needs_major_optimization_before_release",
        "blocked"
    };

    public bool IsSafePreReleaseGateConfig()
    {
        return p10cPreIsPerformanceGate &&
            finalReleaseBuildDeferredToP10C &&
            finalReleasePackageDeferredToP10C &&
            finalArchiveDeferredToP10C &&
            temporaryWindowsProfilingBuildAllowed &&
            buildArtifactsMustStayUncommitted &&
            noP10EFG &&
            noProjectSettingsChangeRequired &&
            noPackagesChangeRequired &&
            noPlateauAssetMutation &&
            noHighDetailSceneMutation &&
            noAddressablesOrAssetBundlesAdded &&
            !trueChunkStreamingImplemented &&
            lowQualityTargetAverageFps >= 24f &&
            lowQualityMinimumOnePercentLowFps >= 15f &&
            passWithLimitationsMinimumAverageFps > 0f &&
            quickFixMinimumAverageFps > 0f &&
            maxFrameSpikeThresholdMs >= 16f &&
            maxAllowedFrameTimeMs >= maxFrameSpikeThresholdMs &&
            maxShortRunMemoryGrowthMb > 0f;
    }

    public string Classify(
        P10CPreRuntimeMetrics metrics,
        int playerLogErrorCount,
        bool buildSucceeded,
        bool profileRunSucceeded)
    {
        if (!buildSucceeded || !profileRunSucceeded || metrics == null || !metrics.measured)
        {
            return "blocked";
        }

        if (playerLogErrorCount > maxPlayerLogErrors || metrics.fatalFreezeObserved)
        {
            return "needs_quick_fix";
        }

        if (metrics.memoryContinuouslyGrowing || metrics.pagingSymptomsObserved)
        {
            return "needs_major_optimization_before_release";
        }

        bool frameGatePasses =
            metrics.averageFps >= lowQualityTargetAverageFps &&
            metrics.onePercentLowFps >= lowQualityMinimumOnePercentLowFps &&
            metrics.maxFrameTimeMs <= maxAllowedFrameTimeMs &&
            metrics.frameSpikeCountOverThreshold <= maxFrameSpikeCountOverThreshold;

        if (frameGatePasses)
        {
            return "pass_for_p10c";
        }

        if (metrics.averageFps >= passWithLimitationsMinimumAverageFps &&
            metrics.maxFrameTimeMs <= maxAllowedFrameTimeMs * 1.5f)
        {
            return "pass_with_limitations";
        }

        return metrics.averageFps >= quickFixMinimumAverageFps
            ? "needs_quick_fix"
            : "needs_major_optimization_before_release";
    }
}

public static class P10CPreDataLoader
{
    public const string PerformanceGateConfigFileName = "p10c_pre_performance_gate_config.json";
    public const string QualityProfilesFileName = "p10c_pre_quality_profiles.json";
    public const string BuiltPlayerProfileSummaryFileName = "p10c_pre_built_player_profile_summary.json";
    public const string OptimizationDecisionFileName = "p10c_pre_optimization_decision.json";
    public const string ManualPlaytestChecklistFileName = "p10c_pre_manual_playtest_checklist.json";

    public static P9BLoadResult<P10CPrePerformanceGateConfig> LoadPerformanceGateConfig()
    {
        return P9BDataLoader.LoadFromPath<P10CPrePerformanceGateConfig>(
            P10DataPath(PerformanceGateConfigFileName),
            "P10-C-Pre performance gate config");
    }

    public static P9BLoadResult<P10CPreQualityProfileCollection> LoadQualityProfiles()
    {
        return P9BDataLoader.LoadFromPath<P10CPreQualityProfileCollection>(
            P10DataPath(QualityProfilesFileName),
            "P10-C-Pre quality profiles");
    }

    public static P9BLoadResult<P10CPreBuiltPlayerProfileSummary> LoadBuiltPlayerProfileSummary()
    {
        return P9BDataLoader.LoadFromPath<P10CPreBuiltPlayerProfileSummary>(
            P10DataPath(BuiltPlayerProfileSummaryFileName),
            "P10-C-Pre built-player profile summary");
    }

    public static P9BLoadResult<P10CPreOptimizationDecision> LoadOptimizationDecision()
    {
        return P9BDataLoader.LoadFromPath<P10CPreOptimizationDecision>(
            P10DataPath(OptimizationDecisionFileName),
            "P10-C-Pre optimization decision");
    }

    public static P9BLoadResult<P10CPreManualPlaytestChecklist> LoadManualPlaytestChecklist()
    {
        return P9BDataLoader.LoadFromPath<P10CPreManualPlaytestChecklist>(
            P10DataPath(ManualPlaytestChecklistFileName),
            "P10-C-Pre manual playtest checklist");
    }

    private static string P10DataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", "P10", fileName);
    }
}
