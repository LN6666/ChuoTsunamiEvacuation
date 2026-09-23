using System;
using UnityEngine;

[Serializable]
public class P10BPlusPlusRuntimeOptimizerState
{
    public string qualityPreset = "Medium";
    public bool markerLayerActive;
    public bool crowdLayerActive;
    public bool lightCurtainActive;
    public bool debugLayerActive;
    public int npcCap;
    public int markerCap;
    public int greenFrameCap;
    public int warmedGreenFramePoolCount;
    public string summary = string.Empty;
}

public class P10BPlusPlusRuntimeOptimizer : MonoBehaviour
{
    [SerializeField] private P10BPlusPlusOptimizationConfig config = new P10BPlusPlusOptimizationConfig();
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private GameObject markerLayerRoot;
    [SerializeField] private GameObject crowdLayerRoot;
    [SerializeField] private GameObject lightCurtainRoot;
    [SerializeField] private GameObject debugLayerRoot;
    [SerializeField] private P10BGreenGroundFrameRuntime greenFrameRuntime;

    private P10BPlusPlusRuntimeOptimizerState lastState = new P10BPlusPlusRuntimeOptimizerState();

    public P10BPlusPlusRuntimeOptimizerState LastState => lastState;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyOptimizationConfig(config);
        }
    }

    public P10BPlusPlusRuntimeOptimizerState ApplyOptimizationConfig(P10BPlusPlusOptimizationConfig newConfig)
    {
        config = newConfig ?? new P10BPlusPlusOptimizationConfig();
        SetLayerActive(markerLayerRoot, config.markerLayerEnabledByDefault);
        SetLayerActive(crowdLayerRoot, config.crowdLayerEnabledByDefault);
        SetLayerActive(lightCurtainRoot, config.lightCurtainEnabledByDefault);
        SetLayerActive(debugLayerRoot, config.debugLayersEnabledByDefault);

        if (config.applyTargetFrameRate)
        {
            Application.targetFrameRate = Mathf.Clamp(config.targetFrameRate, 15, 240);
        }

        int warmedCount = 0;
        if (greenFrameRuntime != null)
        {
            warmedCount = greenFrameRuntime.WarmedPoolObjectCount;
            if (warmedCount == 0)
            {
                warmedCount = greenFrameRuntime.WarmupFramePool();
            }
        }

        lastState = new P10BPlusPlusRuntimeOptimizerState
        {
            qualityPreset = string.IsNullOrWhiteSpace(config.defaultQualityPreset) ? "Medium" : config.defaultQualityPreset,
            markerLayerActive = IsLayerActive(markerLayerRoot, config.markerLayerEnabledByDefault),
            crowdLayerActive = IsLayerActive(crowdLayerRoot, config.crowdLayerEnabledByDefault),
            lightCurtainActive = IsLayerActive(lightCurtainRoot, config.lightCurtainEnabledByDefault),
            debugLayerActive = IsLayerActive(debugLayerRoot, config.debugLayersEnabledByDefault),
            npcCap = Mathf.Max(0, config.maxNpcCount),
            markerCap = Mathf.Max(0, config.maxMarkerCount),
            greenFrameCap = Mathf.Max(0, config.maxGreenFrameCount),
            warmedGreenFramePoolCount = warmedCount
        };
        lastState.summary = "P10-B++ runtime optimizer applied preset=" + lastState.qualityPreset +
                            ", debugLayerActive=" + lastState.debugLayerActive +
                            ", greenFrameCap=" + lastState.greenFrameCap + ".";
        return lastState;
    }

    public void SetDebugLayerEnabled(bool enabled)
    {
        SetLayerActive(debugLayerRoot, enabled);
        lastState.debugLayerActive = enabled;
    }

    public void SetMarkerLayerEnabled(bool enabled)
    {
        SetLayerActive(markerLayerRoot, enabled);
        lastState.markerLayerActive = enabled;
    }

    public void SetCrowdLayerEnabled(bool enabled)
    {
        SetLayerActive(crowdLayerRoot, enabled);
        lastState.crowdLayerActive = enabled;
    }

    public void SetLightCurtainEnabled(bool enabled)
    {
        SetLayerActive(lightCurtainRoot, enabled);
        lastState.lightCurtainActive = enabled;
    }

    public static int ClampCountToCap(int requestedCount, int cap)
    {
        return Mathf.Clamp(requestedCount, 0, Mathf.Max(0, cap));
    }

    private static void SetLayerActive(GameObject layerRoot, bool active)
    {
        if (layerRoot != null)
        {
            layerRoot.SetActive(active);
        }
    }

    private static bool IsLayerActive(GameObject layerRoot, bool fallback)
    {
        return layerRoot == null ? fallback : layerRoot.activeSelf;
    }
}
