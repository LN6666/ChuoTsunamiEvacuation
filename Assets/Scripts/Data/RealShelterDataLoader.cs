using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RealShelterDataLoader
{
    public const string RealSampleFileName = "real_chuo_shelters_sample.json";
    private const string DefaultCoordinateSystem = "EPSG:4326";
    private const string DefaultSourceType = "real_sample";

    [Serializable]
    private class RealShelterDataset
    {
        public string dataset_id = string.Empty;
        public string generated_at = string.Empty;
        public string source_manifest = string.Empty;
        public string coordinate_reference_system = DefaultCoordinateSystem;
        public P3ShelterRecord[] records = new P3ShelterRecord[0];
    }

    [Serializable]
    private class P3ShelterRecord
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public string type = string.Empty;
        public float latitude;
        public float longitude;
        public string address = string.Empty;
        public int capacity;
        public int floors_available;
        public float elevation_m;
        public string source = string.Empty;
        public string source_url = string.Empty;
        public string source_updated_at = string.Empty;
        public string notes = string.Empty;
        public UnityHints unity;

        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string sourceType = string.Empty;
        public string facilityType = string.Empty;
        public string coordinateSystem = string.Empty;
        public UnityPosition unityPosition;
        public bool isOfficialShelter;
        public string disasterType = string.Empty;
        public string[] disasterTypes = new string[0];
        public int safeFloor;
        public bool canEnter;
        public float entryDelaySeconds;
        public float climbTimeSeconds;
        public float crowdingDelaySeconds;
        public string dataSource = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
    }

    [Serializable]
    private class UnityHints
    {
        public string prefab_hint = string.Empty;
        public bool is_entry_enabled;
        public int estimated_stair_floors;
    }

    [Serializable]
    public class UnityPosition
    {
        public float x;
        public float y;
        public float z;

        public UnityPosition Clone()
        {
            return new UnityPosition
            {
                x = x,
                y = y,
                z = z
            };
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public class RealShelterRecord
    {
        public string shelterId = string.Empty;
        public string shelterName = string.Empty;
        public string sourceType = DefaultSourceType;
        public string facilityType = "unknown";
        public string address = string.Empty;
        public float latitude;
        public float longitude;
        public string coordinateSystem = DefaultCoordinateSystem;
        public UnityPosition unityPosition;
        public bool isOfficialShelter;
        public string[] disasterTypes = new string[0];
        public int safeFloor;
        public int capacity;
        public bool hasCapacity;
        public bool canEnter = true;
        public float entryDelaySeconds;
        public float climbTimeSeconds = 10f;
        public float crowdingDelaySeconds;
        public string dataSource = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string notes = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string p3RecordId = string.Empty;
        public string p3Type = string.Empty;
        public string prefabHint = string.Empty;
        public int estimatedStairFloors;

        public bool HasUnityPosition => unityPosition != null;

        public RealShelterRecord Clone()
        {
            return new RealShelterRecord
            {
                shelterId = shelterId,
                shelterName = shelterName,
                sourceType = sourceType,
                facilityType = facilityType,
                address = address,
                latitude = latitude,
                longitude = longitude,
                coordinateSystem = coordinateSystem,
                unityPosition = unityPosition != null ? unityPosition.Clone() : null,
                isOfficialShelter = isOfficialShelter,
                disasterTypes = disasterTypes != null ? (string[])disasterTypes.Clone() : new string[0],
                safeFloor = safeFloor,
                capacity = capacity,
                hasCapacity = hasCapacity,
                canEnter = canEnter,
                entryDelaySeconds = entryDelaySeconds,
                climbTimeSeconds = climbTimeSeconds,
                crowdingDelaySeconds = crowdingDelaySeconds,
                dataSource = dataSource,
                sourceUrl = sourceUrl,
                sourceUpdatedAt = sourceUpdatedAt,
                notes = notes,
                datasetId = datasetId,
                generatedAt = generatedAt,
                p3RecordId = p3RecordId,
                p3Type = p3Type,
                prefabHint = prefabHint,
                estimatedStairFloors = estimatedStairFloors
            };
        }
    }

    public class RealShelterLoadResult
    {
        public bool success;
        public string sourcePath = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateSystem = DefaultCoordinateSystem;
        public RealShelterRecord[] shelters = new RealShelterRecord[0];
    }

    public static RealShelterLoadResult LoadRealSample()
    {
        return LoadFromPath(GetAssetsDataPath(RealSampleFileName));
    }

    public static RealShelterLoadResult LoadFromPath(string path)
    {
        var result = new RealShelterLoadResult
        {
            sourcePath = path ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Debug.LogWarning($"Missing real shelter data at {path}. No real shelter records were loaded.");
            return result;
        }

        try
        {
            return LoadFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load real shelter data from {path}. No real shelter records were loaded. {exception.Message}");
            return result;
        }
    }

    public static RealShelterLoadResult LoadFromJson(string json, string sourceLabel)
    {
        var result = new RealShelterLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "real shelter JSON" : sourceLabel
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Empty real shelter data at {result.sourcePath}. No real shelter records were loaded.");
            return result;
        }

        try
        {
            var dataset = new RealShelterDataset();
            JsonUtility.FromJsonOverwrite(NormalizeJsonUtilityNullableNumbers(json), dataset);

            result.datasetId = NullToEmpty(dataset.dataset_id);
            result.generatedAt = NullToEmpty(dataset.generated_at);
            result.coordinateSystem = FirstNonEmpty(dataset.coordinate_reference_system, DefaultCoordinateSystem);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                Debug.LogWarning($"{result.sourcePath} did not contain any real shelter records.");
                return result;
            }

            var shelters = new List<RealShelterRecord>();
            var seenIds = new HashSet<string>();

            foreach (P3ShelterRecord rawRecord in dataset.records)
            {
                if (rawRecord == null)
                {
                    continue;
                }

                RealShelterRecord record = MapRecord(dataset, rawRecord, result.sourcePath);

                if (string.IsNullOrWhiteSpace(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained a real shelter record with no id/shelterId. The record was skipped.");
                    continue;
                }

                if (!seenIds.Add(record.shelterId))
                {
                    Debug.LogWarning($"{result.sourcePath} contained duplicate real shelter id '{record.shelterId}'. The first entry was used.");
                    continue;
                }

                shelters.Add(record);
            }

            result.shelters = shelters.ToArray();
            result.success = result.shelters.Length > 0;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not parse real shelter data from {result.sourcePath}. No real shelter records were loaded. {exception.Message}");
            return result;
        }
    }

    private static RealShelterRecord MapRecord(RealShelterDataset dataset, P3ShelterRecord rawRecord, string sourceLabel)
    {
        UnityHints unity = rawRecord.unity;
        int estimatedFloors = unity != null ? Mathf.Max(0, unity.estimated_stair_floors) : 0;
        int safeFloor = rawRecord.safeFloor > 0
            ? rawRecord.safeFloor
            : Mathf.Max(0, rawRecord.floors_available > 0 ? rawRecord.floors_available : estimatedFloors);

        var record = new RealShelterRecord
        {
            shelterId = FirstNonEmpty(rawRecord.shelterId, rawRecord.id),
            shelterName = FirstNonEmpty(rawRecord.shelterName, rawRecord.name, rawRecord.id, "Unnamed real shelter"),
            sourceType = FirstNonEmpty(rawRecord.sourceType, DefaultSourceType),
            facilityType = FirstNonEmpty(rawRecord.facilityType, rawRecord.type, "unknown"),
            address = NullToEmpty(rawRecord.address),
            latitude = rawRecord.latitude,
            longitude = rawRecord.longitude,
            coordinateSystem = FirstNonEmpty(rawRecord.coordinateSystem, dataset.coordinate_reference_system, DefaultCoordinateSystem),
            unityPosition = rawRecord.unityPosition != null ? rawRecord.unityPosition.Clone() : null,
            isOfficialShelter = rawRecord.isOfficialShelter,
            disasterTypes = NormalizeDisasterTypes(rawRecord),
            safeFloor = safeFloor,
            capacity = Mathf.Max(0, rawRecord.capacity),
            hasCapacity = rawRecord.capacity > 0,
            canEnter = unity != null ? unity.is_entry_enabled : rawRecord.canEnter,
            entryDelaySeconds = Mathf.Max(0f, rawRecord.entryDelaySeconds),
            climbTimeSeconds = rawRecord.climbTimeSeconds > 0f ? rawRecord.climbTimeSeconds : EstimateClimbTimeSeconds(safeFloor),
            crowdingDelaySeconds = Mathf.Max(0f, rawRecord.crowdingDelaySeconds),
            dataSource = FirstNonEmpty(rawRecord.dataSource, rawRecord.source),
            sourceUrl = FirstNonEmpty(rawRecord.sourceUrl, rawRecord.source_url),
            sourceUpdatedAt = FirstNonEmpty(rawRecord.sourceUpdatedAt, rawRecord.source_updated_at),
            notes = NullToEmpty(rawRecord.notes),
            datasetId = NullToEmpty(dataset.dataset_id),
            generatedAt = NullToEmpty(dataset.generated_at),
            p3RecordId = NullToEmpty(rawRecord.id),
            p3Type = NullToEmpty(rawRecord.type),
            prefabHint = unity != null ? NullToEmpty(unity.prefab_hint) : string.Empty,
            estimatedStairFloors = estimatedFloors
        };

        Sanitize(record, sourceLabel);
        return record;
    }

    private static string[] NormalizeDisasterTypes(P3ShelterRecord rawRecord)
    {
        var normalized = new List<string>();

        if (rawRecord.disasterTypes != null)
        {
            foreach (string disasterType in rawRecord.disasterTypes)
            {
                AddIfPresent(normalized, disasterType);
            }
        }

        AddIfPresent(normalized, rawRecord.disasterType);
        return normalized.ToArray();
    }

    private static void Sanitize(RealShelterRecord record, string sourceLabel)
    {
        if (record == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(record.sourceType))
        {
            record.sourceType = DefaultSourceType;
        }

        if (string.IsNullOrWhiteSpace(record.facilityType))
        {
            record.facilityType = "unknown";
        }

        if (string.IsNullOrWhiteSpace(record.coordinateSystem))
        {
            record.coordinateSystem = DefaultCoordinateSystem;
        }

        if (record.climbTimeSeconds <= 0f)
        {
            record.climbTimeSeconds = 10f;
        }

        if (record.disasterTypes == null)
        {
            record.disasterTypes = new string[0];
        }

        if (float.IsNaN(record.latitude) || float.IsInfinity(record.latitude))
        {
            Debug.LogWarning($"{sourceLabel} shelter {record.shelterId} had an invalid latitude. Using 0.");
            record.latitude = 0f;
        }

        if (float.IsNaN(record.longitude) || float.IsInfinity(record.longitude))
        {
            Debug.LogWarning($"{sourceLabel} shelter {record.shelterId} had an invalid longitude. Using 0.");
            record.longitude = 0f;
        }
    }

    private static float EstimateClimbTimeSeconds(int safeFloor)
    {
        return Mathf.Max(1, safeFloor) * 3f;
    }

    private static void AddIfPresent(List<string> values, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string trimmed = value.Trim();
        if (!values.Contains(trimmed))
        {
            values.Add(trimmed);
        }
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

    private static string NormalizeJsonUtilityNullableNumbers(string json)
    {
        // P3 correctly represents unknown numeric values as null. JsonUtility maps into
        // value-type fields, so normalize those known nullable numeric fields in memory.
        return Regex.Replace(
            json,
            "\"(capacity|floors_available|elevation_m|estimated_stair_floors|safeFloor)\"\\s*:\\s*null",
            "\"$1\": 0");
    }

    private static string GetAssetsDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
