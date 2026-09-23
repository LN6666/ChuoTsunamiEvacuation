using System;

[Serializable]
public class P10CPreRuntimeMetrics
{
    public bool measured;
    public float startupTimeSeconds;
    public float sceneLoadingTimeSeconds;
    public float averageFps;
    public float onePercentLowFps;
    public float minFrameTimeMs;
    public float averageFrameTimeMs;
    public float maxFrameTimeMs;
    public float frameSpikeThresholdMs = 50f;
    public int frameSpikeCountOverThreshold;
    public float managedHeapMb;
    public float profilerAllocatedMemoryMb;
    public float workingSetMb;
    public float privateMemoryMb;
    public float memoryGrowthMb;
    public bool memoryContinuouslyGrowing;
    public int gcCollectionDelta0;
    public int gcCollectionDelta1;
    public int gcCollectionDelta2;
    public bool pagingSymptomsObserved;
    public bool fatalFreezeObserved;
    public int npcCount;
    public int markerCount;
    public int greenFrameCount;
    public bool lightCurtainEnabled;
    public bool resultPanelActive;
    public string weatherMode = "clear_day";
    public string qualityProfile = "Low";
    public string notes = string.Empty;
}

[Serializable]
public class P10CPreBuiltPlayerProfileSummary
{
    public string schemaVersion = "p10c_pre.built_player_profile_summary.v1";
    public string status = "prepared_not_run";
    public bool p10cPreIsPerformanceGate = true;
    public bool finalReleasePackageCreated;
    public bool finalArchiveCreated;
    public bool buildAttempted;
    public bool buildSucceeded;
    public string buildType = "temporary_windows_x64_development_profiling_test_build";
    public string buildOutputPath = string.Empty;
    public string[] buildScenes = Array.Empty<string>();
    public string buildStartedAtLocal = string.Empty;
    public string buildFinishedAtLocal = string.Empty;
    public string buildFailureReason = string.Empty;
    public bool profileRunAttempted;
    public bool profileRunSucceeded;
    public string profileStartedAtLocal = string.Empty;
    public string profileFinishedAtLocal = string.Empty;
    public string profileFailureReason = string.Empty;
    public int playerLogWarningCount;
    public int playerLogErrorCount;
    public P10CPreRuntimeMetrics metrics = new P10CPreRuntimeMetrics();
    public string readinessClassification = "blocked";
    public string summary = string.Empty;

    public bool IsTemporaryBuildOnly()
    {
        return p10cPreIsPerformanceGate &&
            !finalReleasePackageCreated &&
            !finalArchiveCreated &&
            (buildType ?? string.Empty).IndexOf("temporary", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public bool HasMeasuredProfile()
    {
        return profileRunSucceeded && metrics != null && metrics.measured;
    }
}

[Serializable]
public class P10CPreOptimizationDecision
{
    public string schemaVersion = "p10c_pre.optimization_decision.v1";
    public string readinessDecision = "blocked";
    public bool readyForP10CReleaseBuild;
    public bool officialP10CReleaseDeferred = true;
    public bool finalReleasePackageCreated;
    public bool finalArchiveCreated;
    public bool trueChunkStreamingImplemented;
    public bool addressablesOrAssetBundlesAdded;
    public bool projectSettingsChanged;
    public bool packagesChanged;
    public bool plateauAssetsChanged;
    public bool highDetailSceneChanged;
    public bool antiAliasingConfirmedInBuiltPlayer;
    public string streamingDecision = "No true production chunk streaming is implemented.";
    public string antiAliasingDecision = "AA is not confirmed for the final player; built-player visual inspection remains required.";
    public string[] limitations = Array.Empty<string>();
    public string[] nextSteps = Array.Empty<string>();
    public string summary = string.Empty;

    public bool IsHonestPreReleaseDecision()
    {
        return officialP10CReleaseDeferred &&
            !finalReleasePackageCreated &&
            !finalArchiveCreated &&
            !trueChunkStreamingImplemented &&
            !addressablesOrAssetBundlesAdded &&
            !projectSettingsChanged &&
            !packagesChanged &&
            !plateauAssetsChanged &&
            !highDetailSceneChanged;
    }
}

[Serializable]
public class P10CPreChecklistItem
{
    public string id = string.Empty;
    public string label = string.Empty;
    public bool blockingIfFailed;
    public string notes = string.Empty;
}

[Serializable]
public class P10CPreManualPlaytestChecklist
{
    public string schemaVersion = "p10c_pre.manual_playtest_checklist.v1";
    public string branch = "p10c-pre-performance-gate-optimization";
    public bool temporaryBuildOnly = true;
    public bool finalReleasePackageCreated;
    public string temporaryBuildPath = string.Empty;
    public P10CPreChecklistItem[] launchSteps = Array.Empty<P10CPreChecklistItem>();
    public P10CPreChecklistItem[] observations = Array.Empty<P10CPreChecklistItem>();
    public P10CPreChecklistItem[] blockingIssues = Array.Empty<P10CPreChecklistItem>();
    public string quickFixReportTemplate = string.Empty;

    public bool IsPreReleaseChecklist()
    {
        return temporaryBuildOnly && !finalReleasePackageCreated && launchSteps != null && observations != null;
    }
}
