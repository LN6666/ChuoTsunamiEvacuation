#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NewMapSceneSetupUtility
{
    private const string ScenePath = "Assets/Scenes/Chuo_BaseMap.unity";
    private const string AuditOutputPath = "Assets/Data/P10/newmap_unity_scene_audit.json";

    [MenuItem("Tools/Chuo Evacuation/New Map/Ensure Chuo BaseMap Runtime Setup")]
    public static void EnsureChuoBaseMapRuntimeSetup()
    {
        EnsureSceneRootsAndWriteAudit(saveScene: true);
    }

    public static void EnsureChuoBaseMapRuntimeSetupCommandLine()
    {
        try
        {
            EnsureSceneRootsAndWriteAudit(saveScene: true);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            EditorApplication.Exit(1);
        }
    }

    public static void BuildNewMapTempPlayerCommandLine()
    {
        BuildNewMapPlayer("D:/UnityProjects/ChuoTsunamiEvacuation-Builds/NewMapPre", "ChuoTsunamiEvacuation_NewMapPre.exe");
    }

    public static void BuildNewMapFinalTempPlayerCommandLine()
    {
        BuildNewMapPlayer("D:/UnityProjects/ChuoTsunamiEvacuation-Builds/NewMapFinalPre", "ChuoTsunamiEvacuation_NewMapFinalPre.exe");
    }

    public static void BuildNewMapHardeningTempPlayerCommandLine()
    {
        BuildNewMapPlayer("D:/UnityProjects/ChuoTsunamiEvacuation-Builds/NewMapHardeningPre", "ChuoTsunamiEvacuation_NewMapHardeningPre.exe");
    }

    private static void BuildNewMapPlayer(string buildDirectory, string executableName)
    {
        Directory.CreateDirectory(buildDirectory);
        string exePath = Path.Combine(buildDirectory, executableName);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        try
        {
            var report = BuildPipeline.BuildPlayer(options);
            int result = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1;
            Debug.Log($"NewMap temp player build result={report.summary.result} path={exePath} size={report.summary.totalSize}");
            EditorApplication.Exit(result);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void EnsureSceneRootsAndWriteAudit(bool saveScene)
    {
        Directory.CreateDirectory("Assets/Data/P10");
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new InvalidOperationException($"Could not open {ScenePath}");
        }

        NewMapRuntimeBootstrap.EnsureRoots();
        SceneAudit audit = BuildSceneAudit(scene);
        File.WriteAllText(AuditOutputPath, JsonUtility.ToJson(audit, true), new UTF8Encoding(false));
        AssetDatabase.ImportAsset(AuditOutputPath);

        if (saveScene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log(
            $"NewMap scene setup complete. roots={audit.requiredRootCount} missingRoots={audit.missingRootCount} " +
            $"renderers={audit.rendererCount} colliders={audit.colliderCount} output={AuditOutputPath}");
    }

    private static SceneAudit BuildSceneAudit(Scene scene)
    {
        string[] requiredRoots =
        {
            "MapRoot",
            "RuntimeSystemsRoot",
            "PlayerSpawnRoot",
            "ShelterMarkerRoot",
            "CandidateMarkerRoot",
            "HazardVisualRoot",
            "NavigationRoot",
            "CrowdRoot",
            "CollapseDebrisRoot",
            "GreenFrameRoot",
            "UIAnchorRoot",
            "DebugDiagnosticsRoot",
            "PerformanceMetricsRoot"
        };

        int missingRoots = 0;
        foreach (string rootName in requiredRoots)
        {
            if (GameObject.Find(rootName) == null)
            {
                missingRoots++;
            }
        }

        Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
        Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>();
        Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>();
        Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();

        return new SceneAudit
        {
            scenePath = scene.path,
            sceneName = scene.name,
            generatedAtLocal = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            requiredRootCount = requiredRoots.Length,
            missingRootCount = missingRoots,
            rendererCount = renderers.Length,
            colliderCount = colliders.Length,
            cameraCount = cameras.Length,
            canvasCount = canvases.Length,
            note = "Editor audit after ensuring required NewMap roots. Runtime bootstrap creates player/camera/UI during Play Mode."
        };
    }

    [Serializable]
    private class SceneAudit
    {
        public string scenePath;
        public string sceneName;
        public string generatedAtLocal;
        public int requiredRootCount;
        public int missingRootCount;
        public int rendererCount;
        public int colliderCount;
        public int cameraCount;
        public int canvasCount;
        public string note;
    }
}
#endif
