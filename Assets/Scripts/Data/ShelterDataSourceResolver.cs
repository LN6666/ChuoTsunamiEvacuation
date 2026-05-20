using System;
using System.IO;
using UnityEngine;

public static class ShelterDataSourceResolver
{
    public class ShelterDataSourceResult
    {
        public string sourceMode = ShelterSourceConfigLoader.TestSourceMode;
        public bool success;
        public bool usedFallbackToTest;
        public string sourcePath = string.Empty;
        public ShelterDataLoader.ShelterData[] testShelters = new ShelterDataLoader.ShelterData[0];
        public RealShelterDataLoader.RealShelterRecord[] realShelters = new RealShelterDataLoader.RealShelterRecord[0];
        public RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[] realQualifiedShelters =
            new RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[0];
        public RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult realQualifiedLoadResult;

        public bool IsRealSample => string.Equals(
            sourceMode,
            ShelterSourceConfigLoader.RealSampleSourceMode,
            StringComparison.OrdinalIgnoreCase);

        public bool IsRealQualified => string.Equals(
            sourceMode,
            ShelterSourceConfigLoader.RealQualifiedSourceMode,
            StringComparison.OrdinalIgnoreCase);
    }

    public static ShelterDataSourceResult LoadConfiguredSource()
    {
        return LoadFromConfig(ShelterSourceConfigLoader.Load(), GetAssetsDataDirectory());
    }

    public static ShelterDataSourceResult LoadFromConfig(ShelterSourceConfigLoader.ShelterSourceConfig config)
    {
        return LoadFromConfig(config, GetAssetsDataDirectory());
    }

    public static ShelterDataSourceResult LoadFromConfig(
        ShelterSourceConfigLoader.ShelterSourceConfig config,
        string assetsDataDirectory)
    {
        if (config == null)
        {
            Debug.LogWarning("Shelter source config was null. Using test source mode.");
            config = ShelterSourceConfigLoader.CreateDefaultConfig();
        }

        config.Sanitize();

        if (string.Equals(config.sourceMode, ShelterSourceConfigLoader.RealSampleSourceMode, StringComparison.OrdinalIgnoreCase))
        {
            return LoadRealSample(config, assetsDataDirectory);
        }

        if (string.Equals(config.sourceMode, ShelterSourceConfigLoader.RealQualifiedSourceMode, StringComparison.OrdinalIgnoreCase))
        {
            return LoadRealQualified(config);
        }

        return LoadTestSource(config);
    }

    private static ShelterDataSourceResult LoadRealSample(
        ShelterSourceConfigLoader.ShelterSourceConfig config,
        string assetsDataDirectory)
    {
        var result = new ShelterDataSourceResult
        {
            sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode
        };

        string path = ResolveAssetsDataPath(config.realSamplePath, assetsDataDirectory);
        result.sourcePath = path;

        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning("Real shelter sample path was invalid. No real shelter records were loaded.");
            return config.fallbackToTestOnError ? FallbackToTest(result) : result;
        }

        RealShelterDataLoader.RealShelterLoadResult realLoad = RealShelterDataLoader.LoadFromPath(path);
        result.realShelters = realLoad.shelters ?? new RealShelterDataLoader.RealShelterRecord[0];
        result.success = realLoad.success;

        if (result.success)
        {
            return result;
        }

        return config.fallbackToTestOnError ? FallbackToTest(result) : result;
    }

    private static ShelterDataSourceResult LoadRealQualified(ShelterSourceConfigLoader.ShelterSourceConfig config)
    {
        var result = new ShelterDataSourceResult
        {
            sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode
        };

        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult realLoad =
            RealQualifiedShelterDataLoader.LoadFromAssetsData();
        result.realQualifiedLoadResult = realLoad;
        result.realQualifiedShelters = realLoad.records ??
            new RealQualifiedShelterDataLoader.RealQualifiedShelterRecord[0];
        result.sourcePath = realLoad.sourcePath;
        result.success = realLoad.success;

        if (result.success)
        {
            return result;
        }

        return config.fallbackToTestOnError ? FallbackToTest(result) : result;
    }

    private static ShelterDataSourceResult LoadTestSource(ShelterSourceConfigLoader.ShelterSourceConfig config)
    {
        var result = new ShelterDataSourceResult
        {
            sourceMode = ShelterSourceConfigLoader.TestSourceMode,
            sourcePath = ResolveAssetsDataPath(config.testSheltersPath, GetAssetsDataDirectory())
        };

        ShelterDataLoader.Reload();
        result.testShelters = ShelterDataLoader.GetAllShelters();
        result.success = result.testShelters != null && result.testShelters.Length > 0;
        return result;
    }

    private static ShelterDataSourceResult FallbackToTest(ShelterDataSourceResult failedRealResult)
    {
        Debug.LogWarning("Falling back to test shelter data after configured real shelter source loading failed.");

        ShelterDataSourceResult fallback = LoadTestSource(ShelterSourceConfigLoader.CreateDefaultConfig());
        fallback.sourceMode = ShelterSourceConfigLoader.TestSourceMode;
        fallback.usedFallbackToTest = true;
        return fallback;
    }

    private static string ResolveAssetsDataPath(string configuredPath, string assetsDataDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return string.Empty;
        }

        string normalizedPath = configuredPath.Replace('\\', '/').Trim();

        if (normalizedPath.IndexOf("data_pipeline/", StringComparison.OrdinalIgnoreCase) >= 0 ||
            normalizedPath.IndexOf("data_pipeline\\", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Debug.LogWarning("Unity shelter loading must use Assets/Data copies, not data_pipeline paths.");
            return string.Empty;
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        if (normalizedPath.StartsWith("Assets/Data/", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(assetsDataDirectory, normalizedPath.Substring("Assets/Data/".Length));
        }

        return Path.Combine(assetsDataDirectory, configuredPath);
    }

    private static string GetAssetsDataDirectory()
    {
        return Application.dataPath + "/Data";
    }
}
