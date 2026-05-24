using System;
using System.Collections.Generic;
using UnityEngine;

public static class P9WeightedSpawnSelector
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;

    public static P9BWeightedSpawnSelectionResult Select(
        P9BWeightedSpawnZoneCollection zoneCollection,
        P9BWeightedSpawnConfig config,
        int requestedSpawnCount)
    {
        var result = new P9BWeightedSpawnSelectionResult
        {
            scenarioPresetId = config == null ? string.Empty : config.scenarioPresetId,
            deterministicSeed = config == null ? 0 : config.deterministicSeed,
            requestedSpawnCount = Mathf.Max(0, requestedSpawnCount),
            spawnCountCap = config == null ? 0 : config.SpawnCountCap,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };

        if (zoneCollection == null || zoneCollection.zones == null || config == null)
        {
            result.summary = "P9-B weighted spawn selection missing data. No spawns selected.";
            return result;
        }

        int cap = Mathf.Min(Mathf.Max(0, requestedSpawnCount), config.SpawnCountCap);
        var selected = new List<P9BWeightedSpawnSelection>();
        var explanations = new List<P9BWeightedSpawnCandidateExplanation>();
        var candidates = new List<P9BWeightedSpawnCandidate>();
        for (int i = 0; i < zoneCollection.zones.Length; i++)
        {
            P9BWeightedSpawnCandidateExplanation explanation = ExplainCandidate(zoneCollection.zones[i], config);
            explanations.Add(explanation);
            if (!explanation.included)
            {
                continue;
            }

            candidates.Add(new P9BWeightedSpawnCandidate
            {
                record = zoneCollection.zones[i],
                weight = explanation.computedWeight,
                remainingCapacity = Mathf.Max(0, zoneCollection.zones[i].capacity),
                explanation = explanation
            });
        }

        var random = new System.Random(config.deterministicSeed);
        for (int i = 0; i < cap; i++)
        {
            float totalWeight = 0f;
            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                if (candidates[candidateIndex].remainingCapacity > 0)
                {
                    totalWeight += candidates[candidateIndex].weight;
                }
            }

            if (totalWeight <= 0f)
            {
                break;
            }

            float roll = (float)(random.NextDouble() * totalWeight);
            P9BWeightedSpawnCandidate picked = null;
            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                P9BWeightedSpawnCandidate candidate = candidates[candidateIndex];
                if (candidate.remainingCapacity <= 0)
                {
                    continue;
                }

                roll -= candidate.weight;
                if (roll <= 0f)
                {
                    picked = candidate;
                    break;
                }
            }

            if (picked == null)
            {
                break;
            }

            picked.remainingCapacity--;
            selected.Add(CreateSelection(picked.record, picked.weight, picked.explanation.weightExplanation, random, selected.Count));
        }

        result.candidateExplanations = explanations.ToArray();
        result.selectedSpawns = selected.ToArray();
        result.selectedSpawnCount = result.selectedSpawns.Length;
        result.success = true;
        result.summary = "P9-B weighted spawn selected " + result.selectedSpawnCount + " spawn proxies from " +
                         explanations.Count + " eligible/excluded candidates. Metrics only.";
        return result;
    }

    public static float ComputeWeight(P9BWeightedSpawnZoneRecord zone, P9BWeightedSpawnConfig config)
    {
        return ExplainCandidate(zone, config).computedWeight;
    }

    public static P9BWeightedSpawnCandidateExplanation ExplainCandidate(
        P9BWeightedSpawnZoneRecord zone,
        P9BWeightedSpawnConfig config)
    {
        var explanation = new P9BWeightedSpawnCandidateExplanation
        {
            zoneId = zone == null ? string.Empty : zone.zoneId,
            included = false,
            computedWeight = 0f,
            weightExplanation = "missing_zone"
        };

        if (zone == null || config == null)
        {
            return explanation;
        }

        if (zone.explicitlyBlocked)
        {
            explanation.weightExplanation = "excluded_explicitly_blocked";
            return explanation;
        }

        if (config.excludeAlreadyFloodedZones && zone.alreadyFlooded)
        {
            explanation.weightExplanation = "excluded_already_flooded";
            return explanation;
        }

        if (!zone.validPosition)
        {
            explanation.weightExplanation = "excluded_invalid_position";
            return explanation;
        }

        bool unsafeZero = IsZero(zone.Position) &&
                          config.rejectUnsafeZeroPositions &&
                          !config.allowZeroPositionInDebug &&
                          !zone.allowZeroPositionForDebug;
        if (unsafeZero)
        {
            explanation.weightExplanation = "excluded_unsafe_zero_position";
            return explanation;
        }

        if (!string.IsNullOrWhiteSpace(config.spawnCategory) &&
            !string.Equals(config.spawnCategory, zone.spawnCategory, StringComparison.OrdinalIgnoreCase))
        {
            explanation.weightExplanation = "excluded_spawn_category_mismatch";
            return explanation;
        }

        float weight = Mathf.Max(0f, zone.baseWeight);
        P9BWeightedSpawnWeightFactors factors = config.weightFactors ?? new P9BWeightedSpawnWeightFactors();
        weight += Mathf.Max(0f, factors.baseWeight);
        string reasons = "base";

        AddBias(zone.isCoastalOrWaterfront, factors.coastalWaterfrontBias, "coastal_waterfront", ref weight, ref reasons);
        AddBias(zone.isLowElevation, factors.lowElevationBias, "low_elevation", ref weight, ref reasons);
        AddBias(zone.isRiverOrCanalAdjacent, factors.riverCanalAdjacentBias, "river_canal_adjacent", ref weight, ref reasons);
        AddBias(zone.hasHighInundationExposure, factors.highInundationExposureBias, "high_inundation_exposure", ref weight, ref reasons);
        AddBias(zone.isUndergroundOrMetroEntrance, factors.undergroundMetroEntranceBias, "underground_metro", ref weight, ref reasons);
        AddBias(zone.isOfficeCommercialDense, factors.officeCommercialDenseBias, "office_commercial_dense", ref weight, ref reasons);

        if (zone.alreadyFlooded)
        {
            weight *= Mathf.Max(0f, config.alreadyFloodedZoneWeightMultiplier);
            reasons += "+already_flooded_multiplier";
        }

        explanation.included = zone.capacity > 0 && weight > 0f;
        explanation.computedWeight = explanation.included ? weight : 0f;
        explanation.weightExplanation = explanation.included ? reasons : "excluded_zero_capacity_or_weight";
        return explanation;
    }

    private static P9BWeightedSpawnSelection CreateSelection(
        P9BWeightedSpawnZoneRecord zone,
        float weight,
        string explanation,
        System.Random random,
        int index)
    {
        Vector3 offset = CreateDeterministicOffset(zone.radiusMeters, random);
        return new P9BWeightedSpawnSelection
        {
            spawnId = zone.zoneId + "_spawn_" + index.ToString("000"),
            zoneId = zone.zoneId,
            spawnCategory = zone.spawnCategory,
            position = zone.Position + offset,
            selectedWeight = weight,
            weightExplanation = explanation
        };
    }

    private static Vector3 CreateDeterministicOffset(float radius, System.Random random)
    {
        float safeRadius = Mathf.Max(0f, radius);
        if (safeRadius <= 0f)
        {
            return Vector3.zero;
        }

        double angle = random.NextDouble() * Math.PI * 2.0;
        double distance = Math.Sqrt(random.NextDouble()) * safeRadius;
        return new Vector3((float)(Math.Cos(angle) * distance), 0f, (float)(Math.Sin(angle) * distance));
    }

    private static void AddBias(bool enabled, float bias, string reason, ref float weight, ref string reasons)
    {
        if (!enabled)
        {
            return;
        }

        weight += Mathf.Max(0f, bias);
        reasons += "+" + reason;
    }

    private static bool IsZero(Vector3 value)
    {
        return Mathf.Approximately(value.x, 0f) &&
               Mathf.Approximately(value.y, 0f) &&
               Mathf.Approximately(value.z, 0f);
    }
}

internal class P9BWeightedSpawnCandidate
{
    public P9BWeightedSpawnZoneRecord record;
    public float weight;
    public int remainingCapacity;
    public P9BWeightedSpawnCandidateExplanation explanation;
}

[Serializable]
public class P9BWeightedSpawnSelectionResult
{
    public bool success;
    public int deterministicSeed;
    public string scenarioPresetId = string.Empty;
    public int requestedSpawnCount;
    public int spawnCountCap;
    public int selectedSpawnCount;
    public bool affectsGameplaySuccessFailure;
    public P9BWeightedSpawnCandidateExplanation[] candidateExplanations = Array.Empty<P9BWeightedSpawnCandidateExplanation>();
    public P9BWeightedSpawnSelection[] selectedSpawns = Array.Empty<P9BWeightedSpawnSelection>();
    public string summary = string.Empty;
}

[Serializable]
public class P9BWeightedSpawnCandidateExplanation
{
    public string zoneId = string.Empty;
    public bool included;
    public float computedWeight;
    public string weightExplanation = string.Empty;
}

[Serializable]
public class P9BWeightedSpawnSelection
{
    public string spawnId = string.Empty;
    public string zoneId = string.Empty;
    public string spawnCategory = string.Empty;
    public Vector3 position;
    public float selectedWeight;
    public string weightExplanation = string.Empty;
}
