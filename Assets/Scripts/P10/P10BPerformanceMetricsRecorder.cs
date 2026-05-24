using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[Serializable]
public class P10BPerformanceMetricsConfig
{
    public string schemaVersion = "p10b.performance_metrics_config.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public bool collectAverageFps = true;
    public bool collectOnePercentLowFps = true;
    public bool collectFrameTimeMinAvgMax = true;
    public bool collectMemoryUsage = true;
    public bool collectGcProxy = true;
    public bool collectLoadingTime = true;
    public bool collectPlayerLogWarningsErrors = true;
    public bool collectNpcMarkerAndGreenFrameCounts = true;
    public bool collectLightCurtainImpact = true;
    public bool collectResultPanelImpact = true;
    public int maxSamples = 600;
    public float frameSpikeThresholdMs = 50f;
    public string notes = "Editor/runtime metrics are lightweight proxies. Final Windows EXE build/profiling is deferred to P10-C.";
}

public class P10BPerformanceMetricsRecorder : MonoBehaviour
{
    [SerializeField] private P10BPerformanceMetricsConfig config = new P10BPerformanceMetricsConfig();
    [SerializeField] private string scenarioId = "p10b_runtime_smoke";
    [SerializeField] private bool recordInUpdate;

    private readonly List<float> frameTimes = new List<float>();
    private float runStartRealtime;
    private P10BPerformanceSampleSummary lastSummary = new P10BPerformanceSampleSummary();

    public P10BPerformanceSampleSummary LastSummary => lastSummary;
    public int SampleCount => frameTimes.Count;

    private void Update()
    {
        if (recordInUpdate)
        {
            RecordFrameTime(Time.unscaledDeltaTime);
        }
    }

    public void BeginRun(string newScenarioId)
    {
        scenarioId = string.IsNullOrWhiteSpace(newScenarioId) ? scenarioId : newScenarioId;
        frameTimes.Clear();
        runStartRealtime = Time.realtimeSinceStartup;
    }

    public void RecordFrameTime(float frameTimeSeconds)
    {
        if (config != null && frameTimes.Count >= Mathf.Max(1, config.maxSamples))
        {
            return;
        }

        if (!float.IsNaN(frameTimeSeconds) && !float.IsInfinity(frameTimeSeconds) && frameTimeSeconds > 0f)
        {
            frameTimes.Add(frameTimeSeconds);
        }
    }

    public P10BPerformanceSampleSummary FinishRun(
        int npcCount,
        int markerCount,
        int greenFrameCount,
        bool lightCurtainEnabled,
        int warningCount,
        int errorCount)
    {
        float loadingTime = Mathf.Max(0f, Time.realtimeSinceStartup - runStartRealtime);
        lastSummary = CreateSummaryFromFrameTimes(
            scenarioId,
            frameTimes.ToArray(),
            npcCount,
            markerCount,
            greenFrameCount,
            lightCurtainEnabled,
            warningCount,
            errorCount,
            loadingTime,
            config == null ? 50f : config.frameSpikeThresholdMs);
        return lastSummary;
    }

    public static P10BPerformanceSampleSummary CreateSummaryFromFrameTimes(
        string scenarioId,
        float[] frameTimesSeconds,
        int npcCount,
        int markerCount,
        int greenFrameCount,
        bool lightCurtainEnabled,
        int warningCount,
        int errorCount,
        float loadingTimeSeconds)
    {
        return CreateSummaryFromFrameTimes(
            scenarioId,
            frameTimesSeconds,
            npcCount,
            markerCount,
            greenFrameCount,
            lightCurtainEnabled,
            warningCount,
            errorCount,
            loadingTimeSeconds,
            50f);
    }

    public static P10BPerformanceSampleSummary CreateSummaryFromFrameTimes(
        string scenarioId,
        float[] frameTimesSeconds,
        int npcCount,
        int markerCount,
        int greenFrameCount,
        bool lightCurtainEnabled,
        int warningCount,
        int errorCount,
        float loadingTimeSeconds,
        float frameSpikeThresholdMs)
    {
        frameTimesSeconds = frameTimesSeconds ?? Array.Empty<float>();
        var valid = new List<float>();
        for (int i = 0; i < frameTimesSeconds.Length; i++)
        {
            float value = frameTimesSeconds[i];
            if (!float.IsNaN(value) && !float.IsInfinity(value) && value > 0f)
            {
                valid.Add(value);
            }
        }

        var summary = new P10BPerformanceSampleSummary
        {
            scenarioId = scenarioId ?? string.Empty,
            sampleCount = valid.Count,
            npcCount = npcCount,
            markerCount = markerCount,
            greenGroundFrameCount = greenFrameCount,
            lightCurtainEnabled = lightCurtainEnabled,
            playerLogWarningCount = warningCount,
            playerLogErrorCount = errorCount,
            loadingTimeSeconds = loadingTimeSeconds,
            managedMemoryMb = GC.GetTotalMemory(false) / (1024f * 1024f),
            profilerAllocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f)
        };

        if (valid.Count == 0)
        {
            summary.summary = "P10-B performance summary has no valid frame samples.";
            return summary;
        }

        valid.Sort();
        float total = 0f;
        float min = float.MaxValue;
        float max = 0f;
        for (int i = 0; i < valid.Count; i++)
        {
            float frameTime = valid[i];
            total += frameTime;
            min = Mathf.Min(min, frameTime);
            max = Mathf.Max(max, frameTime);
        }

        float average = total / valid.Count;
        int percentileIndex = Mathf.Clamp(Mathf.CeilToInt(valid.Count * 0.99f) - 1, 0, valid.Count - 1);
        float p99FrameTime = valid[percentileIndex];
        float safeSpikeThresholdMs = Mathf.Max(0.001f, frameSpikeThresholdMs);
        int spikeCount = CountFrameSpikes(valid, safeSpikeThresholdMs);
        summary.averageFrameTimeMs = average * 1000f;
        summary.minFrameTimeMs = min * 1000f;
        summary.maxFrameTimeMs = max * 1000f;
        summary.averageFps = average <= 0f ? 0f : 1f / average;
        summary.onePercentLowFps = p99FrameTime <= 0f ? 0f : 1f / p99FrameTime;
        summary.frameSpikeThresholdMs = safeSpikeThresholdMs;
        summary.frameSpikeCountOverThreshold = spikeCount;
        summary.gcCollectionCount0 = GC.CollectionCount(0);
        summary.gcCollectionCount1 = GC.CollectionCount(1);
        summary.gcCollectionCount2 = GC.CollectionCount(2);
        summary.summary = "P10-B metrics scenario=" + summary.scenarioId +
                          ", avgFPS=" + summary.averageFps.ToString("0.0") +
                          ", 1%low=" + summary.onePercentLowFps.ToString("0.0") +
                          ", spikes=" + summary.frameSpikeCountOverThreshold +
                          ", frames=" + summary.greenGroundFrameCount + ".";
        return summary;
    }

    private static int CountFrameSpikes(List<float> sortedFrameTimesSeconds, float frameSpikeThresholdMs)
    {
        float thresholdSeconds = Mathf.Max(0.001f, frameSpikeThresholdMs / 1000f);
        int count = 0;
        for (int i = 0; i < sortedFrameTimesSeconds.Count; i++)
        {
            if (sortedFrameTimesSeconds[i] >= thresholdSeconds)
            {
                count++;
            }
        }

        return count;
    }
}

[Serializable]
public class P10BPerformanceSampleSummary
{
    public string scenarioId = string.Empty;
    public int sampleCount;
    public float averageFps;
    public float onePercentLowFps;
    public float minFrameTimeMs;
    public float averageFrameTimeMs;
    public float maxFrameTimeMs;
    public float managedMemoryMb;
    public float profilerAllocatedMemoryMb;
    public float loadingTimeSeconds;
    public float frameSpikeThresholdMs;
    public int frameSpikeCountOverThreshold;
    public int gcCollectionCount0;
    public int gcCollectionCount1;
    public int gcCollectionCount2;
    public int playerLogWarningCount;
    public int playerLogErrorCount;
    public int npcCount;
    public int markerCount;
    public int greenGroundFrameCount;
    public bool lightCurtainEnabled;
    public string summary = string.Empty;
}

[Serializable]
public class P10BProfilingReport
{
    public string schemaVersion = "p10b.profiling_report_sample.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public bool highDetailManualPlaytestRequired = true;
    public bool beforeAfterOptimizationMetricsRequired = true;
    public P10BPerformanceSampleSummary[] sampleSummaries = Array.Empty<P10BPerformanceSampleSummary>();
    public string[] metricsToCollect = Array.Empty<string>();
    public string notes = string.Empty;
}
