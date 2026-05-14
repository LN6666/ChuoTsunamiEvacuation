using System;
using System.IO;
using UnityEngine;

public static class GameConfigLoader
{
    private const string TsunamiEventConfigFileName = "tsunami_event_config.json";
    private const string AntiCampingConfigFileName = "anti_camping_config.json";

    [Serializable]
    public class TsunamiEventConfig
    {
        public bool manualStartEnabled = true;
        public bool randomStartEnabled;
        public float randomStartMinSeconds = 30f;
        public float randomStartMaxSeconds = 90f;
        public float evacuationCountdownSeconds = 60f;
        public float wallMoveDurationSeconds = 60f;
        public string warningMessage = "Tsunami warning issued. Evacuate to a safe building.";

        public void Sanitize()
        {
            randomStartMinSeconds = Mathf.Max(0f, randomStartMinSeconds);
            randomStartMaxSeconds = Mathf.Max(0f, randomStartMaxSeconds);

            if (randomStartMaxSeconds < randomStartMinSeconds)
            {
                Debug.LogWarning(
                    $"{TsunamiEventConfigFileName} had randomStartMaxSeconds lower than randomStartMinSeconds. " +
                    "Using the minimum value for both.");
                randomStartMaxSeconds = randomStartMinSeconds;
            }

            if (evacuationCountdownSeconds <= 0f)
            {
                Debug.LogWarning($"{TsunamiEventConfigFileName} had invalid evacuationCountdownSeconds. Using 60 seconds.");
                evacuationCountdownSeconds = 60f;
            }

            if (wallMoveDurationSeconds <= 0f)
            {
                Debug.LogWarning($"{TsunamiEventConfigFileName} had invalid wallMoveDurationSeconds. Using 60 seconds.");
                wallMoveDurationSeconds = 60f;
            }

            if (string.IsNullOrWhiteSpace(warningMessage))
            {
                Debug.LogWarning($"{TsunamiEventConfigFileName} had an empty warningMessage. Using the default warning message.");
                warningMessage = "Tsunami warning issued. Evacuate to a safe building.";
            }

            if (!manualStartEnabled && !randomStartEnabled)
            {
                Debug.LogWarning(
                    $"{TsunamiEventConfigFileName} has both manualStartEnabled and randomStartEnabled disabled. " +
                    "The tsunami warning will wait indefinitely until config is changed.");
            }
        }
    }

    [Serializable]
    public class AntiCampingConfig
    {
        public bool antiCampingEnabled;
        public float preWarningCampingThresholdSeconds = 10f;
        public bool blockCampedShelterForRound = true;

        public void Sanitize()
        {
            if (preWarningCampingThresholdSeconds <= 0f)
            {
                Debug.LogWarning($"{AntiCampingConfigFileName} had invalid preWarningCampingThresholdSeconds. Using 10 seconds.");
                preWarningCampingThresholdSeconds = 10f;
            }
        }
    }

    public static TsunamiEventConfig LoadTsunamiEventConfig()
    {
        return LoadTsunamiEventConfigFromPath(GetDataPath(TsunamiEventConfigFileName));
    }

    public static TsunamiEventConfig LoadTsunamiEventConfigFromPath(string path)
    {
        TsunamiEventConfig config = CreateDefaultTsunamiEventConfig();
        LoadJsonInto(config, path, "tsunami event config");
        config.Sanitize();
        return config;
    }

    public static AntiCampingConfig LoadAntiCampingConfig()
    {
        return LoadAntiCampingConfigFromPath(GetDataPath(AntiCampingConfigFileName));
    }

    public static AntiCampingConfig LoadAntiCampingConfigFromPath(string path)
    {
        AntiCampingConfig config = CreateDefaultAntiCampingConfig();
        LoadJsonInto(config, path, "anti-camping config");
        config.Sanitize();
        return config;
    }

    public static TsunamiEventConfig CreateDefaultTsunamiEventConfig()
    {
        return new TsunamiEventConfig
        {
            manualStartEnabled = true,
            randomStartEnabled = false,
            randomStartMinSeconds = 30f,
            randomStartMaxSeconds = 90f,
            evacuationCountdownSeconds = 60f,
            wallMoveDurationSeconds = 60f,
            warningMessage = "Tsunami warning issued. Evacuate to a safe building."
        };
    }

    public static AntiCampingConfig CreateDefaultAntiCampingConfig()
    {
        return new AntiCampingConfig
        {
            antiCampingEnabled = false,
            preWarningCampingThresholdSeconds = 10f,
            blockCampedShelterForRound = true
        };
    }

    private static void LoadJsonInto(object target, string path, string label)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Missing {label} at {path}. Safe defaults will be used.");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"Empty {label} at {path}. Safe defaults will be used.");
                return;
            }

            JsonUtility.FromJsonOverwrite(json, target);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load {label} from {path}. Safe defaults will be used. {exception.Message}");
        }
    }

    private static string GetDataPath(string fileName)
    {
        return Application.dataPath + "/Data/" + fileName;
    }
}
