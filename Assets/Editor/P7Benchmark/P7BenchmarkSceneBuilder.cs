using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class P7BenchmarkSceneBuilder
{
    private const string MenuPath = "Tools/P7 Benchmark/Create Wave 2-A Skeleton Scene";

    [MenuItem(MenuPath)]
    public static void CreateBenchmarkSceneFromMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("P7 benchmark scene creation is editor-only. Exit Play Mode and run the menu item again.");
            return;
        }

        CreateBenchmarkScene();
    }

    public static void BuildSceneFromCommandLine()
    {
        try
        {
            CreateBenchmarkScene();

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"P7 benchmark scene creation failed: {exception}");

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    public static void CreateBenchmarkScene()
    {
        string sceneDirectory = Path.GetDirectoryName(P7BenchmarkMarker.BenchmarkScenePath);
        if (string.IsNullOrWhiteSpace(sceneDirectory))
        {
            throw new InvalidOperationException("P7 benchmark scene path has no directory.");
        }

        Directory.CreateDirectory(sceneDirectory);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject(P7BenchmarkMarker.BenchmarkRootName);
        P7BenchmarkMarker marker = root.AddComponent<P7BenchmarkMarker>();
        marker.ResetToDefaults();
        root.AddComponent<P7BenchmarkMetricsRecorder>();

        Material groundMaterial = CreateMaterial("P7Benchmark_Ground_Material", new Color(0.35f, 0.39f, 0.38f, 1f));
        Material buildingMaterial = CreateMaterial("P7Benchmark_BuildingPlaceholder_Material", new Color(0.58f, 0.68f, 0.78f, 1f));
        Material roadMaterial = CreateMaterial("P7Benchmark_RoadPlaceholder_Material", new Color(0.12f, 0.13f, 0.14f, 1f));
        Material bridgeMaterial = CreateMaterial("P7Benchmark_BridgePlaceholder_Material", new Color(0.7f, 0.62f, 0.45f, 1f));
        Material undergroundMaterial = CreateMaterial("P7Benchmark_UndergroundPlaceholder_Material", new Color(0.42f, 0.35f, 0.5f, 1f));

        CreatePrimitive("BenchmarkGround", PrimitiveType.Cube, root.transform, new Vector3(0f, -0.05f, 0f), new Vector3(28f, 0.1f, 18f), groundMaterial);
        CreatePrimitive("RoadContextPlaceholder", PrimitiveType.Cube, root.transform, new Vector3(0f, 0.02f, 0f), new Vector3(24f, 0.08f, 3f), roadMaterial);
        CreatePrimitive("LOD3CandidateVolumePlaceholder_A", PrimitiveType.Cube, root.transform, new Vector3(-6f, 2f, 4.5f), new Vector3(4f, 4f, 4f), buildingMaterial);
        CreatePrimitive("LOD3CandidateVolumePlaceholder_B", PrimitiveType.Cube, root.transform, new Vector3(1f, 3f, 5f), new Vector3(5f, 6f, 3f), buildingMaterial);
        CreatePrimitive("LOD3CandidateVolumePlaceholder_C", PrimitiveType.Cube, root.transform, new Vector3(7f, 1.5f, -4f), new Vector3(3f, 3f, 5f), buildingMaterial);
        CreatePrimitive("BridgeContextPlaceholder", PrimitiveType.Cube, root.transform, new Vector3(-7f, 0.55f, -4.5f), new Vector3(8f, 0.35f, 2.4f), bridgeMaterial);
        CreatePrimitive("UndergroundCategoryPlaceholder", PrimitiveType.Cube, root.transform, new Vector3(6f, -0.55f, 4f), new Vector3(5f, 0.8f, 3f), undergroundMaterial);

        CreateCamera(root.transform);
        CreateLight(root.transform);

        if (!EditorSceneManager.SaveScene(scene, P7BenchmarkMarker.BenchmarkScenePath))
        {
            throw new InvalidOperationException($"Unity failed to save {P7BenchmarkMarker.BenchmarkScenePath}.");
        }

        AssetDatabase.Refresh();
        Selection.activeGameObject = root;
        Debug.Log($"Created isolated P7 benchmark skeleton scene at {P7BenchmarkMarker.BenchmarkScenePath}. No real assets were imported.");
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        return gameObject;
    }

    private static void CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("P7BenchmarkCamera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(0f, 10f, -16f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 55f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 500f;
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateLight(Transform parent)
    {
        GameObject lightObject = new GameObject("P7BenchmarkDirectionalLight");
        lightObject.transform.SetParent(parent);
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        return new Material(shader)
        {
            name = name,
            color = color
        };
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
            if (string.Equals(args[i], "-p7bWave2aCreateSceneQuit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
