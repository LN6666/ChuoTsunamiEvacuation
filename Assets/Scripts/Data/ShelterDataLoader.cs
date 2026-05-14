using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ShelterDataLoader
{
    private const string ShelterDataFileName = "test_shelters.json";

    private static readonly Dictionary<string, ShelterData> ShelterById = new Dictionary<string, ShelterData>();
    private static bool hasLoaded;

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
                failureReason = failureReason
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
            return true;
        }

        shelterData = null;
        Debug.LogWarning($"Shelter ID '{shelterId}' was not found in {ShelterDataFileName}. Existing scene/Inspector shelter values will be preserved.");
        return false;
    }

    public static void Reload()
    {
        hasLoaded = false;
        ShelterById.Clear();
        EnsureLoaded();
    }

    private static void EnsureLoaded()
    {
        if (hasLoaded)
        {
            return;
        }

        hasLoaded = true;
        ShelterById.Clear();

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
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load shelter data from {path}. Existing scene/Inspector shelter values will be preserved. {exception.Message}");
            ShelterById.Clear();
        }
    }
}
