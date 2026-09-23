using System;
using System.Collections.Generic;
using UnityEngine;

public static class P9CrowdRuntimeMetrics
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;

    public static P9CrowdRuntimeMetricsSnapshot Create(IList<P9CrowdRuntimeAgent> agents)
    {
        var snapshot = new P9CrowdRuntimeMetricsSnapshot
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };

        if (agents == null || agents.Count == 0)
        {
            snapshot.summary = "P9-B crowd metrics: no agents.";
            return snapshot;
        }

        int reached = 0;
        float totalEstimatedTime = 0f;
        var targetCounts = new Dictionary<string, int>();
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i] == null)
            {
                continue;
            }

            if (agents[i].ReachedTarget)
            {
                reached++;
            }

            totalEstimatedTime += agents[i].EstimatedEvacuationTimeSeconds;
            string target = string.IsNullOrWhiteSpace(agents[i].TargetProxyId) ? "unknown" : agents[i].TargetProxyId;
            if (!targetCounts.ContainsKey(target))
            {
                targetCounts[target] = 0;
            }

            targetCounts[target]++;
        }

        snapshot.spawnedCount = agents.Count;
        snapshot.reachedCount = reached;
        snapshot.averageEstimatedEvacuationTimeSeconds = totalEstimatedTime / Mathf.Max(1, agents.Count);
        snapshot.targetSelectionCounts = ToTargetCounts(targetCounts);
        snapshot.congestionHotspotProxyId = FindHotspot(snapshot.targetSelectionCounts);
        snapshot.summary = "P9-B crowd metrics: spawned=" + snapshot.spawnedCount +
                           ", reached=" + snapshot.reachedCount +
                           ", avgEstimatedTime=" + snapshot.averageEstimatedEvacuationTimeSeconds.ToString("0.00") +
                           ". Metrics only.";
        return snapshot;
    }

    private static P9CrowdTargetSelectionCount[] ToTargetCounts(Dictionary<string, int> counts)
    {
        var results = new List<P9CrowdTargetSelectionCount>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            results.Add(new P9CrowdTargetSelectionCount
            {
                targetProxyId = pair.Key,
                count = pair.Value
            });
        }

        results.Sort((left, right) => string.CompareOrdinal(left.targetProxyId, right.targetProxyId));
        return results.ToArray();
    }

    private static string FindHotspot(P9CrowdTargetSelectionCount[] counts)
    {
        string hotspot = string.Empty;
        int max = -1;
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i].count > max)
            {
                max = counts[i].count;
                hotspot = counts[i].targetProxyId;
            }
        }

        return hotspot;
    }
}

[Serializable]
public class P9CrowdRuntimeMetricsSnapshot
{
    public int spawnedCount;
    public int reachedCount;
    public float averageEstimatedEvacuationTimeSeconds;
    public string congestionHotspotProxyId = string.Empty;
    public P9CrowdTargetSelectionCount[] targetSelectionCounts = Array.Empty<P9CrowdTargetSelectionCount>();
    public bool affectsGameplaySuccessFailure;
    public string summary = string.Empty;
}

[Serializable]
public class P9CrowdTargetSelectionCount
{
    public string targetProxyId = string.Empty;
    public int count;
}
