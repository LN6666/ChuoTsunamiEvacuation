using System;
using UnityEngine;

public class P9CrowdSimulationConfig : MonoBehaviour
{
    public const bool SpawnRuntimeEnabledInP9A = false;
    public const bool EntranceQueueRuntimeEnabledInP9A = false;
    public const bool FinalFailureEnabledInP9A = P9RuntimePolicy.ImplementsFinalFailureGameplay;
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresP8DEFinalHandoff = P9RuntimePolicy.RequiresP8DEFinalHandoff;

    [SerializeField] private int maxSpawnedAgents = 32;
    [SerializeField] private int maxActiveCrowdAgents = 32;
    [SerializeField] private float defaultAgentSpacingMeters = 0.75f;
    [SerializeField] private int defaultEntranceQueueWarningLength = 8;
    [SerializeField] private string activeScenarioId = "p9a_sample";
    [SerializeField] private string notes = "P9-A scaffold config only.";

    public int MaxSpawnedAgents => Mathf.Max(0, maxSpawnedAgents);
    public int MaxActiveCrowdAgents => Mathf.Max(0, maxActiveCrowdAgents);
    public float DefaultAgentSpacingMeters => Mathf.Max(0f, defaultAgentSpacingMeters);
    public int DefaultEntranceQueueWarningLength => Mathf.Max(0, defaultEntranceQueueWarningLength);
    public string ActiveScenarioId => activeScenarioId;
    public string Notes => notes;

    public P9CrowdSimulationConfigSnapshot CreateSnapshot()
    {
        return new P9CrowdSimulationConfigSnapshot
        {
            maxSpawnedAgents = MaxSpawnedAgents,
            maxActiveCrowdAgents = MaxActiveCrowdAgents,
            defaultAgentSpacingMeters = DefaultAgentSpacingMeters,
            defaultEntranceQueueWarningLength = DefaultEntranceQueueWarningLength,
            activeScenarioId = activeScenarioId ?? string.Empty,
            spawnRuntimeEnabledInP9A = SpawnRuntimeEnabledInP9A,
            entranceQueueRuntimeEnabledInP9A = EntranceQueueRuntimeEnabledInP9A,
            finalFailureEnabledInP9A = FinalFailureEnabledInP9A,
            notes = notes ?? string.Empty
        };
    }
}

[Serializable]
public class P9CrowdSimulationConfigSnapshot
{
    public int maxSpawnedAgents;
    public int maxActiveCrowdAgents;
    public float defaultAgentSpacingMeters;
    public int defaultEntranceQueueWarningLength;
    public string activeScenarioId = string.Empty;
    public bool spawnRuntimeEnabledInP9A;
    public bool entranceQueueRuntimeEnabledInP9A;
    public bool finalFailureEnabledInP9A;
    public string notes = string.Empty;
}
