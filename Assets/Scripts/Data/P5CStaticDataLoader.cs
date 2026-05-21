using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class P5CStaticDataLoader
{
    public const string IntegratedRouteQualificationFileName = "real_chuo_integrated_route_qualification.json";
    public const string RouteSampleFileName = "real_chuo_osm_routes_sample.json";
    public const string BuildingQualificationFileName = "real_chuo_building_qualification.json";
    public const string ShelterBuildingMatchesFileName = "real_chuo_shelter_building_matches.json";
    public const string P5CSourceType = "p5c_static";
    public const string EstimatedPrototypeRouteLabel = "estimated prototype route, not an official evacuation route";
    public const string UnavailableMessage = "P5-C qualification/route data unavailable for this shelter";
    public const string Wgs84CoordinateSystem = "EPSG:4326";
    public const string GeoJsonLineStringType = "LineString";
    public const string RouteGeometryCoordinateOrder = "longitude, latitude";

    private const string DefaultCoordinateSystem = Wgs84CoordinateSystem;
    private const string DefaultMetricCoordinateSystem = "EPSG:6677";
    private const string DefaultOsmAttribution =
        "Route network data from OpenStreetMap contributors; use under the Open Database License.";
    private const float UnknownFloat = -1f;
    private const int UnknownInt = -1;

    public class LoadResult<TRecord>
    {
        public bool success;
        public string sourcePath = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = DefaultCoordinateSystem;
        public string metricCoordinateReferenceSystem = DefaultMetricCoordinateSystem;
        public string osmAttribution = string.Empty;
        public string notes = string.Empty;
        public TRecord[] records = new TRecord[0];
    }

    public class P5CDataBundle
    {
        public LoadResult<IntegratedRouteQualificationRecord> integratedRouteQualifications =
            new LoadResult<IntegratedRouteQualificationRecord>();
        public LoadResult<RouteSampleRecord> routeSamples = new LoadResult<RouteSampleRecord>();
        public LoadResult<BuildingQualificationRecord> buildingQualifications =
            new LoadResult<BuildingQualificationRecord>();
        public LoadResult<ShelterBuildingMatchRecord> shelterBuildingMatches =
            new LoadResult<ShelterBuildingMatchRecord>();

        public bool HasCoreQualificationData =>
            integratedRouteQualifications != null &&
            integratedRouteQualifications.success &&
            integratedRouteQualifications.records != null &&
            integratedRouteQualifications.records.Length > 0;

        public bool HasAllStaticData =>
            HasCoreQualificationData &&
            routeSamples != null &&
            routeSamples.success &&
            buildingQualifications != null &&
            buildingQualifications.success &&
            shelterBuildingMatches != null &&
            shelterBuildingMatches.success;

        public string OsmAttribution
        {
            get
            {
                if (routeSamples != null && !string.IsNullOrWhiteSpace(routeSamples.osmAttribution))
                {
                    return routeSamples.osmAttribution;
                }

                if (integratedRouteQualifications != null &&
                    !string.IsNullOrWhiteSpace(integratedRouteQualifications.osmAttribution))
                {
                    return integratedRouteQualifications.osmAttribution;
                }

                return DefaultOsmAttribution;
            }
        }

        public bool TryGetEvidenceForShelter(string shelterId, out ShelterEvidence evidence)
        {
            evidence = null;

            if (string.IsNullOrWhiteSpace(shelterId) || !HasCoreQualificationData)
            {
                return false;
            }

            IntegratedRouteQualificationRecord integratedRecord =
                FindByShelterId(integratedRouteQualifications.records, shelterId);

            if (integratedRecord == null)
            {
                return false;
            }

            RouteSampleRecord routeRecord = null;
            if (!string.IsNullOrWhiteSpace(integratedRecord.nearestRouteId) &&
                routeSamples != null &&
                routeSamples.records != null)
            {
                routeRecord = FindRouteById(routeSamples.records, integratedRecord.nearestRouteId);
                if (routeRecord == null)
                {
                    Debug.LogWarning(
                        $"P5-C integrated record for shelter '{shelterId}' referenced route " +
                        $"'{integratedRecord.nearestRouteId}', but that route was not found in {RouteSampleFileName}.");
                }
            }

            BuildingQualificationRecord buildingRecord = null;
            if (buildingQualifications != null && buildingQualifications.records != null)
            {
                buildingRecord = FindByShelterId(buildingQualifications.records, shelterId);
            }

            ShelterBuildingMatchRecord matchRecord = null;
            if (shelterBuildingMatches != null && shelterBuildingMatches.records != null)
            {
                matchRecord = FindByShelterId(shelterBuildingMatches.records, shelterId);
            }

            evidence = new ShelterEvidence
            {
                shelterId = shelterId.Trim(),
                integratedQualification = integratedRecord,
                routeSample = routeRecord,
                buildingQualification = buildingRecord,
                shelterBuildingMatch = matchRecord,
                osmAttribution = OsmAttribution
            };

            return true;
        }
    }

    public class ShelterEvidence
    {
        public string shelterId = string.Empty;
        public IntegratedRouteQualificationRecord integratedQualification;
        public RouteSampleRecord routeSample;
        public BuildingQualificationRecord buildingQualification;
        public ShelterBuildingMatchRecord shelterBuildingMatch;
        public string osmAttribution = DefaultOsmAttribution;

        public bool HasRouteRecord => routeSample != null;
    }

    public class EvidenceSourceRecord
    {
        public string sourceId = string.Empty;
        public string sourceFamily = string.Empty;
        public string sourceName = string.Empty;
        public string officialStatus = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string evidenceNote = string.Empty;
    }

    public class IntegratedRouteQualificationRecord : IHasShelterId
    {
        public string qualificationId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string qualificationStatus = string.Empty;
        public string qualificationReason = string.Empty;
        public EvidenceSourceRecord[] evidenceSources = new EvidenceSourceRecord[0];
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] disasterTypes = new string[0];
        public int safeFloor = UnknownInt;
        public int capacity = UnknownInt;
        public string sourceUpdatedAt = string.Empty;
        public string routeAvailability = string.Empty;
        public float nearestRouteDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public string notes = string.Empty;
        public string routeSource = string.Empty;
        public string routeType = string.Empty;
        public bool isOfficialEvacuationRoute;
        public string nearestRouteId = string.Empty;
        public string nearestRouteOriginId = string.Empty;
        public string nearestRouteOriginName = string.Empty;
        public int availableRouteCount;
        public int failedRouteCount;

        public bool HasNearestRouteDistanceMeters => nearestRouteDistanceMeters >= 0f;
        public bool HasEstimatedTravelTimeSeconds => estimatedTravelTimeSeconds >= 0f;
        public bool IsEstimatedPrototypeRoute => IsPrototypeRoute(routeType, isOfficialEvacuationRoute);
        public string ShelterId => shelterId;
    }

    public class RouteGeometryRecord
    {
        public string type = string.Empty;
        public Vector2[] coordinates = new Vector2[0];
        public string coordinateReferenceSystem = DefaultCoordinateSystem;

        public bool HasCoordinates => coordinates != null && coordinates.Length > 1;
    }

    public class RouteSampleRecord : IHasShelterId
    {
        public string routeId = string.Empty;
        public string originId = string.Empty;
        public string originName = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string qualificationStatus = string.Empty;
        public string routeAvailability = string.Empty;
        public float routeDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public float walkingSpeedMetersPerSecond = UnknownFloat;
        public string routeSource = string.Empty;
        public string routeType = string.Empty;
        public bool isOfficialEvacuationRoute;
        public RouteGeometryRecord geometry = new RouteGeometryRecord();
        public float snapDistanceOriginMeters = UnknownFloat;
        public float snapDistanceTargetMeters = UnknownFloat;
        public string routeFailureReason = string.Empty;
        public string[] warnings = new string[0];
        public string notes = string.Empty;

        public bool HasRouteDistanceMeters => routeDistanceMeters >= 0f;
        public bool HasEstimatedTravelTimeSeconds => estimatedTravelTimeSeconds >= 0f;
        public bool HasGeometry => geometry != null && geometry.HasCoordinates;
        public bool IsEstimatedPrototypeRoute => IsPrototypeRoute(routeType, isOfficialEvacuationRoute);
        public string ShelterId => shelterId;
    }

    public class BuildingQualificationRecord : IHasShelterId
    {
        public string qualificationId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string qualificationStatus = string.Empty;
        public string qualificationReason = string.Empty;
        public EvidenceSourceRecord[] evidenceSources = new EvidenceSourceRecord[0];
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] disasterTypes = new string[0];
        public int safeFloor = UnknownInt;
        public int capacity = UnknownInt;
        public string sourceUpdatedAt = string.Empty;
        public string routeAvailability = string.Empty;
        public float nearestRouteDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public string notes = string.Empty;
        public string ShelterId => shelterId;
    }

    public class ShelterBuildingMatchRecord : IHasShelterId
    {
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string plateauGmlId = string.Empty;
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string sourceMeshCode = string.Empty;
        public string ShelterId => shelterId;
    }

    public interface IHasShelterId
    {
        string ShelterId { get; }
    }

    [Serializable]
    private class IntegratedDataset
    {
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = DefaultCoordinateSystem;
        public string rulebookVersion = string.Empty;
        public string qualificationInputPath = string.Empty;
        public string routeInputPath = string.Empty;
        public string osmAttribution = string.Empty;
        public string notes = string.Empty;
        public IntegratedRecordRaw[] records = new IntegratedRecordRaw[0];
    }

    [Serializable]
    private class RouteDataset
    {
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = DefaultCoordinateSystem;
        public string metricCoordinateReferenceSystem = DefaultMetricCoordinateSystem;
        public string osmAttribution = string.Empty;
        public float walkingSpeedMetersPerSecond = UnknownFloat;
        public int recordCount;
        public string notes = string.Empty;
        public RouteRecordRaw[] records = new RouteRecordRaw[0];
    }

    [Serializable]
    private class BuildingQualificationDataset
    {
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = DefaultCoordinateSystem;
        public string rulebookVersion = string.Empty;
        public string notes = string.Empty;
        public BuildingQualificationRecordRaw[] records = new BuildingQualificationRecordRaw[0];
    }

    [Serializable]
    private class ShelterBuildingMatchDataset
    {
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = DefaultCoordinateSystem;
        public string metricCoordinateReferenceSystem = DefaultMetricCoordinateSystem;
        public ShelterBuildingMatchRecordRaw[] records = new ShelterBuildingMatchRecordRaw[0];
    }

    [Serializable]
    private class EvidenceSourceRaw
    {
        public string sourceId = string.Empty;
        public string sourceFamily = string.Empty;
        public string sourceName = string.Empty;
        public string officialStatus = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string evidenceNote = string.Empty;
    }

    [Serializable]
    private class IntegratedRecordRaw
    {
        public string qualificationId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string qualificationStatus = string.Empty;
        public string qualificationReason = string.Empty;
        public EvidenceSourceRaw[] evidenceSources = new EvidenceSourceRaw[0];
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] disasterTypes = new string[0];
        public int safeFloor = UnknownInt;
        public int capacity = UnknownInt;
        public string sourceUpdatedAt = string.Empty;
        public string routeAvailability = string.Empty;
        public float nearestRouteDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public string notes = string.Empty;
        public string routeSource = string.Empty;
        public string routeType = string.Empty;
        public bool isOfficialEvacuationRoute;
        public string nearestRouteId = string.Empty;
        public string nearestRouteOriginId = string.Empty;
        public string nearestRouteOriginName = string.Empty;
        public int availableRouteCount;
        public int failedRouteCount;
    }

    [Serializable]
    private class RouteGeometryRaw
    {
        public string type = string.Empty;
    }

    [Serializable]
    private class RouteRecordRaw
    {
        public string routeId = string.Empty;
        public string originId = string.Empty;
        public string originName = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string qualificationStatus = string.Empty;
        public string routeAvailability = string.Empty;
        public float routeDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public float walkingSpeedMetersPerSecond = UnknownFloat;
        public string routeSource = string.Empty;
        public string routeType = string.Empty;
        public bool isOfficialEvacuationRoute;
        public RouteGeometryRaw geometry;
        public float snapDistanceOriginMeters = UnknownFloat;
        public float snapDistanceTargetMeters = UnknownFloat;
        public string routeFailureReason = string.Empty;
        public string[] warnings = new string[0];
        public string notes = string.Empty;
    }

    [Serializable]
    private class BuildingQualificationRecordRaw
    {
        public string qualificationId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string qualificationStatus = string.Empty;
        public string qualificationReason = string.Empty;
        public EvidenceSourceRaw[] evidenceSources = new EvidenceSourceRaw[0];
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] disasterTypes = new string[0];
        public int safeFloor = UnknownInt;
        public int capacity = UnknownInt;
        public string sourceUpdatedAt = string.Empty;
        public string routeAvailability = string.Empty;
        public float nearestRouteDistanceMeters = UnknownFloat;
        public float estimatedTravelTimeSeconds = UnknownFloat;
        public string notes = string.Empty;
    }

    [Serializable]
    private class ShelterBuildingMatchRecordRaw
    {
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string plateauGmlId = string.Empty;
        public string matchMethod = string.Empty;
        public float matchDistanceMeters = UnknownFloat;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string sourceMeshCode = string.Empty;
    }

    public static P5CDataBundle LoadBundleFromAssetsData()
    {
        return new P5CDataBundle
        {
            integratedRouteQualifications = LoadIntegratedRouteQualificationsFromPath(
                GetAssetsDataPath(IntegratedRouteQualificationFileName)),
            routeSamples = LoadRouteSamplesFromPath(GetAssetsDataPath(RouteSampleFileName)),
            buildingQualifications = LoadBuildingQualificationsFromPath(GetAssetsDataPath(BuildingQualificationFileName)),
            shelterBuildingMatches = LoadShelterBuildingMatchesFromPath(GetAssetsDataPath(ShelterBuildingMatchesFileName))
        };
    }

    public static LoadResult<IntegratedRouteQualificationRecord> LoadIntegratedRouteQualificationsFromPath(string path)
    {
        return LoadJsonFromPath(path, IntegratedRouteQualificationFileName, LoadIntegratedRouteQualificationsFromJson);
    }

    public static LoadResult<RouteSampleRecord> LoadRouteSamplesFromPath(string path)
    {
        return LoadJsonFromPath(path, RouteSampleFileName, LoadRouteSamplesFromJson);
    }

    public static LoadResult<BuildingQualificationRecord> LoadBuildingQualificationsFromPath(string path)
    {
        return LoadJsonFromPath(path, BuildingQualificationFileName, LoadBuildingQualificationsFromJson);
    }

    public static LoadResult<ShelterBuildingMatchRecord> LoadShelterBuildingMatchesFromPath(string path)
    {
        return LoadJsonFromPath(path, ShelterBuildingMatchesFileName, LoadShelterBuildingMatchesFromJson);
    }

    public static LoadResult<IntegratedRouteQualificationRecord> LoadIntegratedRouteQualificationsFromJson(
        string json,
        string sourceLabel)
    {
        var result = CreateLoadResult<IntegratedRouteQualificationRecord>(sourceLabel);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty P5-C integrated route qualification data at {result.sourcePath}.");
            return result;
        }

        try
        {
            var dataset = new IntegratedDataset();
            JsonUtility.FromJsonOverwrite(NormalizeNullableNumbers(json), dataset);
            PopulateDatasetFields(result, dataset.datasetId, dataset.generatedAt, dataset.coordinateReferenceSystem, string.Empty, dataset.osmAttribution, dataset.notes);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain P5-C integrated qualification records.");
                return result;
            }

            var records = new List<IntegratedRouteQualificationRecord>();
            var seenIds = new HashSet<string>();
            foreach (IntegratedRecordRaw rawRecord in dataset.records)
            {
                IntegratedRouteQualificationRecord record = MapIntegratedRecord(rawRecord);
                if (string.IsNullOrWhiteSpace(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained an integrated qualification record with no shelterId. The record was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate shelterId '{record.shelterId}'. The first P5-C record was used.");
                    continue;
                }

                records.Add(record);
            }

            result.records = records.ToArray();
            result.success = result.records.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse P5-C integrated route qualification data from {result.sourcePath}. {exception.Message}");
            return CreateLoadResult<IntegratedRouteQualificationRecord>(sourceLabel);
        }
    }

    public static LoadResult<RouteSampleRecord> LoadRouteSamplesFromJson(string json, string sourceLabel)
    {
        var result = CreateLoadResult<RouteSampleRecord>(sourceLabel);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty P5-C route sample data at {result.sourcePath}.");
            return result;
        }

        try
        {
            var dataset = new RouteDataset();
            string normalizedJson = NormalizeNullableNumbers(json);
            JsonUtility.FromJsonOverwrite(normalizedJson, dataset);
            PopulateDatasetFields(
                result,
                dataset.datasetId,
                dataset.generatedAt,
                dataset.coordinateReferenceSystem,
                dataset.metricCoordinateReferenceSystem,
                dataset.osmAttribution,
                dataset.notes);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain P5-C route sample records.");
                return result;
            }

            Dictionary<string, RouteGeometryRecord> geometryByRouteId =
                ExtractRouteGeometriesByRouteId(normalizedJson, result.coordinateReferenceSystem);
            var records = new List<RouteSampleRecord>();
            var seenIds = new HashSet<string>();

            foreach (RouteRecordRaw rawRecord in dataset.records)
            {
                RouteSampleRecord record = MapRouteRecord(rawRecord, result.coordinateReferenceSystem);
                if (string.IsNullOrWhiteSpace(record.routeId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained a route sample record with no routeId. The record was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.routeId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate routeId '{record.routeId}'. The first route record was used.");
                    continue;
                }

                if (geometryByRouteId.TryGetValue(record.routeId, out RouteGeometryRecord geometry))
                {
                    record.geometry = geometry;
                }

                records.Add(record);
            }

            result.records = records.ToArray();
            result.success = result.records.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse P5-C route sample data from {result.sourcePath}. {exception.Message}");
            return CreateLoadResult<RouteSampleRecord>(sourceLabel);
        }
    }

    public static LoadResult<BuildingQualificationRecord> LoadBuildingQualificationsFromJson(
        string json,
        string sourceLabel)
    {
        var result = CreateLoadResult<BuildingQualificationRecord>(sourceLabel);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty P5-C building qualification data at {result.sourcePath}.");
            return result;
        }

        try
        {
            var dataset = new BuildingQualificationDataset();
            JsonUtility.FromJsonOverwrite(NormalizeNullableNumbers(json), dataset);
            PopulateDatasetFields(result, dataset.datasetId, dataset.generatedAt, dataset.coordinateReferenceSystem, string.Empty, string.Empty, dataset.notes);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain P5-C building qualification records.");
                return result;
            }

            var records = new List<BuildingQualificationRecord>();
            var seenIds = new HashSet<string>();
            foreach (BuildingQualificationRecordRaw rawRecord in dataset.records)
            {
                BuildingQualificationRecord record = MapBuildingQualificationRecord(rawRecord);
                if (string.IsNullOrWhiteSpace(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained a building qualification record with no shelterId. The record was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate shelterId '{record.shelterId}'. The first building qualification record was used.");
                    continue;
                }

                records.Add(record);
            }

            result.records = records.ToArray();
            result.success = result.records.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse P5-C building qualification data from {result.sourcePath}. {exception.Message}");
            return CreateLoadResult<BuildingQualificationRecord>(sourceLabel);
        }
    }

    public static LoadResult<ShelterBuildingMatchRecord> LoadShelterBuildingMatchesFromJson(
        string json,
        string sourceLabel)
    {
        var result = CreateLoadResult<ShelterBuildingMatchRecord>(sourceLabel);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty P5-C shelter-building match data at {result.sourcePath}.");
            return result;
        }

        try
        {
            var dataset = new ShelterBuildingMatchDataset();
            JsonUtility.FromJsonOverwrite(NormalizeNullableNumbers(json), dataset);
            PopulateDatasetFields(
                result,
                dataset.datasetId,
                dataset.generatedAt,
                dataset.coordinateReferenceSystem,
                dataset.metricCoordinateReferenceSystem,
                string.Empty,
                string.Empty);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain P5-C shelter-building match records.");
                return result;
            }

            var records = new List<ShelterBuildingMatchRecord>();
            var seenIds = new HashSet<string>();
            foreach (ShelterBuildingMatchRecordRaw rawRecord in dataset.records)
            {
                ShelterBuildingMatchRecord record = MapShelterBuildingMatchRecord(rawRecord);
                if (string.IsNullOrWhiteSpace(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained a shelter-building match record with no shelterId. The record was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate shelterId '{record.shelterId}'. The first match record was used.");
                    continue;
                }

                records.Add(record);
            }

            result.records = records.ToArray();
            result.success = result.records.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse P5-C shelter-building match data from {result.sourcePath}. {exception.Message}");
            return CreateLoadResult<ShelterBuildingMatchRecord>(sourceLabel);
        }
    }

    public static bool CanRenderRouteGeometryInCurrentUnityLayout(RouteSampleRecord route)
    {
        if (route == null || route.geometry == null || !route.geometry.HasCoordinates)
        {
            return false;
        }

        // P4/P5 real data has no verified WGS84-to-Unity/PLATEAU transform. Rendering
        // EPSG:4326 OSM line strings in the schematic debug layout would be misleading.
        return string.Equals(route.geometry.coordinateReferenceSystem, "UNITY_DEBUG", StringComparison.OrdinalIgnoreCase);
    }

    public static string ClassifyQualificationStatus(string qualificationStatus)
    {
        if (string.IsNullOrWhiteSpace(qualificationStatus))
        {
            return "unknown";
        }

        switch (qualificationStatus.Trim())
        {
            case "official_confirmed":
            case "official_confirmed_with_review":
                return "official";
            case "strong_candidate":
            case "weak_candidate":
                return "candidate";
            case "not_qualified":
                return "not qualified";
            case "unknown":
                return "unknown";
            default:
                return "unknown";
        }
    }

    public static bool IsPrototypeRoute(string routeType, bool isOfficialEvacuationRoute)
    {
        return !isOfficialEvacuationRoute &&
            !string.IsNullOrWhiteSpace(routeType) &&
            routeType.IndexOf("estimated", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static LoadResult<TRecord> LoadJsonFromPath<TRecord>(
        string path,
        string fileName,
        Func<string, string, LoadResult<TRecord>> loadFromJson)
    {
        var result = CreateLoadResult<TRecord>(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning($"P5-C data path for {fileName} was empty. P5-C data is disabled.");
            return result;
        }

        if (IsForbiddenRuntimePath(path))
        {
            Debug.LogWarning(
                $"Unity P5-C runtime loading must use copied Assets/Data JSON, not raw/cache/download/tmp/data_pipeline paths: {path}");
            return result;
        }

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Missing P5-C data file at {path}. P5-C overlay/feedback will be unavailable.");
            return result;
        }

        try
        {
            return loadFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load P5-C data from {path}. P5-C overlay/feedback will be unavailable. {exception.Message}");
            return result;
        }
    }

    private static LoadResult<TRecord> CreateLoadResult<TRecord>(string sourceLabel)
    {
        return new LoadResult<TRecord>
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "P5-C JSON" : sourceLabel,
            coordinateReferenceSystem = DefaultCoordinateSystem,
            metricCoordinateReferenceSystem = DefaultMetricCoordinateSystem,
            osmAttribution = string.Empty,
            records = new TRecord[0]
        };
    }

    private static void PopulateDatasetFields<TRecord>(
        LoadResult<TRecord> result,
        string datasetId,
        string generatedAt,
        string coordinateReferenceSystem,
        string metricCoordinateReferenceSystem,
        string osmAttribution,
        string notes)
    {
        result.datasetId = NullToEmpty(datasetId);
        result.generatedAt = NullToEmpty(generatedAt);
        result.coordinateReferenceSystem = FirstNonEmpty(coordinateReferenceSystem, DefaultCoordinateSystem);
        result.metricCoordinateReferenceSystem = FirstNonEmpty(metricCoordinateReferenceSystem, DefaultMetricCoordinateSystem);
        result.osmAttribution = NullToEmpty(osmAttribution);
        result.notes = NullToEmpty(notes);
    }

    private static IntegratedRouteQualificationRecord MapIntegratedRecord(IntegratedRecordRaw raw)
    {
        if (raw == null)
        {
            return new IntegratedRouteQualificationRecord();
        }

        return new IntegratedRouteQualificationRecord
        {
            qualificationId = Trim(raw.qualificationId),
            plateauBuildingId = Trim(raw.plateauBuildingId),
            shelterId = Trim(raw.shelterId),
            shelterName = Trim(raw.shelterName),
            officialDesignationStatus = Trim(raw.officialDesignationStatus),
            qualificationStatus = Trim(raw.qualificationStatus),
            qualificationReason = Trim(raw.qualificationReason),
            evidenceSources = MapEvidenceSources(raw.evidenceSources),
            matchMethod = Trim(raw.matchMethod),
            matchDistanceMeters = SanitizeOptionalFloat(raw.matchDistanceMeters),
            confidence = Trim(raw.confidence),
            manualReviewNeeded = raw.manualReviewNeeded,
            warnings = SanitizeStringArray(raw.warnings),
            disasterTypes = SanitizeStringArray(raw.disasterTypes),
            safeFloor = SanitizeOptionalInt(raw.safeFloor),
            capacity = SanitizeOptionalInt(raw.capacity),
            sourceUpdatedAt = Trim(raw.sourceUpdatedAt),
            routeAvailability = Trim(raw.routeAvailability),
            nearestRouteDistanceMeters = SanitizeOptionalFloat(raw.nearestRouteDistanceMeters),
            estimatedTravelTimeSeconds = SanitizeOptionalFloat(raw.estimatedTravelTimeSeconds),
            notes = Trim(raw.notes),
            routeSource = Trim(raw.routeSource),
            routeType = Trim(raw.routeType),
            isOfficialEvacuationRoute = raw.isOfficialEvacuationRoute,
            nearestRouteId = Trim(raw.nearestRouteId),
            nearestRouteOriginId = Trim(raw.nearestRouteOriginId),
            nearestRouteOriginName = Trim(raw.nearestRouteOriginName),
            availableRouteCount = Mathf.Max(0, raw.availableRouteCount),
            failedRouteCount = Mathf.Max(0, raw.failedRouteCount)
        };
    }

    private static RouteSampleRecord MapRouteRecord(RouteRecordRaw raw, string coordinateReferenceSystem)
    {
        if (raw == null)
        {
            return new RouteSampleRecord();
        }

        return new RouteSampleRecord
        {
            routeId = Trim(raw.routeId),
            originId = Trim(raw.originId),
            originName = Trim(raw.originName),
            shelterId = Trim(raw.shelterId),
            shelterName = Trim(raw.shelterName),
            plateauBuildingId = Trim(raw.plateauBuildingId),
            qualificationStatus = Trim(raw.qualificationStatus),
            routeAvailability = Trim(raw.routeAvailability),
            routeDistanceMeters = SanitizeOptionalFloat(raw.routeDistanceMeters),
            estimatedTravelTimeSeconds = SanitizeOptionalFloat(raw.estimatedTravelTimeSeconds),
            walkingSpeedMetersPerSecond = SanitizeOptionalFloat(raw.walkingSpeedMetersPerSecond),
            routeSource = Trim(raw.routeSource),
            routeType = Trim(raw.routeType),
            isOfficialEvacuationRoute = raw.isOfficialEvacuationRoute,
            geometry = new RouteGeometryRecord
            {
                type = raw.geometry != null ? Trim(raw.geometry.type) : string.Empty,
                coordinates = new Vector2[0],
                coordinateReferenceSystem = FirstNonEmpty(coordinateReferenceSystem, DefaultCoordinateSystem)
            },
            snapDistanceOriginMeters = SanitizeOptionalFloat(raw.snapDistanceOriginMeters),
            snapDistanceTargetMeters = SanitizeOptionalFloat(raw.snapDistanceTargetMeters),
            routeFailureReason = Trim(raw.routeFailureReason),
            warnings = SanitizeStringArray(raw.warnings),
            notes = Trim(raw.notes)
        };
    }

    private static BuildingQualificationRecord MapBuildingQualificationRecord(BuildingQualificationRecordRaw raw)
    {
        if (raw == null)
        {
            return new BuildingQualificationRecord();
        }

        return new BuildingQualificationRecord
        {
            qualificationId = Trim(raw.qualificationId),
            plateauBuildingId = Trim(raw.plateauBuildingId),
            shelterId = Trim(raw.shelterId),
            shelterName = Trim(raw.shelterName),
            officialDesignationStatus = Trim(raw.officialDesignationStatus),
            qualificationStatus = Trim(raw.qualificationStatus),
            qualificationReason = Trim(raw.qualificationReason),
            evidenceSources = MapEvidenceSources(raw.evidenceSources),
            matchMethod = Trim(raw.matchMethod),
            matchDistanceMeters = SanitizeOptionalFloat(raw.matchDistanceMeters),
            confidence = Trim(raw.confidence),
            manualReviewNeeded = raw.manualReviewNeeded,
            warnings = SanitizeStringArray(raw.warnings),
            disasterTypes = SanitizeStringArray(raw.disasterTypes),
            safeFloor = SanitizeOptionalInt(raw.safeFloor),
            capacity = SanitizeOptionalInt(raw.capacity),
            sourceUpdatedAt = Trim(raw.sourceUpdatedAt),
            routeAvailability = Trim(raw.routeAvailability),
            nearestRouteDistanceMeters = SanitizeOptionalFloat(raw.nearestRouteDistanceMeters),
            estimatedTravelTimeSeconds = SanitizeOptionalFloat(raw.estimatedTravelTimeSeconds),
            notes = Trim(raw.notes)
        };
    }

    private static ShelterBuildingMatchRecord MapShelterBuildingMatchRecord(ShelterBuildingMatchRecordRaw raw)
    {
        if (raw == null)
        {
            return new ShelterBuildingMatchRecord();
        }

        return new ShelterBuildingMatchRecord
        {
            shelterId = Trim(raw.shelterId),
            shelterName = Trim(raw.shelterName),
            plateauBuildingId = Trim(raw.plateauBuildingId),
            plateauGmlId = Trim(raw.plateauGmlId),
            matchMethod = Trim(raw.matchMethod),
            matchDistanceMeters = SanitizeOptionalFloat(raw.matchDistanceMeters),
            confidence = Trim(raw.confidence),
            manualReviewNeeded = raw.manualReviewNeeded,
            warnings = SanitizeStringArray(raw.warnings),
            sourceMeshCode = Trim(raw.sourceMeshCode)
        };
    }

    private static EvidenceSourceRecord[] MapEvidenceSources(EvidenceSourceRaw[] rawSources)
    {
        if (rawSources == null || rawSources.Length == 0)
        {
            return new EvidenceSourceRecord[0];
        }

        var sources = new List<EvidenceSourceRecord>();
        foreach (EvidenceSourceRaw rawSource in rawSources)
        {
            if (rawSource == null)
            {
                continue;
            }

            sources.Add(new EvidenceSourceRecord
            {
                sourceId = Trim(rawSource.sourceId),
                sourceFamily = Trim(rawSource.sourceFamily),
                sourceName = Trim(rawSource.sourceName),
                officialStatus = Trim(rawSource.officialStatus),
                sourceUrl = Trim(rawSource.sourceUrl),
                sourceUpdatedAt = Trim(rawSource.sourceUpdatedAt),
                evidenceNote = Trim(rawSource.evidenceNote)
            });
        }

        return sources.ToArray();
    }

    private static Dictionary<string, RouteGeometryRecord> ExtractRouteGeometriesByRouteId(
        string json,
        string coordinateReferenceSystem)
    {
        var geometries = new Dictionary<string, RouteGeometryRecord>();

        int recordsPropertyIndex = json.IndexOf("\"records\"", StringComparison.Ordinal);
        if (recordsPropertyIndex < 0)
        {
            return geometries;
        }

        int recordsArrayStart = json.IndexOf('[', recordsPropertyIndex);
        if (recordsArrayStart < 0)
        {
            return geometries;
        }

        int recordsArrayEnd = FindMatchingBracket(json, recordsArrayStart, '[', ']');
        if (recordsArrayEnd < 0)
        {
            return geometries;
        }

        int cursor = recordsArrayStart + 1;
        while (cursor < recordsArrayEnd)
        {
            int objectStart = json.IndexOf('{', cursor);
            if (objectStart < 0 || objectStart > recordsArrayEnd)
            {
                break;
            }

            int objectEnd = FindMatchingBracket(json, objectStart, '{', '}');
            if (objectEnd < 0 || objectEnd > recordsArrayEnd)
            {
                break;
            }

            string recordJson = json.Substring(objectStart, objectEnd - objectStart + 1);
            string routeId = ExtractJsonStringProperty(recordJson, "routeId");
            if (!string.IsNullOrWhiteSpace(routeId))
            {
                RouteGeometryRecord geometry = ExtractRouteGeometry(recordJson, coordinateReferenceSystem);
                if (geometry != null)
                {
                    geometries[routeId] = geometry;
                }
            }

            cursor = objectEnd + 1;
        }

        return geometries;
    }

    private static RouteGeometryRecord ExtractRouteGeometry(string recordJson, string coordinateReferenceSystem)
    {
        string geometryJson = ExtractJsonObjectProperty(recordJson, "geometry");
        if (string.IsNullOrWhiteSpace(geometryJson))
        {
            return null;
        }

        string type = ExtractJsonStringProperty(geometryJson, "type");
        string normalizedCoordinateReferenceSystem = FirstNonEmpty(coordinateReferenceSystem, DefaultCoordinateSystem);
        if (!string.Equals(type, GeoJsonLineStringType, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string coordinatesJson = ExtractJsonArrayProperty(geometryJson, "coordinates");
        Vector2[] coordinates = ParseCoordinatePairs(coordinatesJson);
        if (coordinates.Length < 2 ||
            !AllFinite(coordinates) ||
            (IsWgs84(normalizedCoordinateReferenceSystem) && !AllWgs84CoordinatesUseLonLatOrder(coordinates)))
        {
            return null;
        }

        return new RouteGeometryRecord
        {
            type = Trim(type),
            coordinates = coordinates,
            coordinateReferenceSystem = normalizedCoordinateReferenceSystem
        };
    }

    private static Vector2[] ParseCoordinatePairs(string coordinatesJson)
    {
        if (string.IsNullOrWhiteSpace(coordinatesJson))
        {
            return new Vector2[0];
        }

        var coordinates = new List<Vector2>();
        MatchCollection matches = Regex.Matches(
            coordinatesJson,
            "\\[\\s*(-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)\\s*,\\s*(-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?)\\s*\\]");

        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            if (float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float lon) &&
                float.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float lat))
            {
                coordinates.Add(new Vector2(lon, lat));
            }
        }

        return coordinates.ToArray();
    }

    private static bool AllFinite(Vector2[] coordinates)
    {
        if (coordinates == null || coordinates.Length == 0)
        {
            return false;
        }

        foreach (Vector2 coordinate in coordinates)
        {
            if (float.IsNaN(coordinate.x) || float.IsInfinity(coordinate.x) ||
                float.IsNaN(coordinate.y) || float.IsInfinity(coordinate.y))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AllWgs84CoordinatesUseLonLatOrder(Vector2[] coordinates)
    {
        foreach (Vector2 coordinate in coordinates)
        {
            if (coordinate.x < -180f || coordinate.x > 180f ||
                coordinate.y < -90f || coordinate.y > 90f)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsWgs84(string coordinateReferenceSystem)
    {
        return string.Equals(coordinateReferenceSystem, Wgs84CoordinateSystem, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractJsonObjectProperty(string json, string propertyName)
    {
        int valueStart = FindPropertyValueStart(json, propertyName);
        if (valueStart < 0)
        {
            return string.Empty;
        }

        int objectStart = json.IndexOf('{', valueStart);
        if (objectStart < 0)
        {
            return string.Empty;
        }

        int objectEnd = FindMatchingBracket(json, objectStart, '{', '}');
        return objectEnd < 0 ? string.Empty : json.Substring(objectStart, objectEnd - objectStart + 1);
    }

    private static string ExtractJsonArrayProperty(string json, string propertyName)
    {
        int valueStart = FindPropertyValueStart(json, propertyName);
        if (valueStart < 0)
        {
            return string.Empty;
        }

        int arrayStart = json.IndexOf('[', valueStart);
        if (arrayStart < 0)
        {
            return string.Empty;
        }

        int arrayEnd = FindMatchingBracket(json, arrayStart, '[', ']');
        return arrayEnd < 0 ? string.Empty : json.Substring(arrayStart, arrayEnd - arrayStart + 1);
    }

    private static string ExtractJsonStringProperty(string json, string propertyName)
    {
        Match match = Regex.Match(
            json,
            $"\"{Regex.Escape(propertyName)}\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"");
        return match.Success ? Regex.Unescape(match.Groups[1].Value) : string.Empty;
    }

    private static int FindPropertyValueStart(string json, string propertyName)
    {
        Match match = Regex.Match(json, $"\"{Regex.Escape(propertyName)}\"\\s*:");
        return match.Success ? match.Index + match.Length : -1;
    }

    private static int FindMatchingBracket(string json, int startIndex, char openChar, char closeChar)
    {
        bool inString = false;
        bool escaped = false;
        int depth = 0;

        for (int i = startIndex; i < json.Length; i++)
        {
            char c = json[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == openChar)
            {
                depth++;
            }
            else if (c == closeChar)
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static TRecord FindByShelterId<TRecord>(TRecord[] records, string shelterId)
        where TRecord : class, IHasShelterId
    {
        if (records == null || string.IsNullOrWhiteSpace(shelterId))
        {
            return null;
        }

        foreach (TRecord record in records)
        {
            if (record != null &&
                string.Equals(record.ShelterId, shelterId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return record;
            }
        }

        return null;
    }

    private static RouteSampleRecord FindRouteById(RouteSampleRecord[] records, string routeId)
    {
        if (records == null || string.IsNullOrWhiteSpace(routeId))
        {
            return null;
        }

        foreach (RouteSampleRecord record in records)
        {
            if (record != null &&
                string.Equals(record.routeId, routeId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return record;
            }
        }

        return null;
    }

    private static bool IsForbiddenRuntimePath(string path)
    {
        string normalized = path.Replace('\\', '/').ToLowerInvariant();
        return normalized.Contains("/data_pipeline/") ||
            normalized.Contains("/raw/") ||
            normalized.Contains("/downloads/") ||
            normalized.Contains("/cache/") ||
            normalized.Contains("/tmp/") ||
            normalized.Contains("/.venv/");
    }

    private static string NormalizeNullableNumbers(string json)
    {
        return Regex.Replace(
            json,
            "\"(safeFloor|capacity|matchDistanceMeters|nearestRouteDistanceMeters|estimatedTravelTimeSeconds|routeDistanceMeters|walkingSpeedMetersPerSecond|snapDistanceOriginMeters|snapDistanceTargetMeters)\"\\s*:\\s*null",
            "\"$1\": -1");
    }

    private static float SanitizeOptionalFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
        {
            return UnknownFloat;
        }

        return value;
    }

    private static int SanitizeOptionalInt(int value)
    {
        return value < 0 ? UnknownInt : value;
    }

    private static string[] SanitizeStringArray(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return new string[0];
        }

        var cleanValues = new List<string>();
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                cleanValues.Add(value.Trim());
            }
        }

        return cleanValues.ToArray();
    }

    private static string FirstNonEmpty(string first, string second)
    {
        return string.IsNullOrWhiteSpace(first) ? NullToEmpty(second).Trim() : first.Trim();
    }

    private static string NullToEmpty(string value)
    {
        return value ?? string.Empty;
    }

    private static string Trim(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string GetAssetsDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
