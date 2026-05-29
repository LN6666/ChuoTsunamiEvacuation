using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class NewMapNameLabelController : MonoBehaviour
{
    private readonly List<NewMapNameLabelEntry> entries = new List<NewMapNameLabelEntry>();
    private readonly List<NewMapNameLabelCandidate> candidates = new List<NewMapNameLabelCandidate>();
    private NewMapNameLabelConfig config;
    private Transform player;
    private Camera cameraRef;
    private float nextUpdateTime;

    public int AvailableLabelCount => entries.Count;
    public int ActiveLabelCount { get; private set; }
    public int OfficialShelterLabelCount { get; private set; }
    public int NonOfficialCandidateLabelCount { get; private set; }
    public int TokyoStationLabelCount { get; private set; }
    public int BuildingNameLabelCount { get; private set; }
    public int RoadNameLabelCount { get; private set; }
    public int IdOnlyLabelCount { get; private set; }
    public int NameCacheRecordCount { get; private set; }
    public int ReliableCacheLabelCount { get; private set; }
    public int AddressOnlyHiddenCount { get; private set; }
    public int LowConfidenceHiddenCount { get; private set; }
    public bool NameCacheLoaded { get; private set; }
    public int CulledByDistanceCount { get; private set; }
    public int CulledByCapCount { get; private set; }
    public bool RuntimeNetworkRequestsAllowed => false;
    public bool SceneWideMetadataScanPerformed { get; private set; }
    public bool IdOnlyLabelsVisibleInNormalMode { get; private set; }
    public bool SourceMetadataRoadOrBuildingNamesFound { get; private set; }
    public string SourceNameAvailabilityStatus { get; private set; } = "no_source_name_available";

    public IReadOnlyList<NewMapNameLabelEntry> Entries => entries;

    public static NewMapNameLabelController Create(Transform parent, NewMapPlayerController playerController, List<NewMapRuntimeTarget> targets)
    {
        GameObject labelObject = new GameObject("NewMap_NameLabelController");
        labelObject.transform.SetParent(parent, false);
        NewMapNameLabelController controller = labelObject.AddComponent<NewMapNameLabelController>();
        controller.config = NewMapNameLabelConfig.Load();
        controller.player = playerController != null ? playerController.transform : null;
        controller.BuildEntries(targets, NewMapNameCache.Load(controller.config.minConfidence));
        controller.CreateLabelObjects();
        controller.UpdateLabels(force: true);
        Debug.Log(
            $"NewMap name labels built. availableLabels={controller.AvailableLabelCount} activeLabels={controller.ActiveLabelCount} " +
            $"officialShelterLabels={controller.OfficialShelterLabelCount} nonOfficialCandidateLabels={controller.NonOfficialCandidateLabelCount} " +
            $"roadNameLabels={controller.RoadNameLabelCount} buildingNameLabels={controller.BuildingNameLabelCount} " +
            $"tokyoStationLabels={controller.TokyoStationLabelCount} idOnlyLabels={controller.IdOnlyLabelCount} " +
            $"nameCacheLoaded={controller.NameCacheLoaded} nameCacheRecords={controller.NameCacheRecordCount} " +
            $"reliableCacheLabels={controller.ReliableCacheLabelCount} addressOnlyHidden={controller.AddressOnlyHiddenCount} " +
            $"lowConfidenceHidden={controller.LowConfidenceHiddenCount} " +
            $"runtimeNetworkRequestsAllowed={controller.RuntimeNetworkRequestsAllowed} sourceNameStatus={controller.SourceNameAvailabilityStatus}");
        return controller;
    }

    private void LateUpdate()
    {
        if (config == null || !config.enabled)
        {
            return;
        }

        if (Time.unscaledTime >= nextUpdateTime)
        {
            UpdateLabels(force: false);
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            return;
        }

        Quaternion facing = Quaternion.LookRotation(activeCamera.transform.forward, Vector3.up);
        for (int i = 0; i < entries.Count; i++)
        {
            TextMesh textMesh = entries[i].TextMesh;
            if (textMesh != null && textMesh.gameObject.activeSelf)
            {
                textMesh.transform.rotation = facing;
            }
        }
    }

    private void BuildEntries(List<NewMapRuntimeTarget> targets, NewMapNameCache cache)
    {
        entries.Clear();
        OfficialShelterLabelCount = 0;
        NonOfficialCandidateLabelCount = 0;
        TokyoStationLabelCount = 0;
        BuildingNameLabelCount = 0;
        RoadNameLabelCount = 0;
        IdOnlyLabelCount = 0;
        NameCacheRecordCount = cache != null && cache.labels != null ? cache.labels.Length : 0;
        NameCacheLoaded = NameCacheRecordCount > 0;
        ReliableCacheLabelCount = 0;
        AddressOnlyHiddenCount = 0;
        LowConfidenceHiddenCount = 0;
        SceneWideMetadataScanPerformed = false;
        IdOnlyLabelsVisibleInNormalMode = false;
        SourceMetadataRoadOrBuildingNamesFound = false;

        if (config == null || !config.enabled)
        {
            SourceNameAvailabilityStatus = "labels_disabled";
            return;
        }

        AddRuntimeTargetLabels(targets, cache);
        AddCachedLabels(cache);

        if (!SourceMetadataRoadOrBuildingNamesFound && BuildingNameLabelCount == 0 && RoadNameLabelCount == 0)
        {
            SourceNameAvailabilityStatus = "no_source_name_available_for_generic_building_or_road_names";
        }
        else
        {
            SourceNameAvailabilityStatus = "source_or_cached_names_available";
        }

        entries.Sort((a, b) =>
        {
            int priorityCompare = b.Priority.CompareTo(a.Priority);
            return priorityCompare != 0 ? priorityCompare : string.CompareOrdinal(a.Id, b.Id);
        });
    }

    private void AddRuntimeTargetLabels(List<NewMapRuntimeTarget> targets, NewMapNameCache cache)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            NewMapRuntimeTarget target = targets[i];
            if (target == null || target.Anchor == null || !target.ActiveInGame || string.IsNullOrWhiteSpace(target.DisplayName))
            {
                continue;
            }

            if (target.Category != null && target.Category.Contains("runtime_proxy_training_target"))
            {
                continue;
            }

            string displayName = ResolveRuntimeTargetDisplayName(target, cache);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                continue;
            }

            if (target.IsOfficialShelter && config.showOfficialShelterNames)
            {
                entries.Add(NewMapNameLabelEntry.Create(
                    target.Id,
                    "official_shelter_label",
                    displayName + "\nOfficial Shelter",
                    target.Anchor.position + Vector3.up * 4.5f,
                    100,
                    config.importantLabelMaxDistanceMeters,
                    NewMapNameLabelPalette.OfficialShelter));
                OfficialShelterLabelCount++;
                continue;
            }

            if (!target.IsOfficialShelter &&
                target.NonOfficialWarningRequired &&
                config.showNonOfficialCandidateNames)
            {
                entries.Add(NewMapNameLabelEntry.Create(
                    target.Id,
                    "non_official_candidate_label",
                    displayName + "\nNon-official Candidate",
                    target.Anchor.position + Vector3.up * 4.0f,
                    80,
                    config.importantLabelMaxDistanceMeters,
                    NewMapNameLabelPalette.NonOfficialCandidate));
                NonOfficialCandidateLabelCount++;
            }
        }
    }

    private static string ResolveRuntimeTargetDisplayName(NewMapRuntimeTarget target, NewMapNameCache cache)
    {
        if (target == null)
        {
            return string.Empty;
        }

        NewMapCachedNameLabel cached = cache != null ? cache.FindById(target.Id) : null;
        string cachedName = cached != null ? NormalizeMainName(cached.DisplayNameForRuntime) : string.Empty;
        if (!string.IsNullOrWhiteSpace(cachedName) && cached.confidence >= 0.6f && !cached.disabled && !cached.idOnly)
        {
            return cachedName;
        }

        return NormalizeMainName(target.DisplayName);
    }

    private void AddCachedLabels(NewMapNameCache cache)
    {
        if (cache == null || cache.labels == null)
        {
            return;
        }

        for (int i = 0; i < cache.labels.Length; i++)
        {
            NewMapCachedNameLabel cached = cache.labels[i];
            if (cached == null ||
                string.IsNullOrWhiteSpace(cached.id) ||
                string.IsNullOrWhiteSpace(cached.DisplayNameForRuntime) ||
                cached.disabled)
            {
                continue;
            }

            if (cached.confidence < config.minConfidence)
            {
                LowConfidenceHiddenCount++;
                continue;
            }

            if (cached.idOnly || string.Equals(cached.classification, "debug_id_only", System.StringComparison.OrdinalIgnoreCase))
            {
                IdOnlyLabelCount++;
                if (!config.showIdOnlyLabelsInDebug)
                {
                    continue;
                }
            }

            string type = (cached.objectType ?? string.Empty).ToLowerInvariant();
            string classification = (cached.classification ?? string.Empty).ToLowerInvariant();
            string normalizedName = NormalizeMainName(cached.DisplayNameForRuntime);
            bool addressOnly = classification.Contains("address_only") || string.IsNullOrWhiteSpace(normalizedName);
            if (addressOnly)
            {
                AddressOnlyHiddenCount++;
            }

            if (type == "road")
            {
                if (!config.showRoadNames || addressOnly || !IsRoadLikeCachedResult(cached))
                {
                    continue;
                }

                entries.Add(NewMapNameLabelEntry.Create(
                    cached.id,
                    "road_name_label",
                    normalizedName,
                    cached.position.ToVector3() + Vector3.up * 0.25f,
                    55,
                    config.labelMaxDistanceMeters,
                    NewMapNameLabelPalette.Road));
                RoadNameLabelCount++;
                ReliableCacheLabelCount++;
                SourceMetadataRoadOrBuildingNamesFound = true;
            }
            else if (type == "building")
            {
                if (!config.showBuildingNames || addressOnly || !IsBuildingLikeCachedResult(cached))
                {
                    continue;
                }

                entries.Add(NewMapNameLabelEntry.Create(
                    cached.id,
                    "building_name_label",
                    normalizedName,
                    cached.position.ToVector3() + Vector3.up * 5f,
                    40,
                    config.labelMaxDistanceMeters,
                    NewMapNameLabelPalette.Building));
                BuildingNameLabelCount++;
                ReliableCacheLabelCount++;
                SourceMetadataRoadOrBuildingNamesFound = true;
            }
            else if (type == "landmark" && config.showTokyoStationName && IsTokyoStationName(normalizedName))
            {
                entries.Add(NewMapNameLabelEntry.Create(
                    cached.id,
                    "tokyo_station_label",
                    normalizedName,
                    cached.position.ToVector3() + Vector3.up * 5f,
                    95,
                    config.importantLabelMaxDistanceMeters,
                    NewMapNameLabelPalette.Landmark));
                TokyoStationLabelCount++;
                ReliableCacheLabelCount++;
            }
        }
    }

    public static string NormalizeMainName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        int newline = trimmed.IndexOfAny(new[] { '\r', '\n' });
        if (newline >= 0)
        {
            trimmed = trimmed.Substring(0, newline).Trim();
        }

        if (IsIdLikeName(trimmed) || IsAddressLikeName(trimmed) || LooksLikeMojibake(trimmed))
        {
            return string.Empty;
        }

        return HasJapanese(trimmed) ? trimmed : string.Empty;
    }

    private static bool HasJapanese(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if ((c >= '\u3040' && c <= '\u30ff') || (c >= '\u3400' && c <= '\u9fff'))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTokyoStationName(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            (value.Contains("東京駅") ||
            value.IndexOf("Tokyo Station", System.StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static bool IsIdLikeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        string lower = value.Trim().ToLowerInvariant();
        return lower.StartsWith("bldg_", System.StringComparison.Ordinal) ||
            lower.StartsWith("gml_", System.StringComparison.Ordinal) ||
            lower.StartsWith("sample_plateau", System.StringComparison.Ordinal) ||
            lower.Contains("_unknown_") ||
            lower == "unnamed" ||
            lower == "unknown" ||
            lower.Contains(",");
    }

    private static bool IsAddressLikeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string lower = value.ToLowerInvariant();
        if (lower.Contains("〒") || lower.Contains("postal") || lower.Contains("address"))
        {
            return true;
        }

        return value.Contains("東京都") &&
            value.Contains("中央区") &&
            (value.Contains("丁目") || value.Contains("番") || value.Contains("号"));
    }

    private static bool LooksLikeMojibake(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("�") ||
            value.Contains("繝") ||
            value.Contains("譛") ||
            value.Contains("莠") ||
            value.Contains("荳") ||
            value.Contains("蟆") ||
            value.Contains("縺") ||
            value.Contains("驫") ||
            value.Contains("蜊") ||
            value.Contains("螟") ||
            value.Contains("ｦ") ||
            value.Contains("ｭ");
    }

    private static bool IsRoadLikeCachedResult(NewMapCachedNameLabel cached)
    {
        string rawType = (cached.rawType ?? string.Empty).ToLowerInvariant();
        return rawType.Contains("road") ||
            rawType.Contains("street") ||
            rawType.Contains("highway") ||
            rawType.Contains("path") ||
            rawType.Contains("route") ||
            string.Equals(cached.classification, "online_exact_or_near_match", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cached.classification, "source_metadata_name", System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBuildingLikeCachedResult(NewMapCachedNameLabel cached)
    {
        string rawType = (cached.rawType ?? string.Empty).ToLowerInvariant();
        return rawType.Contains("building") ||
            rawType.Contains("poi") ||
            rawType.Contains("landmark") ||
            rawType.Contains("amenity") ||
            rawType.Contains("tourism") ||
            rawType.Contains("hotel") ||
            rawType.Contains("museum") ||
            string.Equals(cached.classification, "source_metadata_name", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cached.classification, "project_dataset_name", System.StringComparison.OrdinalIgnoreCase);
    }

    private void CreateLabelObjects()
    {
        int sourceLimit = Mathf.Clamp(config.maxCachedLabelSources, 1, 500);
        int count = Mathf.Min(entries.Count, sourceLimit);
        if (entries.Count > count)
        {
            CulledByCapCount += entries.Count - count;
            entries.RemoveRange(count, entries.Count - count);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            NewMapNameLabelEntry entry = entries[i];
            GameObject label = new GameObject("NameLabel_" + entry.Id);
            label.transform.SetParent(transform, true);
            label.transform.position = entry.Position;
            TextMesh text = label.AddComponent<TextMesh>();
            text.text = entry.Text;
            text.characterSize = entry.Category == "road_name_label" ? 1.2f : 1.6f;
            text.fontSize = 48;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = entry.Color;
            label.SetActive(false);
            entry.TextMesh = text;
            entries[i] = entry;
        }
    }

    private void UpdateLabels(bool force)
    {
        if (!force)
        {
            nextUpdateTime = Time.unscaledTime + Mathf.Max(0.05f, config.labelUpdateIntervalSeconds);
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            SetAllLabelsActive(false);
            return;
        }

        Vector3 reference = player != null ? player.position : activeCamera.transform.position;
        candidates.Clear();
        CulledByDistanceCount = 0;
        CulledByCapCount = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            NewMapNameLabelEntry entry = entries[i];
            float distance = Vector3.Distance(reference, entry.Position);
            if (config.enableDistanceCulling && distance > entry.MaxDistance)
            {
                CulledByDistanceCount++;
                continue;
            }

            Vector3 viewport = activeCamera.WorldToViewportPoint(entry.Position);
            if (config.enableFrustumCulling && (viewport.z <= 0f || viewport.x < -0.05f || viewport.x > 1.05f || viewport.y < -0.05f || viewport.y > 1.05f))
            {
                CulledByDistanceCount++;
                continue;
            }

            candidates.Add(new NewMapNameLabelCandidate(i, distance, entry.Priority, viewport));
        }

        candidates.Sort((a, b) =>
        {
            int priorityCompare = b.Priority.CompareTo(a.Priority);
            return priorityCompare != 0 ? priorityCompare : a.Distance.CompareTo(b.Distance);
        });

        SetAllLabelsActive(false);
        ActiveLabelCount = 0;
        int activeRoad = 0;
        int activeBuilding = 0;
        var usedScreenPoints = new List<Vector2>();

        for (int i = 0; i < candidates.Count; i++)
        {
            if (ActiveLabelCount >= config.maxVisibleLabels)
            {
                CulledByCapCount += candidates.Count - i;
                break;
            }

            NewMapNameLabelEntry entry = entries[candidates[i].Index];
            if (entry.Category == "road_name_label" && activeRoad >= config.maxVisibleRoadLabels)
            {
                CulledByCapCount++;
                continue;
            }

            if (entry.Category == "building_name_label" && activeBuilding >= config.maxVisibleBuildingLabels)
            {
                CulledByCapCount++;
                continue;
            }

            if (config.enableLabelDecluttering)
            {
                Vector2 screen = new Vector2(candidates[i].Viewport.x * Screen.width, candidates[i].Viewport.y * Screen.height);
                if (IsTooCloseToUsedScreenPoint(screen, usedScreenPoints, config.minScreenSpacingPixels))
                {
                    CulledByCapCount++;
                    continue;
                }

                usedScreenPoints.Add(screen);
            }

            if (entry.TextMesh != null)
            {
                entry.TextMesh.gameObject.SetActive(true);
                entry.TextMesh.transform.position = entry.Position;
                ActiveLabelCount++;
                if (entry.Category == "road_name_label")
                {
                    activeRoad++;
                }
                else if (entry.Category == "building_name_label")
                {
                    activeBuilding++;
                }
            }
        }
    }

    private static bool IsTooCloseToUsedScreenPoint(Vector2 screenPoint, List<Vector2> used, float minSpacing)
    {
        float spacingSquared = minSpacing * minSpacing;
        for (int i = 0; i < used.Count; i++)
        {
            if ((screenPoint - used[i]).sqrMagnitude < spacingSquared)
            {
                return true;
            }
        }

        return false;
    }

    private void SetAllLabelsActive(bool active)
    {
        ActiveLabelCount = active ? entries.Count : 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].TextMesh != null)
            {
                entries[i].TextMesh.gameObject.SetActive(active);
            }
        }
    }

    private Camera GetActiveCamera()
    {
        if (cameraRef != null && cameraRef.isActiveAndEnabled)
        {
            return cameraRef;
        }

        cameraRef = Camera.main;
        if (cameraRef == null)
        {
            cameraRef = FindObjectOfType<Camera>();
        }

        return cameraRef;
    }
}

public struct NewMapNameLabelEntry
{
    public string Id;
    public string Category;
    public string Text;
    public Vector3 Position;
    public int Priority;
    public float MaxDistance;
    public Color Color;
    public TextMesh TextMesh;

    public static NewMapNameLabelEntry Create(string id, string category, string text, Vector3 position, int priority, float maxDistance, Color color)
    {
        return new NewMapNameLabelEntry
        {
            Id = SanitizeId(id),
            Category = category,
            Text = text,
            Position = position,
            Priority = priority,
            MaxDistance = maxDistance,
            Color = color
        };
    }

    private static string SanitizeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed";
        }

        return value.Replace(' ', '_').Replace('/', '_').Replace('\\', '_');
    }
}

public struct NewMapNameLabelCandidate
{
    public int Index;
    public float Distance;
    public int Priority;
    public Vector3 Viewport;

    public NewMapNameLabelCandidate(int index, float distance, int priority, Vector3 viewport)
    {
        Index = index;
        Distance = distance;
        Priority = priority;
        Viewport = viewport;
    }
}

public static class NewMapNameLabelPalette
{
    public static readonly Color OfficialShelter = new Color(0.18f, 1f, 0.38f, 1f);
    public static readonly Color NonOfficialCandidate = new Color(1f, 0.82f, 0.18f, 1f);
    public static readonly Color Road = new Color(0.92f, 0.92f, 0.88f, 1f);
    public static readonly Color Building = new Color(0.88f, 0.95f, 1f, 1f);
    public static readonly Color Landmark = new Color(0.5f, 0.85f, 1f, 1f);
}

[System.Serializable]
public sealed class NewMapNameLabelConfig
{
    public bool enabled = true;
    public bool showOfficialShelterNames = true;
    public bool showNonOfficialCandidateNames = true;
    public bool showBuildingNames = true;
    public bool showRoadNames = true;
    public bool showTokyoStationName = true;
    public bool showIdOnlyLabelsInDebug;
    public int maxVisibleLabels = 80;
    public int maxVisibleRoadLabels = 30;
    public int maxVisibleBuildingLabels = 30;
    public int maxCachedLabelSources = 300;
    public float labelMaxDistanceMeters = 250f;
    public float importantLabelMaxDistanceMeters = 600f;
    public float minScreenSpacingPixels = 32f;
    public bool enableDistanceCulling = true;
    public bool enableFrustumCulling = true;
    public bool enableLabelDecluttering = true;
    public float labelUpdateIntervalSeconds = 0.25f;
    public float minConfidence = 0.6f;
    public bool runtimeNetworkRequestsAllowed;

    public static NewMapNameLabelConfig Default()
    {
        return new NewMapNameLabelConfig();
    }

    public static NewMapNameLabelConfig Load()
    {
        NewMapNameLabelConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_name_label_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapNameLabelConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap name label config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.maxVisibleLabels = Mathf.Clamp(config.maxVisibleLabels, 0, 300);
        config.maxVisibleRoadLabels = Mathf.Clamp(config.maxVisibleRoadLabels, 0, config.maxVisibleLabels);
        config.maxVisibleBuildingLabels = Mathf.Clamp(config.maxVisibleBuildingLabels, 0, config.maxVisibleLabels);
        config.maxCachedLabelSources = Mathf.Clamp(config.maxCachedLabelSources, 1, 500);
        config.labelMaxDistanceMeters = Mathf.Clamp(config.labelMaxDistanceMeters, 10f, 2000f);
        config.importantLabelMaxDistanceMeters = Mathf.Clamp(config.importantLabelMaxDistanceMeters, 10f, 3000f);
        config.minScreenSpacingPixels = Mathf.Clamp(config.minScreenSpacingPixels, 0f, 300f);
        config.labelUpdateIntervalSeconds = Mathf.Clamp(config.labelUpdateIntervalSeconds, 0.05f, 2f);
        config.minConfidence = Mathf.Clamp01(config.minConfidence);
        config.runtimeNetworkRequestsAllowed = false;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapNameCache
{
    public string generatedAt;
    public bool runtimeNetworkRequestsAllowed;
    public string sourceStatus = "no_source_name_available";
    public NewMapCachedNameLabel[] labels;
    private Dictionary<string, NewMapCachedNameLabel> labelsById;

    public static NewMapNameCache Empty()
    {
        return new NewMapNameCache
        {
            generatedAt = string.Empty,
            runtimeNetworkRequestsAllowed = false,
            sourceStatus = "no_source_name_available",
            labels = new NewMapCachedNameLabel[0]
        };
    }

    public static NewMapNameCache Load(float minConfidence)
    {
        string primaryPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_cache.json");
        string fallbackPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_label_cache.json");
        string path = File.Exists(primaryPath) ? primaryPath : fallbackPath;
        if (!File.Exists(path))
        {
            return Empty();
        }

        try
        {
            NewMapNameCache cache = JsonUtility.FromJson<NewMapNameCache>(File.ReadAllText(path)) ?? Empty();
            cache.runtimeNetworkRequestsAllowed = false;
            if (cache.labels == null)
            {
                cache.labels = new NewMapCachedNameLabel[0];
            }

            return cache;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"NewMap name cache could not be loaded; no cached building/road labels will be shown. {exception.Message}");
            return Empty();
        }
    }

    public NewMapCachedNameLabel FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || labels == null || labels.Length == 0)
        {
            return null;
        }

        if (labelsById == null)
        {
            labelsById = new Dictionary<string, NewMapCachedNameLabel>();
            for (int i = 0; i < labels.Length; i++)
            {
                NewMapCachedNameLabel label = labels[i];
                if (label != null && !string.IsNullOrWhiteSpace(label.id) && !labelsById.ContainsKey(label.id))
                {
                    labelsById.Add(label.id, label);
                }
            }
        }

        return labelsById.TryGetValue(id, out NewMapCachedNameLabel cached) ? cached : null;
    }
}

[System.Serializable]
public sealed class NewMapCachedNameLabel
{
    public string id;
    public string objectType;
    public string name;
    public string finalDisplayName;
    public string finalDisplayNameLanguage;
    public string language;
    public string provider;
    public string source;
    public string sourceField;
    public string classification;
    public string rawType;
    public string rawName;
    public string normalizedName;
    public float confidence;
    public float lat;
    public float lon;
    public bool idOnly;
    public bool disabled;
    public bool hiddenInNormalMode;
    public string hiddenReason;
    public bool attributionRequired;
    public string timestamp;
    public NewMapVector3Data position;
    public NewMapVector3Data unityPosition;

    public string DisplayNameForRuntime
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(finalDisplayName))
            {
                return finalDisplayName;
            }

            if (!string.IsNullOrWhiteSpace(normalizedName))
            {
                return normalizedName;
            }

            return name;
        }
    }
}
