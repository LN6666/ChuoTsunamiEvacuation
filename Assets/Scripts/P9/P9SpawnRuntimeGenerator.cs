using System;
using System.Collections.Generic;
using UnityEngine;

public class P9SpawnRuntimeGenerator : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;
    public const bool RequiresP7HighDetailScene = P9RuntimePolicy.RequiresP7HighDetailScene;

    [SerializeField] private bool generateOnStart;
    [SerializeField] private int requestedSpawnCount = 12;

    private P9BWeightedSpawnSelectionResult lastSelection;

    public P9BWeightedSpawnSelectionResult LastSelection => lastSelection;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateFromSampleData(transform);
        }
    }

    public P9BWeightedSpawnGenerationResult GenerateFromSampleData(Transform parent)
    {
        P9BLoadResult<P9BWeightedSpawnConfig> config = P9BDataLoader.LoadWeightedSpawnConfig();
        P9BLoadResult<P9BWeightedSpawnZoneCollection> zones = P9BDataLoader.LoadSpawnZones();
        if (!config.success || !zones.success)
        {
            return new P9BWeightedSpawnGenerationResult
            {
                summary = "P9-B spawn generation skipped because sample data could not be loaded.",
                affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
            };
        }

        return Generate(zones.data, config.data, requestedSpawnCount, parent);
    }

    public P9BWeightedSpawnGenerationResult Generate(
        P9BWeightedSpawnZoneCollection zones,
        P9BWeightedSpawnConfig config,
        int spawnCount,
        Transform parent)
    {
        lastSelection = P9WeightedSpawnSelector.Select(zones, config, spawnCount);
        return GenerateFromSelection(lastSelection, parent);
    }

    public P9BWeightedSpawnGenerationResult GenerateFromSelection(
        P9BWeightedSpawnSelectionResult selection,
        Transform parent)
    {
        var result = new P9BWeightedSpawnGenerationResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };

        if (selection == null || selection.selectedSpawns == null)
        {
            result.summary = "P9-B spawn selection missing. No runtime spawn markers generated.";
            return result;
        }

        var snapshots = new List<P9RuntimeMarkerSnapshot>();
        for (int i = 0; i < selection.selectedSpawns.Length; i++)
        {
            P9BWeightedSpawnSelection spawn = selection.selectedSpawns[i];
            var markerObject = new GameObject("P9B_SpawnMarker_" + spawn.spawnId);
            if (parent != null)
            {
                markerObject.transform.SetParent(parent, false);
            }

            markerObject.transform.position = spawn.position;
            var marker = markerObject.AddComponent<P9RuntimeMarker>();
            marker.Configure(
                spawn.spawnId,
                "weighted_spawn_marker",
                spawn.zoneId,
                spawn.spawnCategory,
                false,
                false,
                false,
                string.Empty,
                "spawn_proxy",
                spawn.weightExplanation);

            snapshots.Add(marker.CreateSnapshot());
        }

        result.success = true;
        result.generatedMarkerCount = snapshots.Count;
        result.selection = selection;
        result.snapshots = snapshots.ToArray();
        result.summary = "P9-B generated " + result.generatedMarkerCount + " weighted spawn runtime markers.";
        return result;
    }
}

[Serializable]
public class P9BWeightedSpawnGenerationResult
{
    public bool success;
    public int generatedMarkerCount;
    public bool affectsGameplaySuccessFailure;
    public P9BWeightedSpawnSelectionResult selection;
    public P9RuntimeMarkerSnapshot[] snapshots = Array.Empty<P9RuntimeMarkerSnapshot>();
    public string summary = string.Empty;
}
