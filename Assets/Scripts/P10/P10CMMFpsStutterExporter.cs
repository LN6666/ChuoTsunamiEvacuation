using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

[Serializable]
public class P10CMMRuntimeStateSnapshot
{
    public string sceneName = string.Empty;
    public string scenePath = string.Empty;
    public string activeQualityProfile = "Low";
    public string unityQualityLevelName = string.Empty;
    public string activeWeatherNightMode = "clear_day";
    public bool nightOverlayActive;
    public bool greenFrameActive;
    public int greenFrameCount;
    public int greenFrameRuntimeCount;
    public bool candidateMarkerRuntimeActive;
    public int candidateMarkerCount;
    public bool lightCurtainActive;
    public int lightCurtainObjectCount;
    public bool crowdActive;
    public int crowdAgentCount;
    public bool resultPanelActive;
    public int resultPanelObjectCount;
    public string evidence = string.Empty;
}

[Serializable]
public class P10CMMFpsStutterSummary
{
    public string schemaVersion = "p10c_mm.fps_stutter_summary.v1";
    public string status = "prepared_not_run";
    public bool p10cMinusMinusIsExtendedPerformanceGate = true;
    public bool officialP10CReleaseDeferred = true;
    public bool finalReleasePackageCreated;
    public bool finalArchiveCreated;
    public bool noP10EFG = true;
    public string scenarioLabel = "baseline_idle";
    public string captureStartedAtLocal = string.Empty;
    public string captureFinishedAtLocal = string.Empty;
    public float requestedCaptureDurationSeconds;
    public float actualCaptureDurationSeconds;
    public int sampleCapacity;
    public int sampleCount;
    public float averageFps;
    public float minFps;
    public float onePercentLowFps;
    public float minFrameTimeMs;
    public float averageFrameTimeMs;
    public float maxFrameTimeMs;
    public float longestFrameTimeMs;
    public int frameSpikeCountOver33Ms;
    public int frameSpikeCountOver50Ms;
    public int frameSpikeCountOver100Ms;
    public float stutterThresholdMs = 50f;
    public int stutterEventCount;
    public float startupTimeSeconds;
    public float sceneLoadingTimeSeconds;
    public float managedHeapMb;
    public float profilerAllocatedMemoryMb;
    public int gcCollectionDelta0;
    public int gcCollectionDelta1;
    public int gcCollectionDelta2;
    public P10CMMRuntimeStateSnapshot runtimeState = new P10CMMRuntimeStateSnapshot();
    public string outputPath = string.Empty;
    public string[] limitations = Array.Empty<string>();
    public string summary = string.Empty;
}

public class P10CMMFrameSampler
{
    private readonly float[] samplesSeconds;
    private int nextIndex;
    private int sampleCount;

    public P10CMMFrameSampler(int capacity)
    {
        samplesSeconds = new float[Mathf.Max(1, capacity)];
    }

    public int Capacity => samplesSeconds.Length;
    public int SampleCount => sampleCount;

    public void Clear()
    {
        nextIndex = 0;
        sampleCount = 0;
        Array.Clear(samplesSeconds, 0, samplesSeconds.Length);
    }

    public bool Add(float frameTimeSeconds)
    {
        if (float.IsNaN(frameTimeSeconds) || float.IsInfinity(frameTimeSeconds) || frameTimeSeconds <= 0f)
        {
            return false;
        }

        samplesSeconds[nextIndex] = frameTimeSeconds;
        nextIndex = (nextIndex + 1) % samplesSeconds.Length;
        sampleCount = Mathf.Min(sampleCount + 1, samplesSeconds.Length);
        return true;
    }

    public P10CMMFpsStutterSummary CreateSummary(
        string scenarioLabel,
        float actualCaptureDurationSeconds,
        float requestedCaptureDurationSeconds,
        float stutterThresholdMs)
    {
        var summary = new P10CMMFpsStutterSummary
        {
            status = sampleCount > 0 ? "captured" : "no_samples",
            scenarioLabel = string.IsNullOrWhiteSpace(scenarioLabel) ? "baseline_idle" : scenarioLabel,
            requestedCaptureDurationSeconds = Mathf.Max(0f, requestedCaptureDurationSeconds),
            actualCaptureDurationSeconds = Mathf.Max(0f, actualCaptureDurationSeconds),
            sampleCapacity = Capacity,
            sampleCount = sampleCount,
            stutterThresholdMs = Mathf.Max(0.001f, stutterThresholdMs)
        };

        if (sampleCount == 0)
        {
            summary.limitations = new[] { "No valid frame-time samples were captured." };
            summary.summary = "P10-C-- FPS/stutter capture produced no valid frame samples.";
            return summary;
        }

        float[] sorted = new float[sampleCount];
        float total = 0f;
        float min = float.MaxValue;
        float max = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float frameTime = samplesSeconds[i];
            sorted[i] = frameTime;
            total += frameTime;
            min = Mathf.Min(min, frameTime);
            max = Mathf.Max(max, frameTime);
            if (frameTime >= 0.033f)
            {
                summary.frameSpikeCountOver33Ms++;
            }
            if (frameTime >= 0.050f)
            {
                summary.frameSpikeCountOver50Ms++;
            }
            if (frameTime >= 0.100f)
            {
                summary.frameSpikeCountOver100Ms++;
            }
            if (frameTime * 1000f >= summary.stutterThresholdMs)
            {
                summary.stutterEventCount++;
            }
        }

        Array.Sort(sorted);
        float average = total / sampleCount;
        int onePercentIndex = Mathf.Clamp(Mathf.CeilToInt(sampleCount * 0.99f) - 1, 0, sampleCount - 1);
        float onePercentFrameTime = sorted[onePercentIndex];

        summary.minFrameTimeMs = min * 1000f;
        summary.averageFrameTimeMs = average * 1000f;
        summary.maxFrameTimeMs = max * 1000f;
        summary.longestFrameTimeMs = summary.maxFrameTimeMs;
        summary.averageFps = average <= 0f ? 0f : 1f / average;
        summary.minFps = max <= 0f ? 0f : 1f / max;
        summary.onePercentLowFps = onePercentFrameTime <= 0f ? 0f : 1f / onePercentFrameTime;
        summary.summary = "P10-C-- FPS/stutter capture scenario=" + summary.scenarioLabel +
                          ", samples=" + summary.sampleCount +
                          ", avgFPS=" + summary.averageFps.ToString("0.0") +
                          ", 1%low=" + summary.onePercentLowFps.ToString("0.0") +
                          ", stutters=" + summary.stutterEventCount + ".";
        return summary;
    }
}

public class P10CMMFpsStutterExporter : MonoBehaviour
{
    private const string DisableArg = "-p10cMmDisableFpsExporter";
    private const string OutputPathArg = "-p10cMmFpsSummaryPath";
    private const string CaptureSecondsArg = "-p10cMmCaptureSeconds";
    private const string ScenarioArg = "-p10cMmScenario";
    private const string QualityProfileArg = "-p10cMmQualityProfile";
    private const string WeatherModeArg = "-p10cMmWeatherMode";
    private const string SampleCapacityArg = "-p10cMmSampleCapacity";
    private const string StutterThresholdArg = "-p10cMmStutterThresholdMs";
    private const int DefaultSampleCapacity = 60000;

    private static float playerStartRealtime;
    private static bool installed;

    [SerializeField] private string scenarioLabel = "baseline_idle";
    [SerializeField] private string activeQualityProfile = "Low";
    [SerializeField] private string activeWeatherNightMode = "clear_day";
    [SerializeField] private string outputPath = string.Empty;
    [SerializeField] private float requestedCaptureDurationSeconds = 600f;
    [SerializeField] private float stutterThresholdMs = 50f;
    [SerializeField] private int sampleCapacity = DefaultSampleCapacity;

    private P10CMMFrameSampler sampler;
    private float captureStartRealtime;
    private float sceneLoadedRealtime;
    private long startManagedBytes;
    private int startGc0;
    private int startGc1;
    private int startGc2;
    private bool summaryWritten;
    private P10CMMFpsStutterSummary lastSummary = new P10CMMFpsStutterSummary();

    public P10CMMFpsStutterSummary LastSummary => lastSummary;
    public int SampleCount => sampler == null ? 0 : sampler.SampleCount;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void MarkPlayerStart()
    {
        playerStartRealtime = Time.realtimeSinceStartup;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallInPlayer()
    {
        if (installed || HasArgument(DisableArg))
        {
            return;
        }

        installed = true;
        var exporterObject = new GameObject("P10CMM_FpsStutterExporter");
        DontDestroyOnLoad(exporterObject);
        P10CMMFpsStutterExporter exporter = exporterObject.AddComponent<P10CMMFpsStutterExporter>();
        exporter.ConfigureFromCommandLine(Environment.GetCommandLineArgs());
        exporter.BeginCapture();
    }
#endif

    private void Update()
    {
        if (sampler == null || summaryWritten)
        {
            return;
        }

        sampler.Add(Time.unscaledDeltaTime);
        float elapsed = Time.realtimeSinceStartup - captureStartRealtime;
        if (requestedCaptureDurationSeconds > 0f && elapsed >= requestedCaptureDurationSeconds)
        {
            WriteSummary("duration_reached");
        }
    }

    private void OnApplicationQuit()
    {
        if (!summaryWritten && sampler != null)
        {
            WriteSummary("application_quit");
        }
    }

    public void Configure(
        string newScenarioLabel,
        string newQualityProfile,
        string newWeatherMode,
        string newOutputPath,
        float newRequestedCaptureDurationSeconds,
        int newSampleCapacity,
        float newStutterThresholdMs)
    {
        scenarioLabel = string.IsNullOrWhiteSpace(newScenarioLabel) ? "baseline_idle" : newScenarioLabel;
        activeQualityProfile = string.IsNullOrWhiteSpace(newQualityProfile) ? "Low" : newQualityProfile;
        activeWeatherNightMode = string.IsNullOrWhiteSpace(newWeatherMode) ? "clear_day" : newWeatherMode;
        outputPath = string.IsNullOrWhiteSpace(newOutputPath) ? GetDefaultOutputPath() : newOutputPath;
        requestedCaptureDurationSeconds = Mathf.Max(0f, newRequestedCaptureDurationSeconds);
        sampleCapacity = Mathf.Clamp(newSampleCapacity, 60, 120000);
        stutterThresholdMs = Mathf.Max(0.001f, newStutterThresholdMs);
        sampler = new P10CMMFrameSampler(sampleCapacity);
    }

    public void ConfigureFromCommandLine(string[] args)
    {
        Configure(
            GetArgumentValue(args, ScenarioArg, "baseline_idle"),
            GetArgumentValue(args, QualityProfileArg, "Low"),
            GetArgumentValue(args, WeatherModeArg, "clear_day"),
            GetArgumentValue(args, OutputPathArg, GetDefaultOutputPath()),
            ParseFloat(GetArgumentValue(args, CaptureSecondsArg, "600"), 600f),
            ParseInt(GetArgumentValue(args, SampleCapacityArg, DefaultSampleCapacity.ToString()), DefaultSampleCapacity),
            ParseFloat(GetArgumentValue(args, StutterThresholdArg, "50"), 50f));
    }

    public void BeginCapture()
    {
        if (sampler == null)
        {
            Configure(scenarioLabel, activeQualityProfile, activeWeatherNightMode, outputPath, requestedCaptureDurationSeconds, sampleCapacity, stutterThresholdMs);
        }

        sampler.Clear();
        summaryWritten = false;
#if !UNITY_EDITOR
        Application.runInBackground = true;
#endif
        captureStartRealtime = Time.realtimeSinceStartup;
        sceneLoadedRealtime = captureStartRealtime;
        startManagedBytes = GC.GetTotalMemory(false);
        startGc0 = GC.CollectionCount(0);
        startGc1 = GC.CollectionCount(1);
        startGc2 = GC.CollectionCount(2);
    }

    public P10CMMFpsStutterSummary FinishCaptureForTest(float elapsedSeconds)
    {
        if (sampler == null)
        {
            Configure(scenarioLabel, activeQualityProfile, activeWeatherNightMode, outputPath, requestedCaptureDurationSeconds, sampleCapacity, stutterThresholdMs);
        }

        return BuildSummary("test_finished", Mathf.Max(0f, elapsedSeconds));
    }

    public bool RecordFrameForTest(float frameTimeSeconds)
    {
        if (sampler == null)
        {
            Configure(scenarioLabel, activeQualityProfile, activeWeatherNightMode, outputPath, requestedCaptureDurationSeconds, sampleCapacity, stutterThresholdMs);
        }

        return sampler.Add(frameTimeSeconds);
    }

    private void WriteSummary(string status)
    {
        float elapsed = Mathf.Max(0f, Time.realtimeSinceStartup - captureStartRealtime);
        lastSummary = BuildSummary(status, elapsed);
        lastSummary.outputPath = outputPath;

        try
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, JsonUtility.ToJson(lastSummary, true), new UTF8Encoding(false));
            Debug.Log("P10-C-- FPS/stutter summary written to " + outputPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-- FPS/stutter summary could not be written: " + exception.Message);
        }
        finally
        {
            summaryWritten = true;
        }
    }

    private P10CMMFpsStutterSummary BuildSummary(string status, float elapsedSeconds)
    {
        P10CMMFpsStutterSummary summary = sampler.CreateSummary(
            scenarioLabel,
            elapsedSeconds,
            requestedCaptureDurationSeconds,
            stutterThresholdMs);

        summary.status = status;
        summary.captureStartedAtLocal = DateTime.Now.AddSeconds(-elapsedSeconds).ToString("s");
        summary.captureFinishedAtLocal = DateTime.Now.ToString("s");
        summary.startupTimeSeconds = Mathf.Max(0f, sceneLoadedRealtime - playerStartRealtime);
        summary.sceneLoadingTimeSeconds = summary.startupTimeSeconds;
        summary.managedHeapMb = GC.GetTotalMemory(false) / (1024f * 1024f);
        summary.profilerAllocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        summary.gcCollectionDelta0 = Mathf.Max(0, GC.CollectionCount(0) - startGc0);
        summary.gcCollectionDelta1 = Mathf.Max(0, GC.CollectionCount(1) - startGc1);
        summary.gcCollectionDelta2 = Mathf.Max(0, GC.CollectionCount(2) - startGc2);
        summary.runtimeState = CaptureRuntimeState();
        summary.limitations = BuildLimitations(summary.runtimeState);
        summary.summary = "P10-C-- FPS/stutter capture scenario=" + summary.scenarioLabel +
                          ", status=" + summary.status +
                          ", samples=" + summary.sampleCount +
                          ", avgFPS=" + summary.averageFps.ToString("0.0") +
                          ", 1%low=" + summary.onePercentLowFps.ToString("0.0") +
                          ", maxFrameMs=" + summary.maxFrameTimeMs.ToString("0.0") +
                          ", stutters=" + summary.stutterEventCount + ".";
        return summary;
    }

    private P10CMMRuntimeStateSnapshot CaptureRuntimeState()
    {
        Scene scene = SceneManager.GetActiveScene();
        var snapshot = new P10CMMRuntimeStateSnapshot
        {
            sceneName = scene.name ?? string.Empty,
            scenePath = scene.path ?? string.Empty,
            activeQualityProfile = activeQualityProfile ?? "Low",
            unityQualityLevelName = GetUnityQualityLevelName(),
            activeWeatherNightMode = activeWeatherNightMode ?? "clear_day"
        };

        P10BGreenGroundFrameRuntime[] greenFrameRuntimes = FindObjectsOfType<P10BGreenGroundFrameRuntime>();
        snapshot.greenFrameRuntimeCount = greenFrameRuntimes.Length;
        for (int i = 0; i < greenFrameRuntimes.Length; i++)
        {
            P10BGreenGroundFrameMetrics metrics = greenFrameRuntimes[i].LastMetrics;
            if (greenFrameRuntimes[i].TsunamiStarted || metrics.activeFrameCount > 0)
            {
                snapshot.greenFrameActive = true;
            }
            snapshot.greenFrameCount += Mathf.Max(0, metrics.activeFrameCount);
        }

        P9HumanitarianCandidateMarkerRuntime[] candidateMarkers = FindObjectsOfType<P9HumanitarianCandidateMarkerRuntime>();
        snapshot.candidateMarkerCount = candidateMarkers.Length;
        snapshot.candidateMarkerRuntimeActive = candidateMarkers.Length > 0;

        P9CrowdRuntimeAgent[] crowdAgents = FindObjectsOfType<P9CrowdRuntimeAgent>();
        snapshot.crowdAgentCount = crowdAgents.Length;
        snapshot.crowdActive = crowdAgents.Length > 0;

        P8RiskFrontController[] riskFrontControllers = FindObjectsOfType<P8RiskFrontController>();
        for (int i = 0; i < riskFrontControllers.Length; i++)
        {
            if (riskFrontControllers[i].IsVisualVisible)
            {
                snapshot.lightCurtainActive = true;
            }
        }

        snapshot.lightCurtainObjectCount = CountActiveSceneObjectsByName("LightCurtain") + CountActiveSceneObjectsByName("RiskFront");
        snapshot.lightCurtainActive = snapshot.lightCurtainActive || snapshot.lightCurtainObjectCount > 0;
        snapshot.resultPanelObjectCount = CountActiveSceneObjectsByName("ResultPanel");
        snapshot.resultPanelActive = snapshot.resultPanelObjectCount > 0;
        P10BPlusNightOverlay[] nightOverlays = FindObjectsOfType<P10BPlusNightOverlay>();
        for (int i = 0; i < nightOverlays.Length; i++)
        {
            snapshot.nightOverlayActive = snapshot.nightOverlayActive || nightOverlays[i].OverlayEnabled;
        }

        snapshot.evidence = "Runtime state inferred from active scene objects/components; command-line labels provide scenario, quality profile, and weather mode.";
        return snapshot;
    }

    private static string[] BuildLimitations(P10CMMRuntimeStateSnapshot snapshot)
    {
        var limitations = new System.Collections.Generic.List<string>();
        if (snapshot == null)
        {
            limitations.Add("Runtime state snapshot was unavailable.");
            return limitations.ToArray();
        }

        if (!snapshot.greenFrameActive)
        {
            limitations.Add("Green frames were not observed active during this capture.");
        }
        if (!snapshot.lightCurtainActive)
        {
            limitations.Add("Light curtain/risk-front visual was not observed active during this capture.");
        }
        if (!snapshot.crowdActive)
        {
            limitations.Add("Crowd agents were not observed active during this capture.");
        }
        if (!snapshot.resultPanelActive)
        {
            limitations.Add("ResultPanel was not observed active during this capture.");
        }

        return limitations.ToArray();
    }

    private static int CountActiveSceneObjectsByName(string nameFragment)
    {
        int count = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject candidate = objects[i];
            if (candidate == null ||
                !candidate.scene.IsValid() ||
                !candidate.activeInHierarchy ||
                candidate.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private static string GetUnityQualityLevelName()
    {
        string[] names = QualitySettings.names;
        int level = QualitySettings.GetQualityLevel();
        if (names == null || level < 0 || level >= names.Length)
        {
            return level.ToString();
        }

        return names[level] ?? string.Empty;
    }

    private static string GetDefaultOutputPath()
    {
        return Path.Combine(Application.persistentDataPath, "p10c_mm_fps_stutter_summary.json");
    }

    private static string GetArgumentValue(string[] args, string name, string fallback)
    {
        if (args == null)
        {
            return fallback;
        }

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return fallback;
    }

    private static bool HasArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int ParseInt(string value, int fallback)
    {
        return int.TryParse(value, out int parsed) ? parsed : fallback;
    }

    private static float ParseFloat(string value, float fallback)
    {
        return float.TryParse(value, out float parsed) ? parsed : fallback;
    }
}
