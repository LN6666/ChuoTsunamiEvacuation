using System;
using UnityEngine;

[Serializable]
public class P10BPlusPlusFrameTimeSummary
{
    public int capacity;
    public int sampleCount;
    public float minFrameTimeMs;
    public float averageFrameTimeMs;
    public float maxFrameTimeMs;
    public float averageFps;
    public float onePercentLowFps;
    public float frameSpikeThresholdMs = 50f;
    public int frameSpikeCountOverThreshold;
}

public class P10BPlusPlusMetricsRingBuffer
{
    private readonly float[] frameTimesSeconds;
    private int nextIndex;
    private int sampleCount;

    public P10BPlusPlusMetricsRingBuffer(int capacity)
    {
        frameTimesSeconds = new float[Mathf.Max(1, capacity)];
    }

    public int Capacity => frameTimesSeconds.Length;
    public int SampleCount => sampleCount;

    public void Clear()
    {
        nextIndex = 0;
        sampleCount = 0;
        Array.Clear(frameTimesSeconds, 0, frameTimesSeconds.Length);
    }

    public bool AddFrameTime(float frameTimeSeconds)
    {
        if (float.IsNaN(frameTimeSeconds) || float.IsInfinity(frameTimeSeconds) || frameTimeSeconds <= 0f)
        {
            return false;
        }

        frameTimesSeconds[nextIndex] = frameTimeSeconds;
        nextIndex = (nextIndex + 1) % frameTimesSeconds.Length;
        sampleCount = Mathf.Min(sampleCount + 1, frameTimesSeconds.Length);
        return true;
    }

    public P10BPlusPlusFrameTimeSummary CreateSummary(float frameSpikeThresholdMs)
    {
        var summary = new P10BPlusPlusFrameTimeSummary
        {
            capacity = Capacity,
            sampleCount = sampleCount,
            frameSpikeThresholdMs = Mathf.Max(0.001f, frameSpikeThresholdMs)
        };

        if (sampleCount == 0)
        {
            return summary;
        }

        float total = 0f;
        float min = float.MaxValue;
        float max = 0f;
        float spikeThresholdSeconds = summary.frameSpikeThresholdMs / 1000f;
        float[] sorted = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float frameTime = frameTimesSeconds[i];
            sorted[i] = frameTime;
            total += frameTime;
            min = Mathf.Min(min, frameTime);
            max = Mathf.Max(max, frameTime);
            if (frameTime >= spikeThresholdSeconds)
            {
                summary.frameSpikeCountOverThreshold++;
            }
        }

        Array.Sort(sorted);
        float average = total / sampleCount;
        int p99Index = Mathf.Clamp(Mathf.CeilToInt(sampleCount * 0.99f) - 1, 0, sampleCount - 1);
        float p99FrameTime = sorted[p99Index];

        summary.minFrameTimeMs = min * 1000f;
        summary.averageFrameTimeMs = average * 1000f;
        summary.maxFrameTimeMs = max * 1000f;
        summary.averageFps = average <= 0f ? 0f : 1f / average;
        summary.onePercentLowFps = p99FrameTime <= 0f ? 0f : 1f / p99FrameTime;
        return summary;
    }
}
