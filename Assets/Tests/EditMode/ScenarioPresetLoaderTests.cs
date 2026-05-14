using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ScenarioPresetLoaderTests
{
    [TearDown]
    public void ClearShelterScenarioOverrides()
    {
        ShelterDataLoader.ClearRuntimeOverrides();
    }

    [Test]
    public void ScenarioPresetFileCanBeLoaded()
    {
        ScenarioPresetLoader.ScenarioPresetFile presetFile = ScenarioPresetLoader.LoadScenarioPresetFile();

        Assert.NotNull(presetFile);
        Assert.NotNull(presetFile.presets);
        Assert.IsTrue(ScenarioPresetLoader.TryFindPreset(
            presetFile,
            ScenarioPresetLoader.DefaultScenarioId,
            out ScenarioPresetLoader.ScenarioPreset defaultPreset));
        Assert.NotNull(defaultPreset);
    }

    [Test]
    public void DefaultScenarioIsSafe()
    {
        ScenarioPresetLoader.ActiveScenario activeScenario =
            ScenarioPresetLoader.ResolveActiveScenario(ScenarioPresetLoader.CreateDefaultScenarioPresetFile());

        Assert.NotNull(activeScenario);
        Assert.AreEqual(ScenarioPresetLoader.DefaultScenarioId, activeScenario.activeScenarioId);
        Assert.IsTrue(activeScenario.foundRequestedScenario);
        Assert.IsTrue(activeScenario.isDefaultScenario);
    }

    [Test]
    public void UnknownActiveScenarioFallsBackSafely()
    {
        ScenarioPresetLoader.ScenarioPresetFile presetFile = ScenarioPresetLoader.LoadScenarioPresetFile();
        presetFile.activeScenarioId = "unknown_scenario_for_test";

        LogAssert.Expect(LogType.Warning, new Regex("Scenario 'unknown_scenario_for_test' was not found.*Using default"));
        ScenarioPresetLoader.ActiveScenario activeScenario = ScenarioPresetLoader.ResolveActiveScenario(presetFile);

        Assert.NotNull(activeScenario);
        Assert.AreEqual(ScenarioPresetLoader.DefaultScenarioId, activeScenario.activeScenarioId);
        Assert.IsFalse(activeScenario.foundRequestedScenario);
        Assert.IsTrue(activeScenario.isDefaultScenario);
    }

    [Test]
    public void RandomWarningPresetHasValidMinMaxAfterValidation()
    {
        ScenarioPresetLoader.ActiveScenario activeScenario = ResolveScenarioFromProjectFile("random_warning");
        GameConfigLoader.TsunamiEventConfig tsunamiConfig = GameConfigLoader.CreateDefaultTsunamiEventConfig();
        GameConfigLoader.AntiCampingConfig antiCampingConfig = GameConfigLoader.CreateDefaultAntiCampingConfig();

        ScenarioPresetLoader.ApplyScenarioOverrides(activeScenario, tsunamiConfig, antiCampingConfig);

        Assert.IsTrue(tsunamiConfig.manualStartEnabled);
        Assert.IsTrue(tsunamiConfig.randomStartEnabled);
        Assert.GreaterOrEqual(tsunamiConfig.randomStartMinSeconds, 0f);
        Assert.GreaterOrEqual(tsunamiConfig.randomStartMaxSeconds, tsunamiConfig.randomStartMinSeconds);
    }

    [Test]
    public void AntiCampingPresetEnablesAntiCampingOnlyThroughScenarioOverride()
    {
        GameConfigLoader.AntiCampingConfig defaultConfig = GameConfigLoader.LoadAntiCampingConfig();
        Assert.IsFalse(defaultConfig.antiCampingEnabled);

        GameConfigLoader.TsunamiEventConfig tsunamiConfig = GameConfigLoader.CreateDefaultTsunamiEventConfig();
        GameConfigLoader.AntiCampingConfig scenarioConfig = GameConfigLoader.CreateDefaultAntiCampingConfig();

        ScenarioPresetLoader.ApplyScenarioOverrides(
            ResolveScenarioFromProjectFile("anti_camping"),
            tsunamiConfig,
            scenarioConfig);

        Assert.IsTrue(scenarioConfig.antiCampingEnabled);
        Assert.Greater(scenarioConfig.preWarningCampingThresholdSeconds, 0f);
        Assert.IsTrue(scenarioConfig.blockCampedShelterForRound);
    }

    [Test]
    public void ScenarioApplicationDoesNotRewriteBaseJsonFiles()
    {
        string tsunamiPath = GetDataPath("tsunami_event_config.json");
        string sheltersPath = GetDataPath("test_shelters.json");
        string antiCampingPath = GetDataPath("anti_camping_config.json");

        string tsunamiBefore = File.ReadAllText(tsunamiPath);
        string sheltersBefore = File.ReadAllText(sheltersPath);
        string antiCampingBefore = File.ReadAllText(antiCampingPath);

        ScenarioPresetLoader.ActiveScenario activeScenario = ResolveScenarioFromProjectFile("blocked_shelter");
        GameConfigLoader.TsunamiEventConfig tsunamiConfig = GameConfigLoader.LoadTsunamiEventConfig();
        GameConfigLoader.AntiCampingConfig antiCampingConfig = GameConfigLoader.LoadAntiCampingConfig();

        ScenarioPresetLoader.ApplyScenarioOverrides(activeScenario, tsunamiConfig, antiCampingConfig);
        ShelterDataLoader.SetRuntimeOverrides(
            ScenarioPresetLoader.GetShelterOverrides(activeScenario),
            activeScenario.activeScenarioId);

        Assert.AreEqual(tsunamiBefore, File.ReadAllText(tsunamiPath));
        Assert.AreEqual(sheltersBefore, File.ReadAllText(sheltersPath));
        Assert.AreEqual(antiCampingBefore, File.ReadAllText(antiCampingPath));
    }

    [Test]
    public void BlockedShelterScenarioOverridesKnownTestShelterInMemory()
    {
        ShelterDataLoader.Reload();
        Assert.IsTrue(ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData baseShelter));
        Assert.IsTrue(baseShelter.canEnter);

        ScenarioPresetLoader.ActiveScenario activeScenario = ResolveScenarioFromProjectFile("blocked_shelter");
        ShelterDataLoader.SetRuntimeOverrides(
            ScenarioPresetLoader.GetShelterOverrides(activeScenario),
            activeScenario.activeScenarioId);

        Assert.IsTrue(ShelterDataLoader.TryGetShelter("test_shelter_001", out ShelterDataLoader.ShelterData scenarioShelter));
        Assert.IsFalse(scenarioShelter.canEnter);
        Assert.IsTrue(scenarioShelter.failureReason.Contains("blocked_shelter"));
    }

    [Test]
    public void MissingScenarioPresetFileUsesDefaultBehavior()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing_scenario_presets_{Guid.NewGuid():N}.json");

        LogAssert.Expect(LogType.Warning, new Regex("Missing scenario presets.*Using default"));
        ScenarioPresetLoader.ActiveScenario activeScenario = ScenarioPresetLoader.LoadActiveScenarioFromPath(missingPath);

        Assert.NotNull(activeScenario);
        Assert.AreEqual(ScenarioPresetLoader.DefaultScenarioId, activeScenario.activeScenarioId);
        Assert.IsTrue(activeScenario.isDefaultScenario);
    }

    private static ScenarioPresetLoader.ActiveScenario ResolveScenarioFromProjectFile(string scenarioId)
    {
        ScenarioPresetLoader.ScenarioPresetFile presetFile = ScenarioPresetLoader.LoadScenarioPresetFile();
        presetFile.activeScenarioId = scenarioId;
        return ScenarioPresetLoader.ResolveActiveScenario(presetFile);
    }

    private static string GetDataPath(string fileName)
    {
        return Path.Combine(Application.dataPath, "Data", fileName);
    }
}
