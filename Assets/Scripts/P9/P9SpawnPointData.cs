using System;
using UnityEngine;

public class P9SpawnPointData : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;
    public const bool RequiresP7HighDetailScene = P9RuntimePolicy.RequiresP7HighDetailScene;

    [SerializeField] private string spawnPointId = "p9_spawn_point";
    [SerializeField] private string spawnType = "test_origin";
    [SerializeField] private string sourceMode = "test";
    [SerializeField] private int capacity = 1;
    [SerializeField] private float weight = 1f;
    [SerializeField] private int deterministicSeed = 1;
    [SerializeField] private string notes = string.Empty;

    public string SpawnPointId => spawnPointId;
    public string SpawnType => spawnType;
    public string SourceMode => sourceMode;
    public int Capacity => Mathf.Max(0, capacity);
    public float Weight => Mathf.Max(0f, weight);
    public int DeterministicSeed => deterministicSeed;
    public string Notes => notes;

    public void ConfigureForTests(
        string id,
        string type,
        string mode,
        int spawnCapacity,
        float spawnWeight,
        int seed)
    {
        spawnPointId = id ?? string.Empty;
        spawnType = type ?? "test_origin";
        sourceMode = mode ?? "test";
        capacity = Mathf.Max(0, spawnCapacity);
        weight = Mathf.Max(0f, spawnWeight);
        deterministicSeed = seed;
    }

    public void ApplyRecord(P9SpawnPointRecord record)
    {
        if (record == null)
        {
            return;
        }

        spawnPointId = record.spawnPointId ?? string.Empty;
        spawnType = record.spawnType ?? "test_origin";
        sourceMode = record.sourceMode ?? "test";
        capacity = Mathf.Max(0, record.capacity);
        weight = Mathf.Max(0f, record.weight);
        deterministicSeed = record.deterministicSeed;
        notes = record.notes ?? string.Empty;
        transform.position = record.position == null ? transform.position : record.position.ToVector3();
    }

    public P9SpawnPointSnapshot CreateSnapshot()
    {
        return new P9SpawnPointSnapshot
        {
            spawnPointId = spawnPointId ?? string.Empty,
            spawnType = spawnType ?? string.Empty,
            sourceMode = sourceMode ?? string.Empty,
            position = transform.position,
            capacity = Capacity,
            weight = Weight,
            deterministicSeed = deterministicSeed,
            notes = notes ?? string.Empty
        };
    }
}

[Serializable]
public class P9SpawnPointCollection
{
    public string schemaVersion = string.Empty;
    public string sourceMode = string.Empty;
    public string notes = string.Empty;
    public P9SpawnPointRecord[] spawnPoints = Array.Empty<P9SpawnPointRecord>();
}

[Serializable]
public class P9SpawnPointRecord
{
    public string spawnPointId = string.Empty;
    public string spawnType = string.Empty;
    public P9Vector3Data position = new P9Vector3Data();
    public P9CoordinateFields coordinates = new P9CoordinateFields();
    public string sourceMode = string.Empty;
    public int capacity;
    public float weight;
    public string[] activeScenarioIds = Array.Empty<string>();
    public string notes = string.Empty;
    public int deterministicSeed;
}

[Serializable]
public class P9Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[Serializable]
public class P9CoordinateFields
{
    public string coordinateSystem = string.Empty;
    public float latitude;
    public float longitude;
    public string sourceId = string.Empty;
    public string areaName = string.Empty;
}

[Serializable]
public class P9SpawnPointSnapshot
{
    public string spawnPointId = string.Empty;
    public string spawnType = string.Empty;
    public string sourceMode = string.Empty;
    public Vector3 position;
    public int capacity;
    public float weight;
    public int deterministicSeed;
    public string notes = string.Empty;
}
