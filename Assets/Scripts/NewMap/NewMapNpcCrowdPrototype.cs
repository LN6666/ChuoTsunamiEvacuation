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
    private const float BuildingBoundsCellSize = 64f;
    private const int MaxIndexedCellsPerBuildingBound = 64;

    [SerializeField] private int npcCap = BaseNpcCount;
    [SerializeField] private float wanderRadius = 9f;
    [SerializeField] private float wanderSpeed = 0.85f;

    private readonly List<Transform> npcs = new List<Transform>();
    private readonly List<Vector3> npcHomePositions = new List<Vector3>();
    private readonly List<Vector3> npcLastPositions = new List<Vector3>();
    private readonly List<float> npcStoppedSeconds = new List<float>();
    private readonly List<NewMapNpcMovementState> npcStates = new List<NewMapNpcMovementState>();
    private readonly List<Bounds> buildingAvoidanceBounds = new List<Bounds>();
    private readonly Dictionary<long, List<int>> buildingBoundsSpatialIndex = new Dictionary<long, List<int>>();
    private readonly List<int> largeBuildingBoundsIndices = new List<int>();
    private Vector3 center;
    private Vector3 requestedCenter;
    private NewMapPlayableBounds playableBounds;
    private NewMapCircularBoundary circularBoundary;
    private NewMapNpcDistributionConfig distributionConfig;
    private NewMapNpcMovementConfig movementConfig;
    private NewMapNpcLifecycleConfig lifecycleConfig;
    private NewMapPlayerNpcCollisionConfig playerNpcCollisionConfig;
    private Transform referenceTransform;
    private bool crowdFailuresEnabled;
    private bool built;
    private bool circularBoundaryClampEnabled;
    private int npcBodyColliderCount;
    private int npcCreatedAtStartupCount;

    public int NpcCap => npcCap;
    public int ActiveNpcCount => npcs.Count;
    public int RequestedNpcCount { get; private set; }
    public int SpawnedNpcCount => npcs.Count;
    public int CappedNpcCount { get; private set; }
    public string CapReason { get; private set; } = string.Empty;
    public int InvalidPlacementRetryCount { get; private set; }
    public int UsedSectorCount { get; private set; }
    public int UsedRingCount { get; private set; }
    public int RejectedInsideBuildingCount { get; private set; }
    public float DistributionRadiusMeters => distributionConfig != null ? distributionConfig.distributionRadiusMeters : 0f;
    public float MinDistanceFromPlayerMeters => distributionConfig != null ? distributionConfig.minDistanceFromPlayerMeters : 0f;
    public float MinDistanceBetweenNpcMeters => distributionConfig != null ? distributionConfig.minDistanceBetweenNpcMeters : 0f;
    public bool AvoidBuildingsEnabled => distributionConfig != null && distributionConfig.avoidBuildings;
    public bool UsePoolingEnabled => distributionConfig != null && distributionConfig.usePooling;
    public bool FarNpcStaticProxyModeEnabled => movementConfig != null
        ? movementConfig.farNpcStaticProxyMode
        : distributionConfig != null && distributionConfig.farNpcStaticProxyMode;
    public NewMapPlayableBounds RuntimePlayableBounds => playableBounds;
    public bool CircularBoundaryClampEnabled => circularBoundaryClampEnabled && circularBoundary.IsValid;
    public float CircularBoundaryRadiusMeters => circularBoundary.RadiusMeters;
    public float CurrentCongestionDelaySeconds { get; private set; }
    public int MovingCount { get; private set; }
    public int ArrivedCount { get; private set; }
    public int QueuedCount { get; private set; }
    public int StuckCount { get; private set; }
    public int RecoveredCount { get; private set; }
    public int StaticProxyCount { get; private set; }
    public int StoppedWithoutReasonCount { get; private set; }
    public int NpcBuildingAvoidanceRecoveryCount { get; private set; }
    public float AverageSpeedMetersPerSecond { get; private set; }
    public int NpcBodyColliderCount => npcBodyColliderCount;
    public bool PlayerNpcSoftBlockingEnabled => playerNpcCollisionConfig != null && playerNpcCollisionConfig.enabled;
    public float NearNpcCollisionRadiusMeters => playerNpcCollisionConfig != null ? playerNpcCollisionConfig.nearNpcCollisionRadiusMeters : 0f;
    public int NpcCreatedAtStartupCount => npcCreatedAtStartupCount;
    public int GlobalRespawnCount { get; private set; }
    public int IndividualRespawnCount { get; private set; }
    public int PoolRecycleCount { get; private set; }
    public int DestroyedDuringRuntimeCount { get; private set; }
    public int InstantiateAfterStartupCount { get; private set; }
    public int AllStopEventCount { get; private set; }
    public int PlayerContactEventCount { get; private set; }
    public bool AllowGlobalRefresh => lifecycleConfig != null && lifecycleConfig.allowGlobalRefresh;
    public float GlobalRespawnIntervalSeconds => lifecycleConfig != null ? lifecycleConfig.globalRespawnIntervalSeconds : 0f;
    public bool CollisionWithPlayerDoesNotGlobalPause => lifecycleConfig == null || lifecycleConfig.collisionWithPlayerDoesNotGlobalPause;

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition)
    {
        return Create(parent, centerPosition, NewMapPlayableBounds.DefaultDocumented());
    }

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition, NewMapPlayableBounds bounds)
    {
        return Create(parent, centerPosition, bounds, null);
    }

    public static NewMapNpcCrowdPrototype Create(Transform parent, Vector3 centerPosition, NewMapPlayableBounds bounds, IEnumerable<Bounds> buildingBounds)
    {
        return Create(parent, centerPosition, bounds, buildingBounds, default(NewMapCircularBoundary), false);
    }

    public static NewMapNpcCrowdPrototype Create(
        Transform parent,
        Vector3 centerPosition,
        NewMapPlayableBounds bounds,
        IEnumerable<Bounds> buildingBounds,
        NewMapCircularBoundary boundary,
        bool enableCircularBoundaryClamp)
    {
        GameObject crowdObject = new GameObject("NewMap_NPC_CrowdPrototype");
        crowdObject.transform.SetParent(parent, false);
        NewMapNpcCrowdPrototype crowd = crowdObject.AddComponent<NewMapNpcCrowdPrototype>();
        crowd.distributionConfig = NewMapNpcDistributionConfig.Load();
        crowd.movementConfig = NewMapNpcMovementConfig.Load();
        crowd.lifecycleConfig = NewMapNpcLifecycleConfig.Load();
        crowd.playerNpcCollisionConfig = NewMapPlayerNpcCollisionConfig.Load();
        crowd.requestedCenter = centerPosition;
        crowd.playableBounds = bounds.IsValid ? bounds : NewMapPlayableBounds.DefaultDocumented();
        crowd.circularBoundary = boundary;
        crowd.circularBoundaryClampEnabled = enableCircularBoundaryClamp && boundary.IsValid && boundary.AffectsNpc;
        crowd.npcCap = Mathf.Clamp(crowd.distributionConfig.maxNpcCount, 0, 1000);
        if (buildingBounds != null)
        {
            foreach (Bounds buildingBound in buildingBounds)
            {
                crowd.buildingAvoidanceBounds.Add(buildingBound);
            }
        }

        crowd.BuildBuildingBoundsSpatialIndex();
        return crowd;
    }

    public void SetReferenceTransform(Transform transformReference)
    {
        referenceTransform = transformReference;
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

        Vector3 referencePosition = referenceTransform != null ? referenceTransform.position : requestedCenter;
        MovingCount = 0;
        ArrivedCount = 0;
        QueuedCount = 0;
        StuckCount = 0;
        StaticProxyCount = 0;
        StoppedWithoutReasonCount = 0;
        float speedTotal = 0f;
        int speedSamples = 0;

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
            if (farNpc && movementConfig != null && movementConfig.farNpcStaticProxyMode)
            {
                SetNpcState(i, NewMapNpcMovementState.StaticFarProxy);
                StaticProxyCount++;
                continue;
            }

            if (farNpc && Mathf.Repeat(Time.time + i * 0.071f, FarNpcUpdateIntervalSeconds) > Time.deltaTime)
            {
                CountNpcStateForDiagnostics(i);
                continue;
            }

            Vector3 home = npcHomePositions[i];
            float localWanderRadius = farNpc ? 1.5f : wanderRadius;
            float phase = Time.time * (farNpc ? 0.07f : 0.3f) + i * 1.7f;
            Vector3 target = home + new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 0.8f)) * localWanderRadius;
            target = ClampToMovementBoundary(target, 2f);
            if (movementConfig == null || movementConfig.continuousMovementEnabled)
            {
                target = ResolveNpcTargetAvoidingBuildings(target, home, i);
                target = ResolveNpcTargetAvoidingPlayerContact(target, npc.position, referencePosition, i);
            }

            Vector3 delta = target - npc.position;
            delta.y = 0f;
            Vector3 previous = npc.position;
            if (delta.sqrMagnitude > 0.01f)
            {
                Vector3 candidate = npc.position + delta.normalized * wanderSpeed * Time.deltaTime;
                candidate = ResolveNpcPositionAfterBuildingCollision(candidate, previous, i);
                npc.position = candidate;
                npc.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
                SetNpcState(i, NewMapNpcMovementState.Moving);
            }
            else
            {
                SetNpcState(i, crowdFailuresEnabled ? NewMapNpcMovementState.QueuedAtEntrance : NewMapNpcMovementState.Wandering);
            }

            if (!IsInsideRuntimePlayableBounds(npc.position))
            {
                npc.position = ClampToMovementBoundary(npc.position, 2f);
                npcHomePositions[i] = ClampToMovementBoundary(home, 2f);
                RecoveredCount++;
            }

            float moved = Vector3.Distance(previous, npc.position);
            speedTotal += Time.deltaTime > 0f ? moved / Time.deltaTime : 0f;
            speedSamples++;
            UpdateNpcStuckState(i, moved, previous);
            CountNpcStateForDiagnostics(i);
        }

        AverageSpeedMetersPerSecond = speedSamples > 0 ? speedTotal / speedSamples : 0f;
        PreventAllStopDeadlock(referencePosition);
    }

    private void Build(Vector3 centerPosition)
    {
        if (built)
        {
            return;
        }

        distributionConfig = distributionConfig ?? NewMapNpcDistributionConfig.Load();
        movementConfig = movementConfig ?? NewMapNpcMovementConfig.Load();
        lifecycleConfig = lifecycleConfig ?? NewMapNpcLifecycleConfig.Load();
        playerNpcCollisionConfig = playerNpcCollisionConfig ?? NewMapPlayerNpcCollisionConfig.Load();
        npcBodyColliderCount = 0;
        BuildBuildingBoundsSpatialIndex();
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
            npcCreatedAtStartupCount = 0;
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

            if (distributionConfig.avoidBuildings && IsInsideBuildingBounds(snapped, distributionConfig.minDistanceFromBuildingMeters))
            {
                InvalidPlacementRetryCount++;
                RejectedInsideBuildingCount++;
                continue;
            }

            acceptedPositions.Add(snapped);
            usedRings.Add(GetRingIndex(center, snapped, distributionConfig));
        }

        if (acceptedPositions.Count < CappedNpcCount)
        {
            FillFallbackGrid(center, acceptedPositions, CappedNpcCount, playableBounds, distributionConfig, buildingAvoidanceBounds);
        }

        UsedSectorCount = CountUsedSectors(center, acceptedPositions, distributionConfig);
        UsedRingCount = usedRings.Count > 0 ? usedRings.Count : CountUsedRings(center, acceptedPositions, distributionConfig);
        for (int i = 0; i < acceptedPositions.Count; i++)
        {
            acceptedPositions[i] = ClampToMovementBoundary(acceptedPositions[i], 2f);
        }

        Material sharedMaterial = NewMapVisualFactory.CreateMaterial("NewMap_NPC_AmbientPedestrian_Material", new Color(1f, 0.62f, 0.12f, 1f));
        for (int i = 0; i < acceptedPositions.Count; i++)
        {
            GameObject npc = new GameObject($"NewMap_NPC_{i + 1:000}");
            npc.transform.SetParent(transform, true);
            npc.transform.position = acceptedPositions[i];
            NewMapVisualFactory.CreateHumanoid(npc.transform, "NPCVisual", new Color(1f, 0.62f, 0.12f, 1f), sharedMaterial);
            if (ConfigureNpcBodyCollider(npc))
            {
                npcBodyColliderCount++;
            }

            npcs.Add(npc.transform);
            npcHomePositions.Add(acceptedPositions[i]);
            npcLastPositions.Add(acceptedPositions[i]);
            npcStoppedSeconds.Add(0f);
            npcStates.Add(NewMapNpcMovementState.Moving);
        }

        npcCreatedAtStartupCount = npcs.Count;
        built = true;
        Debug.Log(
            $"NewMap NPC distribution built. requestedNpcCount={RequestedNpcCount} spawnedNpcCount={SpawnedNpcCount} " +
            $"cappedNpcCount={CappedNpcCount} capReason={CapReason} radiusMeters={distributionConfig.distributionRadiusMeters} " +
            $"usedSectors={UsedSectorCount} usedRings={UsedRingCount} invalidPlacementRetries={InvalidPlacementRetryCount} " +
            $"rejectedInsideBuildings={RejectedInsideBuildingCount} avoidBuildings={distributionConfig.avoidBuildings} " +
            $"usePooling={distributionConfig.usePooling} farNpcStaticProxyMode={FarNpcStaticProxyModeEnabled} " +
            $"continuousMovementEnabled={movementConfig.continuousMovementEnabled} stuckRecoveryEnabled={movementConfig.stuckRecoveryEnabled} " +
            $"buildingAvoidanceEnabled={movementConfig.buildingAvoidanceEnabled} playerNpcCollisionEnabled={PlayerNpcSoftBlockingEnabled} " +
            $"npcBodyColliders={NpcBodyColliderCount} nearNpcCollisionRadius={NearNpcCollisionRadiusMeters:F2} " +
            $"allowGlobalRefresh={AllowGlobalRefresh} globalRespawnIntervalSeconds={GlobalRespawnIntervalSeconds:F1} " +
            $"createdAtStartup={npcCreatedAtStartupCount} instantiateAfterStartup={InstantiateAfterStartupCount}");
    }

    private bool ConfigureNpcBodyCollider(GameObject npc)
    {
        if (npc == null || playerNpcCollisionConfig == null || !playerNpcCollisionConfig.enabled)
        {
            return false;
        }

        CapsuleCollider collider = npc.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = npc.AddComponent<CapsuleCollider>();
        }

        collider.radius = Mathf.Clamp(playerNpcCollisionConfig.nearNpcCollisionRadiusMeters, 0.15f, 1.5f);
        collider.height = Mathf.Clamp(playerNpcCollisionConfig.nearNpcCollisionHeightMeters, collider.radius * 2f, 3f);
        collider.center = new Vector3(0f, collider.height * 0.5f, 0f);
        collider.direction = 1;
        collider.isTrigger = true;
        collider.enabled = true;
        return true;
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
        if (CircularBoundaryClampEnabled)
        {
            return circularBoundary.ContainsXZ(position, 2f);
        }

        if (playableBounds.IsValid)
        {
            return playableBounds.ContainsXZ(position, 2f);
        }

        return IsInsideDocumentedMapBounds(position);
    }

    private Vector3 ClampToMovementBoundary(Vector3 position, float insetMeters)
    {
        if (CircularBoundaryClampEnabled)
        {
            return circularBoundary.ClampXZ(position, insetMeters);
        }

        return playableBounds.IsValid ? playableBounds.ClampXZ(position, insetMeters) : position;
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

    private static void FillFallbackGrid(
        Vector3 centerPosition,
        List<Vector3> accepted,
        int desiredCount,
        NewMapPlayableBounds playableBounds,
        NewMapNpcDistributionConfig config,
        List<Bounds> buildingBounds)
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
                bool insideBuilding = config != null &&
                    config.avoidBuildings &&
                    IsInsideBuildingBounds(candidate, buildingBounds, config.minDistanceFromBuildingMeters);
                if (insideBounds && !insideBuilding && !IsTooCloseToExisting(candidate, accepted, config != null ? config.minDistanceBetweenNpcMeters : 6f))
                {
                    accepted.Add(new Vector3(candidate.x, Mathf.Clamp(centerPosition.y, -20f, 30f), candidate.z));
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

    private bool IsInsideBuildingBounds(Vector3 position, float marginMeters)
    {
        if (buildingAvoidanceBounds.Count == 0)
        {
            return false;
        }

        if (buildingBoundsSpatialIndex.Count == 0)
        {
            return IsInsideBuildingBounds(position, buildingAvoidanceBounds, marginMeters);
        }

        float margin = Mathf.Max(0f, marginMeters);
        for (int i = 0; i < largeBuildingBoundsIndices.Count; i++)
        {
            if (IsInsideExpandedBoundsXZ(position, buildingAvoidanceBounds[largeBuildingBoundsIndices[i]], margin))
            {
                return true;
            }
        }

        int radius = Mathf.Clamp(Mathf.CeilToInt(margin / BuildingBoundsCellSize) + 1, 1, 4);
        int centerX = CellCoordinate(position.x);
        int centerZ = CellCoordinate(position.z);
        for (int z = centerZ - radius; z <= centerZ + radius; z++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (!buildingBoundsSpatialIndex.TryGetValue(CellKey(x, z), out List<int> indices))
                {
                    continue;
                }

                for (int i = 0; i < indices.Count; i++)
                {
                    if (IsInsideExpandedBoundsXZ(position, buildingAvoidanceBounds[indices[i]], margin))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private Vector3 ResolveNpcTargetAvoidingBuildings(Vector3 target, Vector3 home, int index)
    {
        if (movementConfig == null || !movementConfig.buildingAvoidanceEnabled || !IsInsideBuildingBounds(target, distributionConfig.minDistanceFromBuildingMeters))
        {
            return target;
        }

        float phase = (index + 1) * 2.399963f + Time.time * 0.37f;
        Vector3 alternate = home + new Vector3(Mathf.Cos(phase), 0f, Mathf.Sin(phase)) * Mathf.Max(2f, wanderRadius);
        alternate = ClampToMovementBoundary(alternate, 2f);
        if (!IsInsideBuildingBounds(alternate, distributionConfig.minDistanceFromBuildingMeters))
        {
            SetNpcState(index, NewMapNpcMovementState.Repathing);
            return alternate;
        }

        SetNpcState(index, NewMapNpcMovementState.WaitingAtCrossingOrCrowd);
        return home;
    }

    private Vector3 ResolveNpcPositionAfterBuildingCollision(Vector3 candidate, Vector3 previous, int index)
    {
        if (movementConfig == null || !movementConfig.buildingAvoidanceEnabled || !IsInsideBuildingBounds(candidate, distributionConfig.minDistanceFromBuildingMeters))
        {
            return candidate;
        }

        Vector3 corrected = ResolveNearestOutsideBuildingPosition(candidate, distributionConfig.minDistanceFromBuildingMeters + 0.2f);
        if (IsInsideBuildingBounds(corrected, 0.05f))
        {
            corrected = previous;
        }

        NpcBuildingAvoidanceRecoveryCount++;
        RecoveredCount++;
        SetNpcState(index, NewMapNpcMovementState.StuckRecovering);
        corrected.y = candidate.y;
        return corrected;
    }

    private Vector3 ResolveNpcTargetAvoidingPlayerContact(Vector3 target, Vector3 npcPosition, Vector3 playerPosition, int index)
    {
        if (playerNpcCollisionConfig == null || !playerNpcCollisionConfig.enabled)
        {
            return target;
        }

        float npcRadius = Mathf.Clamp(playerNpcCollisionConfig.nearNpcCollisionRadiusMeters, 0.15f, 1.5f);
        float avoidRadius = npcRadius + 1.1f;
        Vector2 npc = new Vector2(npcPosition.x, npcPosition.z);
        Vector2 player = new Vector2(playerPosition.x, playerPosition.z);
        Vector2 away = npc - player;
        if (away.sqrMagnitude > avoidRadius * avoidRadius)
        {
            return target;
        }

        PlayerContactEventCount++;
        if (away.sqrMagnitude < 0.0001f)
        {
            Vector3 targetDelta = target - npcPosition;
            away = new Vector2(-targetDelta.z, targetDelta.x);
        }

        if (away.sqrMagnitude < 0.0001f)
        {
            away = Vector2.right;
        }

        away.Normalize();
        Vector2 perpendicular = new Vector2(-away.y, away.x);
        float step = Mathf.Max(2f, npcRadius * 4f);
        for (int attempt = 0; attempt < 4; attempt++)
        {
            Vector2 direction = away;
            if (attempt == 1)
            {
                direction = perpendicular;
            }
            else if (attempt == 2)
            {
                direction = -perpendicular;
            }
            else if (attempt == 3)
            {
                direction = away + perpendicular;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            direction.Normalize();
            Vector3 alternate = npcPosition + new Vector3(direction.x, 0f, direction.y) * step;
            alternate = ClampToMovementBoundary(alternate, 2f);
            alternate.y = npcPosition.y;
            if (!IsInsideBuildingBounds(alternate, distributionConfig.minDistanceFromBuildingMeters))
            {
                SetNpcState(index, NewMapNpcMovementState.Repathing);
                return alternate;
            }
        }

        SetNpcState(index, NewMapNpcMovementState.WaitingAtCrossingOrCrowd);
        return npcPosition;
    }

    private Vector3 ResolveNearestOutsideBuildingPosition(Vector3 position, float marginMeters)
    {
        Vector3 corrected = position;
        float bestDistance = float.MaxValue;
        if (buildingBoundsSpatialIndex.Count == 0)
        {
            for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
            {
                TryResolveNearestOutsideBound(position, buildingAvoidanceBounds[i], marginMeters, ref corrected, ref bestDistance);
            }

            return corrected;
        }

        for (int i = 0; i < largeBuildingBoundsIndices.Count; i++)
        {
            TryResolveNearestOutsideBound(position, buildingAvoidanceBounds[largeBuildingBoundsIndices[i]], marginMeters, ref corrected, ref bestDistance);
        }

        int radius = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(0f, marginMeters) / BuildingBoundsCellSize) + 1, 1, 4);
        int centerX = CellCoordinate(position.x);
        int centerZ = CellCoordinate(position.z);
        for (int z = centerZ - radius; z <= centerZ + radius; z++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (!buildingBoundsSpatialIndex.TryGetValue(CellKey(x, z), out List<int> indices))
                {
                    continue;
                }

                for (int i = 0; i < indices.Count; i++)
                {
                    TryResolveNearestOutsideBound(position, buildingAvoidanceBounds[indices[i]], marginMeters, ref corrected, ref bestDistance);
                }
            }
        }

        return corrected;
    }

    private void BuildBuildingBoundsSpatialIndex()
    {
        buildingBoundsSpatialIndex.Clear();
        largeBuildingBoundsIndices.Clear();
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds bounds = buildingAvoidanceBounds[i];
            if (!IsFiniteBounds(bounds))
            {
                continue;
            }

            int minX = CellCoordinate(bounds.min.x);
            int maxX = CellCoordinate(bounds.max.x);
            int minZ = CellCoordinate(bounds.min.z);
            int maxZ = CellCoordinate(bounds.max.z);
            int cellCount = (maxX - minX + 1) * (maxZ - minZ + 1);
            if (cellCount > MaxIndexedCellsPerBuildingBound)
            {
                largeBuildingBoundsIndices.Add(i);
                continue;
            }

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    long key = CellKey(x, z);
                    if (!buildingBoundsSpatialIndex.TryGetValue(key, out List<int> indices))
                    {
                        indices = new List<int>();
                        buildingBoundsSpatialIndex.Add(key, indices);
                    }

                    indices.Add(i);
                }
            }
        }
    }

    private static int CellCoordinate(float value)
    {
        return Mathf.FloorToInt(value / BuildingBoundsCellSize);
    }

    private static long CellKey(int x, int z)
    {
        return ((long)x << 32) ^ (uint)z;
    }

    private static bool IsInsideExpandedBoundsXZ(Vector3 position, Bounds bounds, float marginMeters)
    {
        if (!IsFiniteBounds(bounds))
        {
            return false;
        }

        float margin = Mathf.Max(0f, marginMeters);
        return position.x >= bounds.min.x - margin &&
            position.x <= bounds.max.x + margin &&
            position.z >= bounds.min.z - margin &&
            position.z <= bounds.max.z + margin;
    }

    public bool ResolvePlayerPositionAgainstNpcs(
        Vector3 previousPosition,
        Vector3 candidatePosition,
        NewMapPlayerNpcCollisionConfig config,
        out Vector3 resolvedPosition,
        out bool blocked,
        out bool slowed,
        out bool escapeApplied,
        out float slowdownFactor)
    {
        resolvedPosition = candidatePosition;
        blocked = false;
        slowed = false;
        escapeApplied = false;
        slowdownFactor = 1f;

        config = config ?? playerNpcCollisionConfig ?? NewMapPlayerNpcCollisionConfig.Default();
        if (!config.enabled || !config.preventDirectOverlap)
        {
            return false;
        }

        EnsureBuilt();
        if (npcs.Count == 0)
        {
            return false;
        }

        float npcRadius = Mathf.Clamp(config.nearNpcCollisionRadiusMeters, 0.15f, 1.5f);
        float effectiveRadius = npcRadius + 0.26f;
        float effectiveRadiusSquared = effectiveRadius * effectiveRadius;
        float nearDistance = Mathf.Max(1f, config.nearNpcCollisionDistanceMeters);
        float nearDistanceSquared = nearDistance * nearDistance;
        float slowdownRadius = effectiveRadius + Mathf.Max(0.05f, npcRadius * 1.75f);
        float slowdownRadiusSquared = slowdownRadius * slowdownRadius;
        Vector2 previous = new Vector2(previousPosition.x, previousPosition.z);
        Vector2 candidate = new Vector2(candidatePosition.x, candidatePosition.z);
        Vector2 segment = candidate - previous;
        float segmentLengthSquared = segment.sqrMagnitude;
        float closestOverlap = 0f;
        Vector2 bestNpcCenter = Vector2.zero;
        bool hasBlockingNpc = false;

        for (int i = 0; i < npcs.Count; i++)
        {
            Transform npc = npcs[i];
            if (npc == null)
            {
                continue;
            }

            Vector2 center2 = new Vector2(npc.position.x, npc.position.z);
            if ((center2 - previous).sqrMagnitude > nearDistanceSquared &&
                (center2 - candidate).sqrMagnitude > nearDistanceSquared)
            {
                continue;
            }

            float t = 0f;
            if (segmentLengthSquared > 0.0001f)
            {
                t = Mathf.Clamp01(Vector2.Dot(center2 - previous, segment) / segmentLengthSquared);
            }

            Vector2 closest = previous + segment * t;
            float distanceSquared = (closest - center2).sqrMagnitude;
            if (distanceSquared < effectiveRadiusSquared)
            {
                float overlap = effectiveRadius - Mathf.Sqrt(Mathf.Max(0f, distanceSquared));
                if (overlap > closestOverlap)
                {
                    closestOverlap = overlap;
                    bestNpcCenter = center2;
                    hasBlockingNpc = true;
                }
            }
            else if (config.softSlowdownEnabled && distanceSquared < slowdownRadiusSquared)
            {
                slowed = true;
            }
        }

        if (hasBlockingNpc)
        {
            PlayerContactEventCount++;
            Vector2 pushDirection = previous - bestNpcCenter;
            if (pushDirection.sqrMagnitude < 0.0001f)
            {
                pushDirection = candidate - bestNpcCenter;
            }

            if (pushDirection.sqrMagnitude < 0.0001f && segmentLengthSquared > 0.0001f)
            {
                pushDirection = new Vector2(-segment.y, segment.x);
                escapeApplied = true;
            }

            if (pushDirection.sqrMagnitude < 0.0001f)
            {
                pushDirection = Vector2.right;
                escapeApplied = true;
            }

            pushDirection.Normalize();
            Vector2 corrected = bestNpcCenter + pushDirection * effectiveRadius;
            resolvedPosition = new Vector3(corrected.x, candidatePosition.y, corrected.y);
            blocked = true;
            slowdownFactor = Mathf.Clamp(config.maxSlowdownFactor, 0.05f, 1f);
            return true;
        }

        if (slowed)
        {
            PlayerContactEventCount++;
            slowdownFactor = Mathf.Clamp(config.maxSlowdownFactor, 0.05f, 1f);
            resolvedPosition = Vector3.Lerp(previousPosition, candidatePosition, slowdownFactor);
            return true;
        }

        return false;
    }

    private static void TryResolveNearestOutsideBound(
        Vector3 position,
        Bounds bounds,
        float marginMeters,
        ref Vector3 corrected,
        ref float bestDistance)
    {
        if (!IsInsideExpandedBoundsXZ(position, bounds, marginMeters))
        {
            return;
        }

        float margin = Mathf.Max(0f, marginMeters);
        float minX = bounds.min.x - margin;
        float maxX = bounds.max.x + margin;
        float minZ = bounds.min.z - margin;
        float maxZ = bounds.max.z + margin;
        float left = Mathf.Abs(position.x - minX);
        float right = Mathf.Abs(maxX - position.x);
        float back = Mathf.Abs(position.z - minZ);
        float front = Mathf.Abs(maxZ - position.z);
        float nearest = Mathf.Min(Mathf.Min(left, right), Mathf.Min(back, front));
        if (nearest >= bestDistance)
        {
            return;
        }

        bestDistance = nearest;
        if (nearest == left)
        {
            corrected = new Vector3(minX, position.y, position.z);
        }
        else if (nearest == right)
        {
            corrected = new Vector3(maxX, position.y, position.z);
        }
        else if (nearest == back)
        {
            corrected = new Vector3(position.x, position.y, minZ);
        }
        else
        {
            corrected = new Vector3(position.x, position.y, maxZ);
        }
    }

    private void UpdateNpcStuckState(int index, float movedDistance, Vector3 previous)
    {
        if (movementConfig == null || !movementConfig.stuckRecoveryEnabled || index < 0 || index >= npcStoppedSeconds.Count)
        {
            return;
        }

        if (movedDistance > 0.02f)
        {
            npcStoppedSeconds[index] = 0f;
            npcLastPositions[index] = npcs[index] != null ? npcs[index].position : previous;
            return;
        }

        npcStoppedSeconds[index] += Time.deltaTime;
        float recoverySeconds = lifecycleConfig != null
            ? Mathf.Max(1f, lifecycleConfig.stuckRecoverySeconds)
            : movementConfig.maxIdleWithoutReasonSeconds;
        if (npcStoppedSeconds[index] < recoverySeconds)
        {
            return;
        }

        Transform npc = npcs[index];
        if (npc == null)
        {
            return;
        }

        for (int attempt = 0; attempt < 8; attempt++)
        {
            float phase = index * 1.37f + Time.time + attempt * 0.785398f;
            Vector3 recovery = npcHomePositions[index] + new Vector3(
                Mathf.Cos(phase),
                0f,
                Mathf.Sin(phase * 1.17f)) * Mathf.Max(2f, wanderRadius * (0.45f + attempt * 0.08f));
            recovery = ClampToMovementBoundary(recovery, 2f);
            recovery.y = npc.position.y;
            if (!IsInsideBuildingBounds(recovery, distributionConfig.minDistanceFromBuildingMeters))
            {
                npc.position = recovery;
                npcHomePositions[index] = recovery;
                RecoveredCount++;
                SetNpcState(index, NewMapNpcMovementState.StuckRecovering);
                npcStoppedSeconds[index] = 0f;
                return;
            }
        }

        SetNpcState(index, NewMapNpcMovementState.Repathing);
        npcStoppedSeconds[index] = 0f;
    }

    private void PreventAllStopDeadlock(Vector3 referencePosition)
    {
        if (npcs.Count == 0 || Time.time < 8f || MovingCount > 0)
        {
            return;
        }

        if (crowdFailuresEnabled && QueuedCount > 0)
        {
            return;
        }

        int kicked = 0;
        int maxKicks = Mathf.Min(16, npcs.Count);
        for (int i = 0; i < npcs.Count && kicked < maxKicks; i++)
        {
            Transform npc = npcs[i];
            if (npc == null)
            {
                continue;
            }

            Vector3 away = npc.position - referencePosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f)
            {
                away = new Vector3(Mathf.Cos(i * 2.399963f), 0f, Mathf.Sin(i * 2.399963f));
            }

            away.Normalize();
            Vector3 candidate = npc.position + away * Mathf.Max(2f, wanderRadius * 0.35f);
            candidate = ClampToMovementBoundary(candidate, 2f);
            candidate.y = npc.position.y;
            if (IsInsideBuildingBounds(candidate, distributionConfig.minDistanceFromBuildingMeters))
            {
                continue;
            }

            npcHomePositions[i] = candidate;
            SetNpcState(i, NewMapNpcMovementState.Repathing);
            npcStoppedSeconds[i] = 0f;
            kicked++;
        }

        RecountNpcStatesForDiagnostics();
        if (MovingCount == 0 && kicked == 0)
        {
            AllStopEventCount++;
        }
    }

    private void RecountNpcStatesForDiagnostics()
    {
        MovingCount = 0;
        ArrivedCount = 0;
        QueuedCount = 0;
        StuckCount = 0;
        StaticProxyCount = 0;
        for (int i = 0; i < npcStates.Count; i++)
        {
            CountNpcStateForDiagnostics(i);
        }
    }

    private void SetNpcState(int index, NewMapNpcMovementState state)
    {
        if (index >= 0 && index < npcStates.Count)
        {
            npcStates[index] = state;
        }
    }

    private void CountNpcStateForDiagnostics(int index)
    {
        if (index < 0 || index >= npcStates.Count)
        {
            return;
        }

        switch (npcStates[index])
        {
            case NewMapNpcMovementState.Moving:
            case NewMapNpcMovementState.Wandering:
            case NewMapNpcMovementState.Evacuating:
            case NewMapNpcMovementState.Repathing:
                MovingCount++;
                break;
            case NewMapNpcMovementState.Arrived:
                ArrivedCount++;
                break;
            case NewMapNpcMovementState.Queued:
            case NewMapNpcMovementState.QueuedAtEntrance:
            case NewMapNpcMovementState.WaitingAtCrossingOrCrowd:
                QueuedCount++;
                break;
            case NewMapNpcMovementState.StuckRecovering:
                StuckCount++;
                break;
            case NewMapNpcMovementState.StaticFarProxy:
                StaticProxyCount++;
                break;
        }
    }

    private static bool IsInsideBuildingBounds(Vector3 position, List<Bounds> boundsList, float marginMeters)
    {
        if (boundsList == null || boundsList.Count == 0)
        {
            return false;
        }

        float margin = Mathf.Max(0f, marginMeters);
        for (int i = 0; i < boundsList.Count; i++)
        {
            Bounds bounds = boundsList[i];
            if (!IsFiniteBounds(bounds))
            {
                continue;
            }

            if (position.x >= bounds.min.x - margin &&
                position.x <= bounds.max.x + margin &&
                position.z >= bounds.min.z - margin &&
                position.z <= bounds.max.z + margin)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFiniteBounds(Bounds bounds)
    {
        return IsFinite(bounds.min.x) &&
            IsFinite(bounds.min.y) &&
            IsFinite(bounds.min.z) &&
            IsFinite(bounds.max.x) &&
            IsFinite(bounds.max.y) &&
            IsFinite(bounds.max.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

public enum NewMapNpcMovementState
{
    Moving,
    Wandering,
    Evacuating,
    WaitingAtCrossingOrCrowd,
    QueuedAtEntrance,
    Queued,
    Arrived,
    Repathing,
    StuckRecovering,
    PausedByMode,
    StaticFarProxy
}

[System.Serializable]
public sealed class NewMapNpcMovementConfig
{
    public bool continuousMovementEnabled = true;
    public bool wanderInTourismMode = true;
    public bool evacuationTargetSeekingEnabled = true;
    public float stuckDetectionSeconds = 5f;
    public float repathIntervalSeconds = 3f;
    public float maxIdleWithoutReasonSeconds = 4f;
    public bool farNpcUpdateThrottle = true;
    public bool farNpcStaticProxyMode;
    public bool reactivateFarNpcNearPlayer = true;
    public bool stuckRecoveryEnabled = true;
    public bool buildingAvoidanceEnabled = true;
    public bool boundsClampEnabled = true;

    public static NewMapNpcMovementConfig Default()
    {
        return new NewMapNpcMovementConfig();
    }

    public static NewMapNpcMovementConfig Load()
    {
        NewMapNpcMovementConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_npc_movement_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapNpcMovementConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap NPC movement config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.stuckDetectionSeconds = Mathf.Clamp(config.stuckDetectionSeconds, 1f, 30f);
        config.repathIntervalSeconds = Mathf.Clamp(config.repathIntervalSeconds, 0.5f, 30f);
        config.maxIdleWithoutReasonSeconds = Mathf.Clamp(config.maxIdleWithoutReasonSeconds, 1f, 30f);
        config.continuousMovementEnabled = true;
        config.wanderInTourismMode = true;
        config.stuckRecoveryEnabled = true;
        config.buildingAvoidanceEnabled = true;
        config.boundsClampEnabled = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapNpcLifecycleConfig
{
    public bool generateOnModeStartOnly = true;
    public bool allowGlobalRefresh;
    public float globalRespawnIntervalSeconds;
    public bool individualStuckRecoveryEnabled = true;
    public float stuckRecoverySeconds = 5f;
    public float minNpcLifetimeSeconds = 300f;
    public bool farNpcStaticProxyEnabled = true;
    public bool farNpcDespawnEnabled;
    public bool recycleOnlyWhenOutOfBoundsOrInvalid = true;
    public bool collisionWithPlayerDoesNotGlobalPause = true;

    public static NewMapNpcLifecycleConfig Default()
    {
        return new NewMapNpcLifecycleConfig();
    }

    public static NewMapNpcLifecycleConfig Load()
    {
        NewMapNpcLifecycleConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_npc_lifecycle_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapNpcLifecycleConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap NPC lifecycle config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.generateOnModeStartOnly = true;
        config.allowGlobalRefresh = false;
        config.globalRespawnIntervalSeconds = 0f;
        config.individualStuckRecoveryEnabled = true;
        config.stuckRecoverySeconds = Mathf.Clamp(config.stuckRecoverySeconds, 1f, 30f);
        config.minNpcLifetimeSeconds = Mathf.Max(60f, config.minNpcLifetimeSeconds);
        config.farNpcDespawnEnabled = false;
        config.recycleOnlyWhenOutOfBoundsOrInvalid = true;
        config.collisionWithPlayerDoesNotGlobalPause = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapPlayerNpcCollisionConfig
{
    public bool enabled = true;
    public string mode = "soft_blocking_with_near_capsules";
    public float nearNpcCollisionRadiusMeters = 0.45f;
    public float nearNpcCollisionHeightMeters = 1.7f;
    public float nearNpcCollisionDistanceMeters = 60f;
    public bool farNpcPhysicalCollisionDisabled = true;
    public bool softSlowdownEnabled = true;
    public float maxSlowdownFactor = 0.55f;
    public bool preventDirectOverlap = true;
    public bool stuckEscapeEnabled = true;

    public static NewMapPlayerNpcCollisionConfig Default()
    {
        return new NewMapPlayerNpcCollisionConfig();
    }

    public static NewMapPlayerNpcCollisionConfig Load()
    {
        NewMapPlayerNpcCollisionConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_player_npc_collision_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapPlayerNpcCollisionConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap player/NPC collision config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.enabled = true;
        config.mode = string.IsNullOrWhiteSpace(config.mode) ? "soft_blocking_with_near_capsules" : config.mode;
        config.nearNpcCollisionRadiusMeters = Mathf.Clamp(config.nearNpcCollisionRadiusMeters, 0.15f, 1.5f);
        config.nearNpcCollisionHeightMeters = Mathf.Clamp(config.nearNpcCollisionHeightMeters, 0.8f, 3f);
        config.nearNpcCollisionDistanceMeters = Mathf.Clamp(config.nearNpcCollisionDistanceMeters, 5f, 180f);
        config.farNpcPhysicalCollisionDisabled = true;
        config.maxSlowdownFactor = Mathf.Clamp(config.maxSlowdownFactor, 0.05f, 1f);
        config.preventDirectOverlap = true;
        config.stuckEscapeEnabled = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapNpcDistributionConfig
{
    public bool enabled = true;
    public int npcCountMultiplier = 100;
    public float distributionRadiusMeters = 1000f;
    public float minDistanceFromPlayerMeters = 15f;
    public float minDistanceBetweenNpcMeters = 3f;
    public int maxNpcCount = 800;
    public int npcDistributionSeed = 20260529;
    public bool useSectorDistribution = true;
    public int sectorCount = 32;
    public int ringCount = 6;
    public int maxNpcPerSector = 40;
    public bool snapToGround = true;
    public bool snapToGroundCover = true;
    public bool useGroundProxyFallback = true;
    public bool avoidBuildings = true;
    public bool usePooling = true;
    public bool farNpcUpdateThrottle = true;
    public bool farNpcStaticProxyMode = true;
    public bool tourismCrowdFailureDisabled = true;
    public bool evacuationCrowdDelayBounded = true;
    public float minDistanceFromBuildingMeters = 2f;

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
        config.snapToGroundCover = true;
        config.avoidBuildings = true;
        config.usePooling = true;
        config.farNpcUpdateThrottle = true;
        config.farNpcStaticProxyMode = true;
        config.tourismCrowdFailureDisabled = true;
        config.evacuationCrowdDelayBounded = true;
        config.minDistanceFromBuildingMeters = Mathf.Clamp(config.minDistanceFromBuildingMeters, 0f, 25f);
        return config;
    }
}
