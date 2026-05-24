using System;
using UnityEngine;
using UnityEngine.Profiling;

[Serializable]
public class P10BPlusPlusRuntimeMetricsSummary
{
    public string schemaVersion = "p10b_plus_plus.runtime_metrics_summary.v1";
    public string scenarioId = string.Empty;
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
    public P10BPlusPlusFrameTimeSummary frameTimeSummary = new P10BPlusPlusFrameTimeSummary();
    public float managedHeapMb;
    public float profilerAllocatedMemoryMb;
    public int gcCollectionDelta0;
    public int gcCollectionDelta1;
    public int gcCollectionDelta2;
    public string cpuMeasurementPolicy = "CPU usage is approximated by frame-time and spike counts in Unity runtime; P10-C must record OS-level CPU manually.";
}

public class P10BPlusPlusFrameSpikeDetector : MonoBehaviour
{
    [SerializeField] private P10BPlusPlusOptimizationConfig config = new P10BPlusPlusOptimizationConfig();
    [SerializeField] private bool recordInUpdate;
    [SerializeField] private string scenarioId = "p10b_plus_plus_runtime";
    [SerializeField] private string qualityPreset = "Medium";
    [SerializeField] private string weatherMode = "clear_day";
    [SerializeField] private string language = "en";
    [SerializeField] private bool nightOverlayEnabled;
    [SerializeField] private bool lightCurtainEnabled;
    [SerializeField] private bool resultPanelActive;
    [SerializeField] private bool sprintActive;
    [SerializeField] private int npcCount;
    [SerializeField] private int markerCount;
    [SerializeField] private int greenFrameCount;

    private P10BPlusPlusMetricsRingBuffer ringBuffer;
    private float runStartRealtime;
    private int startGc0;
    private int startGc1;
    private int startGc2;
    private P10BPlusPlusRuntimeMetricsSummary lastSummary = new P10BPlusPlusRuntimeMetricsSummary();

    public P10BPlusPlusRuntimeMetricsSummary LastSummary => lastSummary;
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

    public void Configure(P10BPlusPlusOptimizationConfig newConfig)
    {
        config = newConfig ?? new P10BPlusPlusOptimizationConfig();
        ringBuffer = new P10BPlusPlusMetricsRingBuffer(config.metricsRingBufferCapacity);
    }

    public void BeginRun(string newScenarioId)
    {
        scenarioId = string.IsNullOrWhiteSpace(newScenarioId) ? scenarioId : newScenarioId;
        EnsureRingBuffer().Clear();
        runStartRealtime = Time.realtimeSinceStartup;
        startGc0 = GC.CollectionCount(0);
        startGc1 = GC.CollectionCount(1);
        startGc2 = GC.CollectionCount(2);
    }

    public bool RecordFrame(float frameTimeSeconds)
    {
        return EnsureRingBuffer().AddFrameTime(frameTimeSeconds);
    }

    public void SetRuntimeState(
        string newQualityPreset,
        string newWeatherMode,
        string newLanguage,
        bool newNightOverlayEnabled,
        bool newLightCurtainEnabled,
        bool newResultPanelActive,
        bool newSprintActive,
        int newNpcCount,
        int newMarkerCount,
        int newGreenFrameCount)
    {
        qualityPreset = string.IsNullOrWhiteSpace(newQualityPreset) ? qualityPreset : newQualityPreset;
        weatherMode = string.IsNullOrWhiteSpace(newWeatherMode) ? weatherMode : newWeatherMode;
        language = string.IsNullOrWhiteSpace(newLanguage) ? language : newLanguage;
        nightOverlayEnabled = newNightOverlayEnabled;
        lightCurtainEnabled = newLightCurtainEnabled;
        resultPanelActive = newResultPanelActive;
        sprintActive = newSprintActive;
        npcCount = Mathf.Max(0, newNpcCount);
        markerCount = Mathf.Max(0, newMarkerCount);
        greenFrameCount = Mathf.Max(0, newGreenFrameCount);
    }

    public P10BPlusPlusRuntimeMetricsSummary FinishRun()
    {
        P10BPlusPlusFrameTimeSummary frameSummary = EnsureRingBuffer().CreateSummary(config.frameSpikeThresholdMs);
        lastSummary = new P10BPlusPlusRuntimeMetricsSummary
        {
            scenarioId = scenarioId ?? string.Empty,
            qualityPreset = qualityPreset ?? string.Empty,
            weatherMode = weatherMode ?? string.Empty,
            language = language ?? string.Empty,
            nightOverlayEnabled = nightOverlayEnabled,
            lightCurtainEnabled = lightCurtainEnabled,
            resultPanelActive = resultPanelActive,
            sprintActive = sprintActive,
            npcCount = npcCount,
            markerCount = markerCount,
            greenFrameCount = greenFrameCount,
            loadingTimeSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - runStartRealtime),
            frameTimeSummary = frameSummary,
            managedHeapMb = GC.GetTotalMemory(false) / (1024f * 1024f),
            profilerAllocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
            gcCollectionDelta0 = Mathf.Max(0, GC.CollectionCount(0) - startGc0),
            gcCollectionDelta1 = Mathf.Max(0, GC.CollectionCount(1) - startGc1),
            gcCollectionDelta2 = Mathf.Max(0, GC.CollectionCount(2) - startGc2)
        };
        return lastSummary;
    }

    private P10BPlusPlusMetricsRingBuffer EnsureRingBuffer()
    {
        if (ringBuffer == null)
        {
            int capacity = config == null ? 600 : config.metricsRingBufferCapacity;
            ringBuffer = new P10BPlusPlusMetricsRingBuffer(capacity);
        }

        return ringBuffer;
    }
}
