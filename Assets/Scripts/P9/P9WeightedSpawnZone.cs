using System;
using UnityEngine;

public class P9WeightedSpawnZone : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;

    [SerializeField] private string zoneId = "p9b_spawn_zone";
    [SerializeField] private string displayName = "P9-B Spawn Zone";
    [SerializeField] private string spawnCategory = "civilian_runtime_proxy";
    [SerializeField] private float radiusMeters = 4f;
    [SerializeField] private int capacity = 1;
    [SerializeField] private float baseWeight = 1f;
    [SerializeField] private bool isCoastalOrWaterfront;
    [SerializeField] private bool isLowElevation;
    [SerializeField] private bool isRiverOrCanalAdjacent;
    [SerializeField] private bool hasHighInundationExposure;
    [SerializeField] private bool isUndergroundOrMetroEntrance;
    [SerializeField] private bool isOfficeCommercialDense;
    [SerializeField] private bool explicitlyBlocked;
    [SerializeField] private bool alreadyFlooded;
    [SerializeField] private bool validPosition = true;
    [SerializeField] private bool allowZeroPositionForDebug;
    [SerializeField] private string sourceMode = "rule_based";
    [SerializeField] private string guidanceOnlyNotes = string.Empty;

    public string ZoneId => zoneId;
    public string DisplayName => displayName;
    public string SpawnCategory => spawnCategory;
    public float RadiusMeters => Mathf.Max(0f, radiusMeters);
    public int Capacity => Mathf.Max(0, capacity);
    public float BaseWeight => Mathf.Max(0f, baseWeight);
    public bool IsCoastalOrWaterfront => isCoastalOrWaterfront;
    public bool IsLowElevation => isLowElevation;
    public bool IsRiverOrCanalAdjacent => isRiverOrCanalAdjacent;
    public bool HasHighInundationExposure => hasHighInundationExposure;
    public bool IsUndergroundOrMetroEntrance => isUndergroundOrMetroEntrance;
    public bool IsOfficeCommercialDense => isOfficeCommercialDense;
    public bool ExplicitlyBlocked => explicitlyBlocked;
    public bool AlreadyFlooded => alreadyFlooded;
    public bool ValidPosition => validPosition;
    public bool AllowZeroPositionForDebug => allowZeroPositionForDebug;
    public string SourceMode => sourceMode;
    public string GuidanceOnlyNotes => guidanceOnlyNotes;

    public void ApplyRecord(P9BWeightedSpawnZoneRecord record)
    {
        if (record == null)
        {
            return;
        }

        zoneId = record.zoneId ?? string.Empty;
        displayName = record.displayName ?? string.Empty;
        spawnCategory = record.spawnCategory ?? string.Empty;
        radiusMeters = Mathf.Max(0f, record.radiusMeters);
        capacity = Mathf.Max(0, record.capacity);
        baseWeight = Mathf.Max(0f, record.baseWeight);
        isCoastalOrWaterfront = record.isCoastalOrWaterfront;
        isLowElevation = record.isLowElevation;
        isRiverOrCanalAdjacent = record.isRiverOrCanalAdjacent;
        hasHighInundationExposure = record.hasHighInundationExposure;
        isUndergroundOrMetroEntrance = record.isUndergroundOrMetroEntrance;
        isOfficeCommercialDense = record.isOfficeCommercialDense;
        explicitlyBlocked = record.explicitlyBlocked;
        alreadyFlooded = record.alreadyFlooded;
        validPosition = record.validPosition;
        allowZeroPositionForDebug = record.allowZeroPositionForDebug;
        sourceMode = record.sourceMode ?? string.Empty;
        guidanceOnlyNotes = record.guidanceOnlyNotes ?? string.Empty;
        transform.position = record.position == null ? transform.position : record.position.ToVector3();
    }
}

[Serializable]
public class P9BWeightedSpawnConfig
{
    public string schemaVersion = string.Empty;
    public string scenarioPresetId = string.Empty;
    public int deterministicSeed = 1;
    public int spawnCountCap = 1;
    public string spawnCategory = string.Empty;
    public bool excludeAlreadyFloodedZones = true;
    public bool rejectUnsafeZeroPositions = true;
    public bool allowZeroPositionInDebug;
    public float blockedZoneWeightMultiplier;
    public float alreadyFloodedZoneWeightMultiplier;
    public P9BWeightedSpawnWeightFactors weightFactors = new P9BWeightedSpawnWeightFactors();
    public string notes = string.Empty;

    public int SpawnCountCap => Mathf.Max(0, spawnCountCap);
}

[Serializable]
public class P9BWeightedSpawnWeightFactors
{
    public float baseWeight = 1f;
    public float coastalWaterfrontBias = 2.5f;
    public float lowElevationBias = 2f;
    public float riverCanalAdjacentBias = 1.8f;
    public float highInundationExposureBias = 3f;
    public float undergroundMetroEntranceBias = 1.5f;
    public float officeCommercialDenseBias = 1.4f;
}

[Serializable]
public class P9BWeightedSpawnZoneCollection
{
    public string schemaVersion = string.Empty;
    public string sourceMode = string.Empty;
    public string coordinatePolicy = string.Empty;
    public string notes = string.Empty;
    public P9BWeightedSpawnZoneRecord[] zones = Array.Empty<P9BWeightedSpawnZoneRecord>();
}

[Serializable]
public class P9BWeightedSpawnZoneRecord
{
    public string zoneId = string.Empty;
    public string displayName = string.Empty;
    public string spawnCategory = string.Empty;
    public P9Vector3Data position = new P9Vector3Data();
    public float radiusMeters;
    public int capacity;
    public float baseWeight;
    public bool isCoastalOrWaterfront;
    public bool isLowElevation;
    public bool isRiverOrCanalAdjacent;
    public bool hasHighInundationExposure;
    public bool isUndergroundOrMetroEntrance;
    public bool isOfficeCommercialDense;
    public bool explicitlyBlocked;
    public bool alreadyFlooded;
    public bool validPosition = true;
    public bool allowZeroPositionForDebug;
    public string sourceMode = string.Empty;
    public string guidanceOnlyNotes = string.Empty;

    public Vector3 Position => position == null ? Vector3.zero : position.ToVector3();
}
