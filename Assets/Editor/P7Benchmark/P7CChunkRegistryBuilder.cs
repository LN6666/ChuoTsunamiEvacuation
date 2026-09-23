using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class P7CChunkRegistryBuilder
{
    public const string CandidateId = "53393690";
    public const string ImportRootRelativePath = "Assets/P7Benchmark/Imported/53393690";
    public const string RegistryAssetPath = "Assets/P7Benchmark/P7C_53393690_ChunkRegistry.asset";

    private const string MenuPath = "Tools/P7 Benchmark/P7-C Build Chunk Registry And Scene";
    private const string P7CRootName = "P7C_ChunkLoadingRoot";
    private const string ControllerObjectName = "P7C_ChunkController";
    private const string PlaceholderRootName = "P7C_PlaceholderChunkGroups";
    private const string MetadataRootName = "P7C_Metadata_53393690_raw_citygml_unconverted";

    private static readonly HashSet<string> RenderableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".fbx",
        ".obj",
        ".dae",
        ".blend",
        ".gltf",
        ".glb",
        ".prefab",
        ".mesh",
        ".asset"
    };

    private static readonly HashSet<string> TextureExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".tga",
        ".tif",
        ".tiff",
        ".psd",
        ".exr"
    };

    [MenuItem(MenuPath)]
    public static void BuildFromMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("P7-C chunk registry build is editor-only. Exit Play Mode and run the menu item again.");
            return;
        }

        BuildChunkRegistryAndScene();
    }

    public static void BuildFromCommandLine()
    {
        try
        {
            BuildChunkRegistryAndScene();

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"P7-C chunk registry build failed: {exception}");

            if (ShouldExitAfterCommandLine())
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    public static P7BenchmarkChunkRegistry BuildChunkRegistryAndScene()
    {
        List<P7BenchmarkChunkInfo> chunks = ScanImportedCandidate();
        P7BenchmarkChunkRegistry registry = CreateOrUpdateRegistry(chunks);
        UpdateBenchmarkScene(registry, chunks);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"P7-C chunk registry build complete. Registry: {RegistryAssetPath}. Chunks: {chunks.Count}.");
        return registry;
    }

    public static List<P7BenchmarkChunkInfo> ScanImportedCandidate()
    {
        string repoRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrWhiteSpace(repoRoot))
        {
            throw new InvalidOperationException("Unable to resolve Unity project root from Application.dataPath.");
        }

        string importRoot = Path.Combine(repoRoot, ImportRootRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (!Directory.Exists(importRoot))
        {
            throw new DirectoryNotFoundException($"P7-C import root was not found: {ImportRootRelativePath}");
        }

        FileInfo[] files = new DirectoryInfo(importRoot)
            .GetFiles("*", SearchOption.AllDirectories)
            .Where(file => !file.Extension.Equals(".meta", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return files
            .GroupBy(file => GetChunkCategory(importRoot, file.FullName), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => CreateChunkInfo(importRoot, group.Key, group.ToArray()))
            .ToList();
    }

    private static P7BenchmarkChunkRegistry CreateOrUpdateRegistry(IReadOnlyList<P7BenchmarkChunkInfo> chunks)
    {
        EnsureAssetDirectory(RegistryAssetPath);

        P7BenchmarkChunkRegistry registry = AssetDatabase.LoadAssetAtPath<P7BenchmarkChunkRegistry>(RegistryAssetPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<P7BenchmarkChunkRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryAssetPath);
        }

        registry.Configure(CandidateId, ImportRootRelativePath, chunks);
        EditorUtility.SetDirty(registry);
        return registry;
    }

    private static void UpdateBenchmarkScene(P7BenchmarkChunkRegistry registry, IReadOnlyList<P7BenchmarkChunkInfo> chunks)
    {
        Scene scene;
        if (File.Exists(P7BenchmarkMarker.BenchmarkScenePath))
        {
            scene = EditorSceneManager.OpenScene(P7BenchmarkMarker.BenchmarkScenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        GameObject root = GameObject.Find(P7BenchmarkMarker.BenchmarkRootName);
        if (root == null)
        {
            root = new GameObject(P7BenchmarkMarker.BenchmarkRootName);
        }

        P7BenchmarkMarker marker = GetOrAddComponent<P7BenchmarkMarker>(root);
        marker.ResetToDefaults();

        P7BenchmarkMetricsRecorder metricsRecorder = GetOrAddComponent<P7BenchmarkMetricsRecorder>(root);

        Transform existingP7CRoot = root.transform.Find(P7CRootName);
        if (existingP7CRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(existingP7CRoot.gameObject);
        }

        GameObject p7CRoot = CreateChild(root.transform, P7CRootName);
        GameObject metadataRoot = CreateChild(p7CRoot.transform, MetadataRootName);
        CreateMetadataLabels(metadataRoot.transform, registry);

        GameObject controllerObject = CreateChild(p7CRoot.transform, ControllerObjectName);
        P7BenchmarkChunkController chunkController = controllerObject.AddComponent<P7BenchmarkChunkController>();
        chunkController.SetRegistry(registry);
        chunkController.ClearBindings();

        GameObject placeholderRoot = CreateChild(p7CRoot.transform, PlaceholderRootName);
        CreatePlaceholderChunkGroups(placeholderRoot.transform, chunks, chunkController);

        metricsRecorder.ConfigureChunkContext(registry, chunkController);
        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(metricsRecorder);
        EditorUtility.SetDirty(chunkController);

        if (!EditorSceneManager.SaveScene(scene, P7BenchmarkMarker.BenchmarkScenePath))
        {
            throw new InvalidOperationException($"Unity failed to save {P7BenchmarkMarker.BenchmarkScenePath}.");
        }
    }

    private static void CreateMetadataLabels(Transform parent, P7BenchmarkChunkRegistry registry)
    {
        CreateChild(parent, $"candidate_{registry.CandidateId}");
        CreateChild(parent, "scope_p7benchmark_sandbox_only");
        CreateChild(parent, "citygml_raw_unconverted_no_renderable_mesh_detected");
        CreateChild(parent, $"files_{registry.TotalSourceFileCount}_bytes_{registry.TotalSourceBytes}");
        CreateChild(parent, $"textures_{registry.TotalTextureFileCount}_citygml_{registry.TotalCityGmlFileCount}");
        CreateChild(parent, "no_gameplay_success_failure_effect");
    }

    private static void CreatePlaceholderChunkGroups(
        Transform parent,
        IReadOnlyList<P7BenchmarkChunkInfo> chunks,
        P7BenchmarkChunkController chunkController)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            P7BenchmarkChunkInfo chunk = chunks[i];
            GameObject chunkRoot = CreateChild(parent, $"P7C_Chunk_{chunk.Category}_{chunk.CandidateId}");
            chunkRoot.transform.localPosition = new Vector3((i - (chunks.Count - 1) * 0.5f) * 4f, 0f, 0f);

            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = $"PlaceholderVolume_{chunk.Category}_metadata_only";
            placeholder.transform.SetParent(chunkRoot.transform, false);
            placeholder.transform.localPosition = new Vector3(0f, Mathf.Max(0.25f, GetPlaceholderHeight(chunk) * 0.5f), 0f);
            placeholder.transform.localScale = new Vector3(2.8f, GetPlaceholderHeight(chunk), 2.8f);

            Collider collider = placeholder.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer renderer = placeholder.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(chunk.Category, i);
            }

            CreateChild(chunkRoot.transform, $"metadata_{chunk.SourceFileCount}_files_{chunk.SourceBytes}_bytes");
            CreateChild(chunkRoot.transform, chunk.PlaceholderOnly ? "placeholder_only_raw_citygml" : "renderable_asset_detected_requires_visual_check");

            chunkController.RegisterChunkRoot(chunk.ChunkId, chunkRoot, chunk.InitiallyEnabled);
        }

        chunkController.ApplyInitialChunkState();
    }

    private static P7BenchmarkChunkInfo CreateChunkInfo(string importRoot, string category, IReadOnlyList<FileInfo> files)
    {
        long bytes = files.Sum(file => file.Length);
        int gmlCount = files.Count(file => file.Extension.Equals(".gml", StringComparison.OrdinalIgnoreCase));
        int textureCount = files.Count(file => TextureExtensions.Contains(file.Extension));
        int renderableCount = files.Count(file => RenderableExtensions.Contains(file.Extension));
        string sourceRelativePath = GetChunkSourceRelativePath(importRoot, category, files);
        string chunkId = $"{CandidateId}_{category}";
        bool placeholderOnly = renderableCount == 0;
        string notes = placeholderOnly
            ? "Raw CityGML/texture source; no renderable Unity mesh/model/prefab detected by extension scan."
            : "Renderable extension detected; still requires visual verification and profiler review.";

        return new P7BenchmarkChunkInfo(
            chunkId,
            CandidateId,
            $"{CandidateId} {category}",
            category,
            sourceRelativePath,
            files.Count,
            bytes,
            gmlCount,
            textureCount,
            renderableCount,
            placeholderOnly,
            true,
            notes);
    }

    private static string GetChunkCategory(string importRoot, string fullPath)
    {
        string relative = GetRelativePath(importRoot, fullPath).Replace("\\", "/");
        string[] parts = relative.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2 && string.Equals(parts[0], "udx", StringComparison.OrdinalIgnoreCase))
        {
            return parts[1].ToLowerInvariant();
        }

        return parts.Length > 0 ? parts[0].ToLowerInvariant() : "root";
    }

    private static string GetChunkSourceRelativePath(string importRoot, string category, IReadOnlyList<FileInfo> files)
    {
        FileInfo first = files.FirstOrDefault();
        if (first == null)
        {
            return ImportRootRelativePath;
        }

        string firstRelative = GetRelativePath(importRoot, first.FullName).Replace("\\", "/");
        string[] parts = firstRelative.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2 && string.Equals(parts[0], "udx", StringComparison.OrdinalIgnoreCase))
        {
            return $"{ImportRootRelativePath}/udx/{category}";
        }

        return $"{ImportRootRelativePath}/{category}";
    }

    private static float GetPlaceholderHeight(P7BenchmarkChunkInfo chunk)
    {
        float fileScale = Mathf.Clamp(chunk.SourceFileCount / 1200f, 0.35f, 4f);
        float byteScale = Mathf.Clamp((float)(chunk.SourceBytes / (80f * 1024f * 1024f)), 0.35f, 4f);
        return Mathf.Clamp(0.6f + fileScale + byteScale, 0.8f, 6f);
    }

    private static Material CreateMaterial(string category, int index)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Color[] colors =
        {
            new Color(0.20f, 0.46f, 0.62f, 1f),
            new Color(0.55f, 0.42f, 0.25f, 1f),
            new Color(0.28f, 0.55f, 0.34f, 1f),
            new Color(0.58f, 0.32f, 0.40f, 1f),
            new Color(0.34f, 0.34f, 0.34f, 1f),
            new Color(0.45f, 0.39f, 0.64f, 1f)
        };

        return new Material(shader)
        {
            name = $"P7C_{category}_Placeholder_Material",
            color = colors[Mathf.Abs(index) % colors.Length]
        };
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
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

    private static string GetRelativePath(string root, string fullPath)
    {
        Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
        Uri fileUri = new Uri(fullPath);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace("/", Path.DirectorySeparatorChar.ToString());
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
            if (string.Equals(args[i], "-p7cBuildSceneQuit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
