using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TsunamiHazardFixtureLoader
{
    public const string HazardFixtureFileName = "sample_tsunami_hazard_zones.json";
    private const string DefaultCoordinateSystem = "EPSG:4326";
    private const float UnknownMetricValue = -1f;

    [Serializable]
    private class HazardDataset
    {
        public string dataset_id = string.Empty;
        public string generated_at = string.Empty;
        public string coordinate_reference_system = DefaultCoordinateSystem;
        public HazardSource source;
        public HazardZone[] zones = new HazardZone[0];
    }

    [Serializable]
    private class HazardSource
    {
        public string source_id = string.Empty;
        public string source_family = string.Empty;
        public string source_name = string.Empty;
        public string organization = string.Empty;
        public string official_status = string.Empty;
        public bool is_official_primary;
        public string source_url = string.Empty;
        public string source_updated_at = string.Empty;
        public string notes = string.Empty;
    }

    [Serializable]
    private class HazardGeometry
    {
        public string type = string.Empty;
    }

    [Serializable]
    private class HazardZone
    {
        public string zone_id = string.Empty;
        public string zone_name = string.Empty;
        public string hazard_family = string.Empty;
        public string affected_zone = string.Empty;
        public int hazard_level;
        public string geometry_type = string.Empty;
        public HazardGeometry geometry;
        public string inundation_area = string.Empty;
        public float inundation_depth_m = UnknownMetricValue;
        public float tsunami_height_m = UnknownMetricValue;
        public string notes = string.Empty;
    }

    public class HazardZoneRecord
    {
        public string zoneId = string.Empty;
        public string zoneName = string.Empty;
        public string hazardFamily = string.Empty;
        public string affectedZone = string.Empty;
        public int hazardLevel;
        public string geometryType = string.Empty;
        public string geometryDataType = string.Empty;
        public string inundationArea = string.Empty;
        public float inundationDepthMeters;
        public bool hasInundationDepth;
        public float tsunamiHeightMeters;
        public bool hasTsunamiHeight;
        public string notes = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateSystem = DefaultCoordinateSystem;
        public string sourceId = string.Empty;
        public string sourceFamily = string.Empty;
        public string sourceName = string.Empty;
        public string sourceOrganization = string.Empty;
        public string officialStatus = string.Empty;
        public bool isOfficialPrimary;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string sourceNotes = string.Empty;

        public HazardZoneRecord Clone()
        {
            return new HazardZoneRecord
            {
                zoneId = zoneId,
                zoneName = zoneName,
                hazardFamily = hazardFamily,
                affectedZone = affectedZone,
                hazardLevel = hazardLevel,
                geometryType = geometryType,
                geometryDataType = geometryDataType,
                inundationArea = inundationArea,
                inundationDepthMeters = inundationDepthMeters,
                hasInundationDepth = hasInundationDepth,
                tsunamiHeightMeters = tsunamiHeightMeters,
                hasTsunamiHeight = hasTsunamiHeight,
                notes = notes,
                datasetId = datasetId,
                generatedAt = generatedAt,
                coordinateSystem = coordinateSystem,
                sourceId = sourceId,
                sourceFamily = sourceFamily,
                sourceName = sourceName,
                sourceOrganization = sourceOrganization,
                officialStatus = officialStatus,
                isOfficialPrimary = isOfficialPrimary,
                sourceUrl = sourceUrl,
                sourceUpdatedAt = sourceUpdatedAt,
                sourceNotes = sourceNotes
            };
        }
    }

    public class HazardFixtureLoadResult
    {
        public bool success;
        public string sourcePath = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateSystem = DefaultCoordinateSystem;
        public string sourceName = string.Empty;
        public string officialStatus = string.Empty;
        public HazardZoneRecord[] zones = new HazardZoneRecord[0];
    }

    public static HazardFixtureLoadResult LoadHazardFixture()
    {
        return LoadFromPath(GetAssetsDataPath(HazardFixtureFileName));
    }

    public static HazardFixtureLoadResult LoadFromPath(string path)
    {
        var result = new HazardFixtureLoadResult
        {
            sourcePath = path ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Debug.LogWarning($"Missing tsunami hazard fixture at {path}. No debug hazard zones were loaded.");
            return result;
        }

        try
        {
            return LoadFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load tsunami hazard fixture from {path}. {exception.Message}");
            return result;
        }
    }

    public static HazardFixtureLoadResult LoadFromJson(string json, string sourceLabel)
    {
        var result = new HazardFixtureLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "tsunami hazard fixture JSON" : sourceLabel
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty tsunami hazard fixture at {result.sourcePath}. No debug hazard zones were loaded.");
            return result;
        }

        try
        {
            var dataset = new HazardDataset();
            JsonUtility.FromJsonOverwrite(NormalizeJsonUtilityNullableNumbers(json), dataset);

            result.datasetId = NullToEmpty(dataset.dataset_id);
            result.generatedAt = NullToEmpty(dataset.generated_at);
            result.coordinateSystem = FirstNonEmpty(dataset.coordinate_reference_system, DefaultCoordinateSystem);
            result.sourceName = dataset.source != null ? NullToEmpty(dataset.source.source_name) : string.Empty;
            result.officialStatus = dataset.source != null ? NullToEmpty(dataset.source.official_status) : string.Empty;

            if (dataset.zones == null || dataset.zones.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain any tsunami hazard zones.");
                return result;
            }

            var zones = new List<HazardZoneRecord>();
            var seenIds = new HashSet<string>();

            foreach (HazardZone zone in dataset.zones)
            {
                if (zone == null)
                {
                    continue;
                }

                HazardZoneRecord record = MapZone(dataset, zone);
                if (string.IsNullOrWhiteSpace(record.zoneId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained a tsunami hazard zone with no zone_id. The zone was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.zoneId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate tsunami hazard zone id '{record.zoneId}'. The first entry was used.");
                    continue;
                }

                zones.Add(record);
            }

            result.zones = zones.ToArray();
            result.success = result.zones.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse tsunami hazard fixture from {result.sourcePath}. {exception.Message}");
            return result;
        }
    }

    private static HazardZoneRecord MapZone(HazardDataset dataset, HazardZone zone)
    {
        HazardSource source = dataset.source;
        bool hasDepth = zone.inundation_depth_m >= 0f;
        bool hasHeight = zone.tsunami_height_m >= 0f;

        return new HazardZoneRecord
        {
            zoneId = NullToEmpty(zone.zone_id),
            zoneName = FirstNonEmpty(zone.zone_name, zone.zone_id, "Unnamed hazard zone"),
            hazardFamily = NullToEmpty(zone.hazard_family),
            affectedZone = NullToEmpty(zone.affected_zone),
            hazardLevel = Mathf.Max(0, zone.hazard_level),
            geometryType = NullToEmpty(zone.geometry_type),
            geometryDataType = zone.geometry != null ? NullToEmpty(zone.geometry.type) : string.Empty,
            inundationArea = NullToEmpty(zone.inundation_area),
            inundationDepthMeters = hasDepth ? Mathf.Max(0f, zone.inundation_depth_m) : 0f,
            hasInundationDepth = hasDepth,
            tsunamiHeightMeters = hasHeight ? Mathf.Max(0f, zone.tsunami_height_m) : 0f,
            hasTsunamiHeight = hasHeight,
            notes = NullToEmpty(zone.notes),
            datasetId = NullToEmpty(dataset.dataset_id),
            generatedAt = NullToEmpty(dataset.generated_at),
            coordinateSystem = FirstNonEmpty(dataset.coordinate_reference_system, DefaultCoordinateSystem),
            sourceId = source != null ? NullToEmpty(source.source_id) : string.Empty,
            sourceFamily = source != null ? NullToEmpty(source.source_family) : string.Empty,
            sourceName = source != null ? NullToEmpty(source.source_name) : string.Empty,
            sourceOrganization = source != null ? NullToEmpty(source.organization) : string.Empty,
            officialStatus = source != null ? NullToEmpty(source.official_status) : string.Empty,
            isOfficialPrimary = source != null && source.is_official_primary,
            sourceUrl = source != null ? NullToEmpty(source.source_url) : string.Empty,
            sourceUpdatedAt = source != null ? NullToEmpty(source.source_updated_at) : string.Empty,
            sourceNotes = source != null ? NullToEmpty(source.notes) : string.Empty
        };
    }

    private static string NormalizeJsonUtilityNullableNumbers(string json)
    {
        return Regex.Replace(
            json,
            "\"(inundation_depth_m|tsunami_height_m)\"\\s*:\\s*null",
            $"\"$1\": {UnknownMetricValue:0.#}");
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

    private static string NullToEmpty(string value)
    {
        return value ?? string.Empty;
    }

    private static string GetAssetsDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
