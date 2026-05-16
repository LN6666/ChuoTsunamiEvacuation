using System;
using System.IO;
using UnityEngine;

public static class ShelterSourceConfigLoader
{
    public const string TestSourceMode = "test";
    public const string RealSampleSourceMode = "real_sample";

    private const string ShelterSourceConfigFileName = "shelter_source_config.json";

    [Serializable]
    public class ShelterSourceConfig
    {
        public string sourceMode = TestSourceMode;
        public string realSamplePath = "Assets/Data/real_chuo_shelters_sample.json";
        public bool enableRealSampleLoading;
        public string notes = "P4 hook only. Current gameplay uses test_shelters.json.";

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(sourceMode))
            {
                Debug.LogWarning($"{ShelterSourceConfigFileName} had an empty sourceMode. Using test mode.");
                sourceMode = TestSourceMode;
            }

            sourceMode = sourceMode.Trim();

            if (!string.Equals(sourceMode, TestSourceMode, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceMode, RealSampleSourceMode, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"{ShelterSourceConfigFileName} had unknown sourceMode '{sourceMode}'. Using test mode.");
                sourceMode = TestSourceMode;
            }

            if (string.IsNullOrWhiteSpace(realSamplePath))
            {
                realSamplePath = "Assets/Data/real_chuo_shelters_sample.json";
            }

            if (!string.Equals(sourceMode, TestSourceMode, StringComparison.OrdinalIgnoreCase) || enableRealSampleLoading)
            {
                Debug.LogWarning(
                    $"{ShelterSourceConfigFileName} real-data source mode is a P4 hook only. " +
                    "P2 gameplay continues to load test_shelters.json.");
            }
        }
    }

    public static ShelterSourceConfig Load()
    {
        return LoadFromPath(GetDataPath(ShelterSourceConfigFileName));
    }

    public static ShelterSourceConfig LoadFromPath(string path)
    {
        ShelterSourceConfig config = CreateDefaultConfig();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Missing shelter source config at {path}. Using test source mode.");
            return config;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"Empty shelter source config at {path}. Using test source mode.");
                return config;
            }

            JsonUtility.FromJsonOverwrite(json, config);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load shelter source config from {path}. Using test source mode. {exception.Message}");
            return CreateDefaultConfig();
        }

        config.Sanitize();
        return config;
    }

    public static ShelterSourceConfig CreateDefaultConfig()
    {
        return new ShelterSourceConfig
        {
            sourceMode = TestSourceMode,
            realSamplePath = "Assets/Data/real_chuo_shelters_sample.json",
            enableRealSampleLoading = false,
            notes = "P4 hook only. Current gameplay uses test_shelters.json."
        };
    }

    private static string GetDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
