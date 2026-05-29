using System.IO;
using UnityEngine;

public sealed class NewMapHazardController : MonoBehaviour
{
    private GameObject lightCurtain;
    private GameObject debrisWarning;
    private Vector3 curtainStart;
    private Vector3 curtainEnd;
    private Vector3 inlandDirection = Vector3.forward;
    private Vector3 debrisCenter;
    private Transform hazardRoot;
    private Transform debrisRoot;
    private float stageElapsed;
    private float frontDurationSeconds = 90f;
    private float debrisExposureSeconds;
    private float curtainHeightMeters = 1000f;
    private float curtainLengthMeters = 1500f;
    private float curtainThicknessMeters = 12f;
    private string tsunamiStartSide = "south";
    private Bounds debrisBounds;
    private NewMapTsunamiModeHotfixConfig config;

    public NewMapTsunamiStage Stage { get; private set; } = NewMapTsunamiStage.Inactive;
    public bool RiskChecksActive => Stage == NewMapTsunamiStage.FrontApproaching;
    public float NormalizedFrontProgress => Mathf.Clamp01(stageElapsed / Mathf.Max(1f, frontDurationSeconds));
    public float DebrisExposureSeconds => debrisExposureSeconds;
    public Vector3 DebrisCenterForDiagnostics => debrisBounds.center;
    public bool Stage2VisualsBuiltForDiagnostics => lightCurtain != null && debrisWarning != null;
    public bool LightCurtainVisibleForDiagnostics => lightCurtain != null && lightCurtain.activeSelf;
    public string TsunamiStartSide => tsunamiStartSide;
    public Vector3 TsunamiDirection => inlandDirection;
    public Vector3 CurtainStart => curtainStart;
    public Vector3 CurtainEnd => curtainEnd;
    public float CurtainHeightMeters => curtainHeightMeters;
    public float CurtainLengthMeters => curtainLengthMeters;
    public float CurtainThicknessMeters => curtainThicknessMeters;
    public float FrontDurationSeconds => frontDurationSeconds;

    public static NewMapHazardController Create(Transform hazardRoot, Transform debrisRoot, Vector3 spawnPosition)
    {
        NewMapPlayableBounds fallbackBounds = NewMapPlayableBounds.DefaultDocumented(0f, 80f, 4f);
        return Create(hazardRoot, debrisRoot, spawnPosition, fallbackBounds, spawnPosition.y, NewMapTsunamiModeHotfixConfig.Load());
    }

    public static NewMapHazardController Create(
        Transform hazardRoot,
        Transform debrisRoot,
        Vector3 spawnPosition,
        NewMapPlayableBounds playableBounds,
        float supportSurfaceY,
        NewMapTsunamiModeHotfixConfig hotfixConfig)
    {
        GameObject controllerObject = new GameObject("NewMap_HazardController");
        controllerObject.transform.SetParent(hazardRoot, false);
        NewMapHazardController controller = controllerObject.AddComponent<NewMapHazardController>();
        controller.Build(hazardRoot, debrisRoot, spawnPosition, playableBounds, supportSurfaceY, hotfixConfig);
        return controller;
    }

    public void SetStage(NewMapTsunamiStage stage)
    {
        Stage = stage;
        stageElapsed = 0f;
        debrisExposureSeconds = 0f;

        if (stage == NewMapTsunamiStage.FrontApproaching)
        {
            EnsureStage2VisualsBuilt();
        }

        if (lightCurtain != null)
        {
            lightCurtain.SetActive(stage == NewMapTsunamiStage.FrontApproaching);
            lightCurtain.transform.position = curtainStart;
        }

        if (debrisWarning != null)
        {
            debrisWarning.SetActive(stage == NewMapTsunamiStage.FrontApproaching);
        }
    }

    public void Tick(float deltaTime)
    {
        if (Stage != NewMapTsunamiStage.FrontApproaching)
        {
            return;
        }

        stageElapsed += Mathf.Max(0f, deltaTime);
        if (lightCurtain != null)
        {
            lightCurtain.transform.position = Vector3.Lerp(curtainStart, curtainEnd, NormalizedFrontProgress);
        }
    }

    public bool IsPlayerReachedByFront(Vector3 playerPosition)
    {
        if (!RiskChecksActive)
        {
            return false;
        }

        EnsureStage2VisualsBuilt();
        if (lightCurtain == null)
        {
            return false;
        }

        float signedDistanceAheadOfFront = Vector3.Dot(playerPosition - lightCurtain.transform.position, inlandDirection);
        return signedDistanceAheadOfFront < -0.5f;
    }

    public Vector3 GetFloodedSideSamplePointForDiagnostics()
    {
        return curtainStart - inlandDirection * 5f;
    }

    public bool IsPlayerInDebrisExposure(Vector3 playerPosition, float deltaTime, out string reason)
    {
        reason = string.Empty;
        if (!RiskChecksActive)
        {
            debrisExposureSeconds = Mathf.Max(0f, debrisExposureSeconds - deltaTime);
            return false;
        }

        EnsureStage2VisualsBuilt();
        if (!debrisBounds.Contains(playerPosition))
        {
            debrisExposureSeconds = Mathf.Max(0f, debrisExposureSeconds - deltaTime);
            return false;
        }

        debrisExposureSeconds += deltaTime;
        if (debrisExposureSeconds < 4f)
        {
            return false;
        }

        reason = "collapse_debris_exposure: player remained in the marked debris hazard area during Stage 2.";
        return true;
    }

    private void Build(
        Transform hazardRoot,
        Transform debrisRoot,
        Vector3 spawnPosition,
        NewMapPlayableBounds playableBounds,
        float supportSurfaceY,
        NewMapTsunamiModeHotfixConfig hotfixConfig)
    {
        this.hazardRoot = hazardRoot;
        this.debrisRoot = debrisRoot;
        config = hotfixConfig ?? NewMapTsunamiModeHotfixConfig.Load();
        frontDurationSeconds = config.ActiveFrontDurationSeconds;
        tsunamiStartSide = config.NormalizedTsunamiStartSide;
        curtainHeightMeters = config.CurtainHeightMeters;
        curtainThicknessMeters = config.CurtainThicknessMeters;
        ResolveCurtainPath(playableBounds, spawnPosition, supportSurfaceY);
        debrisCenter = spawnPosition + new Vector3(10f, 1f, 10f);
        debrisBounds = new Bounds(debrisCenter, new Vector3(7f, 2f, 7f));
        Debug.Log(
            $"NewMap tsunami configured startSide={tsunamiStartSide} startLine={curtainStart} " +
            $"direction={inlandDirection} curtainHeight={curtainHeightMeters:0.0} " +
            $"curtainLength={curtainLengthMeters:0.0} curtainThickness={curtainThicknessMeters:0.0} " +
            $"frontDuration={frontDurationSeconds:0.0}");
        SetStage(NewMapTsunamiStage.Inactive);
    }

    private void ResolveCurtainPath(NewMapPlayableBounds playableBounds, Vector3 spawnPosition, float supportSurfaceY)
    {
        if (!playableBounds.IsValid)
        {
            playableBounds = NewMapPlayableBounds.DefaultDocumented(0f, 80f, 4f);
        }

        float margin = Mathf.Max(0f, config.curtainStartMarginMeters);
        float y = supportSurfaceY - 1f + curtainHeightMeters * 0.5f;
        float width = playableBounds.Width;
        float depth = playableBounds.Depth;
        float diagonal = Mathf.Sqrt(width * width + depth * depth);
        curtainLengthMeters = Mathf.Max(config.minimumCurtainLengthMeters, diagonal * Mathf.Max(1f, config.curtainLengthMapDiagonalMultiplier));

        switch (tsunamiStartSide)
        {
            case "north":
                inlandDirection = Vector3.back;
                curtainStart = new Vector3(playableBounds.CenterX, y, playableBounds.MaxZ + margin);
                curtainEnd = new Vector3(playableBounds.CenterX, y, playableBounds.MinZ - margin);
                break;
            case "east":
                inlandDirection = Vector3.left;
                curtainStart = new Vector3(playableBounds.MaxX + margin, y, playableBounds.CenterZ);
                curtainEnd = new Vector3(playableBounds.MinX - margin, y, playableBounds.CenterZ);
                break;
            case "west":
                inlandDirection = Vector3.right;
                curtainStart = new Vector3(playableBounds.MinX - margin, y, playableBounds.CenterZ);
                curtainEnd = new Vector3(playableBounds.MaxX + margin, y, playableBounds.CenterZ);
                break;
            default:
                tsunamiStartSide = "south";
                inlandDirection = Vector3.forward;
                curtainStart = new Vector3(playableBounds.CenterX, y, playableBounds.MinZ - margin);
                curtainEnd = new Vector3(playableBounds.CenterX, y, playableBounds.MaxZ + margin);
                break;
        }
    }

    private void EnsureStage2VisualsBuilt()
    {
        if (lightCurtain != null && debrisWarning != null)
        {
            return;
        }

        Material curtainMaterial = NewMapVisualFactory.CreateMaterial("NewMap_LightCurtain_Material", new Color(0.05f, 0.75f, 1f, 0.32f), true);
        Material debrisMaterial = NewMapVisualFactory.CreateMaterial("NewMap_DebrisWarning_Material", new Color(1f, 0.45f, 0.08f, 0.45f), true);

        lightCurtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lightCurtain.name = "NewMap_Stage2_LightCurtain_RiskFront";
        lightCurtain.transform.SetParent(hazardRoot, true);
        lightCurtain.transform.position = curtainStart;
        bool northSouth = tsunamiStartSide == "north" || tsunamiStartSide == "south";
        lightCurtain.transform.localScale = northSouth
            ? new Vector3(curtainLengthMeters, curtainHeightMeters, curtainThicknessMeters)
            : new Vector3(curtainThicknessMeters, curtainHeightMeters, curtainLengthMeters);
        SetMaterial(lightCurtain, curtainMaterial);
        NewMapVisualFactory.RemoveCollider(lightCurtain);

        debrisWarning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debrisWarning.name = "NewMap_CollapseDebris_ExposureWarning";
        debrisWarning.transform.SetParent(debrisRoot, true);
        debrisWarning.transform.position = debrisCenter;
        debrisWarning.transform.localScale = new Vector3(7f, 2f, 7f);
        SetMaterial(debrisWarning, debrisMaterial);
        NewMapVisualFactory.RemoveCollider(debrisWarning);
        debrisBounds = new Bounds(debrisWarning.transform.position, debrisWarning.transform.localScale);

        bool visible = Stage == NewMapTsunamiStage.FrontApproaching;
        lightCurtain.SetActive(visible);
        debrisWarning.SetActive(visible);
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }
}

[System.Serializable]
public sealed class NewMapTsunamiModeHotfixConfig
{
    public float tsunamiWarningDurationSeconds = -1f;
    public float warningPhaseSeconds = 300f;
    public float activeFrontDurationSeconds = 120f;
    public string tsunamiStartSide = "south";
    public float curtainHeightMeters = 1000f;
    public float minimumCurtainLengthMeters = 1500f;
    public float curtainLengthMapDiagonalMultiplier = 1.5f;
    public float curtainThicknessMeters = 12f;
    public float curtainStartMarginMeters = 60f;
    public float preWarningRandomMaxSeconds = 180f;
    public int preWarningRandomSeed = 20260530;
    public bool deterministicPreWarningSeedEnabled;
    public float preWarningTestOverrideSeconds = -1f;
    public bool enableShelterDirectLines = true;
    public int maxDisplayedShelterLines;
    public float lineUpdateIntervalSeconds = 0.2f;
    public float rankingAutoRefreshIntervalSeconds = 0.5f;
    public float directLineHeightOffsetMeters = 0.35f;
    public float directLineWidthMeters = 0.08f;

    public float WarningPhaseSeconds => Mathf.Clamp(
        tsunamiWarningDurationSeconds > 0f ? tsunamiWarningDurationSeconds : warningPhaseSeconds,
        1f,
        600f);
    public float ActiveFrontDurationSeconds => Mathf.Clamp(activeFrontDurationSeconds, 5f, 3600f);
    public float CurtainHeightMeters => Mathf.Clamp(curtainHeightMeters, 100f, 5000f);
    public float MinimumCurtainLengthMeters => Mathf.Clamp(minimumCurtainLengthMeters, 100f, 10000f);
    public float CurtainThicknessMeters => Mathf.Clamp(curtainThicknessMeters, 1f, 200f);
    public float PreWarningRandomMaxSeconds => Mathf.Clamp(preWarningRandomMaxSeconds, 0f, 180f);
    public string NormalizedTsunamiStartSide => NormalizeSide(tsunamiStartSide);
    public NewMapShelterDirectLineConfig DirectLineConfig => new NewMapShelterDirectLineConfig
    {
        enableShelterDirectLines = enableShelterDirectLines,
        maxDisplayedShelterLines = maxDisplayedShelterLines,
        lineUpdateIntervalSeconds = lineUpdateIntervalSeconds,
        rankingAutoRefreshIntervalSeconds = rankingAutoRefreshIntervalSeconds,
        directLineHeightOffsetMeters = directLineHeightOffsetMeters,
        directLineWidthMeters = directLineWidthMeters
    };

    public static NewMapTsunamiModeHotfixConfig Default()
    {
        return new NewMapTsunamiModeHotfixConfig();
    }

    public static NewMapTsunamiModeHotfixConfig CreateForDiagnostics(float warningDurationSeconds)
    {
        NewMapTsunamiModeHotfixConfig config = Default();
        float duration = Mathf.Clamp(warningDurationSeconds, 1f, 600f);
        config.tsunamiWarningDurationSeconds = duration;
        config.warningPhaseSeconds = duration;
        config.preWarningRandomMaxSeconds = 0f;
        config.preWarningTestOverrideSeconds = 0f;
        return config;
    }

    public static NewMapTsunamiModeHotfixConfig CreateForDiagnostics(float warningDurationSeconds, float preWarningSeconds)
    {
        NewMapTsunamiModeHotfixConfig config = CreateForDiagnostics(warningDurationSeconds);
        config.preWarningRandomMaxSeconds = Mathf.Clamp(preWarningSeconds, 0f, 180f);
        config.preWarningTestOverrideSeconds = Mathf.Clamp(preWarningSeconds, 0f, 180f);
        return config;
    }

    public float ResolvePreWarningWaitSeconds()
    {
        float max = PreWarningRandomMaxSeconds;
        if (preWarningTestOverrideSeconds >= 0f)
        {
            return Mathf.Clamp(preWarningTestOverrideSeconds, 0f, max);
        }

        if (max <= 0.001f)
        {
            return 0f;
        }

        if (deterministicPreWarningSeedEnabled)
        {
            var random = new System.Random(preWarningRandomSeed == 0 ? 1 : preWarningRandomSeed);
            return (float)(random.NextDouble() * max);
        }

        var sessionRandom = new System.Random(CreateSessionRandomSeed(preWarningRandomSeed));
        return (float)(sessionRandom.NextDouble() * max);
    }

    public static NewMapTsunamiModeHotfixConfig Load()
    {
        NewMapTsunamiModeHotfixConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_tsunami_mode_hotfix_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapTsunamiModeHotfixConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap tsunami hotfix config could not be loaded; using defaults. {exception.Message}");
            }
        }

        float warningDuration = config.WarningPhaseSeconds;
        config.tsunamiWarningDurationSeconds = warningDuration;
        config.warningPhaseSeconds = warningDuration;
        config.activeFrontDurationSeconds = config.ActiveFrontDurationSeconds;
        config.tsunamiStartSide = config.NormalizedTsunamiStartSide;
        config.curtainHeightMeters = config.CurtainHeightMeters;
        config.minimumCurtainLengthMeters = config.MinimumCurtainLengthMeters;
        config.curtainLengthMapDiagonalMultiplier = Mathf.Clamp(config.curtainLengthMapDiagonalMultiplier, 1f, 5f);
        config.curtainThicknessMeters = config.CurtainThicknessMeters;
        config.curtainStartMarginMeters = Mathf.Clamp(config.curtainStartMarginMeters, 0f, 1000f);
        config.preWarningRandomMaxSeconds = config.PreWarningRandomMaxSeconds;
        config.preWarningTestOverrideSeconds = config.preWarningTestOverrideSeconds < 0f
            ? -1f
            : Mathf.Clamp(config.preWarningTestOverrideSeconds, 0f, config.preWarningRandomMaxSeconds);
        config.lineUpdateIntervalSeconds = config.DirectLineConfig.LineUpdateIntervalSeconds;
        config.rankingAutoRefreshIntervalSeconds = config.DirectLineConfig.RankingAutoRefreshIntervalSeconds;
        config.directLineHeightOffsetMeters = config.DirectLineConfig.LineHeightOffsetMeters;
        config.directLineWidthMeters = config.DirectLineConfig.LineWidthMeters;
        config.maxDisplayedShelterLines = config.DirectLineConfig.MaxDisplayedShelterLines;
        return config;
    }

    private static int CreateSessionRandomSeed(int configuredSeed)
    {
        unchecked
        {
            int seed = configuredSeed == 0 ? 1 : configuredSeed;
            seed = (seed * 397) ^ System.Environment.TickCount;
            seed = (seed * 397) ^ System.Guid.NewGuid().GetHashCode();
            seed = (seed * 397) ^ (int)(System.DateTime.UtcNow.Ticks & 0x7fffffff);
            return seed == 0 ? 1 : seed;
        }
    }

    private static string NormalizeSide(string side)
    {
        if (string.IsNullOrWhiteSpace(side))
        {
            return "south";
        }

        string lower = side.Trim().ToLowerInvariant();
        return lower == "north" || lower == "east" || lower == "west" ? lower : "south";
    }
}
