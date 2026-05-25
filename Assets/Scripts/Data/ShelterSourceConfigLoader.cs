using System;
using System.IO;
using UnityEngine;

public static class ShelterSourceConfigLoader
{
    public const string TestSourceMode = "test";
    public const string RealSampleSourceMode = "real_sample";
    public const string RealQualifiedSourceMode = "real_qualified";

    private const string ShelterSourceConfigFileName = "shelter_source_config.json";

    [Serializable]
    public class ShelterSourceConfig
    {
        public string sourceMode = TestSourceMode;
        public string testSheltersPath = "test_shelters.json";
        public string realSamplePath = "real_chuo_shelters_sample.json";
        public bool fallbackToTestOnError = true;
        public bool enableRealSampleLoading;
        public bool enableP5COverlay;
        public bool enableHumanitarianCandidates;
        public bool enableLifeFirstCandidateSelection;
        public string notes = "P4/P5 source selection. Default gameplay source remains test.";

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(sourceMode))
            {
                Debug.LogWarning($"{ShelterSourceConfigFileName} had an empty sourceMode. Using test mode.");
                sourceMode = TestSourceMode;
            }

            sourceMode = sourceMode.Trim();

            if (!string.Equals(sourceMode, TestSourceMode, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceMode, RealSampleSourceMode, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sourceMode, RealQualifiedSourceMode, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"{ShelterSourceConfigFileName} had unknown sourceMode '{sourceMode}'. Using test mode.");
                sourceMode = TestSourceMode;
            }

            if (string.IsNullOrWhiteSpace(realSamplePath))
            {
                realSamplePath = "real_chuo_shelters_sample.json";
            }

            if (string.IsNullOrWhiteSpace(testSheltersPath))
            {
                testSheltersPath = "test_shelters.json";
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
            testSheltersPath = "test_shelters.json",
            realSamplePath = "real_chuo_shelters_sample.json",
            fallbackToTestOnError = true,
            enableRealSampleLoading = false,
            enableP5COverlay = false,
            enableHumanitarianCandidates = false,
            enableLifeFirstCandidateSelection = false,
            notes = "P4/P5 source selection. Default gameplay source remains test."
        };
    }

    private static string GetDataPath(string fileName)
    {
        return RuntimeDataPathResolver.GetDataPath(fileName);
    }
}
