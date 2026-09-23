using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class P9SpawnPointLoader
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;
    public const bool RequiresP8DEFinalHandoff = P9RuntimePolicy.RequiresP8DEFinalHandoff;

    public const string SpawnPointSampleFileName = "p9_spawn_points_sample.json";
    public const string CrowdAgentSampleFileName = "p9_crowd_agents_sample.json";
    public const string EntranceSafeFloorProxySampleFileName = "p9_entrance_safe_floor_proxy_sample.json";

    private static readonly string[] AllowedSourceModes =
    {
        "test",
        "rule_based",
        "manual_sample",
        "p8_handoff"
    };

    private static readonly string[] AllowedSpawnTypes =
    {
        "player_start",
        "resident_origin",
        "office_worker_origin",
        "station_exit",
        "underground_exit",
        "waterfront_origin",
        "test_origin"
    };

    private static readonly string[] AllowedAgentTypes =
    {
        "resident",
        "office_worker",
        "visitor",
        "elderly_proxy",
        "test_agent"
    };

    private static readonly string[] AllowedVerticalStatuses =
    {
        "available",
        "warning",
        "restricted",
        "blocked_proxy",
        "unknown"
    };

    private static readonly string[] AllowedInteractionModes =
    {
        "external_proxy",
        "entrance_marker",
        "safe_floor_status"
    };

    public static P9SpawnPointLoadResult LoadSampleSpawnPoints()
    {
        return LoadSpawnPointsFromPath(GetAssetsP9DataPath(SpawnPointSampleFileName));
    }

    public static P9CrowdAgentProfileLoadResult LoadSampleCrowdAgentProfiles()
    {
        return LoadCrowdAgentProfilesFromPath(GetAssetsP9DataPath(CrowdAgentSampleFileName));
    }

    public static P9EntranceSafeFloorProxyLoadResult LoadSampleEntranceSafeFloorProxies()
    {
        return LoadEntranceSafeFloorProxiesFromPath(GetAssetsP9DataPath(EntranceSafeFloorProxySampleFileName));
    }

    public static P9SpawnPointLoadResult LoadSpawnPointsFromPath(string path)
    {
        var result = new P9SpawnPointLoadResult
        {
            sourcePath = path ?? string.Empty,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            result.summary = "Missing P9 spawn point data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            return LoadSpawnPointsFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            result.summary = "Unreadable P9 spawn point data. " + exception.Message;
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9SpawnPointLoadResult LoadSpawnPointsFromJson(string json, string sourceLabel)
    {
        var result = new P9SpawnPointLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "P9 spawn point JSON" : sourceLabel,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            result.summary = "Empty P9 spawn point data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            result.data = JsonUtility.FromJson<P9SpawnPointCollection>(json);
            result.errors = ValidateSpawnPoints(result.data);
            result.success = result.errors.Length == 0;
            result.failSafe = !result.success;
            result.summary = result.success
                ? "P9 spawn point sample loaded: " + result.data.spawnPoints.Length + " records. No gameplay failure effect."
                : "Invalid P9 spawn point sample. P9-A remains no-effect.";
            return result;
        }
        catch (Exception exception)
        {
            result.errors = new[] { "Parse failed: " + exception.Message };
            result.summary = "Could not parse P9 spawn point data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9CrowdAgentProfileLoadResult LoadCrowdAgentProfilesFromPath(string path)
    {
        var result = new P9CrowdAgentProfileLoadResult
        {
            sourcePath = path ?? string.Empty,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            result.summary = "Missing P9 crowd agent profile data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            return LoadCrowdAgentProfilesFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            result.summary = "Unreadable P9 crowd agent profile data. " + exception.Message;
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9CrowdAgentProfileLoadResult LoadCrowdAgentProfilesFromJson(string json, string sourceLabel)
    {
        var result = new P9CrowdAgentProfileLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "P9 crowd agent JSON" : sourceLabel,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            result.summary = "Empty P9 crowd agent profile data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            result.data = JsonUtility.FromJson<P9CrowdAgentProfileCollection>(json);
            result.errors = ValidateCrowdAgents(result.data);
            result.success = result.errors.Length == 0;
            result.failSafe = !result.success;
            result.summary = result.success
                ? "P9 crowd agent sample loaded: " + result.data.agentProfiles.Length + " profiles. No real crowd model claim."
                : "Invalid P9 crowd agent sample. P9-A remains no-effect.";
            return result;
        }
        catch (Exception exception)
        {
            result.errors = new[] { "Parse failed: " + exception.Message };
            result.summary = "Could not parse P9 crowd agent profile data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9EntranceSafeFloorProxyLoadResult LoadEntranceSafeFloorProxiesFromPath(string path)
    {
        var result = new P9EntranceSafeFloorProxyLoadResult
        {
            sourcePath = path ?? string.Empty,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            result.summary = "Missing P9 entrance/safe-floor proxy data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            return LoadEntranceSafeFloorProxiesFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            result.summary = "Unreadable P9 entrance/safe-floor proxy data. " + exception.Message;
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9EntranceSafeFloorProxyLoadResult LoadEntranceSafeFloorProxiesFromJson(string json, string sourceLabel)
    {
        var result = new P9EntranceSafeFloorProxyLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "P9 entrance proxy JSON" : sourceLabel,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            result.summary = "Empty P9 entrance/safe-floor proxy data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            result.data = JsonUtility.FromJson<P9EntranceSafeFloorProxyCollection>(json);
            result.errors = ValidateEntranceProxies(result.data);
            result.success = result.errors.Length == 0;
            result.failSafe = !result.success;
            result.summary = result.success
                ? "P9 entrance/safe-floor proxy sample loaded: " + result.data.proxies.Length + " proxies. No indoor scene gameplay."
                : "Invalid P9 entrance/safe-floor proxy sample. P9-A remains no-effect.";
            return result;
        }
        catch (Exception exception)
        {
            result.errors = new[] { "Parse failed: " + exception.Message };
            result.summary = "Could not parse P9 entrance/safe-floor proxy data. P9-A remains no-effect.";
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    private static string[] ValidateSpawnPoints(P9SpawnPointCollection collection)
    {
        var errors = new List<string>();
        if (collection == null)
        {
            errors.Add("Spawn point collection is null.");
            return errors.ToArray();
        }

        if (!IsAllowed(collection.sourceMode, AllowedSourceModes))
        {
            errors.Add("Collection sourceMode is not allowed: " + collection.sourceMode);
        }

        if (collection.spawnPoints == null || collection.spawnPoints.Length == 0)
        {
            errors.Add("At least one spawn point is required.");
            return errors.ToArray();
        }

        var ids = new HashSet<string>();
        for (int i = 0; i < collection.spawnPoints.Length; i++)
        {
            P9SpawnPointRecord record = collection.spawnPoints[i];
            if (record == null)
            {
                errors.Add("Spawn point record " + i + " is null.");
                continue;
            }

            AddRequiredIdErrors(errors, ids, record.spawnPointId, "spawnPointId");
            if (!IsAllowed(record.spawnType, AllowedSpawnTypes))
            {
                errors.Add("spawnType is not allowed for " + record.spawnPointId + ": " + record.spawnType);
            }

            if (!IsAllowed(record.sourceMode, AllowedSourceModes))
            {
                errors.Add("sourceMode is not allowed for " + record.spawnPointId + ": " + record.sourceMode);
            }

            if (record.capacity < 0)
            {
                errors.Add("capacity must be non-negative for " + record.spawnPointId);
            }

            if (record.weight < 0f)
            {
                errors.Add("weight must be non-negative for " + record.spawnPointId);
            }

            if (record.coordinates == null || string.IsNullOrWhiteSpace(record.coordinates.coordinateSystem))
            {
                errors.Add("coordinates.coordinateSystem is required for " + record.spawnPointId);
            }
        }

        return errors.ToArray();
    }

    private static string[] ValidateCrowdAgents(P9CrowdAgentProfileCollection collection)
    {
        var errors = new List<string>();
        if (collection == null)
        {
            errors.Add("Crowd agent collection is null.");
            return errors.ToArray();
        }

        if (!IsAllowed(collection.sourceMode, AllowedSourceModes))
        {
            errors.Add("Collection sourceMode is not allowed: " + collection.sourceMode);
        }

        if (collection.agentProfiles == null || collection.agentProfiles.Length == 0)
        {
            errors.Add("At least one crowd agent profile is required.");
            return errors.ToArray();
        }

        var ids = new HashSet<string>();
        for (int i = 0; i < collection.agentProfiles.Length; i++)
        {
            P9CrowdAgentProfileRecord record = collection.agentProfiles[i];
            if (record == null)
            {
                errors.Add("Crowd agent profile record " + i + " is null.");
                continue;
            }

            AddRequiredIdErrors(errors, ids, record.agentProfileId, "agentProfileId");
            if (!IsAllowed(record.agentType, AllowedAgentTypes))
            {
                errors.Add("agentType is not allowed for " + record.agentProfileId + ": " + record.agentType);
            }

            if (!IsAllowed(record.sourceMode, AllowedSourceModes))
            {
                errors.Add("sourceMode is not allowed for " + record.agentProfileId + ": " + record.sourceMode);
            }

            if (record.speedMetersPerSecond < 0f)
            {
                errors.Add("speedMetersPerSecond must be non-negative for " + record.agentProfileId);
            }

            if (record.crowdRadius < 0f)
            {
                errors.Add("crowdRadius must be non-negative for " + record.agentProfileId);
            }

            if (record.panicLevelProxy < 0f || record.panicLevelProxy > 1f)
            {
                errors.Add("panicLevelProxy must be 0..1 for " + record.agentProfileId);
            }
        }

        return errors.ToArray();
    }

    private static string[] ValidateEntranceProxies(P9EntranceSafeFloorProxyCollection collection)
    {
        var errors = new List<string>();
        if (collection == null)
        {
            errors.Add("Entrance proxy collection is null.");
            return errors.ToArray();
        }

        if (!IsAllowed(collection.sourceMode, AllowedSourceModes))
        {
            errors.Add("Collection sourceMode is not allowed: " + collection.sourceMode);
        }

        if (collection.proxies == null || collection.proxies.Length == 0)
        {
            errors.Add("At least one entrance/safe-floor proxy is required.");
            return errors.ToArray();
        }

        var ids = new HashSet<string>();
        for (int i = 0; i < collection.proxies.Length; i++)
        {
            P9EntranceSafeFloorProxyRecord record = collection.proxies[i];
            if (record == null)
            {
                errors.Add("Entrance proxy record " + i + " is null.");
                continue;
            }

            AddRequiredIdErrors(errors, ids, record.proxyId, "proxyId");
            if (!IsAllowed(record.verticalEvacuationStatus, AllowedVerticalStatuses))
            {
                errors.Add("verticalEvacuationStatus is not allowed for " + record.proxyId + ": " + record.verticalEvacuationStatus);
            }

            if (!IsAllowed(record.interactionMode, AllowedInteractionModes))
            {
                errors.Add("interactionMode is not allowed for " + record.proxyId + ": " + record.interactionMode);
            }

            if (record.humanitarianCandidateFlag && record.isOfficialShelter)
            {
                errors.Add("Humanitarian candidate must not be official: " + record.proxyId);
            }

            if (record.humanitarianCandidateFlag && !record.nonOfficialWarningRequired)
            {
                errors.Add("Humanitarian candidate requires non-official warning: " + record.proxyId);
            }
        }

        return errors.ToArray();
    }

    private static void AddRequiredIdErrors(List<string> errors, HashSet<string> ids, string id, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            errors.Add(fieldName + " is required.");
            return;
        }

        if (!ids.Add(id))
        {
            errors.Add("Duplicate " + fieldName + ": " + id);
        }
    }

    private static bool IsAllowed(string value, string[] allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value) || allowedValues == null)
        {
            return false;
        }

        for (int i = 0; i < allowedValues.Length; i++)
        {
            if (string.Equals(value, allowedValues[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetAssetsP9DataPath(string fileName)
    {
        return Application.dataPath + "/Data/P9/" + fileName;
    }
}

public class P9SpawnPointLoadResult
{
    public bool success;
    public bool failSafe = true;
    public string sourcePath = string.Empty;
    public P9SpawnPointCollection data;
    public string[] errors = Array.Empty<string>();
    public string summary = string.Empty;
}

public class P9CrowdAgentProfileLoadResult
{
    public bool success;
    public bool failSafe = true;
    public string sourcePath = string.Empty;
    public P9CrowdAgentProfileCollection data;
    public string[] errors = Array.Empty<string>();
    public string summary = string.Empty;
}

public class P9EntranceSafeFloorProxyLoadResult
{
    public bool success;
    public bool failSafe = true;
    public string sourcePath = string.Empty;
    public P9EntranceSafeFloorProxyCollection data;
    public string[] errors = Array.Empty<string>();
    public string summary = string.Empty;
}
