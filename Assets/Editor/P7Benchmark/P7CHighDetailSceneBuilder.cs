using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class P7CHighDetailSceneBuilder
{
    private const string MenuPath = "Tools/P7 Benchmark/P7-C Build High Detail Scene Shell";
    private const string ImportPendingLabelName = "PLATEAUImportStatus_PendingManualSdkImport";
    private const string BaselineIntentLabelName = "P8P9P10_BaselineIntent_PendingP7DConfirmation";
    private const string NoGameplayChangeLabelName = "NoP8P9Systems_NoGameplayRuleChanges";
    private const string TargetSettingsRootName = "PLATEAUSdkTargetSettings_MetadataOnly";
    private const string CompatibilityChecklistRootName = "P2P6CompatibilityChecklist_MetadataOnly";
    private const string OverviewCameraName = "P7HighDetail_OverviewCamera";
    private const string DirectionalLightName = "P7HighDetail_DirectionalLight";

    private static readonly Dictionary<string, string> TargetLodByLayer = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Buildings", "LOD3" },
        { "Roads", "LOD3" },
        { "Bridges", "LOD3" },
        { "Underground", "LOD3 target if available" },
        { "CityFurniture", "LOD2 or LOD3 depending availability and performance" },
        { "Water", "LOD1" },
        { "Vegetation", "LOD3 target if available" },
        { "Relief", "import terrain relief" },
        { "DisasterRisk", "import disaster risk attributes" },
        { "LandUse", "import land use" },
        { "UrbanPlanningDecision", "LOD1" },
        { "P2P6Compatibility", "runtime smoke pending high-detail assets" }
    };

    [MenuItem(MenuPath)]
    public static void BuildFromMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("P7-C high-detail scene build is editor-only. Exit Play Mode and run the menu item again.");
            return;
        }

        BuildSceneShell();
    }

    public static void BuildFromCommandLine()
    {
        try
        {
            BuildSceneShell();

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"P7-C high-detail scene shell build failed: {exception}");

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    public static Scene BuildSceneShell()
    {
        EnsureAssetDirectory(P7HighDetailSceneMetadata.ScenePath);

        Scene scene = File.Exists(P7HighDetailSceneMetadata.ScenePath)
            ? EditorSceneManager.OpenScene(P7HighDetailSceneMetadata.ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = GameObject.Find(P7HighDetailSceneMetadata.RootName);
        if (root == null)
        {
            root = new GameObject(P7HighDetailSceneMetadata.RootName);
        }

        P7HighDetailSceneMetadata metadata = GetOrAddComponent<P7HighDetailSceneMetadata>(root);
        metadata.ResetToDefaults();

        GetOrAddComponent<P7BenchmarkMetricsRecorder>(root);
        CreateHighDetailMetadata(root.transform);
        CreateLayerRoots(root.transform);
        CreateCompatibilityChecklist(root.transform);
        CreateSceneCameraAndLight();

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(metadata);

        if (!EditorSceneManager.SaveScene(scene, P7HighDetailSceneMetadata.ScenePath))
        {
            throw new InvalidOperationException($"Unity failed to save {P7HighDetailSceneMetadata.ScenePath}.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"P7-C high-detail scene shell build complete: {P7HighDetailSceneMetadata.ScenePath}");
        return scene;
    }

    private static void CreateHighDetailMetadata(Transform root)
    {
        GameObject targetSettingsRoot = GetOrCreateChild(root, TargetSettingsRootName);
        GetOrCreateChild(root, P7HighDetailSceneMetadata.ProfilingTargetObjectName);
        GetOrCreateChild(root, ImportPendingLabelName);
        GetOrCreateChild(root, BaselineIntentLabelName);
        GetOrCreateChild(root, NoGameplayChangeLabelName);

        CreateTargetLabel(targetSettingsRoot.transform, "Buildings", "import target LOD3");
        CreateTargetLabel(targetSettingsRoot.transform, "Roads", "import target LOD3");
        CreateTargetLabel(targetSettingsRoot.transform, "UrbanPlanningDecision", "import target LOD1");
        CreateTargetLabel(targetSettingsRoot.transform, "LandUse", "import");
        CreateTargetLabel(targetSettingsRoot.transform, "Underground", "import target LOD3 if available");
        CreateTargetLabel(targetSettingsRoot.transform, "CityFurniture", "import target LOD2 or LOD3");
        CreateTargetLabel(targetSettingsRoot.transform, "Water", "import target LOD1");
        CreateTargetLabel(targetSettingsRoot.transform, "Vegetation", "import target LOD3 if available");
        CreateTargetLabel(targetSettingsRoot.transform, "Bridges", "import target LOD3");
        CreateTargetLabel(targetSettingsRoot.transform, "DisasterRisk", "import");
        CreateTargetLabel(targetSettingsRoot.transform, "Relief", "import terrain relief");
        GetOrCreateChild(targetSettingsRoot.transform, "screenshots_are_target_settings_not_completion_evidence");
    }

    private static void CreateLayerRoots(Transform root)
    {
        foreach (string layerName in P7HighDetailSceneMetadata.ExpectedLayerRootNames)
        {
            GameObject layerRoot = GetOrCreateChild(root, layerName);
            string targetLod = TargetLodByLayer.ContainsKey(layerName) ? TargetLodByLayer[layerName] : "not specified";
            GetOrCreateChild(layerRoot.transform, $"Target_{SanitizeName(targetLod)}");
            GetOrCreateChild(layerRoot.transform, "ActualLoadedStatus_NotImported_RenderableEvidencePending");
        }
    }

    private static void CreateCompatibilityChecklist(Transform root)
    {
        GameObject checklistRoot = GetOrCreateChild(root, CompatibilityChecklistRootName);
        RenameChildIfExists(
            checklistRoot.transform,
            "GameManager_no_Chuo_BaseMap_hard_binding_to_verify",
            "GameManager_old_scene_hard_binding_to_verify");

        string[] checklistItems =
        {
            "PlayerMovement_runtime_smoke_pending",
            "Camera_runtime_smoke_pending",
            "GameManager_old_scene_hard_binding_to_verify",
            "ResultPanel_success_failure_rules_unchanged",
            "ShelterInteraction_placeholder_targets_required",
            "P5DataLoaders_scene_independent_check_required",
            "P6NavigationGuidance_target_objects_required",
            "P6NpcPrototype_test_staging_required"
        };

        foreach (string checklistItem in checklistItems)
        {
            GetOrCreateChild(checklistRoot.transform, checklistItem);
        }
    }

    private static void RenameChildIfExists(Transform parent, string oldName, string newName)
    {
        Transform existing = parent.Find(oldName);
        if (existing != null)
        {
            existing.name = newName;
        }
    }

    private static void CreateSceneCameraAndLight()
    {
        GameObject cameraObject = GameObject.Find(OverviewCameraName);
        if (cameraObject == null)
        {
            cameraObject = new GameObject(OverviewCameraName);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 2000f;
            camera.transform.position = new Vector3(0f, 120f, -220f);
            camera.transform.rotation = Quaternion.Euler(32f, 0f, 0f);
        }

        GameObject lightObject = GameObject.Find(DirectionalLightName);
        if (lightObject == null)
        {
            lightObject = new GameObject(DirectionalLightName);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    private static void CreateTargetLabel(Transform parent, string category, string target)
    {
        GetOrCreateChild(parent, $"{category}_{SanitizeName(target)}");
    }

    private static GameObject GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    private static void EnsureAssetDirectory(string assetPath)
    {
        string directory = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unspecified";
        }

        char[] chars = value.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private static bool ShouldExitAfterCommandLine()
    {
        if (Application.isBatchMode)
        {
            return true;
        }

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "-p7cBuildHighDetailSceneQuit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
