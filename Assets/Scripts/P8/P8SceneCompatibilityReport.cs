using System;

public static class P8SceneCompatibilityReport
{
    public const string Stage = "P8-A";
    public const string BaselineSceneName = "P7_HighDetail_Chuo";
    public const string BaselineScenePath = "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity";
    public const bool UsesChuoBaseMapAsBaseline = false;
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool ImplementsP8BRiskFrontVisualization = false;
    public const bool ImplementsP8CHazardInteractions = false;
    public const bool ImplementsP8DCollapseProxy = false;
    public const bool ImplementsP9CrowdSpawnOrIndoorGameplay = false;
    public const bool ImplementsP10Packaging = false;

    public static string[] GetP8Stages()
    {
        return new[] { "P8-A", "P8-B", "P8-C", "P8-D", "P8-E" };
    }

    public static bool HasExactlyFiveP8Stages()
    {
        string[] stages = GetP8Stages();
        return stages.Length == 5 &&
               stages[0] == "P8-A" &&
               stages[1] == "P8-B" &&
               stages[2] == "P8-C" &&
               stages[3] == "P8-D" &&
               stages[4] == "P8-E";
    }

    public static bool IsForbiddenP8Stage(string stage)
    {
        return string.Equals(stage, "P8-0", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(stage, "P8-F", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(stage, "P8-G", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsP8StageAllowed(string stage)
    {
        if (string.IsNullOrWhiteSpace(stage))
        {
            return false;
        }

        string[] allowedStages = GetP8Stages();
        for (int i = 0; i < allowedStages.Length; i++)
        {
            if (string.Equals(allowedStages[i], stage, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsHighDetailBaselinePath(string scenePath)
    {
        return string.Equals(NormalizePath(scenePath), BaselineScenePath, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLegacyFallbackPath(string scenePath)
    {
        return string.Equals(NormalizePath(scenePath), GetLegacyFallbackScenePath(), StringComparison.OrdinalIgnoreCase);
    }

    public static string GetLegacyFallbackScenePath()
    {
        return "Assets/Scenes/Chuo" + "_BaseMap.unity";
    }

    public static P8CompatibilityCheck[] GetP2P6CompatibilityChecks()
    {
        return new[]
        {
            new P8CompatibilityCheck("P2", "Player movement", "Compatible in source; high-detail spawn/collision smoke pending.", true),
            new P8CompatibilityCheck("P2", "Camera", "Compatible in source; high-detail camera framing smoke pending.", true),
            new P8CompatibilityCheck("P2", "Shelter interaction", "Compatible in source; staged high-detail shelter trigger smoke pending.", true),
            new P8CompatibilityCheck("P2", "ResultPanel success/failure flow", "Compatible in source; success/failure rules unchanged.", false),
            new P8CompatibilityCheck("P3", "Data pipeline outputs", "Runtime JSON/CSV assumptions remain compatible; geospatial placement validation pending.", true),
            new P8CompatibilityCheck("P4", "Real shelter loading and markers", "Adaptable to high-detail scene; representative marker placement smoke pending.", true),
            new P8CompatibilityCheck("P5", "Qualified shelter/route/candidate logic", "Adaptable and fail-safe; real_qualified remains opt-in and no official-route claim is made.", true),
            new P8CompatibilityCheck("P6", "Navigation guidance and NPC prototype", "Can target staged objects; high-detail target smoke pending and no P9 crowd behavior.", true)
        };
    }

    public static string[] GetPendingRuntimeChecks()
    {
        return new[]
        {
            "High-detail player spawn/collision smoke",
            "High-detail camera framing smoke",
            "Representative shelter trigger and ResultPanel flow smoke",
            "P5 marker/route/candidate high-detail placement smoke",
            "P6 guidance/NPC staged-target smoke"
        };
    }

    public static string CreateSummary()
    {
        return Stage + " scene compatibility gate uses " + BaselineScenePath +
               ", keeps " + GetLegacyFallbackScenePath() + " as untouched legacy fallback, and leaves gameplay success/failure rules unchanged.";
    }

    private static string NormalizePath(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return string.Empty;
        }

        return scenePath.Replace("\\", "/").Trim();
    }
}

[Serializable]
public class P8CompatibilityCheck
{
    public string phase;
    public string area;
    public string status;
    public bool runtimeSmokePending;

    public P8CompatibilityCheck(string phase, string area, string status, bool runtimeSmokePending)
    {
        this.phase = phase;
        this.area = area;
        this.status = status;
        this.runtimeSmokePending = runtimeSmokePending;
    }
}
