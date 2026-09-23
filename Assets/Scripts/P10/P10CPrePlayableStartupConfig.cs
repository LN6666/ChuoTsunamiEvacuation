using System;
using System.IO;

[Serializable]
public class P10CPrePlayableStartupConfig
{
    public string schemaVersion = "p10c_pre.playable_startup_config.v1";
    public bool playableStartupMode = true;
    public bool startMenuVisibleOnLaunch = true;
    public bool languageSelectorVisible = true;
    public bool rulesUiAccessible = true;
    public bool startGameLoadsHighDetailScene = true;
    public bool generateRuntimeGameplayBootstrap = true;
    public bool createFallbackGroundWhenNoSceneCollider = true;
    public bool showDiagnosticsOnFailure = true;
    public bool runActualP7SceneIntegrationDiagnostics = true;
    public bool enableProfilingExporterByDefault;
    public bool enableProfilingAutoQuit;
    public float profilingDurationSeconds = 600f;
    public string targetHighDetailScenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public string targetHighDetailSceneName = "P7_HighDetail_Chuo";
    public string p7SourceScenePath = @"D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity";
    public string p7SourceSceneMetaPath = @"D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity.meta";
    public string p9TargetScenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public int minimumRenderableSceneRendererCount = 1;
    public long minimumActualHighDetailSceneBytes = 1024L * 1024L * 1024L;

    public bool IsDefaultPlayable()
    {
        return playableStartupMode &&
            startMenuVisibleOnLaunch &&
            languageSelectorVisible &&
            rulesUiAccessible &&
            startGameLoadsHighDetailScene &&
            generateRuntimeGameplayBootstrap &&
            showDiagnosticsOnFailure &&
            runActualP7SceneIntegrationDiagnostics &&
            !enableProfilingExporterByDefault &&
            !enableProfilingAutoQuit &&
            !string.IsNullOrWhiteSpace(targetHighDetailScenePath) &&
            !string.IsNullOrWhiteSpace(targetHighDetailSceneName);
    }
}

public static class P10CPreHighDetailSceneLocator
{
    public const string SourceSceneEnvironmentVariable = "P10C_PRE_P7_SOURCE_SCENE";
    public const string SourceSceneMetaEnvironmentVariable = "P10C_PRE_P7_SOURCE_SCENE_META";
    public const string DefaultSourceScenePath = @"D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity";
    public const string DefaultSourceSceneMetaPath = @"D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity.meta";
    public const string TargetScenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public const string TargetSceneMetaPath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity.meta";
    public const long MinimumActualHighDetailSceneBytes = 1024L * 1024L * 1024L;

    public static string ResolveSourceScenePath()
    {
        string overridePath = Environment.GetEnvironmentVariable(SourceSceneEnvironmentVariable);
        return string.IsNullOrWhiteSpace(overridePath) ? DefaultSourceScenePath : overridePath;
    }

    public static string ResolveSourceSceneMetaPath()
    {
        string overridePath = Environment.GetEnvironmentVariable(SourceSceneMetaEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return overridePath;
        }

        string sourceScenePath = ResolveSourceScenePath();
        return string.Equals(sourceScenePath, DefaultSourceScenePath, StringComparison.OrdinalIgnoreCase)
            ? DefaultSourceSceneMetaPath
            : sourceScenePath + ".meta";
    }

    public static bool TryGetActualHighDetailSource(out string sourceScenePath, out string sourceMetaPath, out long sourceBytes)
    {
        sourceScenePath = ResolveSourceScenePath();
        sourceMetaPath = ResolveSourceSceneMetaPath();
        sourceBytes = 0L;

        if (!File.Exists(sourceScenePath) || !File.Exists(sourceMetaPath))
        {
            return false;
        }

        sourceBytes = new FileInfo(sourceScenePath).Length;
        return sourceBytes >= MinimumActualHighDetailSceneBytes;
    }
}
