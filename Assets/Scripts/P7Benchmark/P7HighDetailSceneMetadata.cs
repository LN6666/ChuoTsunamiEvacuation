using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class P7HighDetailSceneMetadata : MonoBehaviour
{
    public const string Stage = "P7-C";
    public const string SceneName = "P7_HighDetail_Chuo";
    public const string ScenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public const string RootName = "P7HighDetailRoot";
    public const string ProfilingTargetObjectName = "P7D_ProfilingTarget_Metadata";
    public const string PendingManualPlateauImportStatus = "pending_manual_plateau_sdk_import";
    public const string RuntimeSmokePendingStatus = "p2_p6_runtime_smoke_pending_high_detail_assets";
    public const string BaselineIntentStatus = "intended_p8_p9_p10_baseline_pending_p7d_confirmation";

    private static readonly string[] ExpectedLayerRoots =
    {
        "Buildings",
        "Roads",
        "Bridges",
        "Underground",
        "CityFurniture",
        "Water",
        "Vegetation",
        "Relief",
        "DisasterRisk",
        "LandUse",
        "UrbanPlanningDecision",
        "P2P6Compatibility"
    };

    [SerializeField] private string stage = Stage;
    [SerializeField] private string scenePath = ScenePath;
    [SerializeField] private string importStatus = PendingManualPlateauImportStatus;
    [SerializeField] private string compatibilityStatus = RuntimeSmokePendingStatus;
    [SerializeField] private string baselineIntent = BaselineIntentStatus;
    [SerializeField] private bool p7DProfilingTarget = true;
    [SerializeField] private bool actualPlateauAssetsLoaded;
    [SerializeField] private bool modifiesGameplaySuccessFailure;

    public string SceneStage => string.IsNullOrWhiteSpace(stage) ? Stage : stage;
    public string HighDetailScenePath => string.IsNullOrWhiteSpace(scenePath) ? ScenePath : scenePath;
    public string ImportStatus => string.IsNullOrWhiteSpace(importStatus) ? PendingManualPlateauImportStatus : importStatus;
    public string CompatibilityStatus => string.IsNullOrWhiteSpace(compatibilityStatus) ? RuntimeSmokePendingStatus : compatibilityStatus;
    public string BaselineIntent => string.IsNullOrWhiteSpace(baselineIntent) ? BaselineIntentStatus : baselineIntent;
    public bool IsP7DProfilingTarget => p7DProfilingTarget;
    public bool ActualPlateauAssetsLoaded => actualPlateauAssetsLoaded;
    public bool AffectsGameplaySuccessFailure => modifiesGameplaySuccessFailure;
    public static bool ModifiesGameplaySuccessFailure => false;
    public static IReadOnlyList<string> ExpectedLayerRootNames => ExpectedLayerRoots;

    public void ResetToDefaults()
    {
        stage = Stage;
        scenePath = ScenePath;
        importStatus = PendingManualPlateauImportStatus;
        compatibilityStatus = RuntimeSmokePendingStatus;
        baselineIntent = BaselineIntentStatus;
        p7DProfilingTarget = true;
        actualPlateauAssetsLoaded = false;
        modifiesGameplaySuccessFailure = false;
    }

    public string GetSummaryText()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "P7HighDetailSceneMetadata stage={0}; scene={1}; targetScenePath={2}; p7DProfilingTarget={3}; actualPlateauAssetsLoaded={4}; importStatus={5}; compatibilityStatus={6}; baselineIntent={7}; affectsGameplaySuccessFailure={8}",
            SceneStage,
            SceneName,
            HighDetailScenePath,
            IsP7DProfilingTarget,
            ActualPlateauAssetsLoaded,
            ImportStatus,
            CompatibilityStatus,
            BaselineIntent,
            AffectsGameplaySuccessFailure);
    }

    public static bool IsExpectedLayerRootName(string rootName)
    {
        if (string.IsNullOrWhiteSpace(rootName))
        {
            return false;
        }

        foreach (string expectedRoot in ExpectedLayerRoots)
        {
            if (string.Equals(expectedRoot, rootName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsP7DTargetScenePath(string path)
    {
        return string.Equals(NormalizePath(path), ScenePath, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPendingImportStatus(string status)
    {
        return string.Equals(status, PendingManualPlateauImportStatus, StringComparison.OrdinalIgnoreCase);
    }

    private void Reset()
    {
        ResetToDefaults();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(stage))
        {
            stage = Stage;
        }

        if (string.IsNullOrWhiteSpace(scenePath))
        {
            scenePath = ScenePath;
        }

        if (string.IsNullOrWhiteSpace(importStatus))
        {
            importStatus = PendingManualPlateauImportStatus;
        }

        if (string.IsNullOrWhiteSpace(compatibilityStatus))
        {
            compatibilityStatus = RuntimeSmokePendingStatus;
        }

        if (string.IsNullOrWhiteSpace(baselineIntent))
        {
            baselineIntent = BaselineIntentStatus;
        }

        modifiesGameplaySuccessFailure = false;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace("\\", "/");
    }
}
