using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class NewMapNpcCrowdPrototype : MonoBehaviour
{
    private const int BaseNpcCount = 8;
    private const float FarNpcUpdateDistance = 160f;
    private const float FarNpcUpdateIntervalSeconds = 0.75f;
    private const float MaxCrowdDelaySeconds = 8f;
    private const float ProbeStartHeight = 320f;
    private const float ProbeDistance = 700f;

    [SerializeField] private int npcCap = BaseNpcCount;
    [SerializeField] private float wanderRadius = 9f;
    [SerializeField] private float wanderSpeed = 0.85f;

    private readonly List<Transform> npcs = new List<Transform>();
    private readonly List<Vector3> npcHomePositions = new List<Vector3>();
    private Vector3 center;
    private Vector3 requestedCenter;
    private NewMapPlayableBounds playableBounds;
    private NewMapNpcDistributionConfig distributionConfig;
    private bool crowdFailuresEnabled;
    private bool built;

    public int NpcCap => npcCap;
    public int ActiveNpcCount => npcs.Count;
    public int RequestedNpcCount { get; private set; }
    public int SpawnedNpcCount => npcs.Count;
    public int CappedNpcCount { get; private set; }
    public string CapReason { get; private set; } = string.Empty;
    public int InvalidPlacementRetryCount { get; private set; }
    public int UsedSectorCount { get; private set; }
    public int UsedRingCount { get; private set; }
    public float DistributionRadiusMeters => distributionConfig != null ? distributionConfig.distributionRadiusMeters : 0f;
    public float MinDistanceFromPlayerMeters => distributionConfig != null ? distributionConfig.minDistanceFromPlayerMeters : 0f;
    public NewMapPlayableBounds RuntimePlayableBounds => playableBounds;
    public float CurrentCongestionDelaySeconds { get; private set; }

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition)
    {
        return Create(parent, centerPosition, NewMapPlayableBounds.DefaultDocumented());
    }

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition, NewMapPlayableBounds bounds)
    {
        GameObject crowdObject = new GameObject("NewMap_NPC_CrowdPrototype");
        crowdObject.transform.SetParent(parent, false);
        NewMapNpcCrowdPrototype crowd = crowdObject.AddComponent<NewMapNpcCrowdPrototype>();
        crowd.distributionConfig = NewMapNpcDistributionConfig.Load();
        crowd.requestedCenter = centerPosition;
        crowd.playableBounds = bounds.IsValid ? bounds : NewMapPlayableBounds.DefaultDocumented();
        crowd.npcCap = Mathf.Clamp(crowd.distributionConfig.maxNpcCount, 0, 1000);
        return crowd;
    }

    public void SetCrowdFailuresEnabled(bool enabled)
    {
        crowdFailuresEnabled = enabled;
        EnsureBuilt();
        CurrentCongestionDelaySeconds = enabled ? Mathf.Min(6f, npcs.Count * 0.04f) : 0f;
    }

    public float GetDelayForTarget(NewMapRuntimeTarget target)
    {
        if (!crowdFailuresEnabled || target == null || target.Anchor == null)
        {
            return 0f;
        }

        EnsureBuilt();
        int nearby = 0;
        foreach (Transform npc in npcs)
        {
            if (npc != null && Vector3.Distance(npc.position, target.Anchor.position) <= 8f)
            {
                nearby++;
            }
        }

        if (target.Id == "newmap_proxy_crowd_delay")
        {
            CurrentCongestionDelaySeconds = Mathf.Min(MaxCrowdDelaySeconds, 2.4f);
            return CurrentCongestionDelaySeconds;
        }

        CurrentCongestionDelaySeconds = Mathf.Min(MaxCrowdDelaySeconds, nearby * 0.75f);
        return CurrentCongestionDelaySeconds;
    }

    public Vector3[] GetNpcPositionsForDiagnostics()
    {
        var positions = new Vector3[npcs.Count];
        for (int i = 0; i < npcs.Count; i++)
        {
            positions[i] = npcs[i] != null ? npcs[i].position : Vector3.zero;
        }

        return positions;
    }

    public static Vector3[] GenerateDistributionForDiagnostics(Vector3 centerPosition, NewMapNpcDistributionConfig config)
    {
        if (config == null)
        {
            config = NewMapNpcDistributionConfig.Default();
        }

        int requested = BaseNpcCount * Mathf.Max(1, config.npcCountMultiplier);
        int count = Mathf.Min(requested, Mathf.Max(0, config.maxNpcCount));
        var random = new System.Random(config.npcDistributionSeed);
        var points = new List<Vector3>();
        var usedSectors = new Dictionary<int, int>();

        for (int i = 0; i < count; i++)
        {
            Vector3 candidate = GenerateCandidate(centerPosition, config, random, i, usedSectors);
            points.Add(candidate);
        }

        return points.ToArray();
    }

    private void Update()
    {
        if (!built)
        {
            return;
        }

        Vector3 referencePosition = requestedCenter;
        for (int i = 0; i < npcs.Count; i++)
        {
            Transform npc = npcs[i];
            if (npc == null)
            {
                continue;
            }

            float distanceToPlayer = Vector3.Distance(referencePosition, npc.position);
            bool farNpc = distributionConfig != null &&
                distributionConfig.farNpcUpdateThrottle &&
                distanceToPlayer > FarNpcUpdateDistance;
            if (farNpc && Mathf.Repeat(Time.time + i * 0.071f, FarNpcUpdateIntervalSeconds) > Time.deltaTime)
            {
                continue;
            }

            Vector3 home = npcHomePositions[i];
            float localWanderRadius = farNpc ? 1.5f : wanderRadius;
            float phase = Time.time * (farNpc ? 0.07f : 0.3f) + i * 1.7f;
            Vector3 target = home + new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 0.8f)) * localWanderRadius;
            target = playableBounds.IsValid ? playableBounds.ClampXZ(target, 2f) : target;
            Vector3 delta = target - npc.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.01f)
            {
                npc.position += delta.normalized * wanderSpeed * Time.deltaTime;
                npc.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            }

            if (playableBounds.IsValid && !playableBounds.ContainsXZ(npc.position))
            {
                npc.position = playableBounds.ClampXZ(npc.position, 2f);
                npcHomePositions[i] = playableBounds.ClampXZ(home, 2f);
            }
        }
    }

    private void Build(Vector3 centerPosition)
    {
        if (built)
        {
            return;
        }

        distributionConfig = distributionConfig ?? NewMapNpcDistributionConfig.Load();
        if (!playableBounds.IsValid)
        {
            playableBounds = NewMapPlayableBounds.DefaultDocumented();
        }

        center = centerPosition;
        RequestedNpcCount = BaseNpcCount * Mathf.Max(1, distributionConfig.npcCountMultiplier);
        CappedNpcCount = Mathf.Min(RequestedNpcCount, Mathf.Max(0, distributionConfig.maxNpcCount));
        CapReason = RequestedNpcCount > CappedNpcCount
            ? $"requested {RequestedNpcCount} exceeds maxNpcCount {distributionConfig.maxNpcCount}"
            : "not_capped";
        npcCap = CappedNpcCount;

        if (!distributionConfig.enabled || CappedNpcCount <= 0)
        {
            built = true;
            return;
        }

        var random = new System.Random(distributionConfig.npcDistributionSeed);
        var acceptedPositions = new List<Vector3>();
        var usedSectors = new Dictionary<int, int>();
        var usedRings = new HashSet<int>();
        int maxAttempts = Mathf.Max(CappedNpcCount * 12, CappedNpcCount + 24);
        int attempts = 0;

        while (acceptedPositions.Count < CappedNpcCount && attempts < maxAttempts)
        {
            Vector3 candidate = GenerateCandidate(center, distributionConfig, random, attempts, usedSectors);
            attempts++;

            if (!TrySnapToGround(candidate, center.y, out Vector3 snapped))
            {
                InvalidPlacementRetryCount++;
                continue;
            }

            if (!IsInsideRuntimePlayableBounds(snapped) || IsTooCloseToExisting(snapped, acceptedPositions, distributionConfig.minDistanceBetweenNpcMeters))
            {
                InvalidPlacementRetryCount++;
                continue;
            }

            acceptedPositions.Add(snapped);
            usedRings.Add(GetRingIndex(center, snapped, distributionConfig));
        }

        if (acceptedPositions.Count < CappedNpcCount)
        {
            FillFallbackGrid(center, acceptedPositions, CappedNpcCount, playableBounds);
        }

        UsedSectorCount = CountUsedSectors(center, acceptedPositions, distributionConfig);
        UsedRingCount = usedRings.Count > 0 ? usedRings.Count : CountUsedRings(center, acceptedPositions, distributionConfig);

        Material sharedMaterial = NewMapVisualFactory.CreateMaterial("NewMap_NPC_AmbientPedestrian_Material", new Color(1f, 0.62f, 0.12f, 1f));
        for (int i = 0; i < acceptedPositions.Count; i++)
        {
            GameObject npc = new GameObject($"NewMap_NPC_{i + 1:000}");
            npc.transform.SetParent(transform, true);
            npc.transform.position = acceptedPositions[i];
            NewMapVisualFactory.CreateHumanoid(npc.transform, "NPCVisual", new Color(1f, 0.62f, 0.12f, 1f), sharedMaterial);
            npcs.Add(npc.transform);
            npcHomePositions.Add(acceptedPositions[i]);
        }

        built = true;
        Debug.Log(
            $"NewMap NPC distribution built. requestedNpcCount={RequestedNpcCount} spawnedNpcCount={SpawnedNpcCount} " +
            $"cappedNpcCount={CappedNpcCount} capReason={CapReason} radiusMeters={distributionConfig.distributionRadiusMeters} " +
            $"usedSectors={UsedSectorCount} usedRings={UsedRingCount} invalidPlacementRetries={InvalidPlacementRetryCount}");
    }

    private void EnsureBuilt()
    {
        if (!built)
        {
            Build(requestedCenter);
        }
    }

    private static Vector3 GenerateCandidate(
        Vector3 centerPosition,
        NewMapNpcDistributionConfig config,
        System.Random random,
        int index,
        Dictionary<int, int> usedSectors)
    {
        int sectorCount = Mathf.Max(1, config.useSectorDistribution ? config.sectorCount : 1);
        int ringCount = Mathf.Max(1, config.useSectorDistribution ? config.ringCount : 1);
        int sector = index % sectorCount;
        int ring = (index / sectorCount) % ringCount;

        if (usedSectors != null && config.maxNpcPerSector > 0)
        {
            int guard = 0;
            while (usedSectors.TryGetValue(sector, out int sectorCountUsed) &&
                sectorCountUsed >= config.maxNpcPerSector &&
                guard < sectorCount)
            {
                sector = (sector + 1) % sectorCount;
                guard++;
            }

            usedSectors.TryGetValue(sector, out int current);
            usedSectors[sector] = current + 1;
        }

        float sectorWidth = Mathf.PI * 2f / sectorCount;
        float angle = sector * sectorWidth + (float)random.NextDouble() * sectorWidth;
        float innerRadius = Mathf.Lerp(config.minDistanceFromPlayerMeters, config.distributionRadiusMeters, ring / (float)ringCount);
        float outerRadius = Mathf.Lerp(config.minDistanceFromPlayerMeters, config.distributionRadiusMeters, (ring + 1) / (float)ringCount);
        float radiusSquared = Mathf.Lerp(innerRadius * innerRadius, outerRadius * outerRadius, (float)random.NextDouble());
        float radius = Mathf.Sqrt(radiusSquared);
        return centerPosition + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private static bool TrySnapToGround(Vector3 candidate, float fallbackY, out Vector3 snapped)
    {
        snapped = new Vector3(candidate.x, Mathf.Clamp(fallbackY, -20f, 30f), candidate.z);
        return true;
    }

    private static bool IsInsideDocumentedMapBounds(Vector3 position)
    {
        return position.x >= -1363.6f &&
            position.x <= 1359.3f &&
            position.z >= -1902.1f &&
            position.z <= 2851.3f &&
            position.y >= -5f &&
            position.y <= 8f;
    }

    private bool IsInsideRuntimePlayableBounds(Vector3 position)
    {
        if (playableBounds.IsValid)
        {
            return playableBounds.ContainsXZ(position, 2f);
        }

        return IsInsideDocumentedMapBounds(position);
    }

    private static bool IsTooCloseToExisting(Vector3 candidate, List<Vector3> accepted, float minDistance)
    {
        if (minDistance <= 0f)
        {
            return false;
        }

        float minDistanceSquared = minDistance * minDistance;
        foreach (Vector3 existing in accepted)
        {
            Vector3 delta = candidate - existing;
            delta.y = 0f;
            if (delta.sqrMagnitude < minDistanceSquared)
            {
                return true;
            }
        }

        return false;
    }

    private static void FillFallbackGrid(Vector3 centerPosition, List<Vector3> accepted, int desiredCount, NewMapPlayableBounds playableBounds)
    {
        float spacing = 24f;
        int ring = 1;
        while (accepted.Count < desiredCount && ring < 80)
        {
            int count = Mathf.Max(8, ring * 8);
            for (int i = 0; i < count && accepted.Count < desiredCount; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 candidate = centerPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ring * spacing;
                bool insideBounds = playableBounds.IsValid ? playableBounds.ContainsXZ(candidate, 2f) : IsInsideDocumentedMapBounds(candidate);
                if (insideBounds && !IsTooCloseToExisting(candidate, accepted, 6f))
                {
                    accepted.Add(new Vector3(candidate.x, Mathf.Clamp(centerPosition.y, -1f, 2f), candidate.z));
                }
            }

            ring++;
        }
    }

    private static int CountUsedSectors(Vector3 centerPosition, List<Vector3> positions, NewMapNpcDistributionConfig config)
    {
        var used = new HashSet<int>();
        foreach (Vector3 position in positions)
        {
            used.Add(GetSectorIndex(centerPosition, position, config));
        }

        return used.Count;
    }

    private static int CountUsedRings(Vector3 centerPosition, List<Vector3> positions, NewMapNpcDistributionConfig config)
    {
        var used = new HashSet<int>();
        foreach (Vector3 position in positions)
        {
            used.Add(GetRingIndex(centerPosition, position, config));
        }

        return used.Count;
    }

    private static int GetSectorIndex(Vector3 centerPosition, Vector3 position, NewMapNpcDistributionConfig config)
    {
        int sectorCount = Mathf.Max(1, config.sectorCount);
        Vector3 delta = position - centerPosition;
        float angle = Mathf.Atan2(delta.z, delta.x);
        if (angle < 0f)
        {
            angle += Mathf.PI * 2f;
        }

        return Mathf.Clamp(Mathf.FloorToInt(angle / (Mathf.PI * 2f / sectorCount)), 0, sectorCount - 1);
    }

    private static int GetRingIndex(Vector3 centerPosition, Vector3 position, NewMapNpcDistributionConfig config)
    {
        int ringCount = Mathf.Max(1, config.ringCount);
        Vector3 delta = position - centerPosition;
        delta.y = 0f;
        float radius = Mathf.Clamp(delta.magnitude, config.minDistanceFromPlayerMeters, config.distributionRadiusMeters);
        float normalized = Mathf.InverseLerp(config.minDistanceFromPlayerMeters, config.distributionRadiusMeters, radius);
        return Mathf.Clamp(Mathf.FloorToInt(normalized * ringCount), 0, ringCount - 1);
    }
}

[System.Serializable]
public sealed class NewMapNpcDistributionConfig
{
    public bool enabled = true;
    public int npcCountMultiplier = 20;
    public float distributionRadiusMeters = 1000f;
    public float minDistanceFromPlayerMeters = 20f;
    public float minDistanceBetweenNpcMeters = 6f;
    public int maxNpcCount = 300;
    public int npcDistributionSeed = 20260529;
    public bool useSectorDistribution = true;
    public int sectorCount = 24;
    public int ringCount = 5;
    public int maxNpcPerSector = 20;
    public bool snapToGround = true;
    public bool useGroundProxyFallback = true;
    public bool farNpcUpdateThrottle = true;

    public static NewMapNpcDistributionConfig Default()
    {
        return new NewMapNpcDistributionConfig();
    }

    public static NewMapNpcDistributionConfig Load()
    {
        NewMapNpcDistributionConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_npc_distribution_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapNpcDistributionConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap NPC distribution config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.npcCountMultiplier = Mathf.Clamp(config.npcCountMultiplier, 1, 100);
        config.distributionRadiusMeters = Mathf.Clamp(config.distributionRadiusMeters, 50f, 1500f);
        config.minDistanceFromPlayerMeters = Mathf.Clamp(config.minDistanceFromPlayerMeters, 0f, config.distributionRadiusMeters - 1f);
        config.minDistanceBetweenNpcMeters = Mathf.Clamp(config.minDistanceBetweenNpcMeters, 0f, 50f);
        config.maxNpcCount = Mathf.Clamp(config.maxNpcCount, 0, 1000);
        config.sectorCount = Mathf.Clamp(config.sectorCount, 1, 128);
        config.ringCount = Mathf.Clamp(config.ringCount, 1, 32);
        config.maxNpcPerSector = Mathf.Clamp(config.maxNpcPerSector, 1, 1000);
        return config;
    }
}
