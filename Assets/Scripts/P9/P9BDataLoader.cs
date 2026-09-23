using System;
using System.IO;
using UnityEngine;

public static class P9BDataLoader
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ImplementsFinalFailureGameplay = P9RuntimePolicy.ImplementsFinalFailureGameplay;

    public const string WeightedSpawnConfigFileName = "p9b_weighted_spawn_config.json";
    public const string SpawnZonesSampleFileName = "p9b_spawn_zones_sample.json";
    public const string CrowdScenarioSampleFileName = "p9b_runtime_crowd_scenario_sample.json";
    public const string EntranceMarkerAssignmentsSampleFileName = "p9b_entrance_marker_assignments_sample.json";
    public const string CollapseDebrisRiskZonesSampleFileName = "p9b_collapse_debris_risk_zones_sample.json";
    public const string HumanitarianMarkerRuntimeConfigFileName = "p9b_humanitarian_marker_runtime_config.json";

    public const string P8PersistentMarkerPath = "Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json";
    public const string P8PersistentVisibilityPath = "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json";
    public const string P8SemanticBindingPath = "Assets/Data/P8/p8e_semantic_binding_v1.json";
    public const string P8P2P6MatrixPath = "Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json";
    public const string P8RouteCandidateGeometryPath = "Assets/Data/P8/p8e_route_candidate_geometry_handoff.json";
    public const string P8RiskFrontProgressionPath = "Assets/Data/P8/risk_front_progression_model_config.json";

    public static P9BLoadResult<P9BWeightedSpawnConfig> LoadWeightedSpawnConfig()
    {
        return LoadFromPath<P9BWeightedSpawnConfig>(GetP9DataPath(WeightedSpawnConfigFileName), "P9-B weighted spawn config");
    }

    public static P9BLoadResult<P9BWeightedSpawnZoneCollection> LoadSpawnZones()
    {
        return LoadFromPath<P9BWeightedSpawnZoneCollection>(GetP9DataPath(SpawnZonesSampleFileName), "P9-B spawn zones");
    }

    public static P9BLoadResult<P9BCrowdRuntimeScenario> LoadCrowdScenario()
    {
        return LoadFromPath<P9BCrowdRuntimeScenario>(GetP9DataPath(CrowdScenarioSampleFileName), "P9-B crowd scenario");
    }

    public static P9BLoadResult<P9EntranceSafeFloorProxyCollection> LoadEntranceMarkerAssignments()
    {
        return LoadFromPath<P9EntranceSafeFloorProxyCollection>(GetP9DataPath(EntranceMarkerAssignmentsSampleFileName), "P9-B entrance marker assignments");
    }

    public static P9BLoadResult<P9BCollapseDebrisRiskZoneCollection> LoadCollapseDebrisRiskZones()
    {
        return LoadFromPath<P9BCollapseDebrisRiskZoneCollection>(GetP9DataPath(CollapseDebrisRiskZonesSampleFileName), "P9-B collapse/debris risk zones");
    }

    public static P9BLoadResult<P9BHumanitarianMarkerRuntimeConfig> LoadHumanitarianMarkerRuntimeConfig()
    {
        return LoadFromPath<P9BHumanitarianMarkerRuntimeConfig>(GetP9DataPath(HumanitarianMarkerRuntimeConfigFileName), "P9-B humanitarian marker runtime config");
    }

    public static P9BLoadResult<P9BHumanitarianCandidateMarkerCollection> LoadP8HumanitarianCandidateMarkers()
    {
        return LoadFromPath<P9BHumanitarianCandidateMarkerCollection>(GetAssetRelativePath(P8PersistentMarkerPath), "P8-E humanitarian candidate markers");
    }

    public static P9BLoadResult<P9BSemanticBindingCollection> LoadP8SemanticBindings()
    {
        return LoadFromPath<P9BSemanticBindingCollection>(GetAssetRelativePath(P8SemanticBindingPath), "P8-E semantic binding v1");
    }

    public static P9BLoadResult<P9BRouteCandidateGeometryHandoff> LoadP8RouteCandidateGeometryHandoff()
    {
        return LoadFromPath<P9BRouteCandidateGeometryHandoff>(GetAssetRelativePath(P8RouteCandidateGeometryPath), "P8-E route/candidate geometry handoff");
    }

    public static P9BLoadResult<T> LoadFromPath<T>(string path, string label)
    {
        var result = new P9BLoadResult<T>
        {
            sourcePath = path ?? string.Empty,
            label = label ?? typeof(T).Name,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            result.summary = result.label + " missing. P9-B runtime keeps fallback/no-effect behavior.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            return LoadFromJson<T>(File.ReadAllText(path), path, label);
        }
        catch (Exception exception)
        {
            result.summary = result.label + " unreadable. " + exception.Message;
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9BLoadResult<T> LoadFromJson<T>(string json, string sourcePath, string label)
    {
        var result = new P9BLoadResult<T>
        {
            sourcePath = sourcePath ?? string.Empty,
            label = label ?? typeof(T).Name,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            result.summary = result.label + " empty. P9-B runtime keeps fallback/no-effect behavior.";
            Debug.LogWarning(result.summary);
            return result;
        }

        try
        {
            result.data = JsonUtility.FromJson<T>(json);
            result.success = result.data != null;
            result.failSafe = !result.success;
            result.summary = result.success
                ? result.label + " loaded for P9-B runtime prototype."
                : result.label + " parsed to null. P9-B runtime keeps fallback/no-effect behavior.";
            return result;
        }
        catch (Exception exception)
        {
            result.summary = result.label + " parse failed. " + exception.Message;
            Debug.LogWarning(result.summary);
            return result;
        }
    }

    public static P9BP8HandoffAvailability VerifyP8HandoffAvailability()
    {
        string[] paths =
        {
            P8PersistentVisibilityPath,
            P8PersistentMarkerPath,
            P8SemanticBindingPath,
            P8P2P6MatrixPath,
            P8RouteCandidateGeometryPath,
            P8RiskFrontProgressionPath
        };

        int present = 0;
        for (int i = 0; i < paths.Length; i++)
        {
            if (File.Exists(GetAssetRelativePath(paths[i])))
            {
                present++;
            }
        }

        return new P9BP8HandoffAvailability
        {
            expectedFileCount = paths.Length,
            presentFileCount = present,
            allExpectedFilesPresent = present == paths.Length,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            summary = "P8-E handoff availability for P9-B: " + present + "/" + paths.Length + " expected files present."
        };
    }

    public static string GetP9DataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", "P9", fileName);
    }

    public static string GetAssetRelativePath(string assetRelativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, assetRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }
}

[Serializable]
public class P9BLoadResult<T>
{
    public bool success;
    public bool failSafe = true;
    public string label = string.Empty;
    public string sourcePath = string.Empty;
    public T data;
    public string summary = string.Empty;
}

[Serializable]
public class P9BP8HandoffAvailability
{
    public int expectedFileCount;
    public int presentFileCount;
    public bool allExpectedFilesPresent;
    public bool affectsGameplaySuccessFailure;
    public string summary = string.Empty;
}

[Serializable]
public class P9BSemanticBindingCollection
{
    public string datasetId = string.Empty;
    public bool completeSceneSemanticBinding;
    public bool sceneMutationPerformed;
    public string baselineScene = string.Empty;
    public string p9AssumptionBoundary = string.Empty;
    public P9BSemanticBindingRecord[] bindings = Array.Empty<P9BSemanticBindingRecord>();
}

[Serializable]
public class P9BSemanticBindingRecord
{
    public string category = string.Empty;
    public string bindingMode = string.Empty;
    public string confidence = string.Empty;
    public bool isProxy;
    public bool p9Usable;
    public string p9Limitations = string.Empty;
    public bool sceneObjectEvidenceFound;
    public string objectId = string.Empty;
    public string blocker = string.Empty;
}

[Serializable]
public class P9BRouteCandidateGeometryHandoff
{
    public string datasetId = string.Empty;
    public bool routeCoordinateTransformGeometryValidated;
    public bool wgs84PlateauTransformLimitationRemains;
    public bool routesAreOfficial;
    public bool routesAreEstimatedPrototypeGuidance;
    public bool routesAlignedToNewMapRoads;
    public bool routeRoadGeometryValidated;
    public bool candidatesGeometryBoundToSceneObjects;
    public bool candidatesProxyOrDataBound;
    public bool candidateCoordinateEvidenceAvailable;
    public string p9MayUseRoutesFor = string.Empty;
    public string p9MustNotClaim = string.Empty;
    public string notes = string.Empty;
}
