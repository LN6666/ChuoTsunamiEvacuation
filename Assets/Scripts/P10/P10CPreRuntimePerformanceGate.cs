using System;
using UnityEngine;
using UnityEngine.Profiling;

[Serializable]
public class P10CPreRuntimeState
{
    public string scenarioId = "p10c_pre_runtime_gate";
    public string qualityProfile = "Low";
    public string weatherMode = "clear_day";
    public bool lightCurtainEnabled;
    public bool resultPanelActive;
    public int npcCount;
    public int markerCount;
    public int greenFrameCount;
}

public class P10CPreRuntimePerformanceGate : MonoBehaviour
{
    [SerializeField] private P10CPrePerformanceGateConfig config = new P10CPrePerformanceGateConfig();
    [SerializeField] private P10CPreRuntimeState runtimeState = new P10CPreRuntimeState();
    [SerializeField] private bool recordInUpdate;

    private P10BPlusPlusMetricsRingBuffer ringBuffer;
    private float runStartRealtime;
    private long startManagedBytes;
    private int startGc0;
    private int startGc1;
    private int startGc2;
    private P10CPreBuiltPlayerProfileSummary lastSummary = new P10CPreBuiltPlayerProfileSummary();

    public P10CPreBuiltPlayerProfileSummary LastSummary => lastSummary;
    public int SampleCount => EnsureRingBuffer().SampleCount;

    private void Awake()
    {
        EnsureRingBuffer();
    }

    private void Update()
    {
        if (recordInUpdate)
        {
            RecordFrame(Time.unscaledDeltaTime);
        }
    }

    public void Configure(P10CPrePerformanceGateConfig newConfig)
    {
        config = newConfig ?? new P10CPrePerformanceGateConfig();
        ringBuffer = new P10BPlusPlusMetricsRingBuffer(600);
    }

    public void BeginGateRun(string scenarioId)
    {
        if (!string.IsNullOrWhiteSpace(scenarioId))
        {
            runtimeState.scenarioId = scenarioId;
        }

        EnsureRingBuffer().Clear();
        runStartRealtime = Time.realtimeSinceStartup;
        startManagedBytes = GC.GetTotalMemory(false);
        startGc0 = GC.CollectionCount(0);
        startGc1 = GC.CollectionCount(1);
        startGc2 = GC.CollectionCount(2);
    }

    public bool RecordFrame(float frameTimeSeconds)
    {
        return EnsureRingBuffer().AddFrameTime(frameTimeSeconds);
    }

    public void SetRuntimeState(
        string qualityProfile,
        string weatherMode,
        bool lightCurtainEnabled,
        bool resultPanelActive,
        int npcCount,
        int markerCount,
        int greenFrameCount)
    {
        runtimeState.qualityProfile = string.IsNullOrWhiteSpace(qualityProfile) ? runtimeState.qualityProfile : qualityProfile;
        runtimeState.weatherMode = string.IsNullOrWhiteSpace(weatherMode) ? runtimeState.weatherMode : weatherMode;
        runtimeState.lightCurtainEnabled = lightCurtainEnabled;
        runtimeState.resultPanelActive = resultPanelActive;
        runtimeState.npcCount = Mathf.Max(0, npcCount);
        runtimeState.markerCount = Mathf.Max(0, markerCount);
        runtimeState.greenFrameCount = Mathf.Max(0, greenFrameCount);
    }

    public P10CPreBuiltPlayerProfileSummary FinishGateRun(
        int playerLogWarningCount,
        int playerLogErrorCount)
    {
        P10BPlusPlusFrameTimeSummary frameSummary = EnsureRingBuffer().CreateSummary(config.maxFrameSpikeThresholdMs);
        long managedBytes = GC.GetTotalMemory(false);
        float memoryGrowthMb = Mathf.Max(0f, (managedBytes - startManagedBytes) / (1024f * 1024f));
        var metrics = new P10CPreRuntimeMetrics
        {
            measured = frameSummary.sampleCount > 0,
            startupTimeSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - runStartRealtime),
            averageFps = frameSummary.averageFps,
            onePercentLowFps = frameSummary.onePercentLowFps,
            minFrameTimeMs = frameSummary.minFrameTimeMs,
            averageFrameTimeMs = frameSummary.averageFrameTimeMs,
            maxFrameTimeMs = frameSummary.maxFrameTimeMs,
            frameSpikeThresholdMs = frameSummary.frameSpikeThresholdMs,
            frameSpikeCountOverThreshold = frameSummary.frameSpikeCountOverThreshold,
            managedHeapMb = managedBytes / (1024f * 1024f),
            profilerAllocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
            memoryGrowthMb = memoryGrowthMb,
            memoryContinuouslyGrowing = memoryGrowthMb > config.maxShortRunMemoryGrowthMb,
            gcCollectionDelta0 = Mathf.Max(0, GC.CollectionCount(0) - startGc0),
            gcCollectionDelta1 = Mathf.Max(0, GC.CollectionCount(1) - startGc1),
            gcCollectionDelta2 = Mathf.Max(0, GC.CollectionCount(2) - startGc2),
            npcCount = runtimeState.npcCount,
            markerCount = runtimeState.markerCount,
            greenFrameCount = runtimeState.greenFrameCount,
            lightCurtainEnabled = runtimeState.lightCurtainEnabled,
            resultPanelActive = runtimeState.resultPanelActive,
            weatherMode = runtimeState.weatherMode ?? string.Empty,
            qualityProfile = runtimeState.qualityProfile ?? string.Empty
        };

        string classification = config.Classify(metrics, playerLogErrorCount, true, metrics.measured);
        lastSummary = new P10CPreBuiltPlayerProfileSummary
        {
            status = "runtime_gate_sample",
            p10cPreIsPerformanceGate = true,
            finalReleasePackageCreated = false,
            finalArchiveCreated = false,
            buildAttempted = false,
            buildSucceeded = false,
            buildType = "runtime_metrics_only_not_final_release_build",
            profileRunAttempted = true,
            profileRunSucceeded = metrics.measured,
            playerLogWarningCount = playerLogWarningCount,
            playerLogErrorCount = playerLogErrorCount,
            metrics = metrics,
            readinessClassification = classification,
            summary = "P10-C-Pre runtime gate sample scenario=" + runtimeState.scenarioId +
                      ", avgFPS=" + metrics.averageFps.ToString("0.0") +
                      ", 1%low=" + metrics.onePercentLowFps.ToString("0.0") +
                      ", classification=" + classification + "."
        };
        return lastSummary;
    }

    private P10BPlusPlusMetricsRingBuffer EnsureRingBuffer()
    {
        if (ringBuffer == null)
        {
            ringBuffer = new P10BPlusPlusMetricsRingBuffer(600);
        }

        return ringBuffer;
    }
}
