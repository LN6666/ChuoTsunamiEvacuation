using System;
using System.IO;
using UnityEngine;

public static class ScenarioPresetLoader
{
    public const string DefaultScenarioId = "default";
    private const string ScenarioPresetFileName = "scenario_presets.json";

    [Serializable]
    public class ScenarioPresetFile
    {
        public string activeScenarioId = DefaultScenarioId;
        public ScenarioPreset[] presets = new ScenarioPreset[0];
    }

    [Serializable]
    public class ScenarioPreset
    {
        public string scenarioId = DefaultScenarioId;
        public string displayName = "Default";
        public string description = "Use base JSON config files without scenario overrides.";
        public bool overrideTsunamiEventConfig;
        public GameConfigLoader.TsunamiEventConfig tsunamiEventConfig = GameConfigLoader.CreateDefaultTsunamiEventConfig();
        public bool overrideAntiCampingConfig;
        public GameConfigLoader.AntiCampingConfig antiCampingConfig = GameConfigLoader.CreateDefaultAntiCampingConfig();
        public ShelterDataLoader.ShelterData[] shelterOverrides = new ShelterDataLoader.ShelterData[0];

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                Debug.LogWarning($"{ScenarioPresetFileName} contained a scenario with no scenarioId. Using '{DefaultScenarioId}'.");
                scenarioId = DefaultScenarioId;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = scenarioId;
            }

            if (tsunamiEventConfig == null)
            {
                tsunamiEventConfig = GameConfigLoader.CreateDefaultTsunamiEventConfig();
            }

            if (antiCampingConfig == null)
            {
                antiCampingConfig = GameConfigLoader.CreateDefaultAntiCampingConfig();
            }

            if (overrideTsunamiEventConfig)
            {
                tsunamiEventConfig.Sanitize();
            }

            if (overrideAntiCampingConfig)
            {
                antiCampingConfig.Sanitize();
            }

            if (shelterOverrides == null)
            {
                shelterOverrides = new ShelterDataLoader.ShelterData[0];
                return;
            }

            foreach (ShelterDataLoader.ShelterData shelterOverride in shelterOverrides)
            {
                shelterOverride?.Sanitize($"{ScenarioPresetFileName} scenario '{scenarioId}'");
            }
        }
    }

    public class ActiveScenario
    {
        public string requestedScenarioId = DefaultScenarioId;
        public string activeScenarioId = DefaultScenarioId;
        public string displayName = "Default";
        public string description = "Use base JSON config files without scenario overrides.";
        public bool foundRequestedScenario = true;
        public bool isDefaultScenario = true;
        public ScenarioPreset preset;
    }

    public static ScenarioPresetFile LoadScenarioPresetFile()
    {
        return LoadScenarioPresetFileFromPath(GetDataPath(ScenarioPresetFileName));
    }

    public static ScenarioPresetFile LoadScenarioPresetFileFromPath(string path)
    {
        ScenarioPresetFile presetFile = CreateDefaultScenarioPresetFile();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Missing scenario presets at {path}. Using default scenario behavior.");
            return presetFile;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"Empty scenario presets at {path}. Using default scenario behavior.");
                return presetFile;
            }

            JsonUtility.FromJsonOverwrite(json, presetFile);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load scenario presets from {path}. Using default scenario behavior. {exception.Message}");
            return CreateDefaultScenarioPresetFile();
        }

        SanitizePresetFile(presetFile);
        return presetFile;
    }

    public static ActiveScenario LoadActiveScenario()
    {
        return ResolveActiveScenario(LoadScenarioPresetFile());
    }

    public static ActiveScenario LoadActiveScenarioFromPath(string path)
    {
        return ResolveActiveScenario(LoadScenarioPresetFileFromPath(path));
    }

    public static ActiveScenario ResolveActiveScenario(ScenarioPresetFile presetFile)
    {
        if (presetFile == null)
        {
            Debug.LogWarning("Scenario preset data was null. Using default scenario behavior.");
            presetFile = CreateDefaultScenarioPresetFile();
        }

        SanitizePresetFile(presetFile);

        string requestedId = string.IsNullOrWhiteSpace(presetFile.activeScenarioId)
            ? DefaultScenarioId
            : presetFile.activeScenarioId.Trim();

        if (TryFindPreset(presetFile, requestedId, out ScenarioPreset requestedPreset))
        {
            return CreateActiveScenario(requestedId, requestedPreset, true);
        }

        Debug.LogWarning($"Scenario '{requestedId}' was not found in {ScenarioPresetFileName}. Using default scenario behavior.");

        if (TryFindPreset(presetFile, DefaultScenarioId, out ScenarioPreset defaultPreset))
        {
            return CreateActiveScenario(requestedId, defaultPreset, false);
        }

        return CreateActiveScenario(requestedId, CreateDefaultScenarioPreset(), false);
    }

    public static bool TryFindPreset(ScenarioPresetFile presetFile, string scenarioId, out ScenarioPreset preset)
    {
        preset = null;

        if (presetFile == null || presetFile.presets == null || string.IsNullOrWhiteSpace(scenarioId))
        {
            return false;
        }

        foreach (ScenarioPreset candidate in presetFile.presets)
        {
            if (candidate == null)
            {
                continue;
            }

            if (string.Equals(candidate.scenarioId, scenarioId, StringComparison.OrdinalIgnoreCase))
            {
                preset = candidate;
                return true;
            }
        }

        return false;
    }

    public static void ApplyScenarioOverrides(
        ActiveScenario activeScenario,
        GameConfigLoader.TsunamiEventConfig tsunamiEventConfig,
        GameConfigLoader.AntiCampingConfig antiCampingConfig)
    {
        if (activeScenario == null || activeScenario.preset == null || activeScenario.isDefaultScenario)
        {
            return;
        }

        ScenarioPreset preset = activeScenario.preset;

        if (preset.overrideTsunamiEventConfig)
        {
            if (tsunamiEventConfig == null)
            {
                Debug.LogWarning($"Scenario '{preset.scenarioId}' could not apply tsunami overrides because the runtime config was null.");
            }
            else
            {
                CopyTsunamiConfig(preset.tsunamiEventConfig, tsunamiEventConfig);
                tsunamiEventConfig.Sanitize();
                Debug.Log($"Scenario '{preset.scenarioId}' applied tsunami event overrides in memory.");
            }
        }

        if (preset.overrideAntiCampingConfig)
        {
            if (antiCampingConfig == null)
            {
                Debug.LogWarning($"Scenario '{preset.scenarioId}' could not apply anti-camping overrides because the runtime config was null.");
            }
            else
            {
                CopyAntiCampingConfig(preset.antiCampingConfig, antiCampingConfig);
                antiCampingConfig.Sanitize();
                Debug.Log($"Scenario '{preset.scenarioId}' applied anti-camping overrides in memory.");
            }
        }
    }

    public static ShelterDataLoader.ShelterData[] GetShelterOverrides(ActiveScenario activeScenario)
    {
        if (activeScenario == null ||
            activeScenario.preset == null ||
            activeScenario.preset.shelterOverrides == null ||
            activeScenario.preset.shelterOverrides.Length == 0)
        {
            return new ShelterDataLoader.ShelterData[0];
        }

        ShelterDataLoader.ShelterData[] clones = new ShelterDataLoader.ShelterData[activeScenario.preset.shelterOverrides.Length];

        for (int i = 0; i < activeScenario.preset.shelterOverrides.Length; i++)
        {
            clones[i] = activeScenario.preset.shelterOverrides[i]?.Clone();
        }

        return clones;
    }

    public static ScenarioPresetFile CreateDefaultScenarioPresetFile()
    {
        return new ScenarioPresetFile
        {
            activeScenarioId = DefaultScenarioId,
            presets = new[] { CreateDefaultScenarioPreset() }
        };
    }

    private static ScenarioPreset CreateDefaultScenarioPreset()
    {
        return new ScenarioPreset
        {
            scenarioId = DefaultScenarioId,
            displayName = "Default",
            description = "Use base JSON config files without scenario overrides.",
            overrideTsunamiEventConfig = false,
            overrideAntiCampingConfig = false,
            shelterOverrides = new ShelterDataLoader.ShelterData[0]
        };
    }

    private static ActiveScenario CreateActiveScenario(string requestedId, ScenarioPreset preset, bool foundRequested)
    {
        if (preset == null)
        {
            preset = CreateDefaultScenarioPreset();
        }

        preset.Sanitize();

        bool isDefault = string.Equals(preset.scenarioId, DefaultScenarioId, StringComparison.OrdinalIgnoreCase);
        return new ActiveScenario
        {
            requestedScenarioId = string.IsNullOrWhiteSpace(requestedId) ? DefaultScenarioId : requestedId,
            activeScenarioId = preset.scenarioId,
            displayName = preset.displayName,
            description = preset.description,
            foundRequestedScenario = foundRequested,
            isDefaultScenario = isDefault,
            preset = preset
        };
    }

    private static void SanitizePresetFile(ScenarioPresetFile presetFile)
    {
        if (presetFile == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(presetFile.activeScenarioId))
        {
            presetFile.activeScenarioId = DefaultScenarioId;
        }

        if (presetFile.presets == null || presetFile.presets.Length == 0)
        {
            Debug.LogWarning($"{ScenarioPresetFileName} had no presets. Using default scenario behavior.");
            presetFile.presets = new[] { CreateDefaultScenarioPreset() };
        }

        foreach (ScenarioPreset preset in presetFile.presets)
        {
            preset?.Sanitize();
        }
    }

    private static void CopyTsunamiConfig(GameConfigLoader.TsunamiEventConfig source, GameConfigLoader.TsunamiEventConfig target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.manualStartEnabled = source.manualStartEnabled;
        target.randomStartEnabled = source.randomStartEnabled;
        target.randomStartMinSeconds = source.randomStartMinSeconds;
        target.randomStartMaxSeconds = source.randomStartMaxSeconds;
        target.evacuationCountdownSeconds = source.evacuationCountdownSeconds;
        target.wallMoveDurationSeconds = source.wallMoveDurationSeconds;
        target.warningMessage = source.warningMessage;
    }

    private static void CopyAntiCampingConfig(GameConfigLoader.AntiCampingConfig source, GameConfigLoader.AntiCampingConfig target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.antiCampingEnabled = source.antiCampingEnabled;
        target.preWarningCampingThresholdSeconds = source.preWarningCampingThresholdSeconds;
        target.blockCampedShelterForRound = source.blockCampedShelterForRound;
    }

    private static string GetDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
