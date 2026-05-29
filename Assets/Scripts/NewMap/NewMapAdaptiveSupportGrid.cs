using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class NewMapAdaptiveSupportGridRuntime
{
    private const string GridRootName = "NewMap_AdaptiveSupportGrid";
    private readonly NewMapAdaptiveSupportGridConfig config;
    private readonly NewMapGroundRoadHeightSampleCache cache;
    private readonly List<NewMapGroundRoadHeightSample> usableSamples = new List<NewMapGroundRoadHeightSample>();

    public bool Enabled => config != null && config.enabled;
    public bool HasUsableSamples => usableSamples.Count > 0;
    public bool HasUsableGrid => CellCount > 0 && ColliderCount > 0;
    public int SourceSampleCount => cache != null && cache.samples != null ? cache.samples.Length : 0;
    public int UsableSampleCount => usableSamples.Count;
    public int CellCount { get; private set; }
    public int ColliderCount { get; private set; }
    public int VisibleRendererCount { get; private set; }
    public int RendererDisabledCount { get; private set; }
    public int CellsUsingRoad { get; private set; }
    public int CellsUsingTerrain { get; private set; }
    public int CellsUsingRelief { get; private set; }
    public int CellsUsingBridge { get; private set; }
    public int CellsUsingBuildingBaseFallback { get; private set; }
    public int CellsUsingGlobalFallback { get; private set; }
    public float SupportYMin { get; private set; }
    public float SupportYMax { get; private set; }
    public float SupportYAverage { get; private set; }
    public string LastResolveSource { get; private set; } = "not_resolved";
    public string Status { get; private set; } = "not_built";

    private NewMapAdaptiveSupportGridRuntime(NewMapAdaptiveSupportGridConfig config, NewMapGroundRoadHeightSampleCache cache)
    {
        this.config = config ?? NewMapAdaptiveSupportGridConfig.Default();
        this.cache = cache ?? NewMapGroundRoadHeightSampleCache.Empty();
        BuildUsableSampleList();
    }

    public static NewMapAdaptiveSupportGridRuntime Load()
    {
        return new NewMapAdaptiveSupportGridRuntime(
            NewMapAdaptiveSupportGridConfig.Load(),
            NewMapGroundRoadHeightSampleCache.Load());
    }

    public static NewMapAdaptiveSupportGridRuntime CreateForDiagnostics(
        NewMapAdaptiveSupportGridConfig config,
        NewMapGroundRoadHeightSample[] samples)
    {
        var cache = new NewMapGroundRoadHeightSampleCache { samples = samples ?? new NewMapGroundRoadHeightSample[0] };
        return new NewMapAdaptiveSupportGridRuntime(config, cache);
    }

    public void BuildCollisionGrid(Transform parent, NewMapPlayableBounds bounds, float globalFallbackY)
    {
        ResetGridDiagnostics();
        if (!Enabled)
        {
            Status = "disabled";
            return;
        }

        if (parent == null)
        {
            Status = "missing_parent";
            return;
        }

        if (!bounds.IsValid)
        {
            Status = "invalid_playable_bounds";
            return;
        }

        if (!HasUsableSamples && !config.useGlobalFallback)
        {
            Status = "no_usable_samples";
            return;
        }

        Transform gridRoot = ResetGridRoot(parent);
        float cellSize = ResolveRuntimeCellSize(bounds);
        int columns = Mathf.Max(1, Mathf.CeilToInt(bounds.Width / cellSize));
        int rows = Mathf.Max(1, Mathf.CeilToInt(bounds.Depth / cellSize));
        float supportSum = 0f;
        SupportYMin = float.PositiveInfinity;
        SupportYMax = float.NegativeInfinity;

        for (int row = 0; row < rows; row++)
        {
            float minZ = Mathf.Lerp(bounds.MinZ, bounds.MaxZ, row / (float)rows);
            float maxZ = Mathf.Lerp(bounds.MinZ, bounds.MaxZ, (row + 1) / (float)rows);
            for (int column = 0; column < columns; column++)
            {
                float minX = Mathf.Lerp(bounds.MinX, bounds.MaxX, column / (float)columns);
                float maxX = Mathf.Lerp(bounds.MinX, bounds.MaxX, (column + 1) / (float)columns);
                Vector3 center = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
                float supportY = ResolveSupportY(center, globalFallbackY, out string source);
                RegisterCellSource(source);
                supportSum += supportY;
                SupportYMin = Mathf.Min(SupportYMin, supportY);
                SupportYMax = Mathf.Max(SupportYMax, supportY);
                CellCount++;

                GameObject cell = new GameObject($"SupportCell_{row:000}_{column:000}_{source}");
                cell.transform.SetParent(gridRoot, true);
                cell.transform.position = new Vector3(center.x, supportY, center.z);
                BoxCollider collider = cell.AddComponent<BoxCollider>();
                float thickness = Mathf.Max(0.05f, config.supportThicknessMeters);
                collider.size = new Vector3(Mathf.Max(0.1f, maxX - minX), thickness, Mathf.Max(0.1f, maxZ - minZ));
                collider.center = Vector3.down * (thickness * 0.5f);
                collider.isTrigger = false;
                ColliderCount++;
            }
        }

        SupportYAverage = CellCount > 0 ? supportSum / CellCount : globalFallbackY;
        if (CellCount == 0)
        {
            SupportYMin = globalFallbackY;
            SupportYMax = globalFallbackY;
        }

        Renderer[] renderers = gridRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            if (renderers[i].enabled)
            {
                renderers[i].enabled = false;
                RendererDisabledCount++;
            }
        }

        VisibleRendererCount = CountVisibleRenderers(gridRoot);
        Status = HasUsableSamples ? "adaptive_grid_built_from_ground_road_samples" : "global_fallback_grid_built_no_samples";
    }

    public float ResolveSupportY(Vector3 position, float globalFallbackY)
    {
        return ResolveSupportY(position, globalFallbackY, out _);
    }

    public float ResolveSupportY(Vector3 position, float globalFallbackY, out string source)
    {
        if (TrySelectBestSample(position, out NewMapGroundRoadHeightSample sample))
        {
            source = NormalizeCategory(sample.category);
            LastResolveSource = source;
            return sample.position.y;
        }

        source = config.useGlobalFallback ? "global_fallback" : "unresolved";
        LastResolveSource = source;
        return globalFallbackY;
    }

    public bool TryResolveSupportY(Vector3 position, out float supportY, out string source)
    {
        if (TrySelectBestSample(position, out NewMapGroundRoadHeightSample sample))
        {
            supportY = sample.position.y;
            source = NormalizeCategory(sample.category);
            LastResolveSource = source;
            return true;
        }

        supportY = 0f;
        source = "unresolved";
        LastResolveSource = source;
        return false;
    }

    private void BuildUsableSampleList()
    {
        usableSamples.Clear();
        if (cache == null || cache.samples == null)
        {
            return;
        }

        for (int i = 0; i < cache.samples.Length; i++)
        {
            NewMapGroundRoadHeightSample sample = cache.samples[i];
            if (sample == null || !sample.usableForSupport || sample.confidence < config.minSampleConfidence)
            {
                continue;
            }

            string category = NormalizeCategory(sample.category);
            if (category == "water" || category == "unknown")
            {
                continue;
            }

            if (category == "building_base_fallback" && !config.useBuildingBaseFallback)
            {
                continue;
            }

            if (!IsFinite(sample.position.x) || !IsFinite(sample.position.y) || !IsFinite(sample.position.z))
            {
                continue;
            }

            if (sample.position.y < config.minSupportY || sample.position.y > config.maxSupportY)
            {
                continue;
            }

            usableSamples.Add(sample);
        }
    }

    private bool TrySelectBestSample(Vector3 position, out NewMapGroundRoadHeightSample bestSample)
    {
        bestSample = null;
        if (!Enabled || usableSamples.Count == 0)
        {
            return false;
        }

        float searchRadius = Mathf.Max(1f, config.nearestSampleRadiusMeters);
        float searchRadiusSquared = searchRadius * searchRadius;
        int bestPriority = int.MaxValue;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < usableSamples.Count; i++)
        {
            NewMapGroundRoadHeightSample sample = usableSamples[i];
            float dx = position.x - sample.position.x;
            float dz = position.z - sample.position.z;
            float distanceSquared = dx * dx + dz * dz;
            if (distanceSquared > searchRadiusSquared)
            {
                continue;
            }

            string category = NormalizeCategory(sample.category);
            int priority = CategoryPriority(category);
            if (priority < 0)
            {
                continue;
            }

            float score = priority * 100000f + distanceSquared - Mathf.Clamp01(sample.confidence) * 250f;
            if (priority < bestPriority || (priority == bestPriority && score < bestScore))
            {
                bestPriority = priority;
                bestScore = score;
                bestSample = sample;
            }
        }

        return bestSample != null;
    }

    private static int CategoryPriority(string category)
    {
        switch (NormalizeCategory(category))
        {
            case "road":
                return 0;
            case "terrain":
            case "relief":
                return 1;
            case "bridge":
                return 2;
            case "building_base_fallback":
                return 3;
            default:
                return -1;
        }
    }

    public static string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "unknown";
        }

        string lower = category.Trim().ToLowerInvariant();
        if (lower.Contains("road") || lower.Contains("tran") || lower.Contains("transport") || lower.Contains("street"))
        {
            return "road";
        }

        if (lower.Contains("terrain"))
        {
            return "terrain";
        }

        if (lower.Contains("relief") || lower.Contains("dem"))
        {
            return "relief";
        }

        if (lower.Contains("bridge") || lower.Contains("brid"))
        {
            return "bridge";
        }

        if (lower.Contains("water") || lower.Contains("wtr"))
        {
            return "water";
        }

        if (lower.Contains("building") || lower.Contains("bldg"))
        {
            return "building_base_fallback";
        }

        return lower;
    }

    private void RegisterCellSource(string source)
    {
        switch (NormalizeCategory(source))
        {
            case "road":
                CellsUsingRoad++;
                break;
            case "terrain":
                CellsUsingTerrain++;
                break;
            case "relief":
                CellsUsingRelief++;
                break;
            case "bridge":
                CellsUsingBridge++;
                break;
            case "building_base_fallback":
                CellsUsingBuildingBaseFallback++;
                break;
            default:
                CellsUsingGlobalFallback++;
                break;
        }
    }

    private float ResolveRuntimeCellSize(NewMapPlayableBounds bounds)
    {
        float cellSize = Mathf.Clamp(config.cellSizeMeters, 20f, 500f);
        int maxCells = Mathf.Max(1, config.maxGridCells);
        float area = Mathf.Max(1f, bounds.Width * bounds.Depth);
        int estimatedCells = Mathf.CeilToInt(bounds.Width / cellSize) * Mathf.CeilToInt(bounds.Depth / cellSize);
        if (estimatedCells <= maxCells)
        {
            return cellSize;
        }

        float adjusted = Mathf.Sqrt(area / maxCells) * 1.05f;
        return Mathf.Clamp(adjusted, cellSize, 900f);
    }

    private Transform ResetGridRoot(Transform parent)
    {
        Transform existing = parent.Find(GridRootName);
        if (existing != null)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
            else
            {
                Object.Destroy(existing.gameObject);
            }
        }

        GameObject gridRoot = new GameObject(GridRootName);
        gridRoot.transform.SetParent(parent, false);
        return gridRoot.transform;
    }

    private void ResetGridDiagnostics()
    {
        CellCount = 0;
        ColliderCount = 0;
        VisibleRendererCount = 0;
        RendererDisabledCount = 0;
        CellsUsingRoad = 0;
        CellsUsingTerrain = 0;
        CellsUsingRelief = 0;
        CellsUsingBridge = 0;
        CellsUsingBuildingBaseFallback = 0;
        CellsUsingGlobalFallback = 0;
        SupportYMin = 0f;
        SupportYMax = 0f;
        SupportYAverage = 0f;
        Status = "not_built";
    }

    private static int CountVisibleRenderers(Transform root)
    {
        if (root == null)
        {
            return 0;
        }

        int count = 0;
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

[System.Serializable]
public sealed class NewMapAdaptiveSupportGridConfig
{
    public bool enabled;
    public bool debugVisualizationEnabled;
    public bool rendererEnabledInNormalMode;
    public float cellSizeMeters = 160f;
    public int maxGridCells = 900;
    public float supportThicknessMeters = 0.5f;
    public float nearestSampleRadiusMeters = 320f;
    public float minSampleConfidence = 0.45f;
    public float minSupportY = -20f;
    public float maxSupportY = 30f;
    public bool useBuildingBaseFallback = true;
    public bool useGlobalFallback = true;

    public static NewMapAdaptiveSupportGridConfig Default()
    {
        return new NewMapAdaptiveSupportGridConfig();
    }

    public static NewMapAdaptiveSupportGridConfig Load()
    {
        NewMapAdaptiveSupportGridConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_adaptive_support_grid_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapAdaptiveSupportGridConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap adaptive support grid config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.cellSizeMeters = Mathf.Clamp(config.cellSizeMeters, 20f, 500f);
        config.maxGridCells = Mathf.Clamp(config.maxGridCells, 1, 2500);
        config.supportThicknessMeters = Mathf.Clamp(config.supportThicknessMeters, 0.05f, 2f);
        config.nearestSampleRadiusMeters = Mathf.Clamp(config.nearestSampleRadiusMeters, 20f, 2000f);
        config.minSampleConfidence = Mathf.Clamp01(config.minSampleConfidence);
        if (config.minSupportY > config.maxSupportY)
        {
            float temp = config.minSupportY;
            config.minSupportY = config.maxSupportY;
            config.maxSupportY = temp;
        }

        config.enabled = false;
        config.debugVisualizationEnabled = false;
        config.rendererEnabledInNormalMode = false;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapGroundRoadHeightSampleCache
{
    public string generatedAt;
    public string sourceScenePath;
    public bool valid;
    public string status;
    public int totalSamples;
    public NewMapGroundRoadHeightSample[] samples;

    public static NewMapGroundRoadHeightSampleCache Empty()
    {
        return new NewMapGroundRoadHeightSampleCache
        {
            generatedAt = string.Empty,
            sourceScenePath = string.Empty,
            valid = false,
            status = "missing_or_empty",
            totalSamples = 0,
            samples = new NewMapGroundRoadHeightSample[0]
        };
    }

    public static NewMapGroundRoadHeightSampleCache Load()
    {
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_road_height_samples.json");
        if (!File.Exists(path))
        {
            return Empty();
        }

        try
        {
            NewMapGroundRoadHeightSampleCache cache = JsonUtility.FromJson<NewMapGroundRoadHeightSampleCache>(File.ReadAllText(path)) ?? Empty();
            if (cache.samples == null)
            {
                cache.samples = new NewMapGroundRoadHeightSample[0];
            }

            cache.totalSamples = cache.samples.Length;
            return cache;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"NewMap ground/road sample cache could not be loaded; adaptive support grid will use fallback. {exception.Message}");
            return Empty();
        }
    }
}

[System.Serializable]
public sealed class NewMapGroundRoadHeightSample
{
    public string sampleId;
    public string sourceObjectPath;
    public string category;
    public NewMapVector3Data position;
    public NewMapBoundsData bounds;
    public float confidence;
    public bool usableForSupport;
}

[System.Serializable]
public sealed class NewMapBoundsData
{
    public NewMapVector3Data min;
    public NewMapVector3Data max;
    public NewMapVector3Data center;
    public NewMapVector3Data size;
}
