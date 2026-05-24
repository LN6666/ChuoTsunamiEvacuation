using System;

public static class P9ScenarioDebugSummary
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool WritesFinalRunLogs = false;

    public static P9ScenarioDebugSummaryResult Create(
        P9SpawnPointCollection spawnPoints,
        P9CrowdAgentProfileCollection crowdAgents,
        P9EntranceSafeFloorProxyCollection entranceProxies,
        P9P8HandoffState handoffState)
    {
        int spawnCount = spawnPoints == null || spawnPoints.spawnPoints == null ? 0 : spawnPoints.spawnPoints.Length;
        int crowdCount = crowdAgents == null || crowdAgents.agentProfiles == null ? 0 : crowdAgents.agentProfiles.Length;
        int proxyCount = entranceProxies == null || entranceProxies.proxies == null ? 0 : entranceProxies.proxies.Length;
        bool handoffMissing = handoffState == null || handoffState.isMissingOrFallback;

        return new P9ScenarioDebugSummaryResult
        {
            spawnPointCount = spawnCount,
            crowdAgentProfileCount = crowdCount,
            entranceProxyCount = proxyCount,
            p8HandoffMissingOrFallback = handoffMissing,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            summary = "P9-A scaffold summary: spawnPoints=" + spawnCount +
                      ", crowdProfiles=" + crowdCount +
                      ", entranceProxies=" + proxyCount +
                      ", p8HandoffFallback=" + handoffMissing +
                      ". No final failure gameplay."
        };
    }
}

[Serializable]
public class P9ScenarioDebugSummaryResult
{
    public int spawnPointCount;
    public int crowdAgentProfileCount;
    public int entranceProxyCount;
    public bool p8HandoffMissingOrFallback;
    public bool affectsGameplaySuccessFailure;
    public string summary = string.Empty;
}
