using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RealQualifiedShelterDataLoader
{
    public const string SourceType = "real_qualified";
    public const string PrototypeDebugLayoutCoordinateSystem = "prototype_debug_layout_not_geospatial";
    public const string NoVerifiedGeospatialPlacementNote =
        "Runtime proxy uses deterministic prototype/debug layout; not true geospatial placement.";
    public const string UnavailableMessage = "P5-D real-qualified shelter data unavailable for this shelter";

    private const float UnknownFloat = -1f;
    private const int UnknownInt = -1;

    public class RealQualifiedShelterLoadResult
    {
        public bool success;
        public bool usesAssetsDataOnly;
        public string sourcePath = string.Empty;
        public string[] sourcePaths = new string[0];
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string osmAttribution = string.Empty;
        public string diagnostics = string.Empty;
        public int recordCount;
        public int selectableCount;
        public int nonSelectableCount;
        public int skippedRecordCount;
        public RealQualifiedShelterRecord[] records = new RealQualifiedShelterRecord[0];
        public P5CStaticDataLoader.P5CDataBundle sourceBundle;
    }

    public class RealQualifiedShelterRecord
    {
        public string gameplayShelterId = string.Empty;
        public string officialShelterId = string.Empty;
        public string officialShelterName = string.Empty;
        public string displayName = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string qualificationStatus = string.Empty;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string routeTargetId = string.Empty;
        public string nearestRouteOriginId = string.Empty;
        public string nearestRouteOriginName = string.Empty;
        public float routeDistanceMeters = UnknownFloat;
        public float estimatedRouteTimeSeconds = UnknownFloat;
        public bool hasSourceCoordinate;
        public float latitude;
        public float longitude;
        public string sourceCoordinateSystem = string.Empty;
        public bool isSelectable;
        public bool hasSafeMapping;
        public string nonSelectableReason = string.Empty;
        public string attribution = string.Empty;
        public string routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel;
        public string routeType = string.Empty;
        public string routeSource = string.Empty;
        public bool isOfficialEvacuationRoute;
        public int safeFloor = UnknownInt;
        public int capacity = UnknownInt;
        public string sourceUpdatedAt = string.Empty;
        public string sourceUrl = string.Empty;
        public string notes = string.Empty;
        public P5CStaticDataLoader.IntegratedRouteQualificationRecord integratedQualification;
        public P5CStaticDataLoader.RouteSampleRecord routeSample;
        public P5CStaticDataLoader.BuildingQualificationRecord buildingQualification;
        public P5CStaticDataLoader.ShelterBuildingMatchRecord shelterBuildingMatch;

        public bool HasRouteDistanceMeters => routeDistanceMeters >= 0f;
        public bool HasEstimatedRouteTimeSeconds => estimatedRouteTimeSeconds >= 0f;
    }

    public static RealQualifiedShelterLoadResult LoadFromAssetsData()
    {
        return BuildFromBundle(P5CStaticDataLoader.LoadBundleFromAssetsData(), true);
    }

    public static RealQualifiedShelterLoadResult LoadFromPaths(
        string integratedRouteQualificationPath,
        string routeSamplePath,
        string buildingQualificationPath,
        string shelterBuildingMatchesPath)
    {
        string[] paths =
        {
            integratedRouteQualificationPath,
            routeSamplePath,
            buildingQualificationPath,
            shelterBuildingMatchesPath
        };

        var preflight = new RealQualifiedShelterLoadResult
        {
            sourcePath = JoinSourcePaths(paths),
            sourcePaths = SanitizeSourcePaths(paths),
            usesAssetsDataOnly = AllPathsAreAssetsData(paths)
        };

        if (!preflight.usesAssetsDataOnly)
        {
            preflight.diagnostics =
                "P5-D real_qualified runtime loading must use copied static JSON under Assets/Data only.";
            Debug.LogWarning(preflight.diagnostics);
            return preflight;
        }

        var bundle = new P5CStaticDataLoader.P5CDataBundle
        {
            integratedRouteQualifications =
                P5CStaticDataLoader.LoadIntegratedRouteQualificationsFromPath(integratedRouteQualificationPath),
            routeSamples = P5CStaticDataLoader.LoadRouteSamplesFromPath(routeSamplePath),
            buildingQualifications = P5CStaticDataLoader.LoadBuildingQualificationsFromPath(buildingQualificationPath),
            shelterBuildingMatches = P5CStaticDataLoader.LoadShelterBuildingMatchesFromPath(shelterBuildingMatchesPath)
        };

        return BuildFromBundle(bundle, true);
    }

    public static RealQualifiedShelterLoadResult BuildFromBundle(P5CStaticDataLoader.P5CDataBundle bundle)
    {
        return BuildFromBundle(bundle, false);
    }

    public static bool IsDefaultSelectableQualificationStatus(string qualificationStatus)
    {
        return string.Equals(qualificationStatus, "official_confirmed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(qualificationStatus, "official_confirmed_with_review", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryFindRecord(
        RealQualifiedShelterLoadResult loadResult,
        string gameplayShelterId,
        out RealQualifiedShelterRecord record)
    {
        record = null;

        if (loadResult == null || loadResult.records == null || string.IsNullOrWhiteSpace(gameplayShelterId))
        {
            return false;
        }

        foreach (RealQualifiedShelterRecord candidate in loadResult.records)
        {
            if (candidate != null &&
                string.Equals(candidate.gameplayShelterId, gameplayShelterId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                record = candidate;
                return true;
            }
        }

        return false;
    }

    public static RealQualifiedShelterRecord[] GetSelectableRecords(RealQualifiedShelterRecord[] records)
    {
        if (records == null || records.Length == 0)
        {
            return new RealQualifiedShelterRecord[0];
        }

        var selectable = new List<RealQualifiedShelterRecord>();
        foreach (RealQualifiedShelterRecord record in records)
        {
            if (record != null && record.isSelectable)
            {
                selectable.Add(record);
            }
        }

        return selectable.ToArray();
    }

    public static bool IsForbiddenUnityRuntimePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        string normalized = path.Replace('\\', '/').ToLowerInvariant();
        return normalized.Contains("data_pipeline/") ||
            normalized.Contains("/raw/") ||
            normalized.Contains("/downloads/") ||
            normalized.Contains("/cache/") ||
            normalized.Contains("/tmp/") ||
            normalized.Contains("/.venv/") ||
            normalized.Contains("/osm_cache/");
    }

    private static RealQualifiedShelterLoadResult BuildFromBundle(
        P5CStaticDataLoader.P5CDataBundle bundle,
        bool requireAssetsDataSourcePaths)
    {
        var result = new RealQualifiedShelterLoadResult
        {
            sourceBundle = bundle,
            sourcePaths = ExtractSourcePaths(bundle),
            sourcePath = JoinSourcePaths(ExtractSourcePaths(bundle)),
            usesAssetsDataOnly = !requireAssetsDataSourcePaths || AllPathsAreAssetsData(ExtractSourcePaths(bundle))
        };

        if (bundle == null || !bundle.HasCoreQualificationData)
        {
            result.diagnostics = "P5-D real_qualified data is unavailable because integrated qualification data did not load.";
            Debug.LogWarning(result.diagnostics);
            return result;
        }

        if (requireAssetsDataSourcePaths && !result.usesAssetsDataOnly)
        {
            result.diagnostics =
                "P5-D real_qualified runtime loading must use copied static JSON under Assets/Data only.";
            Debug.LogWarning(result.diagnostics);
            return result;
        }

        P5CStaticDataLoader.LoadResult<P5CStaticDataLoader.IntegratedRouteQualificationRecord> integrated =
            bundle.integratedRouteQualifications;
        result.datasetId = integrated.datasetId;
        result.generatedAt = integrated.generatedAt;
        result.osmAttribution = bundle.OsmAttribution;

        Dictionary<string, int> idCounts = CountShelterIds(integrated.records);
        Dictionary<string, P5CStaticDataLoader.RouteSampleRecord> routesById =
            IndexRoutesByRouteId(bundle.routeSamples != null ? bundle.routeSamples.records : null);
        Dictionary<string, P5CStaticDataLoader.BuildingQualificationRecord> buildingById =
            IndexByShelterId(bundle.buildingQualifications != null ? bundle.buildingQualifications.records : null);
        Dictionary<string, P5CStaticDataLoader.ShelterBuildingMatchRecord> matchById =
            IndexByShelterId(bundle.shelterBuildingMatches != null ? bundle.shelterBuildingMatches.records : null);

        var records = new List<RealQualifiedShelterRecord>();
        foreach (P5CStaticDataLoader.IntegratedRouteQualificationRecord integratedRecord in integrated.records)
        {
            RealQualifiedShelterRecord mapped = MapRecord(
                integratedRecord,
                routesById,
                buildingById,
                matchById,
                idCounts,
                bundle.OsmAttribution);

            if (mapped == null)
            {
                result.skippedRecordCount++;
                continue;
            }

            records.Add(mapped);
            if (mapped.isSelectable)
            {
                result.selectableCount++;
            }
            else
            {
                result.nonSelectableCount++;
            }
        }

        result.records = records.ToArray();
        result.recordCount = result.records.Length;
        result.success = result.selectableCount > 0;
        if (!result.success)
        {
            result.diagnostics = "No selectable P5-D real_qualified shelter records were available.";
        }

        return result;
    }

    private static RealQualifiedShelterRecord MapRecord(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord integratedRecord,
        Dictionary<string, P5CStaticDataLoader.RouteSampleRecord> routesById,
        Dictionary<string, P5CStaticDataLoader.BuildingQualificationRecord> buildingById,
        Dictionary<string, P5CStaticDataLoader.ShelterBuildingMatchRecord> matchById,
        Dictionary<string, int> idCounts,
        string osmAttribution)
    {
        if (integratedRecord == null)
        {
            return null;
        }

        string officialShelterId = Trim(integratedRecord.shelterId);
        var warnings = new List<string>();
        AddWarnings(warnings, integratedRecord.warnings);

        P5CStaticDataLoader.RouteSampleRecord route = null;
        if (!string.IsNullOrWhiteSpace(integratedRecord.nearestRouteId))
        {
            routesById.TryGetValue(integratedRecord.nearestRouteId, out route);
            if (route == null)
            {
                warnings.Add($"nearestRouteId '{integratedRecord.nearestRouteId}' was not found; route preview disabled.");
            }
        }

        P5CStaticDataLoader.BuildingQualificationRecord building = null;
        if (!string.IsNullOrWhiteSpace(officialShelterId))
        {
            buildingById.TryGetValue(officialShelterId, out building);
        }

        P5CStaticDataLoader.ShelterBuildingMatchRecord match = null;
        if (!string.IsNullOrWhiteSpace(officialShelterId))
        {
            matchById.TryGetValue(officialShelterId, out match);
        }

        bool duplicateId = !string.IsNullOrWhiteSpace(officialShelterId) &&
            idCounts.TryGetValue(officialShelterId, out int count) &&
            count > 1;
        bool manualReview = integratedRecord.manualReviewNeeded ||
            string.Equals(
                integratedRecord.qualificationStatus,
                "official_confirmed_with_review",
                StringComparison.OrdinalIgnoreCase);
        bool selectableStatus = IsDefaultSelectableQualificationStatus(integratedRecord.qualificationStatus);
        bool hasPlateauBuildingId = !string.IsNullOrWhiteSpace(integratedRecord.plateauBuildingId);
        bool hasSafeMapping = !string.IsNullOrWhiteSpace(officialShelterId) &&
            !duplicateId &&
            hasPlateauBuildingId &&
            !HasPlateauMismatch(integratedRecord, building, match, warnings);

        if (manualReview && !ContainsWarning(warnings, "manual review"))
        {
            warnings.Add("manual review needed before treating this mapping as final evidence");
        }

        string nonSelectableReason = BuildNonSelectableReason(
            selectableStatus,
            hasSafeMapping,
            officialShelterId,
            duplicateId,
            hasPlateauBuildingId,
            integratedRecord.qualificationStatus);

        float routeDistance = integratedRecord.HasNearestRouteDistanceMeters
            ? integratedRecord.nearestRouteDistanceMeters
            : route != null && route.HasRouteDistanceMeters ? route.routeDistanceMeters : UnknownFloat;
        float routeTime = integratedRecord.HasEstimatedTravelTimeSeconds
            ? integratedRecord.estimatedTravelTimeSeconds
            : route != null && route.HasEstimatedTravelTimeSeconds ? route.estimatedTravelTimeSeconds : UnknownFloat;

        return new RealQualifiedShelterRecord
        {
            gameplayShelterId = officialShelterId,
            officialShelterId = officialShelterId,
            officialShelterName = Trim(integratedRecord.shelterName),
            displayName = FirstNonEmpty(integratedRecord.shelterName, officialShelterId, "Unnamed qualified shelter"),
            plateauBuildingId = Trim(integratedRecord.plateauBuildingId),
            qualificationStatus = Trim(integratedRecord.qualificationStatus),
            confidence = FirstNonEmpty(integratedRecord.confidence, "unknown"),
            manualReviewNeeded = manualReview,
            warnings = warnings.ToArray(),
            routeTargetId = Trim(integratedRecord.nearestRouteId),
            nearestRouteOriginId = Trim(integratedRecord.nearestRouteOriginId),
            nearestRouteOriginName = Trim(integratedRecord.nearestRouteOriginName),
            routeDistanceMeters = SanitizeOptionalFloat(routeDistance),
            estimatedRouteTimeSeconds = SanitizeOptionalFloat(routeTime),
            hasSourceCoordinate = false,
            latitude = 0f,
            longitude = 0f,
            sourceCoordinateSystem = FirstNonEmpty(
                integratedRecord.routeAvailability,
                PrototypeDebugLayoutCoordinateSystem),
            isSelectable = selectableStatus && hasSafeMapping,
            hasSafeMapping = hasSafeMapping,
            nonSelectableReason = nonSelectableReason,
            attribution = FirstNonEmpty(osmAttribution, "Route network data from OpenStreetMap contributors; use under the Open Database License."),
            routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel,
            routeType = Trim(integratedRecord.routeType),
            routeSource = Trim(integratedRecord.routeSource),
            isOfficialEvacuationRoute = integratedRecord.isOfficialEvacuationRoute,
            safeFloor = ResolveOptionalInt(integratedRecord.safeFloor, building != null ? building.safeFloor : UnknownInt),
            capacity = ResolveOptionalInt(integratedRecord.capacity, building != null ? building.capacity : UnknownInt),
            sourceUpdatedAt = Trim(integratedRecord.sourceUpdatedAt),
            sourceUrl = FirstEvidenceUrl(integratedRecord.evidenceSources),
            notes = BuildNotes(integratedRecord, route),
            integratedQualification = integratedRecord,
            routeSample = route,
            buildingQualification = building,
            shelterBuildingMatch = match
        };
    }

    private static string BuildNonSelectableReason(
        bool selectableStatus,
        bool hasSafeMapping,
        string officialShelterId,
        bool duplicateId,
        bool hasPlateauBuildingId,
        string qualificationStatus)
    {
        if (!selectableStatus)
        {
            return $"qualificationStatus '{Trim(qualificationStatus)}' is visible/debug-only in P5-D 1.0";
        }

        if (string.IsNullOrWhiteSpace(officialShelterId))
        {
            return "missing official shelter id";
        }

        if (duplicateId)
        {
            return "duplicate official shelter id; unsafe mapping";
        }

        if (!hasPlateauBuildingId)
        {
            return "missing plateauBuildingId; unsafe building mapping";
        }

        return hasSafeMapping ? string.Empty : "unsafe shelter/building mapping";
    }

    private static bool HasPlateauMismatch(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord integratedRecord,
        P5CStaticDataLoader.BuildingQualificationRecord building,
        P5CStaticDataLoader.ShelterBuildingMatchRecord match,
        List<string> warnings)
    {
        string plateauBuildingId = Trim(integratedRecord.plateauBuildingId);
        bool mismatch = false;

        if (building != null &&
            !string.IsNullOrWhiteSpace(building.plateauBuildingId) &&
            !string.Equals(plateauBuildingId, building.plateauBuildingId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("building qualification plateauBuildingId did not match integrated record; kept non-selectable");
            mismatch = true;
        }

        if (match != null &&
            !string.IsNullOrWhiteSpace(match.plateauBuildingId) &&
            !string.Equals(plateauBuildingId, match.plateauBuildingId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("shelter-building match plateauBuildingId did not match integrated record; kept non-selectable");
            mismatch = true;
        }

        return mismatch;
    }

    private static Dictionary<string, int> CountShelterIds(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord[] records)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (records == null)
        {
            return counts;
        }

        foreach (P5CStaticDataLoader.IntegratedRouteQualificationRecord record in records)
        {
            string shelterId = record != null ? Trim(record.shelterId) : string.Empty;
            if (string.IsNullOrWhiteSpace(shelterId))
            {
                continue;
            }

            counts[shelterId] = counts.TryGetValue(shelterId, out int count) ? count + 1 : 1;
        }

        return counts;
    }

    private static Dictionary<string, P5CStaticDataLoader.RouteSampleRecord> IndexRoutesByRouteId(
        P5CStaticDataLoader.RouteSampleRecord[] records)
    {
        var byId = new Dictionary<string, P5CStaticDataLoader.RouteSampleRecord>(StringComparer.OrdinalIgnoreCase);
        if (records == null)
        {
            return byId;
        }

        foreach (P5CStaticDataLoader.RouteSampleRecord record in records)
        {
            if (record != null && !string.IsNullOrWhiteSpace(record.routeId) && !byId.ContainsKey(record.routeId))
            {
                byId[record.routeId] = record;
            }
        }

        return byId;
    }

    private static Dictionary<string, TRecord> IndexByShelterId<TRecord>(TRecord[] records)
        where TRecord : class, P5CStaticDataLoader.IHasShelterId
    {
        var byId = new Dictionary<string, TRecord>(StringComparer.OrdinalIgnoreCase);
        if (records == null)
        {
            return byId;
        }

        foreach (TRecord record in records)
        {
            if (record != null && !string.IsNullOrWhiteSpace(record.ShelterId) && !byId.ContainsKey(record.ShelterId))
            {
                byId[record.ShelterId] = record;
            }
        }

        return byId;
    }

    private static bool AllPathsAreAssetsData(string[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            return false;
        }

        foreach (string path in paths)
        {
            if (IsForbiddenUnityRuntimePath(path) || !IsAssetsDataPath(path))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAssetsDataPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            string fullPath = Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');
            string assetsDataPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Data"))
                .Replace('\\', '/')
                .TrimEnd('/');
            return fullPath.StartsWith(assetsDataPath + "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fullPath, assetsDataPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string[] ExtractSourcePaths(P5CStaticDataLoader.P5CDataBundle bundle)
    {
        if (bundle == null)
        {
            return new string[0];
        }

        return SanitizeSourcePaths(new[]
        {
            bundle.integratedRouteQualifications != null ? bundle.integratedRouteQualifications.sourcePath : string.Empty,
            bundle.routeSamples != null ? bundle.routeSamples.sourcePath : string.Empty,
            bundle.buildingQualifications != null ? bundle.buildingQualifications.sourcePath : string.Empty,
            bundle.shelterBuildingMatches != null ? bundle.shelterBuildingMatches.sourcePath : string.Empty
        });
    }

    private static string[] SanitizeSourcePaths(string[] paths)
    {
        if (paths == null)
        {
            return new string[0];
        }

        var clean = new List<string>();
        foreach (string path in paths)
        {
            clean.Add(path ?? string.Empty);
        }

        return clean.ToArray();
    }

    private static string JoinSourcePaths(string[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            return string.Empty;
        }

        return string.Join("; ", paths);
    }

    private static string BuildNotes(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord integratedRecord,
        P5CStaticDataLoader.RouteSampleRecord route)
    {
        string notes = integratedRecord != null ? Trim(integratedRecord.notes) : string.Empty;
        string routeNotes = route != null ? Trim(route.notes) : string.Empty;

        if (string.IsNullOrWhiteSpace(routeNotes))
        {
            return FirstNonEmpty(notes, NoVerifiedGeospatialPlacementNote);
        }

        return string.IsNullOrWhiteSpace(notes)
            ? routeNotes
            : $"{notes} {routeNotes}";
    }

    private static string FirstEvidenceUrl(P5CStaticDataLoader.EvidenceSourceRecord[] sources)
    {
        if (sources == null)
        {
            return string.Empty;
        }

        foreach (P5CStaticDataLoader.EvidenceSourceRecord source in sources)
        {
            if (source != null && !string.IsNullOrWhiteSpace(source.sourceUrl))
            {
                return source.sourceUrl.Trim();
            }
        }

        return string.Empty;
    }

    private static void AddWarnings(List<string> destination, string[] warnings)
    {
        if (destination == null || warnings == null)
        {
            return;
        }

        foreach (string warning in warnings)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                destination.Add(warning.Trim());
            }
        }
    }

    private static bool ContainsWarning(List<string> warnings, string text)
    {
        if (warnings == null || string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (string warning in warnings)
        {
            if (!string.IsNullOrWhiteSpace(warning) &&
                warning.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static float SanitizeOptionalFloat(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? UnknownFloat : value;
    }

    private static int ResolveOptionalInt(int first, int second)
    {
        if (first >= 0)
        {
            return first;
        }

        return second >= 0 ? second : UnknownInt;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static string Trim(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
