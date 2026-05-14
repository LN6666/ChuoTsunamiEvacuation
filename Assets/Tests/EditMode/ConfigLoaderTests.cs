using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConfigLoaderTests
{
    [Test]
    public void TsunamiEventConfigFileCanBeLoaded()
    {
        GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfig();

        Assert.NotNull(config);
        Assert.IsTrue(config.manualStartEnabled);
        Assert.IsFalse(config.randomStartEnabled);
        Assert.Greater(config.evacuationCountdownSeconds, 0f);
        Assert.Greater(config.wallMoveDurationSeconds, 0f);
        Assert.IsFalse(string.IsNullOrWhiteSpace(config.warningMessage));
    }

    [Test]
    public void MissingTsunamiEventConfigReturnsSafeDefaults()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing_tsunami_config_{Guid.NewGuid():N}.json");

        LogAssert.Expect(LogType.Warning, new Regex("Missing tsunami event config.*Safe defaults"));
        GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(missingPath);

        AssertSafeTsunamiDefaults(config);
    }

    [Test]
    public void InvalidTsunamiEventConfigReturnsSafeDefaults()
    {
        string invalidPath = CreateTempJson("{ invalid json");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("Could not load tsunami event config.*Safe defaults"));
            GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(invalidPath);

            AssertSafeTsunamiDefaults(config);
        }
        finally
        {
            DeleteTempFile(invalidPath);
        }
    }

    [Test]
    public void PartialTsunamiEventConfigSanitizesUnsafeValues()
    {
        string partialPath = CreateTempJson(
            "{\"evacuationCountdownSeconds\":0,\"wallMoveDurationSeconds\":-5,\"warningMessage\":\"\"}");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("invalid evacuationCountdownSeconds"));
            LogAssert.Expect(LogType.Warning, new Regex("invalid wallMoveDurationSeconds"));
            LogAssert.Expect(LogType.Warning, new Regex("empty warningMessage"));

            GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(partialPath);

            Assert.NotNull(config);
            Assert.IsTrue(config.manualStartEnabled);
            Assert.IsFalse(config.randomStartEnabled);
            Assert.Greater(config.evacuationCountdownSeconds, 0f);
            Assert.Greater(config.wallMoveDurationSeconds, 0f);
            Assert.IsFalse(string.IsNullOrWhiteSpace(config.warningMessage));
        }
        finally
        {
            DeleteTempFile(partialPath);
        }
    }

    [Test]
    public void NegativeRandomStartValuesAreClampedToZero()
    {
        string path = CreateTempJson(
            "{\"randomStartEnabled\":true,\"randomStartMinSeconds\":-5,\"randomStartMaxSeconds\":-1}");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("negative randomStartMinSeconds"));
            LogAssert.Expect(LogType.Warning, new Regex("negative randomStartMaxSeconds"));

            GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(path);

            Assert.NotNull(config);
            Assert.AreEqual(0f, config.randomStartMinSeconds);
            Assert.AreEqual(0f, config.randomStartMaxSeconds);
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [Test]
    public void RandomStartMinGreaterThanMaxIsCorrected()
    {
        string path = CreateTempJson(
            "{\"randomStartEnabled\":true,\"randomStartMinSeconds\":10,\"randomStartMaxSeconds\":2}");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("randomStartMaxSeconds lower than randomStartMinSeconds"));

            GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(path);

            Assert.NotNull(config);
            Assert.AreEqual(10f, config.randomStartMinSeconds);
            Assert.AreEqual(10f, config.randomStartMaxSeconds);
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    [Test]
    public void DisabledManualAndRandomStartEnablesManualFallback()
    {
        string path = CreateTempJson(
            "{\"manualStartEnabled\":false,\"randomStartEnabled\":false}");

        try
        {
            LogAssert.Expect(LogType.Warning, new Regex("manualStartEnabled and randomStartEnabled disabled.*Enabling manual debug start"));

            GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(path);

            Assert.NotNull(config);
            Assert.IsTrue(config.manualStartEnabled);
            Assert.IsFalse(config.randomStartEnabled);
        }
        finally
        {
            DeleteTempFile(path);
        }
    }

    private static void AssertSafeTsunamiDefaults(GameConfigLoader.TsunamiEventConfig config)
    {
        Assert.NotNull(config);
        Assert.IsTrue(config.manualStartEnabled);
        Assert.IsFalse(config.randomStartEnabled);
        Assert.AreEqual(30f, config.randomStartMinSeconds);
        Assert.AreEqual(90f, config.randomStartMaxSeconds);
        Assert.AreEqual(60f, config.evacuationCountdownSeconds);
        Assert.AreEqual(60f, config.wallMoveDurationSeconds);
        Assert.AreEqual("Tsunami warning issued. Evacuate to a safe building.", config.warningMessage);
    }

    private static string CreateTempJson(string json)
    {
        string path = Path.Combine(Path.GetTempPath(), $"chuo_test_config_{Guid.NewGuid():N}.json");
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
