using System;
using System.Collections.Generic;
using UnityEngine;

public class P9CollapseDebrisRiskZone : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool CanCausePlayerFailureInP9B = P9RuntimePolicy.CanCausePlayerFailureInP9B;

    [SerializeField] private string zoneId = string.Empty;
    [SerializeField] private string displayName = string.Empty;
    [SerializeField] private float radiusMeters = 4f;
    [SerializeField] private int riskLevel = 1;
    [SerializeField] private float exposureProbability = 0.05f;
    [SerializeField] private int deterministicSeed = 1;
    [SerializeField] private string hazardSourceMode = "manual_sample";
    [SerializeField] private bool warningOnly = true;
    [SerializeField] private bool canKillPlayer;
    [SerializeField] private bool noPhysicsCollapse = true;
    [SerializeField] private string notes = string.Empty;

    public string ZoneId => zoneId;
    public string DisplayName => displayName;
    public float RadiusMeters => Mathf.Max(0f, radiusMeters);
    public int RiskLevel => Mathf.Max(0, riskLevel);
    public float ExposureProbability => Mathf.Clamp01(exposureProbability);
    public int DeterministicSeed => deterministicSeed;
    public string HazardSourceMode => hazardSourceMode;
    public bool WarningOnly => warningOnly;
    public bool CanKillPlayer => canKillPlayer;
    public bool NoPhysicsCollapse => noPhysicsCollapse;
    public string Notes => notes;

    public void ApplyRecord(P9BCollapseDebrisRiskZoneRecord record)
    {
        if (record == null)
        {
            return;
        }

        zoneId = record.zoneId ?? string.Empty;
        displayName = record.displayName ?? string.Empty;
        radiusMeters = Mathf.Max(0f, record.radiusMeters);
        riskLevel = Mathf.Max(0, record.riskLevel);
        exposureProbability = Mathf.Clamp01(record.exposureProbability);
        deterministicSeed = record.deterministicSeed;
        hazardSourceMode = record.hazardSourceMode ?? string.Empty;
        warningOnly = true;
        canKillPlayer = false;
        noPhysicsCollapse = true;
        notes = record.notes ?? string.Empty;
        transform.position = record.position == null ? transform.position : record.position.ToVector3();
    }

    public P9CollapseDebrisExposureResult CalculateExposure(Vector3 samplePosition, int scenarioSeed)
    {
        bool inZone = Vector3.Distance(transform.position, samplePosition) <= RadiusMeters;
        float roll = StableUnitRandom(scenarioSeed, deterministicSeed, zoneId);
        bool exposureEvent = inZone && roll < ExposureProbability;
        return new P9CollapseDebrisExposureResult
        {
            zoneId = zoneId ?? string.Empty,
            samplePosition = samplePosition,
            inZone = inZone,
            exposureEvent = exposureEvent,
            deterministicRoll = roll,
            canKillPlayer = false,
            playerOutcomeMutationApplied = false,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            reasonCode = exposureEvent ? "collapse_debris_exposure_marker_only_p9b" : "no_exposure_marker_only_p9b"
        };
    }

    public static P9BCollapseDebrisRiskZoneGenerationResult GenerateMarkers(
        P9BCollapseDebrisRiskZoneCollection collection,
        Transform parent)
    {
        var result = new P9BCollapseDebrisRiskZoneGenerationResult
        {
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            canKillPlayer = false
        };

        if (collection == null || collection.zones == null)
        {
            result.summary = "P9-B collapse/debris risk zone data missing. No markers generated.";
            return result;
        }

        var snapshots = new List<P9CollapseDebrisRiskZoneSnapshot>();
        for (int i = 0; i < collection.zones.Length; i++)
        {
            P9BCollapseDebrisRiskZoneRecord record = collection.zones[i];
            if (record == null)
            {
                continue;
            }

            var markerObject = new GameObject("P9B_CollapseDebris_" + SafeObjectName(record.zoneId));
            if (parent != null)
            {
                markerObject.transform.SetParent(parent, false);
            }

            var zone = markerObject.AddComponent<P9CollapseDebrisRiskZone>();
            zone.ApplyRecord(record);

            var marker = markerObject.AddComponent<P9RuntimeMarker>();
            marker.Configure(
                record.zoneId,
                "collapse_debris_risk_zone",
                record.hazardSourceMode,
                record.displayName,
                false,
                false,
                false,
                string.Empty,
                "warning_only_marker",
                "P9-B collapse/debris marker only. No player outcome mutation.");

            snapshots.Add(zone.CreateSnapshot());
        }

        result.success = true;
        result.generatedZoneCount = snapshots.Count;
        result.snapshots = snapshots.ToArray();
        result.summary = "P9-B generated " + snapshots.Count + " collapse/debris risk zone markers.";
        return result;
    }

    public P9CollapseDebrisRiskZoneSnapshot CreateSnapshot()
    {
        return new P9CollapseDebrisRiskZoneSnapshot
        {
            zoneId = zoneId ?? string.Empty,
            displayName = displayName ?? string.Empty,
            position = transform.position,
            radiusMeters = RadiusMeters,
            riskLevel = RiskLevel,
            exposureProbability = ExposureProbability,
            canKillPlayer = false,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            warningOnly = true
        };
    }

    private static float StableUnitRandom(int scenarioSeed, int zoneSeed, string id)
    {
        int hash = 17;
        hash = hash * 31 + scenarioSeed;
        hash = hash * 31 + zoneSeed;
        string safeId = id ?? string.Empty;
        for (int i = 0; i < safeId.Length; i++)
        {
            hash = hash * 31 + safeId[i];
        }

        var random = new System.Random(hash & int.MaxValue);
        return (float)random.NextDouble();
    }

    private static string SafeObjectName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace("/", "_").Replace("\\", "_");
    }
}

[Serializable]
public class P9BCollapseDebrisRiskZoneCollection
{
    public string schemaVersion = string.Empty;
    public string sourceMode = string.Empty;
    public string notes = string.Empty;
    public P9BCollapseDebrisRiskZoneRecord[] zones = Array.Empty<P9BCollapseDebrisRiskZoneRecord>();
}

[Serializable]
public class P9BCollapseDebrisRiskZoneRecord
{
    public string zoneId = string.Empty;
    public string displayName = string.Empty;
    public P9Vector3Data position = new P9Vector3Data();
    public float radiusMeters;
    public int riskLevel;
    public float exposureProbability;
    public int deterministicSeed;
    public string hazardSourceMode = string.Empty;
    public bool warningOnly;
    public bool affectsGameplaySuccessFailure;
    public bool canKillPlayer;
    public bool noPhysicsCollapse = true;
    public string notes = string.Empty;
}

[Serializable]
public class P9CollapseDebrisExposureResult
{
    public string zoneId = string.Empty;
    public Vector3 samplePosition;
    public bool inZone;
    public bool exposureEvent;
    public float deterministicRoll;
    public bool canKillPlayer;
    public bool playerOutcomeMutationApplied;
    public bool affectsGameplaySuccessFailure;
    public string reasonCode = string.Empty;
}

[Serializable]
public class P9BCollapseDebrisRiskZoneGenerationResult
{
    public bool success;
    public int generatedZoneCount;
    public bool affectsGameplaySuccessFailure;
    public bool canKillPlayer;
    public P9CollapseDebrisRiskZoneSnapshot[] snapshots = Array.Empty<P9CollapseDebrisRiskZoneSnapshot>();
    public string summary = string.Empty;
}

[Serializable]
public class P9CollapseDebrisRiskZoneSnapshot
{
    public string zoneId = string.Empty;
    public string displayName = string.Empty;
    public Vector3 position;
    public float radiusMeters;
    public int riskLevel;
    public float exposureProbability;
    public bool canKillPlayer;
    public bool affectsGameplaySuccessFailure;
    public bool warningOnly;
}
