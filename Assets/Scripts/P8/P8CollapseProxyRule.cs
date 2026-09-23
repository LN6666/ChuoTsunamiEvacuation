using System;
using UnityEngine;

[Serializable]
public class P8CollapseProxyDecision
{
    public bool eligible;
    public bool showCollapsedProxyVisual;
    public P8CollapseProxyState state = P8CollapseProxyState.None;
    public float probabilityRoll;
    public float configuredProbability;
    public int deterministicHash;
    public string reason = string.Empty;
}

public static class P8CollapseProxyRule
{
    public const bool IsStructuralEngineeringAssessment = false;
    public const bool UsesPhysicsCollapse = false;
    public const bool UsesDebrisSimulation = false;

    public static P8CollapseProxyDecision Evaluate(
        string stableId,
        P8InfrastructureDamageConfig config,
        float inundationDepthMeters,
        float hazardIntensity,
        int collapseSelectionRank)
    {
        config = config ?? P8InfrastructureDamageConfig.Default();
        var decision = new P8CollapseProxyDecision
        {
            configuredProbability = config.ClampedCollapseProbability()
        };

        bool hazardEligible = inundationDepthMeters >= Mathf.Max(0f, config.collapseProxyDepthMeters) ||
                              hazardIntensity >= Mathf.Clamp01(config.collapseProxyIntensity);
        if (!config.enableCollapseProxyVisual || !hazardEligible)
        {
            decision.state = P8CollapseProxyState.SuppressedByConfig;
            decision.reason = "Collapse proxy visual disabled or hazard below configured threshold.";
            return decision;
        }

        decision.eligible = true;
        if (config.maxCollapseProxySampleCount <= 0 || collapseSelectionRank >= config.maxCollapseProxySampleCount)
        {
            decision.state = P8CollapseProxyState.SuppressedBySampleLimit;
            decision.reason = "Deterministic collapse proxy sample limit reached.";
            return decision;
        }

        decision.deterministicHash = ComputeStableHash((stableId ?? string.Empty) + ":" + config.collapseProxyRandomSeed);
        decision.probabilityRoll = HashToUnitInterval(decision.deterministicHash);
        if (decision.probabilityRoll > decision.configuredProbability)
        {
            decision.state = P8CollapseProxyState.SuppressedByProbability;
            decision.reason = "Deterministic roll exceeds low-probability collapse proxy threshold.";
            return decision;
        }

        decision.state = P8CollapseProxyState.CollapsedProxyVisual;
        decision.showCollapsedProxyVisual = true;
        decision.reason = "Collapsed proxy visual selected by deterministic low-probability rule. This is not real collapse.";
        return decision;
    }

    public static int ComputeStableHash(string text)
    {
        unchecked
        {
            const int fnvOffset = unchecked((int)2166136261);
            const int fnvPrime = 16777619;
            int hash = fnvOffset;
            string value = text ?? string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= fnvPrime;
            }

            return hash & int.MaxValue;
        }
    }

    private static float HashToUnitInterval(int hash)
    {
        return (hash % 10000) / 10000f;
    }
}
