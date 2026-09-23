using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ShelterSourceConfigLoaderTests
{
    [Test]
    public void ShelterSourceModeDefaultsToTest()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config =
            ShelterSourceConfigLoader.CreateDefaultConfig();

        Assert.NotNull(config);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableRealSampleLoading);
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);
    }

    [Test]
    public void ShelterSourceConfigFileCanBeLoaded()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.Load();

        Assert.NotNull(config);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableRealSampleLoading);
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);
    }

    [Test]
    public void MissingShelterSourceConfigUsesTestMode()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing_shelter_source_{Guid.NewGuid():N}.json");

        LogAssert.Expect(LogType.Warning, new Regex("Missing shelter source config.*Using test source mode"));
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.LoadFromPath(missingPath);

        Assert.NotNull(config);
        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.IsFalse(config.enableRealSampleLoading);
        Assert.IsFalse(config.enableHumanitarianCandidates);
        Assert.IsFalse(config.enableLifeFirstCandidateSelection);
    }

    [Test]
    public void UnknownShelterSourceModeFallsBackToTest()
    {
        string path = CreateTempJson("{\"sourceMode\":\"unknown_source_mode\"}");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("unknown sourceMode.*Using test mode"));
            ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.LoadFromPath(path);

            Assert.NotNull(config);
            Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [Test]
    public void RealDataHookDoesNotAffectCurrentTestShelterLoading()
    {
        ShelterSourceConfigLoader.ShelterSourceConfig config = ShelterSourceConfigLoader.Load();
        ShelterDataLoader.Reload();
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();

        Assert.AreEqual(ShelterSourceConfigLoader.TestSourceMode, config.sourceMode);
        Assert.GreaterOrEqual(shelters.Length, 5);
        Assert.IsTrue(ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData shelter));
        Assert.AreEqual("test", shelter.sourceType);
    }

    private static string CreateTempJson(string json)
    {
        string path = Path.Combine(Path.GetTempPath(), $"chuo_shelter_source_test_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static void DeleteTempFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
