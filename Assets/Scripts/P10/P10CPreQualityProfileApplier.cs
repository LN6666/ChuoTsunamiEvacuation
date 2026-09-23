using System;
using UnityEngine;

[Serializable]
public class P10CPreQualityProfileCollection
{
    public string schemaVersion = "p10c_pre.quality_profiles.v1";
    public bool projectSettingsChangeRequired;
    public bool packageChangeRequired;
    public bool renderPipelineChangeRequired;
    public string defaultProfileId = "Low";
    public P10CPreQualityProfile[] profiles = Array.Empty<P10CPreQualityProfile>();

    public bool IsSafeProfileSet()
    {
        if (projectSettingsChangeRequired || packageChangeRequired || renderPipelineChangeRequired || profiles == null || profiles.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i] == null || !profiles[i].IsBoundedAndSafe())
            {
                return false;
            }
        }

        return true;
    }
}

[Serializable]
public class P10CPreQualityProfile
{
    public string profileId = "Low";
    public string targetUse = "ordinary_pc_playability";
    public bool markerLayerEnabled = true;
    public bool crowdLayerEnabled = true;
    public bool lightCurtainEnabled;
    public float lightCurtainIntensityScale = 0.5f;
    public bool debugLabelsEnabled;
    public bool uiDebugOverlaysEnabled;
    public bool routeProxyDebugVisible;
    public bool candidateMarkerLabelsEnabled;
    public bool collapseDebrisDebugMarkersEnabled;
    public int maxNpcCount = 40;
    public int maxMarkerCount = 90;
    public int maxGreenFrameCount = 90;
    public int metricsSampleCapacity = 600;
    public float memorySampleIntervalSeconds = 1f;
    public bool applyTargetFrameRate = true;
    public int targetFrameRate = 30;
    public string notes = string.Empty;

    public bool IsBoundedAndSafe()
    {
        return maxNpcCount >= 0 &&
            maxNpcCount <= 250 &&
            maxMarkerCount >= 0 &&
            maxMarkerCount <= 300 &&
            maxGreenFrameCount >= 0 &&
            maxGreenFrameCount <= 160 &&
            metricsSampleCapacity >= 60 &&
            memorySampleIntervalSeconds > 0f &&
            targetFrameRate >= 15 &&
            targetFrameRate <= 240 &&
            lightCurtainIntensityScale >= 0f &&
            lightCurtainIntensityScale <= 1f &&
            !debugLabelsEnabled &&
            !uiDebugOverlaysEnabled;
    }
}

[Serializable]
public class P10CPreQualityProfileApplyResult
{
    public string profileId = "Low";
    public bool applied;
    public bool markerLayerActive;
    public bool crowdLayerActive;
    public bool lightCurtainActive;
    public bool debugLayerActive;
    public int npcCap;
    public int markerCap;
    public int greenFrameCap;
    public string summary = string.Empty;
}

public class P10CPreQualityProfileApplier : MonoBehaviour
{
    [SerializeField] private P10CPreQualityProfileCollection profiles = new P10CPreQualityProfileCollection();
    [SerializeField] private string profileId = "Low";
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private P10BPlusPlusRuntimeOptimizer runtimeOptimizer;
    [SerializeField] private GameObject markerLayerRoot;
    [SerializeField] private GameObject crowdLayerRoot;
    [SerializeField] private GameObject lightCurtainRoot;
    [SerializeField] private GameObject debugLayerRoot;

    private P10CPreQualityProfileApplyResult lastResult = new P10CPreQualityProfileApplyResult();

    public P10CPreQualityProfileApplyResult LastResult => lastResult;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyProfile(profileId);
        }
    }

    public P10CPreQualityProfileApplyResult ApplyProfile(string requestedProfileId)
    {
        P10CPreQualityProfile profile = FindProfile(profiles, requestedProfileId);
        if (profile == null)
        {
            profile = FindProfile(profiles, profiles == null ? "Low" : profiles.defaultProfileId) ?? new P10CPreQualityProfile();
        }

        return ApplyProfile(profile);
    }

    public P10CPreQualityProfileApplyResult ApplyProfile(P10CPreQualityProfile profile)
    {
        profile = profile ?? new P10CPreQualityProfile();
        SetRootActive(markerLayerRoot, profile.markerLayerEnabled);
        SetRootActive(crowdLayerRoot, profile.crowdLayerEnabled);
        SetRootActive(lightCurtainRoot, profile.lightCurtainEnabled);
        SetRootActive(debugLayerRoot, profile.debugLabelsEnabled || profile.uiDebugOverlaysEnabled);

        P10BPlusPlusRuntimeOptimizerState optimizerState = null;
        if (runtimeOptimizer != null)
        {
            optimizerState = runtimeOptimizer.ApplyOptimizationConfig(BuildRuntimeOptimizerConfig(profile));
        }

        lastResult = new P10CPreQualityProfileApplyResult
        {
            profileId = string.IsNullOrWhiteSpace(profile.profileId) ? "Low" : profile.profileId,
            applied = profile.IsBoundedAndSafe(),
            markerLayerActive = IsRootActive(markerLayerRoot, optimizerState == null ? profile.markerLayerEnabled : optimizerState.markerLayerActive),
            crowdLayerActive = IsRootActive(crowdLayerRoot, optimizerState == null ? profile.crowdLayerEnabled : optimizerState.crowdLayerActive),
            lightCurtainActive = IsRootActive(lightCurtainRoot, optimizerState == null ? profile.lightCurtainEnabled : optimizerState.lightCurtainActive),
            debugLayerActive = IsRootActive(debugLayerRoot, false),
            npcCap = Mathf.Max(0, profile.maxNpcCount),
            markerCap = Mathf.Max(0, profile.maxMarkerCount),
            greenFrameCap = Mathf.Max(0, profile.maxGreenFrameCount)
        };
        lastResult.summary = "P10-C-Pre quality profile applied profile=" + lastResult.profileId +
                             ", npcCap=" + lastResult.npcCap +
                             ", markerCap=" + lastResult.markerCap +
                             ", greenFrameCap=" + lastResult.greenFrameCap + ".";
        return lastResult;
    }

    public static P10BPlusPlusOptimizationConfig BuildRuntimeOptimizerConfig(P10CPreQualityProfile profile)
    {
        profile = profile ?? new P10CPreQualityProfile();
        return new P10BPlusPlusOptimizationConfig
        {
            finalWindowsExeBuildDeferredToP10C = true,
            noAddressablesOrPackagesAdded = true,
            projectSettingsChangeRequired = false,
            mutatesPlateauAssets = false,
            mutatesHighDetailScene = false,
            createsReleaseOrArchiveArtifacts = false,
            defaultQualityPreset = string.IsNullOrWhiteSpace(profile.profileId) ? "Low" : profile.profileId,
            debugLayersEnabledByDefault = profile.debugLabelsEnabled || profile.uiDebugOverlaysEnabled,
            markerLayerEnabledByDefault = profile.markerLayerEnabled,
            crowdLayerEnabledByDefault = profile.crowdLayerEnabled,
            lightCurtainEnabledByDefault = profile.lightCurtainEnabled,
            maxNpcCount = Mathf.Max(0, profile.maxNpcCount),
            maxMarkerCount = Mathf.Max(0, profile.maxMarkerCount),
            maxGreenFrameCount = Mathf.Max(0, profile.maxGreenFrameCount),
            greenFramePoolWarmupCount = Mathf.Min(Mathf.Max(0, profile.maxGreenFrameCount), 64),
            activationBudgetPerFrame = 24,
            metricsRingBufferCapacity = Mathf.Max(60, profile.metricsSampleCapacity),
            frameSpikeThresholdMs = 50f,
            uiRefreshMinIntervalSeconds = 0.25f,
            memorySampleIntervalSeconds = Mathf.Max(0.25f, profile.memorySampleIntervalSeconds),
            applyTargetFrameRate = profile.applyTargetFrameRate,
            targetFrameRate = Mathf.Clamp(profile.targetFrameRate, 15, 240),
            notes = "Generated from P10-C-Pre quality profile without ProjectSettings, Packages, PLATEAU, or scene mutation."
        };
    }

    public static P10CPreQualityProfile FindProfile(P10CPreQualityProfileCollection collection, string requestedProfileId)
    {
        if (collection == null || collection.profiles == null)
        {
            return null;
        }

        for (int i = 0; i < collection.profiles.Length; i++)
        {
            P10CPreQualityProfile profile = collection.profiles[i];
            if (profile != null && string.Equals(profile.profileId, requestedProfileId, StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }
        }

        return null;
    }

    private static void SetRootActive(GameObject root, bool active)
    {
        if (root != null)
        {
            root.SetActive(active);
        }
    }

    private static bool IsRootActive(GameObject root, bool fallback)
    {
        return root == null ? fallback : root.activeSelf;
    }
}
