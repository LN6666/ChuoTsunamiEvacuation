using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class NewMapRuntimeBootstrap : MonoBehaviour
{
    private const bool SuppressSceneMeshCollidersForManualTest = false;
    private const bool EnablePlayerRuntimeSceneWideBoundsScan = true;
    private const float GroundSkinOffset = 0.04f;
    private const float Round2FallbackSupportSurfaceY = 1.2f;
    private const string GameplaySelfAuditSmokeArg = "-newmapSelfAuditSmoke";
    private const string RuntimeNonOfficialCandidateResourcePath = "NewMap/newmap_runtime_non_official_candidates";

    private static readonly string[] RequiredRoots =
    {
        "MapRoot",
        "RuntimeSystemsRoot",
        "PlayerSpawnRoot",
        "ShelterMarkerRoot",
        "CandidateMarkerRoot",
        "HazardVisualRoot",
        "NavigationRoot",
        "CrowdRoot",
        "CollapseDebrisRoot",
        "GreenFrameRoot",
        "UIAnchorRoot",
        "DebugDiagnosticsRoot",
        "GameplaySupportRoot",
        "GameplayGroundCoverRoot",
        "PlayableBoundsRoot",
        "PerformanceMetricsRoot"
    };

    public static bool EnableLocalTrainingProxyTargetsForDiagnostics { get; set; }

    public Bounds LastMapBounds { get; private set; }
    public bool LastMapBoundsValid { get; private set; }
    public bool LastUsedGroundSupportProxy { get; private set; }
    public bool LastRuntimeCollisionSupportProxyActive { get; private set; }
    public float LastRuntimeGroundSurfaceY { get; private set; }
    public float LastOldRuntimeGroundSurfaceY { get; private set; }
    public float LastPlayerSpawnGroundDelta { get; private set; }
    public float LastSampledBuildingBaseY { get; private set; }
    public float LastSampledMapMinY { get; private set; }
    public float LastVisualGroundReferenceY { get; private set; }
    public float LastSupportToVisualGroundDelta { get; private set; }
    public int LastVisualGroundSampleCount { get; private set; }
    public int LastActiveTargetHeightOffsetViolations { get; private set; }
    public float LastMaxActiveTargetHeightOffset { get; private set; }
    public bool LastVisualGroundSampleValid { get; private set; }
    public bool LastRuntimeCollisionSupportRendererVisible { get; private set; }
    public int LastDisabledSceneMeshColliderCount { get; private set; }
    public int LastRendererCount { get; private set; }
    public int LastColliderCount { get; private set; }
    public bool LastMeshColliderDisableComplete { get; private set; }
    public int LastSpawnAttemptCount { get; private set; }
    public int LastSpawnAcceptedCount { get; private set; }
    public int LastSpawnRejectedInsideBuildingCount { get; private set; }
    public int LastSpawnRejectedNoGroundCount { get; private set; }
    public int LastSpawnRejectedOutOfBoundsCount { get; private set; }
    public int LastSpawnRejectedTooCloseToBuildingCount { get; private set; }
    public bool LastSpawnFallbackUsed { get; private set; }
    public bool LastSpawnValidationPassed { get; private set; }
    public string LastSpawnMode { get; private set; } = string.Empty;
    public string LastFallbackSafeSpawnId { get; private set; } = string.Empty;
    public string LastSpawnValidationSource { get; private set; } = string.Empty;
    public Vector3 LastFinalSpawnPosition { get; private set; }
    public float LastNearestBuildingDistance { get; private set; }
    public int LastBuildingBoundsCacheCount { get; private set; }
    public bool LastBuildingBoundsCacheBuilt { get; private set; }
    public bool LastRuntimeCollisionSupportColliderActive { get; private set; }
    public int LastSupportRendererCount { get; private set; }
    public int LastVisibleSupportRendererCount { get; private set; }
    public int LastSupportRendererDisabledCount { get; private set; }
    public int LastBlueDebugGroundRendererDisabledCount { get; private set; }
    public int LastPlayableAirWallColliderCount { get; private set; }
    public int LastPlayableAirWallVisibleRendererCount { get; private set; }
    public bool LastPlayableBoundsValid { get; private set; }
    public NewMapPlayableBounds LastPlayableBounds { get; private set; }
    public string LastPlayableBoundsSource { get; private set; } = string.Empty;
    public float LastSupportToRoadDelta { get; private set; }
    public float LastSupportToBuildingBaseDelta { get; private set; }
    public float LastRoadSampleY { get; private set; }
    public int LastRoadSampleCount { get; private set; }
    public bool LastRoadSampleValid { get; private set; }
    public float LastBuildingRoadVerticalOffsetApplied { get; private set; }
    public int LastBuildingRoadAlignedRootCount { get; private set; }
    public string LastBuildingRoadAlignmentStatus { get; private set; } = "not_evaluated";
    public bool LastAdaptiveSupportGridEnabled { get; private set; }
    public bool LastAdaptiveSupportGridActive { get; private set; }
    public int LastAdaptiveSupportGridCellCount { get; private set; }
    public int LastAdaptiveSupportGridColliderCount { get; private set; }
    public int LastAdaptiveSupportGridVisibleRendererCount { get; private set; }
    public int LastAdaptiveCellsUsingRoad { get; private set; }
    public int LastAdaptiveCellsUsingTerrain { get; private set; }
    public int LastAdaptiveCellsUsingRelief { get; private set; }
    public int LastAdaptiveCellsUsingBridge { get; private set; }
    public int LastAdaptiveCellsUsingBuildingBaseFallback { get; private set; }
    public int LastAdaptiveCellsUsingGlobalFallback { get; private set; }
    public float LastAdaptiveSupportYMin { get; private set; }
    public float LastAdaptiveSupportYMax { get; private set; }
    public float LastAdaptiveSupportYAverage { get; private set; }
    public string LastAdaptiveSupportGridStatus { get; private set; } = "not_built";
    public bool LastSafeGroundEnabled { get; private set; }
    public int LastSafeGroundColliderCount { get; private set; }
    public int LastSafeGroundRendererCount { get; private set; }
    public bool LastSafeGroundRendererHidden { get; private set; }
    public float LastSafeGroundSupportY { get; private set; }
    public bool LastFallOutPreventionEnabled { get; private set; }
    public int LastLargeBlueGroundRendererDisabledCount { get; private set; }
    public int LastVisibleLargeBlueGroundRendererCount { get; private set; }
    public bool LastGameplayGroundCoverEnabled { get; private set; }
    public bool LastGameplayGroundCoverActive { get; private set; }
    public int LastGameplayGroundCoverTileCount { get; private set; }
    public int LastGameplayGroundCoverColliderCount { get; private set; }
    public int LastGameplayGroundCoverRendererCount { get; private set; }
    public int LastGameplayGroundCoverVisibleRendererCount { get; private set; }
    public float LastGameplayGroundCoverY { get; private set; }
    public float LastGameplayGroundCoverYMin { get; private set; }
    public float LastGameplayGroundCoverYMax { get; private set; }
    public float LastGameplayGroundCoverTotalArea { get; private set; }
    public float LastGameplayGroundCoverOpacity { get; private set; }
    public string LastGameplayGroundCoverMaterialName { get; private set; } = string.Empty;
    public string LastGameplayGroundCoverMaterialSource { get; private set; } = string.Empty;
    public bool LastGameplayGroundCoverMaterialBlueLike { get; private set; }
    public bool LastGameplayGroundCoverMaterialMagentaLike { get; private set; }
    public bool LastBuildingSnapdownEnabled { get; private set; }
    public int LastBuildingSnapdownScannedCount { get; private set; }
    public int LastFloatingBuildingCandidateCount { get; private set; }
    public int LastBuildingSnapdownMovedCount { get; private set; }
    public int LastBuildingSnapdownSkippedCount { get; private set; }
    public int LastBuildingSnapdownRemainingFloatingCount { get; private set; }
    public float LastBuildingSnapdownAverageOffset { get; private set; }
    public float LastBuildingSnapdownMaxOffset { get; private set; }
    public float LastBuildingSnapdownReferenceY { get; private set; }
    public float LastBuildingSnapdownThresholdMeters { get; private set; }
    public string LastBuildingSnapdownStatus { get; private set; } = "not_evaluated";

    private readonly List<Bounds> buildingAvoidanceBounds = new List<Bounds>();
    private NewMapSpawnConfig spawnConfig;
    private NewMapSafeSpawnPointDataset safeSpawnDataset;
    private NewMapPlayableBoundsConfig playableBoundsConfig;
    private NewMapAdaptiveSupportGridRuntime adaptiveSupportGrid;
    private NewMapSafeGroundConfig safeGroundConfig;
    private NewMapGameplayGroundCoverConfig groundCoverConfig;
    private NewMapFloatingBuildingSnapdownConfig buildingSnapdownConfig;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        ConfigurePlayerLogging();

        Scene scene = SceneManager.GetActiveScene();
        bool isChuoBaseMap =
            string.Equals(scene.name, NewMapRuntimeConstants.SceneName, System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scene.path, NewMapRuntimeConstants.ScenePath, System.StringComparison.OrdinalIgnoreCase);

        if (!isChuoBaseMap || FindObjectOfType<NewMapRuntimeBootstrap>() != null)
        {
            return;
        }

        CreateForCurrentScene();
    }

    private static void ConfigurePlayerLogging()
    {
        if (Application.isEditor)
        {
            return;
        }

        Application.runInBackground = true;
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }

    public static NewMapRuntimeBootstrap CreateForCurrentScene()
    {
        Dictionary<string, Transform> roots = EnsureRoots();
        GameObject bootstrapObject = new GameObject("NewMap_RuntimeBootstrap");
        bootstrapObject.transform.SetParent(roots["RuntimeSystemsRoot"], false);
        NewMapRuntimeBootstrap bootstrap = bootstrapObject.AddComponent<NewMapRuntimeBootstrap>();
        bootstrap.Build(roots);
        return bootstrap;
    }

    public static Dictionary<string, Transform> EnsureRoots()
    {
        var roots = new Dictionary<string, Transform>();
        foreach (string rootName in RequiredRoots)
        {
            GameObject root = FindSceneObjectByExactName(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
            }

            roots[rootName] = root.transform;
        }

        return roots;
    }

    private static GameObject FindSceneObjectByExactName(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root != null && string.Equals(root.name, objectName, System.StringComparison.Ordinal))
            {
                return root;
            }
        }

        return null;
    }

    private void Build(Dictionary<string, Transform> roots)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        spawnConfig = NewMapSpawnConfig.Load();
        safeSpawnDataset = NewMapSafeSpawnPointDataset.Load();
        playableBoundsConfig = NewMapPlayableBoundsConfig.Load();
        safeGroundConfig = NewMapSafeGroundConfig.Load();
        groundCoverConfig = NewMapGameplayGroundCoverConfig.Load();
        buildingSnapdownConfig = NewMapFloatingBuildingSnapdownConfig.Load();
        PrepareManualTestRoots(roots);
        EnforceSupportSurfaceVisibility(roots);
        ApplyRound3BuildingRoadVerticalAlignment();
        ApplyFloatingBuildingSnapdownToGameplayGroundCover();
        Physics.SyncTransforms();
        LastMapBoundsValid = TryResolveRuntimeMapBounds(out Bounds mapBounds, out int rendererCount, out int colliderCount);
        LastMapBounds = mapBounds;
        LastRendererCount = rendererCount;
        LastColliderCount = colliderCount;
        LastPlayableBounds = ResolvePlayableBounds(mapBounds, LastMapBoundsValid, playableBoundsConfig);
        LastPlayableBoundsValid = LastPlayableBounds.IsValid;
        adaptiveSupportGrid = NewMapAdaptiveSupportGridRuntime.Load();
        if (adaptiveSupportGrid.Enabled)
        {
            adaptiveSupportGrid.BuildCollisionGrid(roots["GameplaySupportRoot"], LastPlayableBounds, LastVisualGroundReferenceY);
        }
        ApplyAdaptiveSupportGridDiagnostics();
        Physics.SyncTransforms();
        long boundsMs = stopwatch.ElapsedMilliseconds;

        LastOldRuntimeGroundSurfaceY = 0f;
        Vector3 spawn = ResolveSpawnPosition(mapBounds, LastMapBoundsValid, roots["DebugDiagnosticsRoot"]);
        LastFinalSpawnPosition = spawn;
        LastPlayerSpawnGroundDelta = spawn.y - LastRuntimeGroundSurfaceY;
        EnsureGameplayGroundCover(roots["GameplayGroundCoverRoot"], LastPlayableBounds, LastRuntimeGroundSurfaceY);
        EnsureRuntimeCollisionSupport(roots["GameplaySupportRoot"], new Vector3(spawn.x, LastRuntimeGroundSurfaceY, spawn.z));
        EnforceSupportSurfaceVisibility(roots);
        EnsurePlayableBoundsAirWalls(roots["PlayableBoundsRoot"], LastPlayableBounds, LastRuntimeGroundSurfaceY);
        Physics.SyncTransforms();
        if (SuppressSceneMeshCollidersForManualTest)
        {
            StartCoroutine(DisableSceneMeshCollidersStaged());
        }
        long spawnSupportMs = stopwatch.ElapsedMilliseconds - boundsMs;

        NewMapPlayerController player = NewMapPlayerController.Create(roots["PlayerSpawnRoot"], spawn);
        player.ConfigureGroundSafety(
            spawn,
            LastPlayableBounds,
            LastRuntimeGroundSurfaceY,
            safeGroundConfig != null ? safeGroundConfig.fallRecoveryBelowY : -8f,
            safeGroundConfig == null || safeGroundConfig.recoverOutsidePlayableBounds,
            safeGroundConfig != null && safeGroundConfig.logRecoveryEvents);
        NewMapRuntimeUI.EnsureRuntimeEventSystem();
        NewMapRuntimeUI ui = NewMapRuntimeUI.Create(roots["UIAnchorRoot"]);
        NewMapLightingController lighting = NewMapLightingController.Create(roots["RuntimeSystemsRoot"]);
        NewMapHazardController hazard = NewMapHazardController.Create(roots["HazardVisualRoot"], roots["CollapseDebrisRoot"], spawn);
        NewMapNpcCrowdPrototype crowd = NewMapNpcCrowdPrototype.Create(roots["CrowdRoot"], spawn, LastPlayableBounds, buildingAvoidanceBounds);
        NewMapPerformanceProbe.Create(roots["PerformanceMetricsRoot"]);
        long systemsMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs;
        List<NewMapRuntimeTarget> targets = CreateVerifiedOfficialShelterTargets(roots);
        targets.AddRange(CreateRecoveredNonOfficialCandidateTargets(roots, spawn));
        if (ShouldEnableLocalTrainingProxyTargets())
        {
            targets.AddRange(CreateLocalRuntimeTargets(roots, spawn));
        }

        CalculateActiveTargetHeightOffsets(targets);
        NewMapNameLabelController.Create(roots["NavigationRoot"], player, targets);
        long targetsMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs - systemsMs;

        NewMapGameController controller = gameObject.AddComponent<NewMapGameController>();
        controller.Configure(player, ui, lighting, hazard, crowd, targets, BuildDiagnosticText());
        if (ShouldRunGameplaySelfAuditSmoke())
        {
            StartCoroutine(RunGameplaySelfAuditSmoke(controller, player, ui, lighting, hazard, crowd));
        }

        long configureMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs - systemsMs - targetsMs;
        stopwatch.Stop();
        Debug.Log(
            $"NewMap runtime bootstrap timings: boundsMs={boundsMs} spawnSupportMs={spawnSupportMs} " +
            $"systemsMs={systemsMs} targetsMs={targetsMs} configureMs={configureMs} totalMs={stopwatch.ElapsedMilliseconds}");
        string meshColliderShutdown = SuppressSceneMeshCollidersForManualTest ? "staged" : "disabled_runtime_startup";
        Debug.Log(
            $"NewMap runtime bootstrap completed. renderers={LastRendererCount} colliders={LastColliderCount} " +
            $"groundSupportProxy={LastUsedGroundSupportProxy} collisionSupportProxy={LastRuntimeCollisionSupportProxyActive} " +
            $"oldSupportSurfaceY={LastOldRuntimeGroundSurfaceY:F2} supportSurfaceY={LastRuntimeGroundSurfaceY:F2} " +
            $"visualGroundY={LastVisualGroundReferenceY:F2} sampledBuildingBaseY={LastSampledBuildingBaseY:F2} " +
            $"sampledMapMinY={LastSampledMapMinY:F2} visualGroundSamples={LastVisualGroundSampleCount} " +
            $"supportToVisualGroundDelta={LastSupportToVisualGroundDelta:F2} spawnGroundDelta={LastPlayerSpawnGroundDelta:F2} " +
            $"activeTargetMaxHeightOffset={LastMaxActiveTargetHeightOffset:F2} activeTargetHeightOffsetViolations={LastActiveTargetHeightOffsetViolations} " +
            $"supportRendererVisible={LastRuntimeCollisionSupportRendererVisible} meshColliderShutdown={meshColliderShutdown} " +
            $"activeRuntimeTargets={targets.Count} spawnValidationPassed={LastSpawnValidationPassed} " +
            $"spawnMode={LastSpawnMode} spawnAttempts={LastSpawnAttemptCount} spawnAccepted={LastSpawnAcceptedCount} " +
            $"spawnRejectedInsideBuilding={LastSpawnRejectedInsideBuildingCount} spawnRejectedNoGround={LastSpawnRejectedNoGroundCount} " +
            $"spawnRejectedOutOfBounds={LastSpawnRejectedOutOfBoundsCount} spawnRejectedTooCloseToBuilding={LastSpawnRejectedTooCloseToBuildingCount} " +
            $"spawnFallbackUsed={LastSpawnFallbackUsed} fallbackSafeSpawnId={LastFallbackSafeSpawnId} " +
            $"nearestBuildingDistance={LastNearestBuildingDistance:F2} buildingBoundsCached={LastBuildingBoundsCacheCount} " +
            $"spawnX={LastFinalSpawnPosition.x:F2} spawnY={LastFinalSpawnPosition.y:F2} spawnZ={LastFinalSpawnPosition.z:F2} " +
            $"supportColliderActive={LastRuntimeCollisionSupportColliderActive} supportRendererCount={LastSupportRendererCount} " +
            $"supportVisibleRenderers={LastVisibleSupportRendererCount} supportDisabledRenderers={LastSupportRendererDisabledCount} " +
            $"blueDebugGroundDisabled={LastBlueDebugGroundRendererDisabledCount} playableBoundsValid={LastPlayableBoundsValid} " +
            $"playableBoundsSource={LastPlayableBoundsSource} playableMinX={LastPlayableBounds.MinX:F2} playableMaxX={LastPlayableBounds.MaxX:F2} " +
            $"playableMinZ={LastPlayableBounds.MinZ:F2} playableMaxZ={LastPlayableBounds.MaxZ:F2} " +
            $"airWallColliders={LastPlayableAirWallColliderCount} airWallVisibleRenderers={LastPlayableAirWallVisibleRendererCount} " +
            $"roadSampleY={LastRoadSampleY:F2} roadSamples={LastRoadSampleCount} supportToRoadDelta={LastSupportToRoadDelta:F2} " +
            $"supportToBuildingBaseDelta={LastSupportToBuildingBaseDelta:F2} buildingRoadYOffsetApplied={LastBuildingRoadVerticalOffsetApplied:F2} " +
            $"buildingRoadAlignedRoots={LastBuildingRoadAlignedRootCount} buildingRoadAlignmentStatus={LastBuildingRoadAlignmentStatus} " +
            $"adaptiveGridEnabled={LastAdaptiveSupportGridEnabled} adaptiveGridActive={LastAdaptiveSupportGridActive} " +
            $"adaptiveGridCells={LastAdaptiveSupportGridCellCount} adaptiveGridColliders={LastAdaptiveSupportGridColliderCount} " +
            $"adaptiveGridVisibleRenderers={LastAdaptiveSupportGridVisibleRendererCount} adaptiveRoadCells={LastAdaptiveCellsUsingRoad} " +
            $"adaptiveTerrainCells={LastAdaptiveCellsUsingTerrain} adaptiveReliefCells={LastAdaptiveCellsUsingRelief} " +
            $"adaptiveBridgeCells={LastAdaptiveCellsUsingBridge} adaptiveBuildingFallbackCells={LastAdaptiveCellsUsingBuildingBaseFallback} " +
            $"adaptiveGlobalFallbackCells={LastAdaptiveCellsUsingGlobalFallback} adaptiveSupportYMin={LastAdaptiveSupportYMin:F2} " +
            $"adaptiveSupportYMax={LastAdaptiveSupportYMax:F2} adaptiveSupportYAvg={LastAdaptiveSupportYAverage:F2} " +
            $"adaptiveGridStatus={LastAdaptiveSupportGridStatus} safeGroundEnabled={LastSafeGroundEnabled} " +
            $"safeGroundSupportY={LastSafeGroundSupportY:F2} safeGroundColliders={LastSafeGroundColliderCount} " +
            $"safeGroundRenderers={LastSafeGroundRendererCount} safeGroundRendererHidden={LastSafeGroundRendererHidden} " +
            $"fallOutPreventionEnabled={LastFallOutPreventionEnabled} largeBlueGroundDisabled={LastLargeBlueGroundRendererDisabledCount} " +
            $"visibleLargeBlueGroundRenderers={LastVisibleLargeBlueGroundRendererCount} gameplayGroundCoverEnabled={LastGameplayGroundCoverEnabled} " +
            $"gameplayGroundCoverActive={LastGameplayGroundCoverActive} gameplayGroundCoverTiles={LastGameplayGroundCoverTileCount} " +
            $"gameplayGroundCoverColliders={LastGameplayGroundCoverColliderCount} gameplayGroundCoverRenderers={LastGameplayGroundCoverRendererCount} " +
            $"gameplayGroundCoverVisibleRenderers={LastGameplayGroundCoverVisibleRendererCount} gameplayGroundCoverY={LastGameplayGroundCoverY:F2} " +
            $"gameplayGroundCoverArea={LastGameplayGroundCoverTotalArea:F2} gameplayGroundCoverMaterial={SafeLog(LastGameplayGroundCoverMaterialName)} " +
            $"gameplayGroundCoverMaterialSource={SafeLog(LastGameplayGroundCoverMaterialSource)} gameplayGroundCoverOpacity={LastGameplayGroundCoverOpacity:F2} " +
            $"gameplayGroundCoverBlueLike={LastGameplayGroundCoverMaterialBlueLike} gameplayGroundCoverMagentaLike={LastGameplayGroundCoverMaterialMagentaLike} " +
            $"buildingSnapdownEnabled={LastBuildingSnapdownEnabled} buildingSnapdownScanned={LastBuildingSnapdownScannedCount} " +
            $"floatingBuildingCandidates={LastFloatingBuildingCandidateCount} buildingsSnappedDown={LastBuildingSnapdownMovedCount} " +
            $"buildingSnapdownSkipped={LastBuildingSnapdownSkippedCount} buildingSnapdownRemainingFloating={LastBuildingSnapdownRemainingFloatingCount} " +
            $"buildingSnapdownAverageOffset={LastBuildingSnapdownAverageOffset:F2} buildingSnapdownMaxOffset={LastBuildingSnapdownMaxOffset:F2} " +
            $"buildingSnapdownReferenceY={LastBuildingSnapdownReferenceY:F2} buildingSnapdownThreshold={LastBuildingSnapdownThresholdMeters:F2} " +
            $"buildingSnapdownStatus={SafeLog(LastBuildingSnapdownStatus)}");
    }

    private static void PrepareManualTestRoots(Dictionary<string, Transform> roots)
    {
        if (roots.TryGetValue("GameplaySupportRoot", out Transform supportRoot) && supportRoot != null)
        {
            supportRoot.gameObject.SetActive(true);
        }

        if (roots.TryGetValue("GameplayGroundCoverRoot", out Transform groundCoverRoot) && groundCoverRoot != null)
        {
            groundCoverRoot.gameObject.SetActive(true);
        }

        if (roots.TryGetValue("PlayableBoundsRoot", out Transform boundsRoot) && boundsRoot != null)
        {
            boundsRoot.gameObject.SetActive(true);
        }

        if (roots.TryGetValue("DebugDiagnosticsRoot", out Transform diagnosticsRoot) && diagnosticsRoot != null)
        {
            diagnosticsRoot.gameObject.SetActive(false);
        }
    }

    private static bool ShouldRunGameplaySelfAuditSmoke()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], GameplaySelfAuditSmokeArg, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ShouldEnableLocalTrainingProxyTargets()
    {
        return EnableLocalTrainingProxyTargetsForDiagnostics || ShouldRunGameplaySelfAuditSmoke();
    }

    private IEnumerator RunGameplaySelfAuditSmoke(
        NewMapGameController controller,
        NewMapPlayerController player,
        NewMapRuntimeUI ui,
        NewMapLightingController lighting,
        NewMapHazardController hazard,
        NewMapNpcCrowdPrototype crowd)
    {
        yield return null;

        NewMapRuntimeTarget official = null;
        NewMapRuntimeTarget recoveredNonOfficial = null;
        NewMapRuntimeTarget routeProxy = null;
        int officialCount = 0;
        int nonOfficialCount = 0;
        int routeGuideCount = 0;
        foreach (NewMapRuntimeTarget target in controller.RuntimeTargets)
        {
            if (target == null || !target.ActiveInGame)
            {
                continue;
            }

            if (target.IsOfficialShelter)
            {
                officialCount++;
                if (official == null)
                {
                    official = target;
                }
            }
            else
            {
                nonOfficialCount++;
                if (recoveredNonOfficial == null && target.Category != null && target.Category.Contains("humanitarian_candidate"))
                {
                    recoveredNonOfficial = target;
                }
            }

            if (target.RouteGuide != null || target.RouteGuideFactory != null)
            {
                routeGuideCount++;
                if (routeProxy == null)
                {
                    routeProxy = target;
                }
            }
        }

        LogGameplaySmoke("runtime_target_counts", officialCount >= 15 && nonOfficialCount >= 82, $"official={officialCount} nonOfficial={nonOfficialCount} routeGuides={routeGuideCount}");
        LogGameplaySmoke("start_menu_visible", ui != null && ui.IsStartMenuVisible, "Start Menu visible after bootstrap reset");
        NewMapNameLabelController labelController = FindObjectOfType<NewMapNameLabelController>();
        LogGameplaySmoke(
            "name_labels_offline_real_sources_only",
            labelController != null &&
            !labelController.RuntimeNetworkRequestsAllowed &&
            !labelController.IdOnlyLabelsVisibleInNormalMode &&
            !string.IsNullOrWhiteSpace(labelController.SourceNameAvailabilityStatus),
            labelController != null
                ? $"available={labelController.AvailableLabelCount} road={labelController.RoadNameLabelCount} building={labelController.BuildingNameLabelCount} status={SafeLog(labelController.SourceNameAvailabilityStatus)}"
                : "Name label controller missing");

        controller.StartTourismMode();
        yield return null;
        LogGameplaySmoke(
            "tourism_free_roam_no_failure",
            controller.Mode == NewMapGameMode.Tourism && controller.Stage == NewMapTsunamiStage.Inactive && hazard != null && !hazard.RiskChecksActive && player != null && !player.StaminaEnabled && crowd != null && crowd.CurrentCongestionDelaySeconds <= 0.001f,
            "Tourism mode disables tsunami, hazard checks, crowd failure, and stamina");

        if (player != null)
        {
            float yawBeforeNoButton = player.CurrentYaw;
            float pitchBeforeNoButton = player.CurrentPitch;
            bool noButtonApplied = player.ApplyLookInputForDiagnostics(8f, -4f, false);
            bool noButtonStayedStill =
                Mathf.Abs(player.CurrentYaw - yawBeforeNoButton) < 0.001f &&
                Mathf.Abs(player.CurrentPitch - pitchBeforeNoButton) < 0.001f;
            bool leftDragApplied = player.ApplyLookInputForDiagnostics(8f, -4f, "LeftMouse");
            bool leftDragRotated =
                Mathf.Abs(player.CurrentYaw - yawBeforeNoButton) > 0.001f &&
                Mathf.Abs(player.CurrentPitch - pitchBeforeNoButton) > 0.001f;
            player.ReleaseLookDragForDiagnostics();
            float yawBeforeRight = player.CurrentYaw;
            float pitchBeforeRight = player.CurrentPitch;
            bool rightDragApplied = player.ApplyLookInputForDiagnostics(8f, -4f, "RightMouse");
            bool rightDragRotated =
                Mathf.Abs(player.CurrentYaw - yawBeforeRight) > 0.001f &&
                Mathf.Abs(player.CurrentPitch - pitchBeforeRight) > 0.001f;
            player.ReleaseLookDragForDiagnostics();
            LogGameplaySmoke(
                "mouse_drag_look_requires_button",
                !noButtonApplied && noButtonStayedStill && leftDragApplied && leftDragRotated && rightDragApplied && rightDragRotated && !player.IsMouseLookDragging,
                $"buttons={player.LookMouseButtonName} requiresButton={player.LookRequiresMouseButton}");
            LogGameplaySmoke(
                "mouse_left_right_drag_look",
                leftDragApplied && leftDragRotated && rightDragApplied && rightDragRotated && !player.IsMouseLookDragging,
                $"allowedButtons={player.LookMouseButtonName}");
        }
        else
        {
            LogGameplaySmoke("mouse_drag_look_requires_button", false, "Player missing");
            LogGameplaySmoke("mouse_left_right_drag_look", false, "Player missing");
        }

        NewMapSpawnConfig currentSpawnConfig = spawnConfig ?? NewMapSpawnConfig.Default();
        bool spawnBuildingClear = LastBuildingBoundsCacheCount == 0 || LastNearestBuildingDistance >= currentSpawnConfig.minDistanceFromBuildingMeters;
        LogGameplaySmoke(
            "spawn_road_playable_ground_validation",
            LastSpawnValidationPassed && LastSpawnAcceptedCount == 1 && spawnBuildingClear && Mathf.Abs(LastPlayerSpawnGroundDelta) <= 0.35f,
            $"source={SafeLog(LastSpawnValidationSource)} attempts={LastSpawnAttemptCount} insideRejected={LastSpawnRejectedInsideBuildingCount} noGroundRejected={LastSpawnRejectedNoGroundCount} nearestBuildingDistance={LastNearestBuildingDistance:0.00} fallbackUsed={LastSpawnFallbackUsed}");

        if (recoveredNonOfficial != null)
        {
            bool inspected = controller.TryInteractForDiagnostics(recoveredNonOfficial.Id);
            LogGameplaySmoke(
                "tourism_non_official_inspection_warning",
                inspected && ui != null && ui.LastResultReason == "Tourism inspection" && ui.LastResultDetail.Contains("Non-official candidate") && ui.LastResultDetail.Contains("not a safety approval"),
                $"target={recoveredNonOfficial.Id} reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");
        }
        else
        {
            LogGameplaySmoke("tourism_non_official_inspection_warning", false, "No recovered non-official candidate target was active");
        }

        controller.StartEvacuationMode();
        yield return null;
        LogGameplaySmoke(
            "evacuation_stage1_warning",
            controller.Mode == NewMapGameMode.Evacuation && controller.Stage == NewMapTsunamiStage.Warning && hazard != null && !hazard.RiskChecksActive && player != null && player.StaminaEnabled,
            "Evacuation starts in Stage 1 with hazard checks inactive and stamina enabled");

        controller.SetWeather(NewMapWeatherPreset.NightClear);
        yield return null;
        float nightSkyBrightness = lighting != null ? lighting.SkyBrightness : 1f;
        float nightReadability = lighting != null ? lighting.NightBuildingReadabilityScore : 0f;
        bool nightReadable =
            lighting != null &&
            nightSkyBrightness < 0.12f &&
            nightReadability > 0.35f &&
            lighting.FillLightIntensity > 0.1f;
        controller.SetWeather(NewMapWeatherPreset.ClearDay);
        yield return null;
        bool dayRestored = lighting != null && lighting.SkyBrightness > 0.6f && lighting.DirectionalLightIntensity > 1.0f;
        LogGameplaySmoke(
            "night_lighting_dark_sky_readable_buildings",
            nightReadable && dayRestored,
            $"nightSky={nightSkyBrightness:0.000} readability={nightReadability:0.000} dayRestored={dayRestored}");

        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        bool anyGuidanceVisible = false;
        foreach (NewMapRuntimeTarget target in controller.RuntimeTargets)
        {
            if (target != null && target.GreenFrame != null && target.GreenFrame.activeSelf)
            {
                anyGuidanceVisible = true;
                break;
            }
        }

        LogGameplaySmoke(
            "evacuation_stage2_front_and_green_frames",
            hazard != null && hazard.RiskChecksActive && hazard.LightCurtainVisibleForDiagnostics && anyGuidanceVisible,
            "Stage 2 builds light curtain and activates active-target green frames");

        if (official != null)
        {
            controller.StartEvacuationMode();
            controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
            yield return null;
            bool entered = controller.TryInteractForDiagnostics(official.Id);
            string entryDetail = ui != null ? ui.LastResultDetail : string.Empty;
            bool completed = controller.CompleteSafeFloorSequenceForDiagnostics();
            LogGameplaySmoke(
                "success_official_shelter",
                entered && completed && ui != null && ui.LastResultReason == "safe_floor_reached" && entryDetail.Contains("Official Chuo shelter anchor") && entryDetail.Contains("no official route is claimed"),
                $"target={official.Id} finalReason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");
        }
        else
        {
            LogGameplaySmoke("success_official_shelter", false, "No official shelter target was active");
        }

        if (recoveredNonOfficial != null)
        {
            controller.StartEvacuationMode();
            controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
            yield return null;
            bool entered = controller.TryInteractForDiagnostics(recoveredNonOfficial.Id);
            string entryDetail = ui != null ? ui.LastResultDetail : string.Empty;
            bool completed = controller.CompleteSafeFloorSequenceForDiagnostics();
            LogGameplaySmoke(
                "success_non_official_candidate_with_warning",
                entered && completed && ui != null && ui.LastResultReason == "safe_floor_reached" && entryDetail.Contains("Non-official humanitarian candidate") && entryDetail.Contains("not a safety approval") && !recoveredNonOfficial.IsOfficialShelter,
                $"target={recoveredNonOfficial.Id} finalReason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");
        }
        else
        {
            LogGameplaySmoke("success_non_official_candidate_with_warning", false, "No recovered non-official candidate target was active");
        }

        if (routeProxy != null)
        {
            controller.StartEvacuationMode();
            controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
            yield return null;
            bool entered = controller.TryInteractForDiagnostics(routeProxy.Id);
            string detail = ui != null ? ui.LastResultDetail : string.Empty;
            LogGameplaySmoke(
                "route_proxy_wording",
                entered && detail.Contains("estimated prototype guidance") && detail.Contains("not an official evacuation route"),
                $"target={routeProxy.Id} reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");
        }
        else
        {
            LogGameplaySmoke("route_proxy_wording", false, "No runtime route proxy target was active");
        }

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        bool crowdStarted = controller.TryInteractForDiagnostics("newmap_proxy_crowd_delay");
        LogGameplaySmoke(
            "crowd_delay_success_or_failure",
            crowdStarted && ui != null && ui.LastResultDetail.Contains("Crowd delay:") && !ui.LastResultDetail.Contains("Crowd delay: 0.0s"),
            $"reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        bool blocked = controller.TryInteractForDiagnostics("newmap_proxy_blocked_entrance");
        LogGameplaySmoke("entrance_blocked_failure", blocked && ui != null && ui.LastResultReason == "entrance_blocked", $"reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        bool noFloor = controller.TryInteractForDiagnostics("newmap_proxy_no_safe_floor");
        LogGameplaySmoke("safe_floor_unavailable_failure", noFloor && ui != null && ui.LastResultReason == "safe_floor_unavailable", $"reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        if (player != null && hazard != null)
        {
            player.transform.position = hazard.DebrisCenterForDiagnostics;
        }

        bool debris = controller.TryApplyDebrisExposureForDiagnostics(5f);
        LogGameplaySmoke("collapse_debris_exposure_failure", debris && ui != null && ui.LastResultReason == "collapse_debris_exposure", $"reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Vector3 frontFailurePosition = player != null ? player.transform.position + new Vector3(-200f, 0f, 0f) : new Vector3(-200f, 0f, 0f);
        bool front = controller.TryApplyTsunamiFrontForDiagnostics(frontFailurePosition);
        LogGameplaySmoke("tsunami_front_failure", front && ui != null && ui.LastResultReason == "tsunami_front_contact", $"reason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");

        controller.StartTourismMode();
        if (player != null && hazard != null)
        {
            player.transform.position = hazard.DebrisCenterForDiagnostics;
        }

        yield return null;
        string tourismReason = ui != null ? ui.LastResultReason : string.Empty;
        yield return null;
        LogGameplaySmoke("collapse_disabled_success", controller.Mode == NewMapGameMode.Tourism && (ui == null || ui.LastResultReason == tourismReason), "Tourism mode keeps collapse/debris failure disabled");

        bool disabledSelectable = controller.TryInteractForDiagnostics("disabled_out_of_new_map_candidate_probe");
        LogGameplaySmoke("disabled_target_not_selectable", !disabledSelectable, "Disabled/out-of-map probe id is absent from runtime targets");

        if (ui != null)
        {
            controller.TryInteractForDiagnostics("newmap_proxy_safe_floor");
            yield return null;
            bool resultVisible = ui.IsResultVisible;
            ui.ToggleRules();
            yield return null;
            bool rulesVisible = ui.IsRulesVisible && ui.RulesPanelHasScrollRect;
            ui.ToggleRules();
            controller.SetPaused(true);
            yield return null;
            bool pauseVisible = ui.IsPauseVisible;
            controller.SetPaused(false);
            LogGameplaySmoke("ui_rules_pause_result_panel", rulesVisible && pauseVisible && resultVisible, "Rules scroll, pause menu, and ResultPanel are reachable");
        }
        else
        {
            LogGameplaySmoke("ui_rules_pause_result_panel", false, "Runtime UI was not created");
        }

        player?.SetControlEnabled(false);
        Debug.Log("NewMap gameplay self-audit smoke completed.");
    }

    private static void LogGameplaySmoke(string scenarioId, bool passed, string detail)
    {
        Debug.Log($"NewMap gameplay self-audit smoke: scenario={scenarioId} result={(passed ? "pass" : "fail")} detail={SafeLog(detail)}");
    }

    private static string SafeLog(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace('\n', ' ').Replace('\r', ' ').Replace('|', '/');
    }

    private string BuildDiagnosticText()
    {
        if (LastGameplayGroundCoverActive)
        {
            return
                "Ground: visible road-like gameplay ground cover with collision; this is not GIS-grade terrain/road accuracy" +
                " | Blue/fall-through gameplay gaps are covered or blocked by colliders and air walls" +
                " | Old P3/P5 targets disabled unless remapped.";
        }

        if (LastAdaptiveSupportGridActive)
        {
            return
                "Ground: adaptive invisible support grid aligned to local ground/road sample cache" +
                " | Runtime support colliders active and renderers hidden; scene MeshCollider shutdown is disabled at player startup" +
                " | Old P3/P5 targets disabled unless remapped.";
        }

        if (LastSafeGroundEnabled)
        {
            return
                "Ground: rollback-safe invisible gameplay support surface; adaptive relief grid disabled" +
                " | Runtime support collider active and renderers hidden; scene MeshCollider shutdown is disabled at player startup" +
                " | Old P3/P5 targets disabled unless remapped.";
        }

        string ground = LastUsedGroundSupportProxy
            ? "Ground: invisible runtime support proxy aligned to the round-2 fallback visual height"
            : "Ground: invisible runtime support proxy aligned to sampled visual building/ground base";
        string collisionProxy = LastRuntimeCollisionSupportProxyActive
            ? " | Runtime collision support proxy active and renderer hidden; scene MeshCollider shutdown is disabled at player startup"
            : string.Empty;
        return $"{ground}{collisionProxy} | Old P3/P5 targets disabled unless remapped.";
    }

    private void ApplyAdaptiveSupportGridDiagnostics()
    {
        LastAdaptiveSupportGridEnabled = adaptiveSupportGrid != null && adaptiveSupportGrid.Enabled;
        LastAdaptiveSupportGridActive = adaptiveSupportGrid != null && adaptiveSupportGrid.HasUsableGrid;
        LastAdaptiveSupportGridCellCount = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellCount : 0;
        LastAdaptiveSupportGridColliderCount = adaptiveSupportGrid != null ? adaptiveSupportGrid.ColliderCount : 0;
        LastAdaptiveSupportGridVisibleRendererCount = adaptiveSupportGrid != null ? adaptiveSupportGrid.VisibleRendererCount : 0;
        LastAdaptiveCellsUsingRoad = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingRoad : 0;
        LastAdaptiveCellsUsingTerrain = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingTerrain : 0;
        LastAdaptiveCellsUsingRelief = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingRelief : 0;
        LastAdaptiveCellsUsingBridge = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingBridge : 0;
        LastAdaptiveCellsUsingBuildingBaseFallback = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingBuildingBaseFallback : 0;
        LastAdaptiveCellsUsingGlobalFallback = adaptiveSupportGrid != null ? adaptiveSupportGrid.CellsUsingGlobalFallback : 0;
        LastAdaptiveSupportYMin = adaptiveSupportGrid != null ? adaptiveSupportGrid.SupportYMin : 0f;
        LastAdaptiveSupportYMax = adaptiveSupportGrid != null ? adaptiveSupportGrid.SupportYMax : 0f;
        LastAdaptiveSupportYAverage = adaptiveSupportGrid != null ? adaptiveSupportGrid.SupportYAverage : 0f;
        LastAdaptiveSupportGridStatus = adaptiveSupportGrid != null ? adaptiveSupportGrid.Status : "not_loaded";
    }

    private bool TryResolveRuntimeMapBounds(out Bounds bounds, out int rendererCount, out int colliderCount)
    {
        if (EnablePlayerRuntimeSceneWideBoundsScan)
        {
            return TryCalculateMapBounds(out bounds, out rendererCount, out colliderCount);
        }

        rendererCount = 0;
        colliderCount = 0;
        bounds = new Bounds(Vector3.zero, new Vector3(700f, 80f, 700f));
        ResetVisualGroundSamples();
        return false;
    }

    private bool TryCalculateMapBounds(out Bounds bounds, out int rendererCount, out int colliderCount)
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        Collider[] colliders = FindObjectsOfType<Collider>();
        rendererCount = 0;
        colliderCount = 0;
        bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        var baseSamples = new List<float>();
        var roadSamples = new List<float>();
        buildingAvoidanceBounds.Clear();
        ResetVisualGroundSamples();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null || IsRuntimeGeneratedOrUiRenderer(renderer))
            {
                continue;
            }

            rendererCount++;
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }

            if (IsUsableVisualGroundSample(renderer.bounds))
            {
                baseSamples.Add(renderer.bounds.min.y);
            }

            if (IsUsableRoadOrGroundSample(renderer))
            {
                roadSamples.Add(renderer.bounds.max.y);
            }

            if (IsUsableBuildingAvoidanceBounds(renderer.bounds))
            {
                buildingAvoidanceBounds.Add(renderer.bounds);
            }
        }

        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.GetComponentInParent<Canvas>() == null)
            {
                colliderCount++;
            }
        }

        if (baseSamples.Count > 0)
        {
            baseSamples.Sort();
            int referenceIndex = Mathf.Clamp(Mathf.RoundToInt((baseSamples.Count - 1) * 0.10f), 0, baseSamples.Count - 1);
            LastVisualGroundSampleValid = true;
            LastVisualGroundSampleCount = baseSamples.Count;
            LastSampledMapMinY = baseSamples[0];
            LastSampledBuildingBaseY = baseSamples[referenceIndex];
            LastVisualGroundReferenceY = LastSampledBuildingBaseY;
        }

        if (roadSamples.Count > 0)
        {
            roadSamples.Sort();
            int roadReferenceIndex = Mathf.Clamp(Mathf.RoundToInt((roadSamples.Count - 1) * 0.50f), 0, roadSamples.Count - 1);
            LastRoadSampleValid = true;
            LastRoadSampleCount = roadSamples.Count;
            LastRoadSampleY = roadSamples[roadReferenceIndex];
            if (LastBuildingRoadAlignedRootCount > 0 && Mathf.Abs(LastBuildingRoadVerticalOffsetApplied) > 0.01f)
            {
                LastSampledBuildingBaseY = LastRoadSampleY;
                LastVisualGroundReferenceY = LastRoadSampleY;
                LastBuildingRoadAlignmentStatus += "_corrected_reference_applied";
            }
            else if (Mathf.Abs(LastRoadSampleY - LastSampledBuildingBaseY) <= 1.5f)
            {
                LastVisualGroundReferenceY = LastRoadSampleY;
            }
        }

        LastBuildingBoundsCacheCount = buildingAvoidanceBounds.Count;
        LastBuildingBoundsCacheBuilt = LastBuildingBoundsCacheCount > 0;

        return hasBounds && bounds.size.sqrMagnitude > 1f;
    }

    private Vector3 ResolveSpawnPosition(Bounds mapBounds, bool hasBounds, Transform diagnosticsRoot)
    {
        ResetSpawnValidationDiagnostics();
        NewMapSpawnConfig config = spawnConfig ?? NewMapSpawnConfig.Default();
        LastSpawnMode = string.IsNullOrWhiteSpace(config.spawnMode)
            ? "road_or_playable_ground_only"
            : config.spawnMode;

        Vector3 basePosition = hasBounds ? mapBounds.center : Vector3.zero;
        float rayStartY = hasBounds ? mapBounds.max.y + 250f : 250f;
        float supportSurfaceY = ResolveSupportSurfaceY(mapBounds, hasBounds, basePosition, rayStartY);
        LastRuntimeGroundSurfaceY = supportSurfaceY;
        LastSupportToVisualGroundDelta = Mathf.Abs(LastRuntimeGroundSurfaceY - LastVisualGroundReferenceY);
        LastSupportToRoadDelta = LastRoadSampleValid ? Mathf.Abs(LastRuntimeGroundSurfaceY - LastRoadSampleY) : -1f;
        LastSupportToBuildingBaseDelta = Mathf.Abs(LastRuntimeGroundSurfaceY - LastSampledBuildingBaseY);
        bool hasSupportSurface = LastVisualGroundSampleValid || hasBounds || LastUsedGroundSupportProxy;

        Vector3 candidate = new Vector3(basePosition.x, 0f, basePosition.z);
        float candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
        candidate.y = candidateSupportY + GroundSkinOffset;
        if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "map_bounds_center", out Vector3 accepted))
        {
            return accepted;
        }

        var random = new System.Random(config.spawnRandomSeed);
        int maxAttempts = Mathf.Clamp(config.maxSpawnAttempts, 1, 2000);
        float radius = Mathf.Clamp(config.spawnRadiusMeters, 25f, 2500f);
        while (config.randomSpawnEnabled && LastSpawnAttemptCount < maxAttempts)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
            candidate = basePosition + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
            candidate.y = candidateSupportY + GroundSkinOffset;

            if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "random_playable_support", out accepted))
            {
                return accepted;
            }
        }

        LastSpawnFallbackUsed = true;
        foreach (NewMapSafeSpawnPointRecord safePoint in GetSafeSpawnPointRecords())
        {
            if (safePoint == null || string.IsNullOrWhiteSpace(safePoint.id))
            {
                continue;
            }

            candidate = new Vector3(safePoint.position.x, 0f, safePoint.position.z);
            candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
            candidate.y = candidateSupportY + GroundSkinOffset;
            if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "fallback_safe_spawn:" + safePoint.id, out accepted))
            {
                LastFallbackSafeSpawnId = safePoint.id;
                return accepted;
            }
        }

        LastSpawnValidationPassed = false;
        LastFallbackSafeSpawnId = string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId) ? "none" : config.fallbackSafeSpawnId;
        LastNearestBuildingDistance = CalculateNearestBuildingDistance(candidate);
        LastSpawnValidationSource = "unvalidated_last_resort_support_center";
        LastRuntimeGroundSurfaceY = ResolveLocalSupportSurfaceY(basePosition, supportSurfaceY);
        return new Vector3(basePosition.x, LastRuntimeGroundSurfaceY + GroundSkinOffset, basePosition.z);
    }

    private float ResolveSupportSurfaceY(Bounds mapBounds, bool hasBounds, Vector3 basePosition, float rayStartY)
    {
        NewMapGameplayGroundCoverConfig coverConfig = groundCoverConfig ?? NewMapGameplayGroundCoverConfig.Default();
        if (coverConfig.enabled && coverConfig.forceFixedCoverY)
        {
            float coverY = Mathf.Clamp(coverConfig.coverY, -20f, 30f);
            LastGameplayGroundCoverEnabled = true;
            LastGameplayGroundCoverY = coverY;
            LastSafeGroundEnabled = true;
            LastSafeGroundSupportY = coverY;
            LastFallOutPreventionEnabled = true;
            LastUsedGroundSupportProxy = true;
            if (!LastVisualGroundSampleValid)
            {
                LastSampledMapMinY = hasBounds ? mapBounds.min.y : coverY;
                LastSampledBuildingBaseY = coverY;
                LastVisualGroundReferenceY = coverY;
                LastVisualGroundSampleValid = true;
            }

            return coverY;
        }

        NewMapSafeGroundConfig groundConfig = safeGroundConfig ?? NewMapSafeGroundConfig.Default();
        if (groundConfig.enabled && groundConfig.forceFixedSupportY)
        {
            float rollbackSupportY = Mathf.Clamp(groundConfig.supportY, -20f, 30f);
            LastSafeGroundEnabled = true;
            LastSafeGroundSupportY = rollbackSupportY;
            LastFallOutPreventionEnabled = groundConfig.fallRecoveryEnabled;
            LastUsedGroundSupportProxy = true;
            if (!LastVisualGroundSampleValid)
            {
                LastSampledMapMinY = hasBounds ? mapBounds.min.y : rollbackSupportY;
                LastSampledBuildingBaseY = rollbackSupportY;
                LastVisualGroundReferenceY = rollbackSupportY;
                LastVisualGroundSampleValid = true;
            }

            return rollbackSupportY;
        }

        if (adaptiveSupportGrid != null && adaptiveSupportGrid.TryResolveSupportY(basePosition, out float adaptiveSupportY, out _))
        {
            LastUsedGroundSupportProxy = false;
            LastVisualGroundSampleValid = true;
            LastVisualGroundReferenceY = adaptiveSupportY;
            return adaptiveSupportY;
        }

        Vector3[] offsets =
        {
            Vector3.zero,
            new Vector3(20f, 0f, 20f),
            new Vector3(-20f, 0f, 20f),
            new Vector3(20f, 0f, -20f),
            new Vector3(-20f, 0f, -20f)
        };

        float supportSurfaceY;
        if (LastVisualGroundSampleValid)
        {
            supportSurfaceY = LastVisualGroundReferenceY;
            LastUsedGroundSupportProxy = false;
            return supportSurfaceY;
        }

        foreach (Vector3 offset in offsets)
        {
            Vector3 origin = new Vector3(basePosition.x + offset.x, rayStartY, basePosition.z + offset.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore) &&
                hit.point.y > -5f &&
                hit.point.y < 8f)
            {
                LastUsedGroundSupportProxy = false;
                supportSurfaceY = hit.point.y;
                LastSampledMapMinY = supportSurfaceY;
                LastSampledBuildingBaseY = supportSurfaceY;
                LastVisualGroundReferenceY = supportSurfaceY;
                return supportSurfaceY;
            }
        }

        LastUsedGroundSupportProxy = true;
        supportSurfaceY = hasBounds ? Mathf.Max(Round2FallbackSupportSurfaceY, mapBounds.min.y) : Round2FallbackSupportSurfaceY;
        LastSampledMapMinY = hasBounds ? mapBounds.min.y : supportSurfaceY;
        LastSampledBuildingBaseY = supportSurfaceY;
        LastVisualGroundReferenceY = supportSurfaceY;
        return supportSurfaceY;
    }

    private float ResolveLocalSupportSurfaceY(Vector3 position, float fallbackY)
    {
        if (adaptiveSupportGrid != null && adaptiveSupportGrid.Enabled && adaptiveSupportGrid.HasUsableGrid)
        {
            return adaptiveSupportGrid.ResolveSupportY(position, fallbackY);
        }

        return fallbackY;
    }

    private void ResetSpawnValidationDiagnostics()
    {
        LastSpawnAttemptCount = 0;
        LastSpawnAcceptedCount = 0;
        LastSpawnRejectedInsideBuildingCount = 0;
        LastSpawnRejectedNoGroundCount = 0;
        LastSpawnRejectedOutOfBoundsCount = 0;
        LastSpawnRejectedTooCloseToBuildingCount = 0;
        LastSpawnFallbackUsed = false;
        LastSpawnValidationPassed = false;
        LastFallbackSafeSpawnId = "none";
        LastSpawnValidationSource = string.Empty;
        LastFinalSpawnPosition = Vector3.zero;
        LastNearestBuildingDistance = 9999f;
    }

    private IEnumerable<NewMapSafeSpawnPointRecord> GetSafeSpawnPointRecords()
    {
        NewMapSafeSpawnPointDataset dataset = safeSpawnDataset ?? NewMapSafeSpawnPointDataset.Default();
        if (dataset.records == null || dataset.records.Length == 0)
        {
            dataset = NewMapSafeSpawnPointDataset.Default();
        }

        for (int i = 0; i < dataset.records.Length; i++)
        {
            yield return dataset.records[i];
        }
    }

    private bool TryValidateSpawnCandidate(
        Vector3 candidate,
        float supportSurfaceY,
        bool hasSupportSurface,
        Bounds mapBounds,
        bool hasBounds,
        NewMapSpawnConfig config,
        string source,
        out Vector3 accepted)
    {
        LastSpawnAttemptCount++;
        accepted = Vector3.zero;

        if (!TryResolveGroundedSpawn(candidate, supportSurfaceY, hasSupportSurface, LastAdaptiveSupportGridActive, config, out Vector3 grounded))
        {
            LastSpawnRejectedNoGroundCount++;
            return false;
        }

        if (!IsInsidePlayableBounds(grounded, mapBounds, hasBounds) ||
            (LastPlayableBoundsValid && !LastPlayableBounds.ContainsXZ(grounded, config.minDistanceFromAirWallMeters)))
        {
            LastSpawnRejectedOutOfBoundsCount++;
            return false;
        }

        float nearestDistance = CalculateNearestBuildingDistance(grounded);
        if (config.useBuildingBoundsRejection && IsInsideBuildingBounds(grounded))
        {
            LastNearestBuildingDistance = nearestDistance;
            LastSpawnRejectedInsideBuildingCount++;
            return false;
        }

        if (config.useBuildingBoundsRejection && nearestDistance < config.minDistanceFromBuildingMeters)
        {
            LastNearestBuildingDistance = nearestDistance;
            LastSpawnRejectedTooCloseToBuildingCount++;
            return false;
        }

        accepted = grounded;
        LastSpawnAcceptedCount++;
        LastSpawnValidationPassed = true;
        LastSpawnValidationSource = source;
        LastNearestBuildingDistance = nearestDistance;
        LastFinalSpawnPosition = accepted;
        LastRuntimeGroundSurfaceY = supportSurfaceY;
        return true;
    }

    private static bool TryResolveGroundedSpawn(
        Vector3 candidate,
        float supportSurfaceY,
        bool hasSupportSurface,
        bool preferSupportSurface,
        NewMapSpawnConfig config,
        out Vector3 grounded)
    {
        grounded = new Vector3(candidate.x, supportSurfaceY + GroundSkinOffset, candidate.z);
        if (!IsFiniteVector3(grounded) || supportSurfaceY < -20f || supportSurfaceY > 30f)
        {
            return false;
        }

        if (preferSupportSurface && hasSupportSurface && config.useGroundSupportFallback)
        {
            return true;
        }

        if (config.useGroundProbe)
        {
            Vector3 origin = new Vector3(candidate.x, Mathf.Max(candidate.y + 64f, supportSurfaceY + 320f), candidate.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 700f, ~0, QueryTriggerInteraction.Ignore) &&
                hit.point.y > -20f &&
                hit.point.y < 30f &&
                Mathf.Abs(hit.point.y - supportSurfaceY) <= 1.25f)
            {
                grounded = hit.point + Vector3.up * GroundSkinOffset;
                return true;
            }
        }

        return config.useGroundSupportFallback && hasSupportSurface;
    }

    private static bool IsInsidePlayableBounds(Vector3 position, Bounds mapBounds, bool hasBounds)
    {
        if (!hasBounds)
        {
            return true;
        }

        if (mapBounds.size.x < 100f || mapBounds.size.z < 100f)
        {
            return true;
        }

        const float tolerance = 2f;
        return position.x >= mapBounds.min.x - tolerance &&
            position.x <= mapBounds.max.x + tolerance &&
            position.z >= mapBounds.min.z - tolerance &&
            position.z <= mapBounds.max.z + tolerance &&
            position.y >= -20f &&
            position.y <= 30f;
    }

    private bool IsInsideBuildingBounds(Vector3 position)
    {
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds bounds = buildingAvoidanceBounds[i];
            if (position.x >= bounds.min.x &&
                position.x <= bounds.max.x &&
                position.z >= bounds.min.z &&
                position.z <= bounds.max.z)
            {
                return true;
            }
        }

        return false;
    }

    private float CalculateNearestBuildingDistance(Vector3 position)
    {
        if (buildingAvoidanceBounds.Count == 0)
        {
            return 9999f;
        }

        float nearest = 9999f;
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds bounds = buildingAvoidanceBounds[i];
            float dx = AxisDistance(position.x, bounds.min.x, bounds.max.x);
            float dz = AxisDistance(position.z, bounds.min.z, bounds.max.z);
            nearest = Mathf.Min(nearest, Mathf.Sqrt(dx * dx + dz * dz));
        }

        return nearest;
    }

    private static float AxisDistance(float value, float min, float max)
    {
        if (value < min)
        {
            return min - value;
        }

        if (value > max)
        {
            return value - max;
        }

        return 0f;
    }

    private void ResetVisualGroundSamples()
    {
        LastVisualGroundSampleValid = false;
        LastVisualGroundSampleCount = 0;
        LastSampledMapMinY = Round2FallbackSupportSurfaceY;
        LastSampledBuildingBaseY = Round2FallbackSupportSurfaceY;
        LastVisualGroundReferenceY = Round2FallbackSupportSurfaceY;
        LastSupportToVisualGroundDelta = 0f;
        LastRoadSampleY = Round2FallbackSupportSurfaceY;
        LastRoadSampleCount = 0;
        LastRoadSampleValid = false;
    }

    private static bool IsRuntimeGeneratedOrUiRenderer(Renderer renderer)
    {
        Transform current = renderer.transform;
        while (current != null)
        {
            string name = current.name;
            if (name.StartsWith("NewMap_", System.StringComparison.Ordinal) ||
                IsRuntimeRootName(name) ||
                name.Contains("Marker") ||
                name.Contains("Label") ||
                name.Contains("UI") ||
                name.Contains("DebugDiagnostics") ||
                name.Contains("GameplaySupport") ||
                name.Contains("PlayableBounds"))
            {
                return true;
            }

            current = current.parent;
        }

        return renderer is LineRenderer || renderer is TrailRenderer;
    }

    private static bool IsRuntimeRootName(string name)
    {
        switch (name)
        {
            case "RuntimeSystemsRoot":
            case "PlayerSpawnRoot":
            case "ShelterMarkerRoot":
            case "CandidateMarkerRoot":
            case "HazardVisualRoot":
            case "NavigationRoot":
            case "CrowdRoot":
            case "CollapseDebrisRoot":
            case "GreenFrameRoot":
            case "UIAnchorRoot":
            case "DebugDiagnosticsRoot":
            case "GameplaySupportRoot":
            case "GameplayGroundCoverRoot":
            case "PlayableBoundsRoot":
            case "PerformanceMetricsRoot":
                return true;
            default:
                return false;
        }
    }

    private static bool IsUsableRoadOrGroundSample(Renderer renderer)
    {
        if (renderer == null || IsRuntimeGeneratedOrUiRenderer(renderer))
        {
            return false;
        }

        Bounds bounds = renderer.bounds;
        if (!IsFiniteVector3(bounds.center) || !IsFiniteVector3(bounds.min) || !IsFiniteVector3(bounds.max))
        {
            return false;
        }

        if (bounds.size.x < 2f || bounds.size.z < 2f || bounds.size.y > 1.25f)
        {
            return false;
        }

        string searchable = BuildRendererSearchText(renderer).ToLowerInvariant();
        bool semanticName =
            searchable.Contains("road") ||
            searchable.Contains("street") ||
            searchable.Contains("tran") ||
            searchable.Contains("traffic") ||
            searchable.Contains("ground") ||
            searchable.Contains("terrain") ||
            searchable.Contains("surface") ||
            searchable.Contains("dem");

        return semanticName && bounds.max.y > -20f && bounds.max.y < 30f;
    }

    private static string BuildRendererSearchText(Renderer renderer)
    {
        var builder = new System.Text.StringBuilder();
        Transform current = renderer.transform;
        while (current != null)
        {
            builder.Append(current.name).Append(' ');
            current = current.parent;
        }

        Material material = renderer.sharedMaterial;
        if (material != null)
        {
            builder.Append(material.name).Append(' ');
            if (material.shader != null)
            {
                builder.Append(material.shader.name);
            }
        }

        return builder.ToString();
    }

    private static bool IsUsableVisualGroundSample(Bounds bounds)
    {
        if (!IsFiniteVector3(bounds.center) || !IsFiniteVector3(bounds.min) || !IsFiniteVector3(bounds.max))
        {
            return false;
        }

        if (bounds.size.y < 0.2f || bounds.size.x < 0.5f || bounds.size.z < 0.5f)
        {
            return false;
        }

        return bounds.min.y > -20f && bounds.min.y < 30f;
    }

    private static bool IsUsableBuildingAvoidanceBounds(Bounds bounds)
    {
        if (!IsFiniteVector3(bounds.center) || !IsFiniteVector3(bounds.min) || !IsFiniteVector3(bounds.max))
        {
            return false;
        }

        if (bounds.size.y < 1.0f || bounds.size.x < 0.5f || bounds.size.z < 0.5f)
        {
            return false;
        }

        if (bounds.size.x > 400f || bounds.size.z > 400f)
        {
            return false;
        }

        return bounds.min.y > -20f && bounds.min.y < 80f;
    }

    private void ApplyRound3BuildingRoadVerticalAlignment()
    {
        LastBuildingRoadVerticalOffsetApplied = 0f;
        LastBuildingRoadAlignedRootCount = 0;
        LastBuildingRoadAlignmentStatus = "no_action";

        Renderer[] renderers = FindObjectsOfType<Renderer>();
        var roadSamples = new List<float>();
        var buildingSamples = new List<float>();
        var buildingTransforms = new HashSet<Transform>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null || IsRuntimeGeneratedOrUiRenderer(renderer))
            {
                continue;
            }

            if (IsUsableRoadOrGroundSample(renderer))
            {
                roadSamples.Add(renderer.bounds.max.y);
                continue;
            }

            if (IsBuildingRendererCandidate(renderer) && IsUsableBuildingAvoidanceBounds(renderer.bounds))
            {
                buildingSamples.Add(renderer.bounds.min.y);
                if (renderer.transform != null)
                {
                    buildingTransforms.Add(renderer.transform);
                }
            }
        }

        if (roadSamples.Count < 32 || buildingSamples.Count < 32 || buildingTransforms.Count == 0)
        {
            LastBuildingRoadAlignmentStatus = "insufficient_road_or_building_samples_source_relief_alignment_disabled";
            return;
        }

        roadSamples.Sort();
        buildingSamples.Sort();
        float roadY = roadSamples[Mathf.Clamp(Mathf.RoundToInt((roadSamples.Count - 1) * 0.50f), 0, roadSamples.Count - 1)];
        float buildingBaseY = buildingSamples[Mathf.Clamp(Mathf.RoundToInt((buildingSamples.Count - 1) * 0.10f), 0, buildingSamples.Count - 1)];
        float offset = buildingBaseY - roadY;
        if (Mathf.Abs(offset) < 0.5f)
        {
            LastBuildingRoadAlignmentStatus = "already_aligned";
            return;
        }

        if (Mathf.Abs(offset) > 5f)
        {
            LastBuildingRoadAlignmentStatus = "offset_too_large_documented_only";
            return;
        }

        foreach (Transform root in buildingTransforms)
        {
            if (root == null)
            {
                continue;
            }

            root.position -= Vector3.up * offset;
            LastBuildingRoadAlignedRootCount++;
        }

        LastBuildingRoadVerticalOffsetApplied = offset;
        LastBuildingRoadAlignmentStatus = "building_roots_shifted_to_transport_ground_reference";
        Physics.SyncTransforms();
    }

    private bool TryApplySourceSampleBuildingVerticalAlignment(List<float> buildingSamples, HashSet<Transform> buildingTransforms)
    {
        if (buildingSamples == null || buildingSamples.Count < 32 || buildingTransforms == null || buildingTransforms.Count == 0)
        {
            return false;
        }

        NewMapGroundRoadHeightSampleCache cache = NewMapGroundRoadHeightSampleCache.Load();
        if (cache.samples == null || cache.samples.Length < 8)
        {
            return false;
        }

        var sourceGroundSamples = new List<float>();
        for (int i = 0; i < cache.samples.Length; i++)
        {
            NewMapGroundRoadHeightSample sample = cache.samples[i];
            if (sample == null || !sample.usableForSupport || sample.confidence < 0.45f)
            {
                continue;
            }

            string category = NewMapAdaptiveSupportGridRuntime.NormalizeCategory(sample.category);
            if (category == "road" || category == "terrain" || category == "relief" || category == "bridge")
            {
                sourceGroundSamples.Add(sample.position.y);
            }
        }

        if (sourceGroundSamples.Count < 8)
        {
            return false;
        }

        sourceGroundSamples.Sort();
        float sourceRange = sourceGroundSamples[sourceGroundSamples.Count - 1] - sourceGroundSamples[0];
        if (sourceRange > 6f)
        {
            LastBuildingRoadAlignmentStatus = "source_ground_varies_too_much_for_global_building_shift";
            return true;
        }

        buildingSamples.Sort();
        float sourceGroundY = Percentile(sourceGroundSamples, 0.50f);
        float buildingBaseY = Percentile(buildingSamples, 0.10f);
        float offset = buildingBaseY - sourceGroundY;
        if (Mathf.Abs(offset) < 0.5f)
        {
            LastBuildingRoadAlignmentStatus = "source_ground_and_building_bases_already_aligned";
            return true;
        }

        if (Mathf.Abs(offset) > 5f)
        {
            LastBuildingRoadAlignmentStatus = "source_ground_offset_too_large_documented_only";
            return true;
        }

        foreach (Transform root in buildingTransforms)
        {
            if (root == null)
            {
                continue;
            }

            root.position -= Vector3.up * offset;
            LastBuildingRoadAlignedRootCount++;
        }

        LastBuildingRoadVerticalOffsetApplied = offset;
        LastBuildingRoadAlignmentStatus = "building_roots_shifted_to_source_ground_road_reference";
        Physics.SyncTransforms();
        return true;
    }

    private void ApplyFloatingBuildingSnapdownToGameplayGroundCover()
    {
        ResetBuildingSnapdownDiagnostics();
        NewMapFloatingBuildingSnapdownConfig config = buildingSnapdownConfig ?? NewMapFloatingBuildingSnapdownConfig.Default();
        LastBuildingSnapdownEnabled = config.enabled;
        LastBuildingSnapdownThresholdMeters = config.floatingGapThresholdMeters;

        NewMapGameplayGroundCoverConfig coverConfig = groundCoverConfig ?? NewMapGameplayGroundCoverConfig.Default();
        float referenceY = coverConfig.enabled && coverConfig.forceFixedCoverY
            ? Mathf.Clamp(coverConfig.coverY, -20f, 30f)
            : Mathf.Clamp(LastGameplayGroundCoverY, -20f, 30f);
        LastBuildingSnapdownReferenceY = referenceY;

        if (!config.enabled)
        {
            LastBuildingSnapdownStatus = "disabled_by_config";
            return;
        }

        Dictionary<Transform, BuildingSnapdownGroup> groups = CollectBuildingSnapdownGroups();
        LastBuildingSnapdownScannedCount = groups.Count;
        if (groups.Count == 0)
        {
            LastBuildingSnapdownStatus = "no_building_like_renderer_groups_found";
            return;
        }

        float totalOffset = 0f;
        foreach (BuildingSnapdownGroup group in groups.Values)
        {
            if (group == null || group.Root == null || !group.HasBounds)
            {
                LastBuildingSnapdownSkippedCount++;
                continue;
            }

            float gap = group.Bounds.min.y - referenceY;
            if (gap < config.floatingGapThresholdMeters)
            {
                continue;
            }

            LastFloatingBuildingCandidateCount++;
            bool safeToMove =
                group.RendererCount > 0 &&
                gap <= config.maxSnapdownMeters &&
                !ContainsSnapdownExcludedText(GetTransformPath(group.Root).ToLowerInvariant());
            if (!safeToMove)
            {
                LastBuildingSnapdownSkippedCount++;
                LastBuildingSnapdownRemainingFloatingCount++;
                continue;
            }

            group.Root.position -= Vector3.up * gap;
            LastBuildingSnapdownMovedCount++;
            totalOffset += gap;
            LastBuildingSnapdownMaxOffset = Mathf.Max(LastBuildingSnapdownMaxOffset, gap);
        }

        if (LastBuildingSnapdownMovedCount > 0)
        {
            LastBuildingSnapdownAverageOffset = totalOffset / LastBuildingSnapdownMovedCount;
            LastBuildingSnapdownStatus = LastBuildingSnapdownSkippedCount > 0
                ? "floating_buildings_snapped_to_gameplay_ground_cover_with_skips"
                : "floating_buildings_snapped_to_gameplay_ground_cover";
            Physics.SyncTransforms();
        }
        else if (LastFloatingBuildingCandidateCount > 0)
        {
            LastBuildingSnapdownStatus = "floating_candidates_found_but_skipped_by_safety_limits";
        }
        else
        {
            LastBuildingSnapdownStatus = "no_floating_buildings_above_threshold";
        }
    }

    private Dictionary<Transform, BuildingSnapdownGroup> CollectBuildingSnapdownGroups()
    {
        var groups = new Dictionary<Transform, BuildingSnapdownGroup>();
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!IsBuildingSnapdownRendererCandidate(renderer))
            {
                continue;
            }

            Transform root = ResolveBuildingSnapdownRoot(renderer.transform);
            if (root == null || IsRuntimeRootName(root.name) || string.Equals(root.name, "MapRoot", System.StringComparison.Ordinal))
            {
                continue;
            }

            if (!groups.TryGetValue(root, out BuildingSnapdownGroup group))
            {
                group = new BuildingSnapdownGroup
                {
                    Root = root,
                    ObjectPath = GetTransformPath(root)
                };
                groups[root] = group;
            }

            group.Add(renderer.bounds);
        }

        return groups;
    }

    private static bool IsBuildingSnapdownRendererCandidate(Renderer renderer)
    {
        if (renderer == null || !renderer.enabled || renderer.GetComponentInParent<Canvas>() != null || IsRuntimeGeneratedOrUiRenderer(renderer))
        {
            return false;
        }

        if (renderer is LineRenderer || renderer is TrailRenderer || IsUsableRoadOrGroundSample(renderer))
        {
            return false;
        }

        Bounds bounds = renderer.bounds;
        if (!IsUsableBuildingAvoidanceBounds(bounds))
        {
            return false;
        }

        string searchable = BuildRendererSearchText(renderer).ToLowerInvariant();
        if (ContainsSnapdownExcludedText(searchable))
        {
            return false;
        }

        return IsBuildingRendererCandidate(renderer) ||
            searchable.Contains("bldg") ||
            searchable.Contains("building");
    }

    private static Transform ResolveBuildingSnapdownRoot(Transform rendererTransform)
    {
        Transform current = rendererTransform;
        Transform best = null;
        while (current != null)
        {
            if (IsRuntimeRootName(current.name) || string.Equals(current.name, "MapRoot", System.StringComparison.Ordinal))
            {
                break;
            }

            if (IsStrongBuildingRootName(current.name))
            {
                best = current;
            }

            current = current.parent;
        }

        return best != null ? best : rendererTransform;
    }

    private static bool IsStrongBuildingRootName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        string lower = name.ToLowerInvariant();
        return lower.StartsWith("bldg_", System.StringComparison.Ordinal) ||
            lower.Contains("bldg_") ||
            lower.Contains("building");
    }

    private static bool ContainsSnapdownExcludedText(string searchable)
    {
        if (string.IsNullOrWhiteSpace(searchable))
        {
            return false;
        }

        return searchable.Contains("road") ||
            searchable.Contains("street") ||
            searchable.Contains("tran") ||
            searchable.Contains("traffic") ||
            searchable.Contains("ground") ||
            searchable.Contains("terrain") ||
            searchable.Contains("relief") ||
            searchable.Contains("water") ||
            searchable.Contains("river") ||
            searchable.Contains("sea") ||
            searchable.Contains("support") ||
            searchable.Contains("collider") ||
            searchable.Contains("airwall") ||
            searchable.Contains("playablebounds") ||
            searchable.Contains("marker") ||
            searchable.Contains("label") ||
            searchable.Contains("greenframe") ||
            searchable.Contains("npc") ||
            searchable.Contains("player") ||
            searchable.Contains("ui") ||
            searchable.Contains("hazard") ||
            searchable.Contains("tsunami");
    }

    private static string GetTransformPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        var names = new List<string>();
        Transform current = transform;
        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names.ToArray());
    }

    private void ResetBuildingSnapdownDiagnostics()
    {
        LastBuildingSnapdownEnabled = false;
        LastBuildingSnapdownScannedCount = 0;
        LastFloatingBuildingCandidateCount = 0;
        LastBuildingSnapdownMovedCount = 0;
        LastBuildingSnapdownSkippedCount = 0;
        LastBuildingSnapdownRemainingFloatingCount = 0;
        LastBuildingSnapdownAverageOffset = 0f;
        LastBuildingSnapdownMaxOffset = 0f;
        LastBuildingSnapdownReferenceY = 0f;
        LastBuildingSnapdownThresholdMeters = 0f;
        LastBuildingSnapdownStatus = "not_evaluated";
    }

    private static float Percentile(List<float> sortedValues, float percentile)
    {
        if (sortedValues == null || sortedValues.Count == 0)
        {
            return 0f;
        }

        int index = Mathf.Clamp(Mathf.RoundToInt((sortedValues.Count - 1) * Mathf.Clamp01(percentile)), 0, sortedValues.Count - 1);
        return sortedValues[index];
    }

    private static bool IsBuildingRendererCandidate(Renderer renderer)
    {
        Transform current = renderer.transform;
        while (current != null)
        {
            string name = current.name.ToLowerInvariant();
            if (name.StartsWith("bldg_") || name.Contains("/bldg_") || name.Contains("building"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void EnforceSupportSurfaceVisibility(Dictionary<string, Transform> roots)
    {
        LastSupportRendererCount = 0;
        LastVisibleSupportRendererCount = 0;
        LastSupportRendererDisabledCount = 0;
        LastBlueDebugGroundRendererDisabledCount = 0;
        LastRuntimeCollisionSupportRendererVisible = false;
        LastRuntimeCollisionSupportColliderActive = false;
        LastLargeBlueGroundRendererDisabledCount = 0;
        LastVisibleLargeBlueGroundRendererCount = 0;

        if (roots != null && roots.TryGetValue("GameplaySupportRoot", out Transform supportRoot) && supportRoot != null)
        {
            Renderer[] supportRenderers = supportRoot.GetComponentsInChildren<Renderer>(true);
            LastSupportRendererCount = supportRenderers.Length;
            for (int i = 0; i < supportRenderers.Length; i++)
            {
                Renderer renderer = supportRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    LastSupportRendererDisabledCount++;
                }
            }

            Collider[] supportColliders = supportRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < supportColliders.Length; i++)
            {
                if (supportColliders[i] != null && supportColliders[i].enabled)
                {
                    LastRuntimeCollisionSupportColliderActive = true;
                    break;
                }
            }
        }

        if (roots != null && roots.TryGetValue("GameplayGroundCoverRoot", out Transform groundCoverRoot) && groundCoverRoot != null)
        {
            Collider[] groundCoverColliders = groundCoverRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < groundCoverColliders.Length; i++)
            {
                if (groundCoverColliders[i] != null && groundCoverColliders[i].enabled)
                {
                    LastRuntimeCollisionSupportProxyActive = true;
                    LastRuntimeCollisionSupportColliderActive = true;
                    break;
                }
            }
        }

        Renderer[] renderers = FindObjectsOfType<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            bool debugSupport = IsDebugSupportGroundCandidate(renderer.transform);
            bool largeBlueGround = IsLargeBlueGroundSurfaceCandidate(renderer);
            if (!debugSupport && !largeBlueGround)
            {
                continue;
            }

            bool blueSupport = IsBlueishMaterial(renderer);
            if (renderer.enabled)
            {
                renderer.enabled = false;
                LastSupportRendererDisabledCount++;
                if (blueSupport)
                {
                    LastBlueDebugGroundRendererDisabledCount++;
                }

                if (largeBlueGround)
                {
                    LastLargeBlueGroundRendererDisabledCount++;
                }
            }

            if (renderer.enabled)
            {
                LastVisibleSupportRendererCount++;
                LastRuntimeCollisionSupportRendererVisible = true;
                if (largeBlueGround)
                {
                    LastVisibleLargeBlueGroundRendererCount++;
                }
            }
        }
    }

    private static bool IsDebugSupportGroundCandidate(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            string name = current.name.ToLowerInvariant();
            if (name.Contains("gameplaysupport") ||
                name.Contains("runtimesupport") ||
                name.Contains("groundsupport") ||
                name.Contains("supportproxy") ||
                name.Contains("collisionproxy") ||
                name.Contains("colliderdebug") ||
                name.Contains("debugground") ||
                name.Contains("lowspec") ||
                name.Contains("testplane"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsBlueishMaterial(Renderer renderer)
    {
        Material material = renderer.sharedMaterial;
        if (material == null || !material.HasProperty("_Color"))
        {
            return false;
        }

        Color color = material.color;
        return color.b > 0.45f && color.b > color.r * 1.35f && color.b > color.g * 1.15f;
    }

    private static bool IsLargeBlueGroundSurfaceCandidate(Renderer renderer)
    {
        if (renderer == null || !renderer.enabled || !IsBlueishMaterial(renderer))
        {
            return false;
        }

        if (renderer.GetComponentInParent<Canvas>() != null || renderer is LineRenderer || renderer is TrailRenderer)
        {
            return false;
        }

        Bounds bounds = renderer.bounds;
        if (!IsFiniteVector3(bounds.center) || !IsFiniteVector3(bounds.size))
        {
            return false;
        }

        bool largeFlatSurface = bounds.size.y <= 2.5f && (bounds.size.x >= 25f || bounds.size.z >= 25f);
        if (!largeFlatSurface)
        {
            return false;
        }

        string searchable = BuildRendererSearchText(renderer).ToLowerInvariant();
        if (searchable.Contains("sky") || searchable.Contains("label") || searchable.Contains("marker") || searchable.Contains("ui"))
        {
            return false;
        }

        return true;
    }

    private NewMapPlayableBounds ResolvePlayableBounds(Bounds mapBounds, bool hasBounds, NewMapPlayableBoundsConfig config)
    {
        config = config ?? NewMapPlayableBoundsConfig.Default();
        if (config.manualBoundsEnabled)
        {
            LastPlayableBoundsSource = "manual_configured_bounds";
            return new NewMapPlayableBounds(
                Mathf.Min(config.minX, config.maxX),
                Mathf.Max(config.minX, config.maxX),
                Mathf.Min(config.minZ, config.maxZ),
                Mathf.Max(config.minZ, config.maxZ),
                config.marginMeters,
                config.boundaryHeightMeters,
                config.boundaryThicknessMeters).WithAppliedMargin();
        }

        if (config.autoDetectFromMapBounds && hasBounds && mapBounds.size.x >= 100f && mapBounds.size.z >= 100f)
        {
            LastPlayableBoundsSource = "auto_detected_map_renderer_bounds";
            return new NewMapPlayableBounds(
                mapBounds.min.x,
                mapBounds.max.x,
                mapBounds.min.z,
                mapBounds.max.z,
                config.marginMeters,
                config.boundaryHeightMeters,
                config.boundaryThicknessMeters).WithAppliedMargin();
        }

        LastPlayableBoundsSource = "documented_newmap_fallback_bounds";
        return NewMapPlayableBounds.DefaultDocumented(
            config.marginMeters,
            config.boundaryHeightMeters,
            config.boundaryThicknessMeters).WithAppliedMargin();
    }

    private void EnsurePlayableBoundsAirWalls(Transform root, NewMapPlayableBounds bounds, float supportSurfaceY)
    {
        LastPlayableAirWallColliderCount = 0;
        LastPlayableAirWallVisibleRendererCount = 0;
        if (root == null || !bounds.IsValid || playableBoundsConfig == null || !playableBoundsConfig.enabled)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        float height = Mathf.Max(5f, bounds.BoundaryHeightMeters);
        float thickness = Mathf.Max(0.5f, bounds.BoundaryThicknessMeters);
        float y = supportSurfaceY - 1f + height * 0.5f;
        float width = Mathf.Max(1f, bounds.Width + thickness * 2f);
        float depth = Mathf.Max(1f, bounds.Depth + thickness * 2f);

        CreateAirWall(root, "AirWall_North", new Vector3(bounds.CenterX, y, bounds.MaxZ + thickness * 0.5f), new Vector3(width, height, thickness));
        CreateAirWall(root, "AirWall_South", new Vector3(bounds.CenterX, y, bounds.MinZ - thickness * 0.5f), new Vector3(width, height, thickness));
        CreateAirWall(root, "AirWall_East", new Vector3(bounds.MaxX + thickness * 0.5f, y, bounds.CenterZ), new Vector3(thickness, height, depth));
        CreateAirWall(root, "AirWall_West", new Vector3(bounds.MinX - thickness * 0.5f, y, bounds.CenterZ), new Vector3(thickness, height, depth));
    }

    private void CreateAirWall(Transform root, string name, Vector3 center, Vector3 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(root, true);
        wall.transform.position = center;
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = false;
        LastPlayableAirWallColliderCount++;

        Renderer[] renderers = wall.GetComponentsInChildren<Renderer>(true);
        LastPlayableAirWallVisibleRendererCount += CountEnabledRenderers(renderers);
    }

    private static int CountEnabledRenderers(Renderer[] renderers)
    {
        int count = 0;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    private void EnsureGameplayGroundCover(Transform root, NewMapPlayableBounds bounds, float coverY)
    {
        ResetGameplayGroundCoverDiagnostics();
        NewMapGameplayGroundCoverConfig config = groundCoverConfig ?? NewMapGameplayGroundCoverConfig.Default();
        LastGameplayGroundCoverEnabled = config.enabled;
        LastGameplayGroundCoverY = coverY;
        LastGameplayGroundCoverOpacity = Mathf.Clamp01(config.materialAlpha);

        if (!config.enabled || root == null || !bounds.IsValid)
        {
            return;
        }

        ClearChildren(root);
        Material material = ResolveGameplayGroundCoverMaterial(config, out string materialSource);
        Color materialColor = ReadMaterialColor(material, config.ToColor());
        LastGameplayGroundCoverMaterialName = material != null ? material.name : "missing_material";
        LastGameplayGroundCoverMaterialSource = materialSource;
        LastGameplayGroundCoverMaterialBlueLike = IsBlueLikeColor(materialColor);
        LastGameplayGroundCoverMaterialMagentaLike = IsMagentaLikeColor(materialColor);

        float tileSize = Mathf.Clamp(config.tileSizeMeters, 80f, 900f);
        int maxTiles = Mathf.Clamp(config.maxTileCount, 1, 2000);
        int columns = Mathf.Max(1, Mathf.CeilToInt(bounds.Width / tileSize));
        int rows = Mathf.Max(1, Mathf.CeilToInt(bounds.Depth / tileSize));
        if (columns * rows > maxTiles)
        {
            float adjusted = Mathf.Sqrt(Mathf.Max(1f, bounds.Width * bounds.Depth) / maxTiles) * 1.05f;
            tileSize = Mathf.Clamp(adjusted, tileSize, 1200f);
            columns = Mathf.Max(1, Mathf.CeilToInt(bounds.Width / tileSize));
            rows = Mathf.Max(1, Mathf.CeilToInt(bounds.Depth / tileSize));
        }

        float thickness = Mathf.Clamp(config.thicknessMeters, 0.05f, 2f);
        float overlap = Mathf.Clamp(config.overlapMeters, 0f, 2f);
        int tileIndex = 1;
        for (int row = 0; row < rows; row++)
        {
            float minZ = Mathf.Lerp(bounds.MinZ, bounds.MaxZ, row / (float)rows);
            float maxZ = Mathf.Lerp(bounds.MinZ, bounds.MaxZ, (row + 1) / (float)rows);
            for (int column = 0; column < columns; column++)
            {
                float minX = Mathf.Lerp(bounds.MinX, bounds.MaxX, column / (float)columns);
                float maxX = Mathf.Lerp(bounds.MinX, bounds.MaxX, (column + 1) / (float)columns);
                Vector3 center = new Vector3((minX + maxX) * 0.5f, coverY - thickness * 0.5f, (minZ + maxZ) * 0.5f);
                Vector3 scale = new Vector3(Mathf.Max(0.1f, maxX - minX + overlap), thickness, Mathf.Max(0.1f, maxZ - minZ + overlap));
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"GroundCover_Tile_{tileIndex:000}";
                tile.transform.SetParent(root, true);
                tile.transform.position = center;
                tile.transform.localScale = scale;

                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = config.rendererEnabledInNormalMode;
                    if (material != null)
                    {
                        renderer.sharedMaterial = material;
                    }

                    LastGameplayGroundCoverRendererCount++;
                    if (renderer.enabled)
                    {
                        LastGameplayGroundCoverVisibleRendererCount++;
                    }
                }

                Collider collider = tile.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = config.colliderEnabled;
                    collider.isTrigger = false;
                    if (collider.enabled)
                    {
                        LastGameplayGroundCoverColliderCount++;
                    }
                }

                LastGameplayGroundCoverTileCount++;
                tileIndex++;
            }
        }

        LastGameplayGroundCoverActive =
            LastGameplayGroundCoverTileCount > 0 &&
            LastGameplayGroundCoverColliderCount == LastGameplayGroundCoverTileCount &&
            LastGameplayGroundCoverVisibleRendererCount == LastGameplayGroundCoverTileCount &&
            !LastGameplayGroundCoverMaterialBlueLike &&
            !LastGameplayGroundCoverMaterialMagentaLike;
        LastGameplayGroundCoverYMin = coverY;
        LastGameplayGroundCoverYMax = coverY;
        LastGameplayGroundCoverTotalArea = bounds.Width * bounds.Depth;
        LastRuntimeCollisionSupportProxyActive = LastRuntimeCollisionSupportProxyActive || LastGameplayGroundCoverColliderCount > 0;
        LastRuntimeCollisionSupportColliderActive = LastRuntimeCollisionSupportColliderActive || LastGameplayGroundCoverColliderCount > 0;
        LastRuntimeCollisionSupportRendererVisible = false;
        LastSafeGroundEnabled = true;
        LastSafeGroundSupportY = coverY;
        LastSafeGroundColliderCount = LastGameplayGroundCoverColliderCount;
        LastSafeGroundRendererCount = 0;
        LastSafeGroundRendererHidden = true;
        LastFallOutPreventionEnabled = true;
        Physics.SyncTransforms();
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ResetGameplayGroundCoverDiagnostics()
    {
        LastGameplayGroundCoverEnabled = false;
        LastGameplayGroundCoverActive = false;
        LastGameplayGroundCoverTileCount = 0;
        LastGameplayGroundCoverColliderCount = 0;
        LastGameplayGroundCoverRendererCount = 0;
        LastGameplayGroundCoverVisibleRendererCount = 0;
        LastGameplayGroundCoverYMin = 0f;
        LastGameplayGroundCoverYMax = 0f;
        LastGameplayGroundCoverTotalArea = 0f;
        LastGameplayGroundCoverOpacity = 0f;
        LastGameplayGroundCoverMaterialName = string.Empty;
        LastGameplayGroundCoverMaterialSource = string.Empty;
        LastGameplayGroundCoverMaterialBlueLike = false;
        LastGameplayGroundCoverMaterialMagentaLike = false;
    }

    private Material ResolveGameplayGroundCoverMaterial(NewMapGameplayGroundCoverConfig config, out string materialSource)
    {
        if (config.useImportedRoadMaterialIfAvailable && TryFindImportedRoadLikeMaterial(out Material importedMaterial))
        {
            materialSource = "imported_road_like_scene_material";
            return importedMaterial;
        }

        Material resourceMaterial = Resources.Load<Material>("NewMap/P10_NewMap_RoadGroundCover");
        if (resourceMaterial != null)
        {
            materialSource = "Assets/Resources/NewMap/P10_NewMap_RoadGroundCover.mat";
            return resourceMaterial;
        }

        materialSource = "runtime_generated_neutral_asphalt_material";
        return NewMapVisualFactory.CreateMaterial("P10_NewMap_RoadGroundCover_Runtime", config.ToColor(), false);
    }

    private bool TryFindImportedRoadLikeMaterial(out Material material)
    {
        material = null;
        Renderer[] renderers = FindObjectsOfType<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || IsRuntimeGeneratedOrUiRenderer(renderer))
            {
                continue;
            }

            Material candidate = renderer.sharedMaterial;
            if (candidate == null || !IsRoadLikeMaterialName(candidate.name))
            {
                continue;
            }

            Color color = ReadMaterialColor(candidate, Color.gray);
            if (IsBlueLikeColor(color) || IsMagentaLikeColor(color) || color.a < 0.99f)
            {
                continue;
            }

            material = candidate;
            return true;
        }

        return false;
    }

    private static bool IsRoadLikeMaterialName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string lower = value.ToLowerInvariant();
        return lower.Contains("road") ||
            lower.Contains("asphalt") ||
            lower.Contains("transport") ||
            lower.Contains("tran") ||
            lower.Contains("street") ||
            lower.Contains("surface");
    }

    private static Color ReadMaterialColor(Material material, Color fallback)
    {
        if (material == null)
        {
            return fallback;
        }

        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.color;
        }

        return fallback;
    }

    private static bool IsBlueLikeColor(Color color)
    {
        return color.b > 0.45f && color.b > color.r * 1.35f && color.b > color.g * 1.15f;
    }

    private static bool IsMagentaLikeColor(Color color)
    {
        return color.r > 0.65f && color.b > 0.65f && color.g < 0.3f;
    }

    private void CalculateActiveTargetHeightOffsets(List<NewMapRuntimeTarget> targets)
    {
        LastMaxActiveTargetHeightOffset = 0f;
        LastActiveTargetHeightOffsetViolations = 0;
        if (targets == null)
        {
            return;
        }

        foreach (NewMapRuntimeTarget target in targets)
        {
            if (target == null || target.Anchor == null || !target.ActiveInGame)
            {
                continue;
            }

            float localSupportY = ResolveLocalSupportSurfaceY(target.Anchor.position, LastRuntimeGroundSurfaceY);
            float offset = Mathf.Abs(target.Anchor.position.y - (localSupportY + GroundSkinOffset));
            LastMaxActiveTargetHeightOffset = Mathf.Max(LastMaxActiveTargetHeightOffset, offset);
            if (offset > 2.5f)
            {
                LastActiveTargetHeightOffsetViolations++;
            }
        }
    }

    private void EnsureRuntimeCollisionSupport(Transform parent, Vector3 center)
    {
        if (adaptiveSupportGrid != null && adaptiveSupportGrid.Enabled && adaptiveSupportGrid.HasUsableGrid)
        {
            LastRuntimeCollisionSupportProxyActive = true;
            LastRuntimeCollisionSupportColliderActive = adaptiveSupportGrid.ColliderCount > 0;
            LastRuntimeCollisionSupportRendererVisible = adaptiveSupportGrid.VisibleRendererCount > 0;
            return;
        }

        EnsureRuntimeCollisionSupportProxy(parent, center);
    }

    private void EnsureRuntimeCollisionSupportProxy(Transform parent, Vector3 center)
    {
        if (LastRuntimeCollisionSupportProxyActive)
        {
            return;
        }

        CreateGroundSupportProxy(parent, center);
        LastRuntimeCollisionSupportProxyActive = true;
    }

    private void CreateGroundSupportProxy(Transform parent, Vector3 center)
    {
        GameObject support = new GameObject("NewMap_RuntimeGroundSupport_DocumentedProxy");
        support.name = "NewMap_RuntimeGroundSupport_DocumentedProxy";
        support.transform.SetParent(parent, true);
        support.transform.position = center;
        BoxCollider collider = support.AddComponent<BoxCollider>();
        NewMapSafeGroundConfig groundConfig = safeGroundConfig ?? NewMapSafeGroundConfig.Default();
        float width = Mathf.Max(100f, groundConfig.supportSizeX);
        float depth = Mathf.Max(100f, groundConfig.supportSizeZ);
        float thickness = Mathf.Clamp(groundConfig.supportThicknessMeters, 0.05f, 5f);
        collider.size = new Vector3(width, thickness, depth);
        collider.center = Vector3.down * (thickness * 0.5f);
        collider.isTrigger = false;

        LastRuntimeCollisionSupportColliderActive = collider.enabled;
        LastRuntimeCollisionSupportRendererVisible = false;
        LastSafeGroundEnabled = groundConfig.enabled;
        LastSafeGroundSupportY = center.y;
        LastSafeGroundColliderCount = collider.enabled ? 1 : 0;
        LastSafeGroundRendererCount = support.GetComponentsInChildren<Renderer>(true).Length;
        LastSafeGroundRendererHidden = LastSafeGroundRendererCount == 0;
        LastFallOutPreventionEnabled = groundConfig.fallRecoveryEnabled;
    }

    private IEnumerator DisableSceneMeshCollidersStaged()
    {
        const int batchSize = 512;
        int disabled = 0;
        int inspected = 0;
        var stack = new Stack<Transform>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root != null)
            {
                stack.Push(root.transform);
            }
        }

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            if (current == null)
            {
                continue;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }

            MeshCollider meshCollider = current.GetComponent<MeshCollider>();
            if (meshCollider != null && meshCollider.enabled)
            {
                meshCollider.enabled = false;
                disabled++;
            }

            inspected++;
            if (inspected % batchSize == 0)
            {
                LastDisabledSceneMeshColliderCount = disabled;
                yield return null;
            }
        }

        LastDisabledSceneMeshColliderCount = disabled;
        LastMeshColliderDisableComplete = true;
        Physics.SyncTransforms();
        Debug.Log($"NewMap staged MeshCollider shutdown completed. disabledSceneMeshColliders={disabled}");
    }

    private List<NewMapRuntimeTarget> CreateVerifiedOfficialShelterTargets(Dictionary<string, Transform> roots)
    {
        var targets = new List<NewMapRuntimeTarget>();
        var wantedGmlIds = new HashSet<string>();
        foreach (OfficialShelterAnchorRecord record in OfficialShelterAnchorRecords)
        {
            if (record.IsActivationEligible)
            {
                wantedGmlIds.Add(record.PlateauGmlId);
            }
        }

        Dictionary<string, GameObject> anchorObjects = FindSceneObjectsByName(wantedGmlIds);
        foreach (OfficialShelterAnchorRecord record in OfficialShelterAnchorRecords)
        {
            if (!record.IsActivationEligible || !anchorObjects.TryGetValue(record.PlateauGmlId, out GameObject anchorObject))
            {
                continue;
            }

            if (!TryGetRendererBounds(anchorObject, out Bounds bounds))
            {
                continue;
            }

            Vector3 position = new Vector3(bounds.center.x, bounds.min.y + 0.08f, bounds.center.z);
            if (!IsFiniteVector3(position))
            {
                continue;
            }

            position.y = ResolveLocalSupportSurfaceY(position, LastRuntimeGroundSurfaceY) + GroundSkinOffset;
            targets.Add(CreateOfficialShelterTarget(roots, record, position));
        }

        return targets;
    }

    private List<NewMapRuntimeTarget> CreateRecoveredNonOfficialCandidateTargets(Dictionary<string, Transform> roots, Vector3 spawn)
    {
        var targets = new List<NewMapRuntimeTarget>();
        TextAsset candidateAsset = Resources.Load<TextAsset>(RuntimeNonOfficialCandidateResourcePath);
        if (candidateAsset == null || string.IsNullOrWhiteSpace(candidateAsset.text))
        {
            return targets;
        }

        RuntimeNonOfficialCandidateDataset dataset;
        try
        {
            dataset = JsonUtility.FromJson<RuntimeNonOfficialCandidateDataset>(candidateAsset.text);
        }
        catch (System.Exception)
        {
            return targets;
        }

        if (dataset == null || dataset.records == null)
        {
            return targets;
        }

        foreach (RuntimeNonOfficialCandidateRecord record in dataset.records)
        {
            if (record == null ||
                !record.activeInGame ||
                record.isOfficialShelter ||
                !record.nonOfficialWarningRequired ||
                record.safeApprovedByDefault ||
                string.IsNullOrWhiteSpace(record.id))
            {
                continue;
            }

            Vector3 position = new Vector3(record.unityX, record.unityY, record.unityZ);
            if (!IsFiniteVector3(position))
            {
                continue;
            }

            position.y = ResolveLocalSupportSurfaceY(position, spawn.y - GroundSkinOffset) + GroundSkinOffset;
            targets.Add(CreateRecoveredCandidateTarget(roots, record, spawn, position));
        }

        return targets;
    }

    private static Dictionary<string, GameObject> FindSceneObjectsByName(HashSet<string> wantedNames)
    {
        var found = new Dictionary<string, GameObject>();
        if (wantedNames == null || wantedNames.Count == 0)
        {
            return found;
        }

        var stack = new Stack<Transform>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root != null)
            {
                stack.Push(root.transform);
            }
        }

        while (stack.Count > 0 && found.Count < wantedNames.Count)
        {
            Transform current = stack.Pop();
            if (current == null)
            {
                continue;
            }

            if (wantedNames.Contains(current.name) && !found.ContainsKey(current.name))
            {
                found.Add(current.name, current.gameObject);
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return found;
    }

    private static bool TryGetRendererBounds(GameObject anchorObject, out Bounds bounds)
    {
        bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (anchorObject == null)
        {
            return false;
        }

        Renderer[] renderers = anchorObject.GetComponentsInChildren<Renderer>(false);
        bool hasBounds = false;
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds && bounds.size.sqrMagnitude > 0.01f && IsFiniteVector3(bounds.center);
    }

    private static bool IsFiniteVector3(Vector3 value)
    {
        return IsFiniteComponent(value.x) && IsFiniteComponent(value.y) && IsFiniteComponent(value.z);
    }

    private static bool IsFiniteComponent(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static NewMapRuntimeTarget CreateOfficialShelterTarget(
        Dictionary<string, Transform> roots,
        OfficialShelterAnchorRecord record,
        Vector3 position)
    {
        GameObject anchor = new GameObject(record.ShelterId);
        anchor.transform.SetParent(roots["ShelterMarkerRoot"], true);
        anchor.transform.position = position;

        Material markerMaterial = NewMapVisualFactory.CreateMaterial(
            record.ShelterId + "_OfficialMarkerMaterial",
            new Color(0.1f, 0.45f, 1f, 0.92f),
            false);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = record.ShelterId + "_marker_official_verified_gml";
        marker.transform.SetParent(anchor.transform, false);
        marker.transform.localPosition = Vector3.up * 0.08f;
        marker.transform.localScale = new Vector3(2.8f, 0.1f, 2.8f);
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null && markerMaterial != null)
        {
            markerRenderer.sharedMaterial = markerMaterial;
        }

        NewMapVisualFactory.RemoveCollider(marker);

        GameObject frame = CreateGreenFrame(roots["GreenFrameRoot"], record.ShelterId + "_official_green_frame", position);
        frame.SetActive(false);
        GameObject label = new GameObject(record.ShelterId + "_official_label");
        label.transform.SetParent(anchor.transform, false);
        label.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = record.DisplayName + "\nOfficial shelter\nGML anchor verified";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.22f;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;

        return new NewMapRuntimeTarget
        {
            Id = record.ShelterId,
            DisplayName = record.DisplayName,
            Category = "official_shelter_verified_gml_anchor",
            IsOfficialShelter = true,
            NonOfficialWarningRequired = false,
            SafeApprovedByDefault = false,
            EntranceBlocked = false,
            SafeFloorAvailable = true,
            InteractionDistance = 5f,
            ClimbSeconds = 10f,
            Anchor = anchor.transform,
            Marker = marker,
            GreenFrame = frame,
            RouteGuide = null,
            FinalBehavior =
                "Official shelter record activated only because the matched PLATEAU GML object exists in Chuo_BaseMap. " +
                "Safe-floor timing is a gameplay prototype; no official route or GIS-grade route validation is claimed."
        };
    }

    private List<NewMapRuntimeTarget> CreateLocalRuntimeTargets(Dictionary<string, Transform> roots, Vector3 spawn)
    {
        var targets = new List<NewMapRuntimeTarget>
        {
            CreateTarget(
                roots,
                "newmap_proxy_safe_floor",
                "Local Training Proxy - Safe Floor",
                spawn,
                AlignToLocalSupport(spawn + new Vector3(12f, 0f, 10f), spawn.y - GroundSkinOffset),
                false,
                false,
                true,
                "Runtime safe-floor proxy: E starts vertical evacuation and can succeed."),
            CreateTarget(
                roots,
                "newmap_proxy_blocked_entrance",
                "Local Training Proxy - Blocked Entrance",
                spawn,
                AlignToLocalSupport(spawn + new Vector3(18f, 0f, -7f), spawn.y - GroundSkinOffset),
                false,
                true,
                true,
                "Runtime entrance-blocked proxy: E triggers entrance_blocked result."),
            CreateTarget(
                roots,
                "newmap_proxy_no_safe_floor",
                "Local Training Proxy - No Safe Floor",
                spawn,
                AlignToLocalSupport(spawn + new Vector3(-13f, 0f, 11f), spawn.y - GroundSkinOffset),
                false,
                false,
                false,
                "Runtime safe-floor failure proxy: E triggers safe_floor_unavailable result."),
            CreateTarget(
                roots,
                "newmap_proxy_crowd_delay",
                "Local Training Proxy - Crowd Delay",
                spawn,
                AlignToLocalSupport(spawn + new Vector3(8f, 0f, 4f), spawn.y - GroundSkinOffset),
                false,
                false,
                true,
                "Runtime crowd-delay proxy: E starts safe-floor climb with local NPC congestion delay.")
        };

        return targets;
    }

    private Vector3 AlignToLocalSupport(Vector3 position, float fallbackY)
    {
        position.y = ResolveLocalSupportSurfaceY(position, fallbackY) + GroundSkinOffset;
        return position;
    }

    private static NewMapRuntimeTarget CreateTarget(
        Dictionary<string, Transform> roots,
        string id,
        string displayName,
        Vector3 spawn,
        Vector3 position,
        bool isOfficial,
        bool entranceBlocked,
        bool safeFloorAvailable,
        string finalBehavior)
    {
        GameObject anchor = new GameObject(id);
        anchor.transform.SetParent(roots["CandidateMarkerRoot"], true);
        anchor.transform.position = position;

        Material markerMaterial = NewMapVisualFactory.CreateMaterial(
            id + "_MarkerMaterial",
            entranceBlocked ? new Color(1f, 0.2f, 0.12f, 0.9f) : new Color(0.1f, 0.8f, 0.38f, 0.9f),
            false);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = id + "_marker_non_official";
        marker.transform.SetParent(anchor.transform, false);
        marker.transform.localPosition = Vector3.up * 0.08f;
        marker.transform.localScale = new Vector3(2.4f, 0.08f, 2.4f);
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null && markerMaterial != null)
        {
            markerRenderer.sharedMaterial = markerMaterial;
        }
        NewMapVisualFactory.RemoveCollider(marker);

        GameObject frame = CreateGreenFrame(roots["GreenFrameRoot"], id + "_green_frame", position);
        frame.SetActive(false);
        GameObject routeGuide = CreateEstimatedRouteGuide(roots["NavigationRoot"], id + "_estimated_route_proxy", spawn, position);
        routeGuide.SetActive(false);
        GameObject label = new GameObject(id + "_label");
        label.transform.SetParent(anchor.transform, false);
        label.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = displayName + "\nNon-official training proxy";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.22f;
        textMesh.fontSize = 22;
        textMesh.color = Color.white;

        return new NewMapRuntimeTarget
        {
            Id = id,
            DisplayName = displayName,
            Category = "runtime_proxy_training_target",
            IsOfficialShelter = isOfficial,
            NonOfficialWarningRequired = !isOfficial,
            SafeApprovedByDefault = false,
            EntranceBlocked = entranceBlocked,
            SafeFloorAvailable = safeFloorAvailable,
            InteractionDistance = 4f,
            ClimbSeconds = 5f,
            Anchor = anchor.transform,
            Marker = marker,
            GreenFrame = frame,
            RouteGuide = routeGuide,
            FinalBehavior = finalBehavior
        };
    }

    private static NewMapRuntimeTarget CreateRecoveredCandidateTarget(
        Dictionary<string, Transform> roots,
        RuntimeNonOfficialCandidateRecord record,
        Vector3 spawn,
        Vector3 position)
    {
        GameObject anchor = new GameObject(record.id);
        anchor.transform.SetParent(roots["CandidateMarkerRoot"], true);
        anchor.transform.position = position;

        Material markerMaterial = NewMapVisualFactory.CreateMaterial(
            record.id + "_RecoveredCandidateMarkerMaterial",
            new Color(0.12f, 0.78f, 0.42f, 0.82f),
            false);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = record.id + "_marker_non_official_recovered";
        marker.transform.SetParent(anchor.transform, false);
        marker.transform.localPosition = Vector3.up * 0.08f;
        marker.transform.localScale = new Vector3(1.65f, 0.07f, 1.65f);
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null && markerMaterial != null)
        {
            markerRenderer.sharedMaterial = markerMaterial;
        }
        NewMapVisualFactory.RemoveCollider(marker);

        string displayName = string.IsNullOrWhiteSpace(record.displayName) ? record.id : record.displayName;
        string classification = string.IsNullOrWhiteSpace(record.anchorClassification)
            ? "active_coordinate_proxy_anchor"
            : record.anchorClassification;

        return new NewMapRuntimeTarget
        {
            Id = record.id,
            DisplayName = displayName,
            Category = "non_official_humanitarian_candidate_" + classification,
            IsOfficialShelter = false,
            NonOfficialWarningRequired = true,
            SafeApprovedByDefault = false,
            EntranceBlocked = false,
            SafeFloorAvailable = true,
            InteractionDistance = 5f,
            ClimbSeconds = 7f,
            Anchor = anchor.transform,
            Marker = marker,
            GreenFrame = null,
            RouteGuide = null,
            GreenFrameFactory = () =>
            {
                GameObject frame = CreateGreenFrame(roots["GreenFrameRoot"], record.id + "_green_frame", position);
                frame.SetActive(false);
                return frame;
            },
            FinalBehavior =
                "Recovered non-official humanitarian candidate from the P8/P9 handoff using the validated NewMap transform. " +
                "This is not an official shelter, not safe-approved by default, and not connected to an official evacuation route."
        };
    }

    private static GameObject CreateGreenFrame(Transform parent, string name, Vector3 position)
    {
        GameObject frame = new GameObject(name);
        frame.transform.SetParent(parent, true);
        frame.transform.position = position + Vector3.up * 0.06f;

        LineRenderer line = frame.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = 0.08f;
        line.positionCount = 4;
        line.sharedMaterial = NewMapVisualFactory.CreateMaterial(name + "_Material", new Color(0.2f, 1f, 0.25f, 0.82f), true);
        float half = 2.2f;
        line.SetPosition(0, new Vector3(-half, 0f, -half));
        line.SetPosition(1, new Vector3(half, 0f, -half));
        line.SetPosition(2, new Vector3(half, 0f, half));
        line.SetPosition(3, new Vector3(-half, 0f, half));
        return frame;
    }

    private static GameObject CreateEstimatedRouteGuide(Transform parent, string name, Vector3 spawn, Vector3 target)
    {
        GameObject route = new GameObject(name);
        route.transform.SetParent(parent, true);

        LineRenderer line = route.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = 0.12f;
        line.positionCount = 3;
        line.sharedMaterial = NewMapVisualFactory.CreateMaterial(name + "_Material", new Color(1f, 0.86f, 0.16f, 0.78f), true);
        Vector3 start = new Vector3(spawn.x, target.y + 0.16f, spawn.z);
        Vector3 middle = Vector3.Lerp(start, target + Vector3.up * 0.16f, 0.5f) + Vector3.right * 1.8f;
        Vector3 end = target + Vector3.up * 0.16f;
        line.SetPosition(0, start);
        line.SetPosition(1, middle);
        line.SetPosition(2, end);
        return route;
    }

    [System.Serializable]
    private sealed class RuntimeNonOfficialCandidateDataset
    {
        public RuntimeNonOfficialCandidateRecord[] records;
    }

    [System.Serializable]
    private sealed class RuntimeNonOfficialCandidateRecord
    {
        public string id;
        public string displayName;
        public string anchorClassification;
        public bool activeInGame;
        public bool isOfficialShelter;
        public bool nonOfficialWarningRequired;
        public bool safeApprovedByDefault;
        public float unityX;
        public float unityY;
        public float unityZ;
    }

    private sealed class BuildingSnapdownGroup
    {
        public Transform Root;
        public string ObjectPath;
        public Bounds Bounds;
        public bool HasBounds;
        public int RendererCount;

        public void Add(Bounds bounds)
        {
            if (!HasBounds)
            {
                Bounds = bounds;
                HasBounds = true;
            }
            else
            {
                Bounds.Encapsulate(bounds);
            }

            RendererCount++;
        }
    }

    private sealed class OfficialShelterAnchorRecord
    {
        public string ShelterId;
        public string DisplayName;
        public string PlateauGmlId;
        public string MatchMethod;
        public string Confidence;
        public bool ManualReviewNeeded;

        public bool IsActivationEligible =>
            !string.IsNullOrWhiteSpace(PlateauGmlId) &&
            string.Equals(MatchMethod, "contains", System.StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Confidence, "high", System.StringComparison.OrdinalIgnoreCase) &&
            !ManualReviewNeeded;
    }

    private static readonly OfficialShelterAnchorRecord[] OfficialShelterAnchorRecords =
    {
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_001", DisplayName = "城東小学校", PlateauGmlId = "bldg_25d370de-2c35-457b-b756-3444a3d02eb3", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_002", DisplayName = "京橋プラザ", PlateauGmlId = "bldg_b79d201f-b27b-4e20-b342-fb09087fb41d", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_003", DisplayName = "泰明小学校", PlateauGmlId = "bldg_932d32e9-22aa-492c-980d-c1bf7cc0f79b", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_004", DisplayName = "銀座中学校", PlateauGmlId = "bldg_0a55bdd2-72f8-4464-b4df-1da32ec57f02", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_005", DisplayName = "中央小学校", PlateauGmlId = "bldg_7c79b5e1-dddc-4c1b-acbf-694a96b559b7", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_006", DisplayName = "明石小学校", PlateauGmlId = "bldg_ccdc4e97-2b53-462a-b188-7b75c84d30b5", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_007", DisplayName = "京橋築地小学校", PlateauGmlId = "bldg_74bfe18f-c482-4385-b52f-02aaaf3dcc34", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_008", DisplayName = "京華スクエア", PlateauGmlId = "bldg_228dc70f-56a0-453b-bcd6-3eec08bb3504", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_009", DisplayName = "明正小学校", PlateauGmlId = "bldg_79e83e58-9d57-4934-b422-191a0d1a0727", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_010", DisplayName = "常盤小学校", PlateauGmlId = "bldg_be0b4c30-e006-40a6-8952-be27fbbc620e", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_011", DisplayName = "十思スクエア", PlateauGmlId = "bldg_692282aa-7aed-474a-8182-51b2b958c65a", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_012", DisplayName = "日本橋小学校", PlateauGmlId = "bldg_32def57b-ec59-414c-9748-f455b4a65a51", MatchMethod = "nearest", Confidence = "medium", ManualReviewNeeded = true },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_013", DisplayName = "有馬小学校", PlateauGmlId = "bldg_4a32eca7-6527-4776-94a2-9f9fc0ed5930", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_014", DisplayName = "久松小学校", PlateauGmlId = "bldg_0cc7b33b-161f-4893-9753-3494f4d1ef69", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_015", DisplayName = "日本橋中学校", PlateauGmlId = "bldg_34c149ac-2d23-4c9a-bdf3-c32684ba631b", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_016", DisplayName = "阪本小学校", PlateauGmlId = "bldg_70594176-a51a-4425-b8b2-b75edbdef7a1", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_017", DisplayName = "佃島小学校", PlateauGmlId = "bldg_3ca362a4-293c-4ac2-bfbb-018a20677e1a", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_018", DisplayName = "佃中学校", PlateauGmlId = "bldg_3ca362a4-293c-4ac2-bfbb-018a20677e1a", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_019", DisplayName = "月島第一小学校", PlateauGmlId = "bldg_1b30504b-e662-41fb-8da6-219dcef2b1a4", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_020", DisplayName = "月島第二小学校", PlateauGmlId = "bldg_997fde71-9138-4d1a-ba84-6ff57047e6a7", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_021", DisplayName = "月島第三小学校", PlateauGmlId = "bldg_28476e16-ba7c-4fe3-bf1d-90cf577fe301", MatchMethod = "nearest", Confidence = "high", ManualReviewNeeded = true },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_022", DisplayName = "晴海中学校", PlateauGmlId = "bldg_f843c6a2-8d52-4dcc-bc01-bc7d0ede2733", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_023", DisplayName = "豊海小学校", PlateauGmlId = "bldg_01c61dfd-c5c9-454f-afbc-662aa6709160", MatchMethod = "nearest", Confidence = "medium", ManualReviewNeeded = true },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_024", DisplayName = "中央区役所", PlateauGmlId = "bldg_35741517-9a06-4d9b-81ed-d11ec2576b30", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_025", DisplayName = "日本橋区民センター", PlateauGmlId = "bldg_8df3166b-df28-4208-9fbd-883cf98547a8", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_026", DisplayName = "月島区民センター", PlateauGmlId = "bldg_fee39d2c-fd06-4f35-b0a1-a093a3e16fd5", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false },
        new OfficialShelterAnchorRecord { ShelterId = "chuo_official_emergency_027", DisplayName = "(旧)ほっとプラザはるみ", PlateauGmlId = "bldg_c64d9bf2-61ed-48d8-8315-8efadf440863", MatchMethod = "contains", Confidence = "high", ManualReviewNeeded = false }
    };
}

[System.Serializable]
public sealed class NewMapGameplayGroundCoverConfig
{
    public bool enabled = true;
    public bool forceFixedCoverY = true;
    public float coverY = 0f;
    public float tileSizeMeters = 320f;
    public int maxTileCount = 600;
    public float thicknessMeters = 0.18f;
    public float overlapMeters = 0.08f;
    public bool rendererEnabledInNormalMode = true;
    public bool colliderEnabled = true;
    public bool useImportedRoadMaterialIfAvailable = true;
    public float materialRed = 0.34f;
    public float materialGreen = 0.35f;
    public float materialBlue = 0.33f;
    public float materialAlpha = 1f;
    public string materialName = "P10_NewMap_RoadGroundCover";
    public string strategy = "visible_road_like_gameplay_ground_cover_not_gis_grade";

    public static NewMapGameplayGroundCoverConfig Default()
    {
        return new NewMapGameplayGroundCoverConfig();
    }

    public static NewMapGameplayGroundCoverConfig Load()
    {
        NewMapGameplayGroundCoverConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_gameplay_ground_cover_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapGameplayGroundCoverConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap gameplay ground-cover config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.coverY = Mathf.Clamp(config.coverY, -20f, 30f);
        config.tileSizeMeters = Mathf.Clamp(config.tileSizeMeters, 80f, 900f);
        config.maxTileCount = Mathf.Clamp(config.maxTileCount, 1, 2000);
        config.thicknessMeters = Mathf.Clamp(config.thicknessMeters, 0.05f, 2f);
        config.overlapMeters = Mathf.Clamp(config.overlapMeters, 0f, 2f);
        config.materialRed = Mathf.Clamp01(config.materialRed);
        config.materialGreen = Mathf.Clamp01(config.materialGreen);
        config.materialBlue = Mathf.Clamp01(config.materialBlue);
        config.materialAlpha = 1f;
        config.rendererEnabledInNormalMode = true;
        config.colliderEnabled = true;
        config.enabled = true;
        config.forceFixedCoverY = true;
        return config;
    }

    public Color ToColor()
    {
        return new Color(materialRed, materialGreen, materialBlue, materialAlpha);
    }
}

[System.Serializable]
public sealed class NewMapFloatingBuildingSnapdownConfig
{
    public bool enabled = true;
    public float floatingGapThresholdMeters = 0.5f;
    public float maxSnapdownMeters = 8f;
    public bool useGameplayGroundCoverAsReference = true;
    public bool preserveXZRotationScale = true;
    public string strategy = "runtime_visual_building_snapdown_to_gameplay_ground_cover_not_gis_grade";

    public static NewMapFloatingBuildingSnapdownConfig Default()
    {
        return new NewMapFloatingBuildingSnapdownConfig();
    }

    public static NewMapFloatingBuildingSnapdownConfig Load()
    {
        NewMapFloatingBuildingSnapdownConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_floating_building_snapdown_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapFloatingBuildingSnapdownConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap floating-building snapdown config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.floatingGapThresholdMeters = Mathf.Clamp(config.floatingGapThresholdMeters, 0.1f, 5f);
        config.maxSnapdownMeters = Mathf.Clamp(config.maxSnapdownMeters, config.floatingGapThresholdMeters, 20f);
        config.useGameplayGroundCoverAsReference = true;
        config.preserveXZRotationScale = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapSafeGroundConfig
{
    public bool enabled = true;
    public bool forceFixedSupportY = true;
    public float supportY = 0f;
    public float supportSizeX = 6000f;
    public float supportSizeZ = 6000f;
    public float supportThicknessMeters = 0.5f;
    public bool rendererEnabledInNormalMode;
    public bool fallRecoveryEnabled = true;
    public float fallRecoveryBelowY = -8f;
    public bool recoverOutsidePlayableBounds = true;
    public bool logRecoveryEvents;
    public string strategy = "rollback_safe_single_invisible_support_surface";

    public static NewMapSafeGroundConfig Default()
    {
        return new NewMapSafeGroundConfig();
    }

    public static NewMapSafeGroundConfig Load()
    {
        NewMapSafeGroundConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_safe_ground_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapSafeGroundConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap safe ground config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.supportY = Mathf.Clamp(config.supportY, -20f, 30f);
        config.supportSizeX = Mathf.Clamp(config.supportSizeX, 100f, 10000f);
        config.supportSizeZ = Mathf.Clamp(config.supportSizeZ, 100f, 10000f);
        config.supportThicknessMeters = Mathf.Clamp(config.supportThicknessMeters, 0.05f, 5f);
        config.fallRecoveryBelowY = Mathf.Clamp(config.fallRecoveryBelowY, -50f, 5f);
        config.rendererEnabledInNormalMode = false;
        config.forceFixedSupportY = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapSpawnConfig
{
    public string spawnMode = "road_or_playable_ground_only";
    public bool randomSpawnEnabled = true;
    public float spawnRadiusMeters = 1000f;
    public float minDistanceFromBuildingMeters = 2f;
    public int maxSpawnAttempts = 200;
    public bool useBuildingBoundsRejection = true;
    public bool useGroundProbe = true;
    public bool useGroundSupportFallback = true;
    public string fallbackSafeSpawnId = "newmap_safe_spawn_01";
    public int spawnRandomSeed = 20260529;
    public float minDistanceFromAirWallMeters = 2f;

    public static NewMapSpawnConfig Default()
    {
        return new NewMapSpawnConfig();
    }

    public static NewMapSpawnConfig Load()
    {
        NewMapSpawnConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_spawn_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapSpawnConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap spawn config could not be loaded; using defaults. {exception.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(config.spawnMode))
        {
            config.spawnMode = "road_or_playable_ground_only";
        }

        config.spawnRadiusMeters = Mathf.Clamp(config.spawnRadiusMeters, 25f, 2500f);
        config.minDistanceFromBuildingMeters = Mathf.Clamp(config.minDistanceFromBuildingMeters, 0f, 25f);
        config.minDistanceFromAirWallMeters = Mathf.Clamp(config.minDistanceFromAirWallMeters, 0f, 50f);
        config.maxSpawnAttempts = Mathf.Clamp(config.maxSpawnAttempts, 1, 2000);
        if (string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId))
        {
            config.fallbackSafeSpawnId = "newmap_safe_spawn_01";
        }

        return config;
    }
}

[System.Serializable]
public struct NewMapPlayableBounds
{
    public float MinX;
    public float MaxX;
    public float MinZ;
    public float MaxZ;
    public float MarginMeters;
    public float BoundaryHeightMeters;
    public float BoundaryThicknessMeters;

    public NewMapPlayableBounds(
        float minX,
        float maxX,
        float minZ,
        float maxZ,
        float marginMeters,
        float boundaryHeightMeters,
        float boundaryThicknessMeters)
    {
        MinX = minX;
        MaxX = maxX;
        MinZ = minZ;
        MaxZ = maxZ;
        MarginMeters = marginMeters;
        BoundaryHeightMeters = boundaryHeightMeters;
        BoundaryThicknessMeters = boundaryThicknessMeters;
    }

    public bool IsValid => MaxX > MinX + 1f && MaxZ > MinZ + 1f;
    public float Width => Mathf.Max(0f, MaxX - MinX);
    public float Depth => Mathf.Max(0f, MaxZ - MinZ);
    public float CenterX => (MinX + MaxX) * 0.5f;
    public float CenterZ => (MinZ + MaxZ) * 0.5f;

    public static NewMapPlayableBounds DefaultDocumented(float marginMeters = 2f, float heightMeters = 80f, float thicknessMeters = 4f)
    {
        return new NewMapPlayableBounds(
            -1363.6f,
            1359.3f,
            -1902.1f,
            2851.3f,
            marginMeters,
            heightMeters,
            thicknessMeters);
    }

    public NewMapPlayableBounds WithAppliedMargin()
    {
        float margin = Mathf.Max(0f, MarginMeters);
        float maxAllowedMargin = Mathf.Min(Width, Depth) * 0.45f;
        margin = Mathf.Min(margin, maxAllowedMargin);
        return new NewMapPlayableBounds(
            MinX + margin,
            MaxX - margin,
            MinZ + margin,
            MaxZ - margin,
            MarginMeters,
            BoundaryHeightMeters,
            BoundaryThicknessMeters);
    }

    public bool ContainsXZ(Vector3 position, float insetMeters = 0f)
    {
        if (!IsValid)
        {
            return false;
        }

        float inset = Mathf.Max(0f, insetMeters);
        return position.x >= MinX + inset &&
            position.x <= MaxX - inset &&
            position.z >= MinZ + inset &&
            position.z <= MaxZ - inset &&
            position.y >= -20f &&
            position.y <= 80f;
    }

    public Vector3 ClampXZ(Vector3 position, float insetMeters = 0f)
    {
        if (!IsValid)
        {
            return position;
        }

        float inset = Mathf.Max(0f, insetMeters);
        position.x = Mathf.Clamp(position.x, MinX + inset, MaxX - inset);
        position.z = Mathf.Clamp(position.z, MinZ + inset, MaxZ - inset);
        return position;
    }
}

[System.Serializable]
public sealed class NewMapPlayableBoundsConfig
{
    public bool enabled = true;
    public bool autoDetectFromMapBounds = true;
    public bool manualBoundsEnabled;
    public float minX = -1363.6f;
    public float maxX = 1359.3f;
    public float minZ = -1902.1f;
    public float maxZ = 2851.3f;
    public float marginMeters = 2f;
    public float boundaryHeightMeters = 80f;
    public float boundaryThicknessMeters = 4f;
    public bool debugVisualizationEnabled;
    public bool blockNpcMovement = true;
    public float minSpawnDistanceFromAirWallMeters = 2f;

    public static NewMapPlayableBoundsConfig Default()
    {
        return new NewMapPlayableBoundsConfig();
    }

    public static NewMapPlayableBoundsConfig Load()
    {
        NewMapPlayableBoundsConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_playable_bounds_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapPlayableBoundsConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap playable bounds config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.marginMeters = Mathf.Clamp(config.marginMeters, 0f, 200f);
        config.boundaryHeightMeters = Mathf.Clamp(config.boundaryHeightMeters, 5f, 500f);
        config.boundaryThicknessMeters = Mathf.Clamp(config.boundaryThicknessMeters, 0.5f, 50f);
        config.minSpawnDistanceFromAirWallMeters = Mathf.Clamp(config.minSpawnDistanceFromAirWallMeters, 0f, 100f);
        return config;
    }
}

[System.Serializable]
public sealed class NewMapSafeSpawnPointDataset
{
    public string activeScene = NewMapRuntimeConstants.ScenePath;
    public string coordinateStatus = "playable_ground_proxy_verified";
    public NewMapSafeSpawnPointRecord[] records;

    public static NewMapSafeSpawnPointDataset Default()
    {
        return new NewMapSafeSpawnPointDataset
        {
            records = new[]
            {
                NewMapSafeSpawnPointRecord.Create("newmap_safe_spawn_01", 0f, 0f, 420f, "fallback playable-ground proxy near Chuo map center"),
                NewMapSafeSpawnPointRecord.Create("newmap_safe_spawn_02", 120f, 0f, 420f, "alternate proxy point east of fallback"),
                NewMapSafeSpawnPointRecord.Create("newmap_safe_spawn_03", -120f, 0f, 420f, "alternate proxy point west of fallback"),
                NewMapSafeSpawnPointRecord.Create("newmap_safe_spawn_04", 0f, 0f, 620f, "alternate proxy point north of fallback"),
                NewMapSafeSpawnPointRecord.Create("newmap_safe_spawn_05", 180f, 0f, 620f, "wider alternate proxy point for building-overlap retries")
            }
        };
    }

    public static NewMapSafeSpawnPointDataset Load()
    {
        NewMapSafeSpawnPointDataset dataset = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_safe_spawn_points.json");
        if (File.Exists(path))
        {
            try
            {
                dataset = JsonUtility.FromJson<NewMapSafeSpawnPointDataset>(File.ReadAllText(path)) ?? dataset;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap safe spawn points could not be loaded; using defaults. {exception.Message}");
            }
        }

        if (dataset.records == null || dataset.records.Length == 0)
        {
            dataset.records = Default().records;
        }

        return dataset;
    }
}

[System.Serializable]
public sealed class NewMapSafeSpawnPointRecord
{
    public string id;
    public NewMapVector3Data position;
    public string sourceReason;
    public string roadPlayableGroundStatus = "playable_ground_proxy_verified";
    public string nearbyActiveTargets;
    public string modeCompatibility = "tourism_and_evacuation";

    public static NewMapSafeSpawnPointRecord Create(string id, float x, float y, float z, string reason)
    {
        return new NewMapSafeSpawnPointRecord
        {
            id = id,
            position = new NewMapVector3Data(x, y, z),
            sourceReason = reason,
            nearbyActiveTargets = "runtime targets are height-aligned after spawn; no official road geometry claimed"
        };
    }
}

[System.Serializable]
public struct NewMapVector3Data
{
    public float x;
    public float y;
    public float z;

    public NewMapVector3Data(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
