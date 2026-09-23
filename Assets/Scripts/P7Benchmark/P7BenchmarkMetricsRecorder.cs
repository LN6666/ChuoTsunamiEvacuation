using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class P7BenchmarkMetricsRecorder : MonoBehaviour
{
    private const int DefaultMaxSamples = 18000;

    [SerializeField] private bool recordOnEnable;
    [SerializeField] private int maxSamples = DefaultMaxSamples;
    [SerializeField] private P7BenchmarkChunkRegistry chunkRegistry;
    [SerializeField] private P7BenchmarkChunkController chunkController;

    private readonly List<float> frameTimeSamples = new List<float>();
    private bool isRecording;
    private float elapsedSeconds;

    public bool IsRecording => isRecording;
    public int SampleCount => frameTimeSamples.Count;
    public float ElapsedSeconds => elapsedSeconds;
    public float AverageFps => CalculateAverageFps(frameTimeSamples);
    public float ApproximateOnePercentLowFps => CalculateApproximateOnePercentLowFps(frameTimeSamples);
    public int ActiveChunkCount => chunkController != null ? chunkController.ActiveChunkCount : 0;
    public int ChunkBindingCount => chunkController != null ? chunkController.BindingCount : 0;
    public int ImportedCandidateFileCount => chunkRegistry != null ? chunkRegistry.TotalSourceFileCount : 0;
    public long ImportedCandidateBytes => chunkRegistry != null ? chunkRegistry.TotalSourceBytes : 0L;

    public void BeginRecording(bool clearExistingSamples = true)
    {
        if (clearExistingSamples)
        {
            ClearSamples();
        }

        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void ClearSamples()
    {
        frameTimeSamples.Clear();
        elapsedSeconds = 0f;
    }

    public void ConfigureChunkContext(P7BenchmarkChunkRegistry registry, P7BenchmarkChunkController controller)
    {
        chunkRegistry = registry;
        chunkController = controller;
    }

    public void RecordFrameTime(float deltaSeconds)
    {
        if (!IsValidFrameTime(deltaSeconds))
        {
            return;
        }

        elapsedSeconds += deltaSeconds;
        frameTimeSamples.Add(deltaSeconds);
        TrimOldSamplesIfNeeded();
    }

    public string ExportSummaryString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "P7BenchmarkMetricsSummary stage={0}; label={1}; scope={2}; samples={3}; elapsedSeconds={4:0.###}; averageFps={5:0.###}; onePercentLowFps={6:0.###}; averageFrameMs={7:0.###}; activeChunks={8}; chunkBindings={9}; importedCandidate={10}; importedFiles={11}; importedBytes={12}; chunkStates={13}",
            P7BenchmarkMarker.BenchmarkStage,
            P7BenchmarkMarker.BenchmarkLabel,
            P7BenchmarkMarker.BenchmarkScope,
            SampleCount,
            ElapsedSeconds,
            AverageFps,
            ApproximateOnePercentLowFps,
            CalculateAverageFrameTimeSeconds(frameTimeSamples) * 1000f,
            ActiveChunkCount,
            ChunkBindingCount,
            chunkRegistry != null ? chunkRegistry.CandidateId : P7BenchmarkChunkInfo.DefaultCandidateId,
            ImportedCandidateFileCount,
            ImportedCandidateBytes,
            chunkController != null ? chunkController.GetChunkStateSummaryText() : "none");
    }

    public IReadOnlyList<float> GetFrameTimeSamples()
    {
        return frameTimeSamples.AsReadOnly();
    }

    public static float CalculateAverageFps(IEnumerable<float> samples)
    {
        float totalSeconds = 0f;
        int validCount = 0;

        foreach (float sample in EnumerateValidSamples(samples))
        {
            totalSeconds += sample;
            validCount++;
        }

        if (validCount == 0 || totalSeconds <= 0f)
        {
            return 0f;
        }

        return validCount / totalSeconds;
    }

    public static float CalculateAverageFrameTimeSeconds(IEnumerable<float> samples)
    {
        float totalSeconds = 0f;
        int validCount = 0;

        foreach (float sample in EnumerateValidSamples(samples))
        {
            totalSeconds += sample;
            validCount++;
        }

        if (validCount == 0)
        {
            return 0f;
        }

        return totalSeconds / validCount;
    }

    public static float CalculateApproximateOnePercentLowFps(IEnumerable<float> samples)
    {
        List<float> validSamples = new List<float>();
        foreach (float sample in EnumerateValidSamples(samples))
        {
            validSamples.Add(sample);
        }

        if (validSamples.Count == 0)
        {
            return 0f;
        }

        validSamples.Sort((left, right) => right.CompareTo(left));
        int slowFrameCount = Mathf.Max(1, Mathf.CeilToInt(validSamples.Count * 0.01f));
        float slowFrameTotal = 0f;

        for (int i = 0; i < slowFrameCount; i++)
        {
            slowFrameTotal += validSamples[i];
        }

        float averageSlowFrameSeconds = slowFrameTotal / slowFrameCount;
        return averageSlowFrameSeconds > 0f ? 1f / averageSlowFrameSeconds : 0f;
    }

    private void OnEnable()
    {
        if (recordOnEnable)
        {
            BeginRecording();
        }
    }

    private void OnDisable()
    {
        StopRecording();
    }

    private void Update()
    {
        if (!isRecording)
        {
            return;
        }

        RecordFrameTime(Time.unscaledDeltaTime);
    }

    private void OnValidate()
    {
        if (maxSamples < 0)
        {
            maxSamples = DefaultMaxSamples;
        }
    }

    private void TrimOldSamplesIfNeeded()
    {
        if (maxSamples <= 0)
        {
            return;
        }

        while (frameTimeSamples.Count > maxSamples)
        {
            frameTimeSamples.RemoveAt(0);
        }
    }

    private static IEnumerable<float> EnumerateValidSamples(IEnumerable<float> samples)
    {
        if (samples == null)
        {
            yield break;
        }

        foreach (float sample in samples)
        {
            if (IsValidFrameTime(sample))
            {
                yield return sample;
            }
        }
    }

    private static bool IsValidFrameTime(float seconds)
    {
        return seconds > 0f && !float.IsNaN(seconds) && !float.IsInfinity(seconds);
    }
}
