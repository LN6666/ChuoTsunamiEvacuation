using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ShelterDataLoader
{
    private const string ShelterDataFileName = "test_shelters.json";

    private static readonly Dictionary<string, ShelterData> ShelterById = new Dictionary<string, ShelterData>();
    private static readonly Dictionary<string, ShelterData> RuntimeShelterOverridesById = new Dictionary<string, ShelterData>();
    private static readonly HashSet<string> RuntimeShelterIds = new HashSet<string>();
    private static readonly List<string> ShelterIdsInLoadOrder = new List<string>();
    private static bool hasLoaded;
    private static string runtimeOverrideSource = "scenario";
    private static string runtimeShelterSource = "runtime";

    [Serializable]
    private class ShelterDataList
    {
        public ShelterData[] shelters = new ShelterData[0];
    }

    [Serializable]
    public class ShelterData
    {
        public string shelterId = "test_shelter_001";
        public string shelterName = "Test Shelter";
        public string shelterRank = "S";
        public bool isOfficialShelter = true;
        public bool canEnter = true;
        public float entryDelaySeconds;
        public float climbTimeSeconds = 10f;
        public float crowdingDelaySeconds;
        public string failureReason = "This shelter is not available.";
        public string sourceType = "test";
        public string facilityType = "debug_shelter";
        public LayoutPosition layoutPosition = new LayoutPosition();
        public string realFacilityName = string.Empty;
        public string address = string.Empty;
        public float latitude;
        public float longitude;
        public string coordinateSystem = "debug_platform";
        public string plateauBuildingId = string.Empty;
        public int safeFloor;
        public int capacity;
        public string dataSource = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string notes = string.Empty;

        public ShelterData Clone()
        {
            return new ShelterData
            {
                shelterId = shelterId,
                shelterName = shelterName,
                shelterRank = shelterRank,
                isOfficialShelter = isOfficialShelter,
                canEnter = canEnter,
                entryDelaySeconds = entryDelaySeconds,
                climbTimeSeconds = climbTimeSeconds,
                crowdingDelaySeconds = crowdingDelaySeconds,
                failureReason = failureReason,
                sourceType = sourceType,
                facilityType = facilityType,
                layoutPosition = layoutPosition != null ? layoutPosition.Clone() : new LayoutPosition(),
                realFacilityName = realFacilityName,
                address = address,
                latitude = latitude,
                longitude = longitude,
                coordinateSystem = coordinateSystem,
                plateauBuildingId = plateauBuildingId,
                safeFloor = safeFloor,
                capacity = capacity,
                dataSource = dataSource,
                sourceUrl = sourceUrl,
                sourceUpdatedAt = sourceUpdatedAt,
                notes = notes
            };
        }

        public void Sanitize(string source)
        {
            if (string.IsNullOrWhiteSpace(shelterId))
            {
                Debug.LogWarning($"{source} contained a shelter with no shelterId. Using test_shelter_001.");
                shelterId = "test_shelter_001";
            }

            if (string.IsNullOrWhiteSpace(shelterName))
            {
                Debug.LogWarning($"{source} shelter {shelterId} had no shelterName. Using shelterId as display name.");
                shelterName = shelterId;
            }

            if (string.IsNullOrWhiteSpace(shelterRank))
            {
                Debug.LogWarning($"{source} shelter {shelterId} had no shelterRank. Using B.");
                shelterRank = "B";
            }

            entryDelaySeconds = Mathf.Max(0f, entryDelaySeconds);
            climbTimeSeconds = climbTimeSeconds <= 0f ? 10f : climbTimeSeconds;
            crowdingDelaySeconds = Mathf.Max(0f, crowdingDelaySeconds);

            if (string.IsNullOrWhiteSpace(failureReason))
            {
                failureReason = "This shelter is not available.";
            }

            if (string.IsNullOrWhiteSpace(sourceType))
            {
                sourceType = "test";
            }

            if (string.IsNullOrWhiteSpace(facilityType))
            {
                facilityType = "debug_shelter";
            }

            if (layoutPosition == null)
            {
                Debug.LogWarning($"{source} shelter {shelterId} had no layoutPosition. Using debug platform origin.");
                layoutPosition = new LayoutPosition();
            }

            layoutPosition.Sanitize($"{source} shelter {shelterId}");

            if (string.IsNullOrWhiteSpace(coordinateSystem))
            {
                coordinateSystem = "debug_platform";
            }

            safeFloor = Mathf.Max(0, safeFloor);
            capacity = Mathf.Max(0, capacity);
        }
    }

    [Serializable]
    public class LayoutPosition
    {
        public float x;
        public float y;
        public float z;

        public LayoutPosition Clone()
        {
            return new LayoutPosition
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

        public void Sanitize(string source)
        {
            if (float.IsNaN(x) || float.IsInfinity(x))
            {
                Debug.LogWarning($"{source} had an invalid layoutPosition.x. Using 0.");
                x = 0f;
            }

            if (float.IsNaN(y) || float.IsInfinity(y))
            {
                Debug.LogWarning($"{source} had an invalid layoutPosition.y. Using 0.");
                y = 0f;
            }

            if (float.IsNaN(z) || float.IsInfinity(z))
            {
                Debug.LogWarning($"{source} had an invalid layoutPosition.z. Using 0.");
                z = 0f;
            }
        }
    }

    public static ShelterData GetShelterOrDefault(string shelterId)
    {
        TryGetShelter(shelterId, out ShelterData shelterData);
        return shelterData;
    }

    public static bool TryGetShelter(string shelterId, out ShelterData shelterData)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(shelterId))
        {
            shelterData = null;
            Debug.LogWarning($"{ShelterDataFileName} lookup skipped because shelterId was empty. Existing scene/Inspector shelter values will be preserved.");
            return false;
        }

        if (ShelterById.TryGetValue(shelterId, out ShelterData foundData))
        {
            shelterData = foundData.Clone();

            if (RuntimeShelterOverridesById.TryGetValue(shelterId, out ShelterData overrideData))
            {
                shelterData = overrideData.Clone();
            }

            return true;
        }

        shelterData = null;
        Debug.LogWarning($"Shelter ID '{shelterId}' was not found in {ShelterDataFileName}. Existing scene/Inspector shelter values will be preserved.");
        return false;
    }

    public static ShelterData[] GetAllShelters()
    {
        EnsureLoaded();

        var shelters = new List<ShelterData>();

        foreach (string shelterId in ShelterIdsInLoadOrder)
        {
            if (!ShelterById.TryGetValue(shelterId, out ShelterData shelterData))
            {
                continue;
            }

            ShelterData clone = shelterData.Clone();

            if (RuntimeShelterOverridesById.TryGetValue(shelterId, out ShelterData overrideData))
            {
                clone = overrideData.Clone();
            }

            shelters.Add(clone);
        }

        return shelters.ToArray();
    }

    public static void Reload()
    {
        hasLoaded = false;
        ShelterById.Clear();
        ShelterIdsInLoadOrder.Clear();
        RuntimeShelterIds.Clear();
        EnsureLoaded();
    }

    public static void ClearRuntimeOverrides()
    {
        RuntimeShelterOverridesById.Clear();
        runtimeOverrideSource = "scenario";
    }

    public static void ClearRuntimeShelters()
    {
        EnsureLoaded();

        foreach (string runtimeShelterId in RuntimeShelterIds)
        {
            ShelterById.Remove(runtimeShelterId);
            ShelterIdsInLoadOrder.Remove(runtimeShelterId);
            RuntimeShelterOverridesById.Remove(runtimeShelterId);
        }

        RuntimeShelterIds.Clear();
        runtimeShelterSource = "runtime";
    }

    public static void RegisterRuntimeShelters(ShelterData[] shelterDataList, string source)
    {
        if (shelterDataList == null || shelterDataList.Length == 0)
        {
            return;
        }

        EnsureLoaded();
        runtimeShelterSource = string.IsNullOrWhiteSpace(source) ? "runtime" : source;

        foreach (ShelterData shelterData in shelterDataList)
        {
            if (shelterData == null)
            {
                continue;
            }

            ShelterData clone = shelterData.Clone();
            if (string.IsNullOrWhiteSpace(clone.shelterId))
            {
                Debug.LogWarning($"Runtime shelter from '{runtimeShelterSource}' was skipped because shelterId was empty.");
                continue;
            }

            clone.Sanitize(runtimeShelterSource);

            if (!ShelterById.ContainsKey(clone.shelterId))
            {
                ShelterIdsInLoadOrder.Add(clone.shelterId);
            }

            ShelterById[clone.shelterId] = clone;
            RuntimeShelterIds.Add(clone.shelterId);
        }

        if (RuntimeShelterIds.Count > 0)
        {
            Debug.Log($"Registered {RuntimeShelterIds.Count} runtime shelter(s) from '{runtimeShelterSource}'.");
        }
    }

    public static void SetRuntimeOverrides(ShelterData[] shelterOverrides, string source)
    {
        ClearRuntimeOverrides();

        if (shelterOverrides == null || shelterOverrides.Length == 0)
        {
            return;
        }

        EnsureLoaded();
        runtimeOverrideSource = string.IsNullOrWhiteSpace(source) ? "scenario" : source;

        foreach (ShelterData shelterOverride in shelterOverrides)
        {
            if (shelterOverride == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(shelterOverride.shelterId))
            {
                Debug.LogWarning($"Scenario shelter override in '{runtimeOverrideSource}' was skipped because shelterId was empty.");
                continue;
            }

            ShelterData overrideClone = shelterOverride.Clone();
            overrideClone.Sanitize($"scenario '{runtimeOverrideSource}'");

            if (!ShelterById.ContainsKey(overrideClone.shelterId))
            {
                Debug.LogWarning(
                    $"Scenario shelter override '{overrideClone.shelterId}' from '{runtimeOverrideSource}' was skipped because it is not present in {ShelterDataFileName}.");
                continue;
            }

            RuntimeShelterOverridesById[overrideClone.shelterId] = overrideClone;
        }

        if (RuntimeShelterOverridesById.Count > 0)
        {
            Debug.Log($"Applied {RuntimeShelterOverridesById.Count} shelter override(s) from scenario '{runtimeOverrideSource}'.");
        }
    }

    private static void EnsureLoaded()
    {
        if (hasLoaded)
        {
            return;
        }

        hasLoaded = true;
        ShelterById.Clear();
        ShelterIdsInLoadOrder.Clear();

        string path = Application.dataPath + "/Data/" + ShelterDataFileName;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Missing shelter data at {path}. Existing scene/Inspector shelter values will be preserved.");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"Empty shelter data at {path}. Existing scene/Inspector shelter values will be preserved.");
                return;
            }

            ShelterDataList list = new ShelterDataList();
            JsonUtility.FromJsonOverwrite(json, list);

            if (list.shelters == null || list.shelters.Length == 0)
            {
                Debug.LogWarning($"{ShelterDataFileName} did not contain any shelters. Existing scene/Inspector shelter values will be preserved.");
                return;
            }

            foreach (ShelterData shelterData in list.shelters)
            {
                if (shelterData == null)
                {
                    continue;
                }

                shelterData.Sanitize(ShelterDataFileName);

                if (ShelterById.ContainsKey(shelterData.shelterId))
                {
                    Debug.LogWarning($"{ShelterDataFileName} contains duplicate shelterId '{shelterData.shelterId}'. The first entry will be used.");
                    continue;
                }

                ShelterById.Add(shelterData.shelterId, shelterData.Clone());
                ShelterIdsInLoadOrder.Add(shelterData.shelterId);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load shelter data from {path}. Existing scene/Inspector shelter values will be preserved. {exception.Message}");
            ShelterById.Clear();
            ShelterIdsInLoadOrder.Clear();
        }
    }
}
