using System;
using System.Collections.Generic;
using UnityEngine;

public class P9CrowdRuntimeSpawner : MonoBehaviour
{
    public const bool AffectsPlayerSuccessFailure = P9RuntimePolicy.CanCausePlayerFailureInP9B;
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ClaimsRealCrowdModel = false;

    [SerializeField] private bool spawnOnStart;

    private readonly List<P9CrowdRuntimeAgent> spawnedAgents = new List<P9CrowdRuntimeAgent>();
    private P9CrowdRuntimeSpawnResult lastSpawnResult;

    public IReadOnlyList<P9CrowdRuntimeAgent> SpawnedAgents => spawnedAgents;
    public P9CrowdRuntimeSpawnResult LastSpawnResult => lastSpawnResult;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnFromSampleData(transform);
        }
    }

    public P9CrowdRuntimeSpawnResult SpawnFromSampleData(Transform parent)
    {
        P9BLoadResult<P9BWeightedSpawnConfig> spawnConfig = P9BDataLoader.LoadWeightedSpawnConfig();
        P9BLoadResult<P9BWeightedSpawnZoneCollection> spawnZones = P9BDataLoader.LoadSpawnZones();
        P9BLoadResult<P9BCrowdRuntimeScenario> scenario = P9BDataLoader.LoadCrowdScenario();
        P9BLoadResult<P9EntranceSafeFloorProxyCollection> targets = P9BDataLoader.LoadEntranceMarkerAssignments();
        if (!spawnConfig.success || !spawnZones.success || !scenario.success || !targets.success)
        {
            return new P9CrowdRuntimeSpawnResult
            {
                summary = "P9-B crowd spawn skipped because sample data could not be loaded.",
                affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
            };
        }

        P9BWeightedSpawnSelectionResult selection = P9WeightedSpawnSelector.Select(
            spawnZones.data,
            spawnConfig.data,
            scenario.data.requestedSpawnCount);
        return Spawn(selection, targets.data, scenario.data, parent);
    }

    public P9CrowdRuntimeSpawnResult Spawn(
        P9BWeightedSpawnSelectionResult selection,
        P9EntranceSafeFloorProxyCollection targetCollection,
        P9BCrowdRuntimeScenario scenario,
        Transform parent)
    {
        spawnedAgents.Clear();
        var result = new P9CrowdRuntimeSpawnResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };

        if (selection == null || selection.selectedSpawns == null || targetCollection == null || targetCollection.proxies == null || scenario == null)
        {
            result.summary = "P9-B crowd spawn missing inputs. No agents spawned.";
            lastSpawnResult = result;
            return result;
        }

        int cap = Mathf.Min(selection.selectedSpawns.Length, Mathf.Max(0, scenario.maxSpawnedAgents));
        cap = Mathf.Min(cap, Mathf.Max(0, scenario.maxActiveCrowdAgents));
        for (int i = 0; i < cap; i++)
        {
            P9BWeightedSpawnSelection spawn = selection.selectedSpawns[i];
            P9EntranceSafeFloorProxyRecord target = targetCollection.proxies[i % targetCollection.proxies.Length];
            if (target == null)
            {
                continue;
            }

            var agentObject = new GameObject("P9B_CrowdAgent_" + i.ToString("000"));
            if (parent != null)
            {
                agentObject.transform.SetParent(parent, false);
            }

            agentObject.transform.position = spawn.position;
            var agent = agentObject.AddComponent<P9CrowdRuntimeAgent>();
            agent.Configure(
                "p9b_agent_" + i.ToString("000"),
                spawn.spawnId,
                target.proxyId,
                target.entrancePosition == null ? Vector3.zero : target.entrancePosition.ToVector3(),
                scenario.defaultAgentSpeedMetersPerSecond,
                scenario.simulateMovementInUpdate);
            spawnedAgents.Add(agent);
        }

        result.success = true;
        result.spawnedCount = spawnedAgents.Count;
        result.metrics = P9CrowdRuntimeMetrics.Create(spawnedAgents);
        result.summary = "P9-B spawned " + spawnedAgents.Count + " lightweight crowd agents. Metrics only.";
        lastSpawnResult = result;
        return result;
    }
}

[Serializable]
public class P9BCrowdRuntimeScenario
{
    public string schemaVersion = string.Empty;
    public string scenarioId = string.Empty;
    public int deterministicSeed;
    public int maxSpawnedAgents;
    public int maxActiveCrowdAgents;
    public int requestedSpawnCount;
    public string spawnCategory = string.Empty;
    public float defaultAgentSpeedMetersPerSecond = 1.2f;
    public float agentSpacingMeters = 0.75f;
    public string targetSelectionMode = string.Empty;
    public bool simulateMovementInUpdate;
    public int queueWarningLength;
    public bool metricsOnlyNoFailureEffect = true;
    public string notes = string.Empty;
}

[Serializable]
public class P9CrowdRuntimeSpawnResult
{
    public bool success;
    public int spawnedCount;
    public bool affectsGameplaySuccessFailure;
    public P9CrowdRuntimeMetricsSnapshot metrics = new P9CrowdRuntimeMetricsSnapshot();
    public string summary = string.Empty;
}
