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
    public int LastSpawnFinalOverlapCheckCount { get; private set; }
    public int LastSpawnRejectedFinalOverlapCount { get; private set; }
    public int LastSpawnRejectedUnderBuildingOverhangCount { get; private set; }
    public bool LastSpawnFallbackUsed { get; private set; }
    public bool LastSpawnValidationPassed { get; private set; }
    public string LastSpawnMode { get; private set; } = string.Empty;
    public string LastFallbackSafeSpawnId { get; private set; } = string.Empty;
    public string LastSpawnValidationSource { get; private set; } = string.Empty;
    public Vector3 LastFinalSpawnPosition { get; private set; }
    public int LastSpawnRandomSeedUsed { get; private set; }
    public bool LastSpawnDeterministicSeedEnabled { get; private set; }
    public float LastNearestBuildingDistance { get; private set; }
    public int LastBuildingBoundsCacheCount { get; private set; }
    public int LastBuildingRendererBoundsCacheCount { get; private set; }
    public bool LastBuildingBoundsCacheBuilt { get; private set; }
    public bool LastRuntimeCollisionSupportColliderActive { get; private set; }
    public int LastSupportRendererCount { get; private set; }
    public int LastVisibleSupportRendererCount { get; private set; }
    public int LastSupportRendererDisabledCount { get; private set; }
    public int LastBlueDebugGroundRendererDisabledCount { get; private set; }
    public int LastPlayableAirWallColliderCount { get; private set; }
    public int LastPlayableAirWallVisibleRendererCount { get; private set; }
    public NewMapCircularBoundary LastCircularBoundary { get; private set; }
    public bool LastCircularBoundaryEnabled { get; private set; }
    public bool LastCircularBoundaryPlayerClampEnabled { get; private set; }
    public bool LastCircularBoundaryNpcClampEnabled { get; private set; }
    public int LastCircularBoundaryDiagnosticColliderCount { get; private set; }
    public int LastOldRectangularAirWallDisabledCount { get; private set; }
    public int LastRouteVisualBlockingColliderCount { get; private set; }
    public int LastGreenFrameBlockingColliderCount { get; private set; }
    public int LastLabelBlockingColliderCount { get; private set; }
    public int LastHazardVisualBlockingColliderCount { get; private set; }
    public int LastAirwallHardTotalCollidersScanned { get; private set; }
    public int LastUnexpectedAirwallBlockersFound { get; private set; }
    public int LastUnexpectedAirwallBlockersRemoved { get; private set; }
    public int LastUnexpectedAirwallBlockersResized { get; private set; }
    public int LastUnexpectedAirwallBlockersConvertedToTrigger { get; private set; }
    public int LastConcaveMeshTriggerOffenderCount { get; private set; }
    public int LastConcaveMeshTriggerFixedCount { get; private set; }
    public int LastConcaveMeshTriggerProxyCount { get; private set; }
    public int LastBuildingEntryTriggerCount { get; private set; }
    public int LastBuildingEntryPhysicalBlockerCount { get; private set; }
    public int LastBoundaryAirWallsPreserved { get; private set; }
    public int LastInvalidZoneBlockersPreserved { get; private set; }
    public int LastUnknownBlockersInsidePlayableArea { get; private set; }
    public int LastBuildingObstacleBoundsFiltered { get; private set; }
    public int LastBuildingObstacleBoundsShrunk { get; private set; }
    public bool LastBuildingPrecisionEnabled { get; private set; }
    public int LastBuildingPrecisionCandidateCount { get; private set; }
    public int LastBuildingPrecisionInflatedBoundsFound { get; private set; }
    public int LastBuildingPrecisionInflatedBoundsSkipped { get; private set; }
    public int LastBuildingPrecisionTightProxyCount { get; private set; }
    public int LastBuildingPrecisionMeshFootprintCount { get; private set; }
    public int LastBuildingPrecisionRendererFallbackCount { get; private set; }
    public int LastBuildingPrecisionSkippedClusterRootCount { get; private set; }
    public float LastBuildingPrecisionAverageShrinkRatio { get; private set; }
    public float LastBuildingPrecisionMaxWidth { get; private set; }
    public float LastBuildingPrecisionMaxDepth { get; private set; }
    public float LastBuildingPrecisionColliderHeight { get; private set; }
    public int LastBuildingPrecisionSampledCorridorCount { get; private set; }
    public int LastBuildingPrecisionUnexpectedCorridorBlockers { get; private set; }
    public int LastBuildingPrecisionActiveTargetApproachBlocked { get; private set; }
    public int LastBuildingPrecisionTargetClearanceBoundsSplit { get; private set; }
    public int LastBuildingPrecisionTargetClearanceBoundsRemoved { get; private set; }
    public int LastBuildingPrecisionTargetClearanceZones { get; private set; }
    public int LastActiveTargetsInsidePlayableBoundaryCount { get; private set; }
    public int LastActiveTargetsOutsidePlayableBoundaryDisabledCount { get; private set; }
    public int LastRouteGuidesSuppressedOutsidePlayableBoundaryCount { get; private set; }
    public bool LastBuildingFinalRefinementEnabled { get; private set; }
    public int LastBuildingFinalRefinementProxiesScanned { get; private set; }
    public int LastBuildingFinalRefinementOverflowFound { get; private set; }
    public int LastBuildingFinalRefinementProxiesShrunk { get; private set; }
    public int LastBuildingFinalRefinementProxiesSplit { get; private set; }
    public int LastBuildingFinalRefinementSplitPiecesCreated { get; private set; }
    public int LastBuildingFinalRefinementProxiesDisabled { get; private set; }
    public int LastBuildingFinalRefinementSpawnClearanceZones { get; private set; }
    public bool LastSampledValidPathsPassable { get; private set; }
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
    public bool LastGroundCoverRaiseEnabled { get; private set; }
    public float LastGroundCoverRaiseOldY { get; private set; }
    public float LastGroundCoverRaiseNewY { get; private set; }
    public float LastGroundCoverRaiseOffset { get; private set; }
    public float LastGroundCoverRaiseMedianBuildingBaseY { get; private set; }
    public float LastGroundCoverRaiseAverageBuildingBaseY { get; private set; }
    public float LastGroundCoverRaiseP25BuildingBaseY { get; private set; }
    public int LastGroundCoverRaiseSampleCount { get; private set; }
    public int LastGroundCoverRaiseOutlierCount { get; private set; }
    public int LastGroundCoverRaiseSkippedObjectCount { get; private set; }
    public bool LastGroundCoverRaiseCapped { get; private set; }
    public float LastGroundCoverRaiseRemainingAverageGap { get; private set; }
    public float LastGroundCoverRaiseRemainingMaxGap { get; private set; }
    public string LastGroundCoverRaiseStatus { get; private set; } = "not_evaluated";
    public bool LastGroundMicroRaiseEnabled { get; private set; }
    public float LastGroundMicroRaisePreviousY { get; private set; }
    public float LastGroundMicroRaiseAdditionalMeters { get; private set; }
    public float LastGroundMicroRaiseNewY { get; private set; }
    public bool LastGroundMicroRaiseAppliedToSupportColliders { get; private set; }
    public string LastGroundMicroRaiseStatus { get; private set; } = "not_evaluated";
    public bool LastGroundRaise30Enabled { get; private set; }
    public string LastGroundRaise30BaselineMode { get; private set; } = "not_evaluated";
    public float LastGroundRaise30OldGroundY { get; private set; }
    public float LastGroundRaise30OldRaiseOffset { get; private set; }
    public float LastGroundRaise30NewGroundY { get; private set; }
    public float LastGroundRaise30NewRaiseOffset { get; private set; }
    public float LastGroundRaise30ActualRaiseMeters { get; private set; }
    public float LastGroundRaise30ActualRaisePercent { get; private set; }
    public string LastGroundRaise30Status { get; private set; } = "not_evaluated";
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
    private readonly List<Bounds> buildingRendererBounds = new List<Bounds>();
    private NewMapSpawnConfig spawnConfig;
    private NewMapSafeSpawnPointDataset safeSpawnDataset;
    private NewMapPlayableBoundsConfig playableBoundsConfig;
    private NewMapCircularBoundaryConfig circularBoundaryConfig;
    private NewMapAdaptiveSupportGridRuntime adaptiveSupportGrid;
    private NewMapSafeGroundConfig safeGroundConfig;
    private NewMapGameplayGroundCoverConfig groundCoverConfig;
    private NewMapFloatingBuildingSnapdownConfig buildingSnapdownConfig;
    private NewMapGroundCoverRaiseConfig groundRaiseConfig;
    private NewMapFinalGroundMicroRaiseConfig groundMicroRaiseConfig;
    private NewMapGroundRaise30Config groundRaise30Config;
    private NewMapAirwallHardCleanupConfig airwallCleanupConfig;
    private NewMapBuildingCollisionPrecisionConfig buildingPrecisionConfig;
    private NewMapBuildingCollisionFinalRefinementConfig buildingFinalRefinementConfig;
    private NewMapTsunamiModeHotfixConfig tsunamiHotfixConfig;
    private Transform debugDiagnosticsRoot;
    private float buildingPrecisionShrinkRatioTotal;

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
        circularBoundaryConfig = NewMapCircularBoundaryConfig.Load();
        safeGroundConfig = NewMapSafeGroundConfig.Load();
        groundCoverConfig = NewMapGameplayGroundCoverConfig.Load();
        buildingSnapdownConfig = NewMapFloatingBuildingSnapdownConfig.Load();
        groundRaiseConfig = NewMapGroundCoverRaiseConfig.Load();
        groundMicroRaiseConfig = NewMapFinalGroundMicroRaiseConfig.Load();
        groundRaise30Config = NewMapGroundRaise30Config.Load();
        airwallCleanupConfig = NewMapAirwallHardCleanupConfig.Load();
        buildingPrecisionConfig = NewMapBuildingCollisionPrecisionConfig.Load();
        buildingFinalRefinementConfig = NewMapBuildingCollisionFinalRefinementConfig.Load();
        tsunamiHotfixConfig = NewMapTsunamiModeHotfixConfig.Load();
        debugDiagnosticsRoot = roots["DebugDiagnosticsRoot"];
        PrepareManualTestRoots(roots);
        EnforceSupportSurfaceVisibility(roots);
        if (groundRaiseConfig != null && groundRaiseConfig.enabled)
        {
            DisableBuildingVerticalMovesForGroundCoverRaise();
        }
        else
        {
            ApplyRound3BuildingRoadVerticalAlignment();
            ApplyFloatingBuildingSnapdownToGameplayGroundCover();
        }
        Physics.SyncTransforms();
        LastMapBoundsValid = TryResolveRuntimeMapBounds(out Bounds mapBounds, out int rendererCount, out int colliderCount);
        LastMapBounds = mapBounds;
        LastRendererCount = rendererCount;
        LastColliderCount = colliderCount;
        LastPlayableBounds = ResolvePlayableBounds(mapBounds, LastMapBoundsValid, playableBoundsConfig);
        LastPlayableBoundsValid = LastPlayableBounds.IsValid;
        LastCircularBoundary = ResolveCircularBoundary(mapBounds, LastMapBoundsValid, LastPlayableBounds, circularBoundaryConfig);
        LastCircularBoundaryEnabled = LastCircularBoundary.IsValid && circularBoundaryConfig.enabled;
        LastCircularBoundaryPlayerClampEnabled = LastCircularBoundaryEnabled && circularBoundaryConfig.affectsPlayer;
        LastCircularBoundaryNpcClampEnabled = LastCircularBoundaryEnabled && circularBoundaryConfig.affectsNpc;
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
        Vector3 supportCenter = LastCircularBoundaryEnabled
            ? new Vector3(LastCircularBoundary.Center.x, LastRuntimeGroundSurfaceY, LastCircularBoundary.Center.y)
            : new Vector3(spawn.x, LastRuntimeGroundSurfaceY, spawn.z);
        EnsureRuntimeCollisionSupport(roots["GameplaySupportRoot"], supportCenter);
        EnforceSupportSurfaceVisibility(roots);
        EnsureCircularBoundaryDiagnostics(roots["PlayableBoundsRoot"], LastCircularBoundary);
        NormalizeBuildingPrecisionBoundsToGroundY();
        CarveBuildingPrecisionSpawnClearance(spawn);
        Physics.SyncTransforms();
        if (SuppressSceneMeshCollidersForManualTest)
        {
            StartCoroutine(DisableSceneMeshCollidersStaged());
        }
        long spawnSupportMs = stopwatch.ElapsedMilliseconds - boundsMs;

        List<NewMapRuntimeTarget> targets = CreateVerifiedOfficialShelterTargets(roots);
        targets.AddRange(CreateRecoveredNonOfficialCandidateTargets(roots, spawn));
        if (ShouldEnableLocalTrainingProxyTargets())
        {
            targets.AddRange(CreateLocalRuntimeTargets(roots, spawn));
        }

        targets = FilterTargetsByPlayableBoundary(targets);
        CarveBuildingPrecisionTargetClearances(targets);
        RunBuildingCollisionPrecisionCorridorDiagnostics(targets, spawn);
        Physics.SyncTransforms();

        NewMapPlayerController player = NewMapPlayerController.Create(roots["PlayerSpawnRoot"], spawn);
        player.ConfigureGroundSafety(
            spawn,
            LastPlayableBounds,
            LastCircularBoundary,
            LastCircularBoundaryPlayerClampEnabled,
            LastRuntimeGroundSurfaceY,
            safeGroundConfig != null ? safeGroundConfig.fallRecoveryBelowY : -8f,
            safeGroundConfig == null || safeGroundConfig.recoverOutsidePlayableBounds,
            safeGroundConfig != null && safeGroundConfig.logRecoveryEvents);
        player.ConfigureBuildingCollision(
            buildingAvoidanceBounds,
            ResolvePlayerBuildingCollisionMargin(),
            groundRaiseConfig == null || groundRaiseConfig.playerBuildingCollisionEnabled);
        NewMapRuntimeUI.EnsureRuntimeEventSystem();
        NewMapRuntimeUI ui = NewMapRuntimeUI.Create(roots["UIAnchorRoot"]);
        NewMapLightingController lighting = NewMapLightingController.Create(roots["RuntimeSystemsRoot"]);
        NewMapHazardController hazard = NewMapHazardController.Create(
            roots["HazardVisualRoot"],
            roots["CollapseDebrisRoot"],
            spawn,
            LastPlayableBounds,
            LastRuntimeGroundSurfaceY,
            tsunamiHotfixConfig);
        Vector3 crowdCenter = LastCircularBoundaryEnabled && LastCircularBoundary.IsValid
            ? new Vector3(LastCircularBoundary.Center.x, LastRuntimeGroundSurfaceY + GroundSkinOffset, LastCircularBoundary.Center.y)
            : spawn;
        NewMapNpcCrowdPrototype crowd = NewMapNpcCrowdPrototype.Create(
            roots["CrowdRoot"],
            crowdCenter,
            LastPlayableBounds,
            buildingAvoidanceBounds,
            LastCircularBoundary,
            LastCircularBoundaryNpcClampEnabled);
        crowd.SetReferenceTransform(player.transform);
        player.ConfigurePlayerNpcCollision(crowd, NewMapPlayerNpcCollisionConfig.Load());
        NewMapPerformanceProbe.Create(roots["PerformanceMetricsRoot"]);
        long systemsMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs;
        CalculateActiveTargetHeightOffsets(targets);
        NewMapNameLabelController.Create(roots["NavigationRoot"], player, targets);
        NewMapShelterDirectLineController directLines = NewMapShelterDirectLineController.Create(
            roots["NavigationRoot"],
            player,
            targets,
            tsunamiHotfixConfig.DirectLineConfig);
        AuditAndCleanupUnexpectedAirwallColliders();
        long targetsMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs - systemsMs;

        NewMapGameController controller = gameObject.AddComponent<NewMapGameController>();
        controller.Configure(player, ui, lighting, hazard, crowd, directLines, targets, BuildDiagnosticText(), RespawnPlayerForNewRun, tsunamiHotfixConfig);
        CreateBuildingEntryTriggers(roots["CandidateMarkerRoot"], targets, controller);
        if (ShouldRunGameplaySelfAuditSmoke())
        {
            StartCoroutine(RunGameplaySelfAuditSmoke(controller, player, ui, lighting, hazard, crowd));
            StartCoroutine(RunNpcLifecycleDiagnosticSmoke(player, crowd));
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
            $"spawnMode={LastSpawnMode} spawnValidationSource={LastSpawnValidationSource} spawnAttempts={LastSpawnAttemptCount} spawnAccepted={LastSpawnAcceptedCount} " +
            $"spawnRejectedInsideBuilding={LastSpawnRejectedInsideBuildingCount} spawnRejectedNoGround={LastSpawnRejectedNoGroundCount} " +
            $"spawnRejectedOutOfBounds={LastSpawnRejectedOutOfBoundsCount} spawnRejectedTooCloseToBuilding={LastSpawnRejectedTooCloseToBuildingCount} " +
            $"spawnFallbackUsed={LastSpawnFallbackUsed} fallbackSafeSpawnId={LastFallbackSafeSpawnId} " +
            $"spawnFinalOverlapChecks={LastSpawnFinalOverlapCheckCount} spawnRejectedFinalOverlap={LastSpawnRejectedFinalOverlapCount} " +
            $"spawnRejectedUnderBuildingOverhang={LastSpawnRejectedUnderBuildingOverhangCount} " +
            $"spawnRandomSeedUsed={LastSpawnRandomSeedUsed} spawnDeterministicSeed={LastSpawnDeterministicSeedEnabled} " +
            $"nearestBuildingDistance={LastNearestBuildingDistance:F2} buildingBoundsCached={LastBuildingBoundsCacheCount} " +
            $"buildingRendererBoundsCached={LastBuildingRendererBoundsCacheCount} " +
            $"spawnX={LastFinalSpawnPosition.x:F2} spawnY={LastFinalSpawnPosition.y:F2} spawnZ={LastFinalSpawnPosition.z:F2} " +
            $"playerBuildingCollisionEnabled={player.BuildingCollisionEnabled} playerBuildingCollisionBounds={player.BuildingCollisionBoundsCount} " +
            $"playerBuildingCollisionBlocked={player.BuildingCollisionBlockedCount} playerBuildingCollisionRecoveries={player.BuildingCollisionRecoveryCount} " +
            $"playerNpcCollisionEnabled={player.PlayerNpcCollisionEnabled} playerNpcCollisionBlocked={player.PlayerNpcCollisionBlockedCount} " +
            $"playerNpcCollisionSlowdowns={player.PlayerNpcCollisionSlowdownCount} playerNpcCollisionEscapes={player.PlayerNpcCollisionEscapeCount} " +
            $"npcBodyColliders={crowd.NpcBodyColliderCount} npcSoftBlockingEnabled={crowd.PlayerNpcSoftBlockingEnabled} " +
            $"supportColliderActive={LastRuntimeCollisionSupportColliderActive} supportRendererCount={LastSupportRendererCount} " +
            $"supportVisibleRenderers={LastVisibleSupportRendererCount} supportDisabledRenderers={LastSupportRendererDisabledCount} " +
            $"blueDebugGroundDisabled={LastBlueDebugGroundRendererDisabledCount} playableBoundsValid={LastPlayableBoundsValid} " +
            $"playableBoundsSource={LastPlayableBoundsSource} playableMinX={LastPlayableBounds.MinX:F2} playableMaxX={LastPlayableBounds.MaxX:F2} " +
            $"playableMinZ={LastPlayableBounds.MinZ:F2} playableMaxZ={LastPlayableBounds.MaxZ:F2} " +
            $"circularBoundaryEnabled={LastCircularBoundaryEnabled} circularBoundaryCenterX={LastCircularBoundary.Center.x:F2} " +
            $"circularBoundaryCenterZ={LastCircularBoundary.Center.y:F2} circularBoundaryRadius={LastCircularBoundary.RadiusMeters:F1} " +
            $"circularBoundaryPlayerClamp={LastCircularBoundaryPlayerClampEnabled} circularBoundaryNpcClamp={LastCircularBoundaryNpcClampEnabled} " +
            $"circularBoundaryDiagnosticColliders={LastCircularBoundaryDiagnosticColliderCount} " +
            $"activeTargetsInsidePlayableBoundary={LastActiveTargetsInsidePlayableBoundaryCount} " +
            $"activeTargetsOutsidePlayableBoundaryDisabled={LastActiveTargetsOutsidePlayableBoundaryDisabledCount} " +
            $"routeGuidesSuppressedOutsidePlayableBoundary={LastRouteGuidesSuppressedOutsidePlayableBoundaryCount} " +
            $"airWallColliders={LastPlayableAirWallColliderCount} airWallVisibleRenderers={LastPlayableAirWallVisibleRendererCount} " +
            $"airwallHardCollidersScanned={LastAirwallHardTotalCollidersScanned} unexpectedAirwallBlockers={LastUnexpectedAirwallBlockersFound} " +
            $"airwallBlockersRemoved={LastUnexpectedAirwallBlockersRemoved} airwallBlockersResized={LastUnexpectedAirwallBlockersResized} " +
            $"airwallBlockersConvertedToTrigger={LastUnexpectedAirwallBlockersConvertedToTrigger} boundaryAirWallsPreserved={LastBoundaryAirWallsPreserved} " +
            $"oldRectangularAirWallsDisabled={LastOldRectangularAirWallDisabledCount} routeVisualBlockers={LastRouteVisualBlockingColliderCount} " +
            $"greenFrameVisualBlockers={LastGreenFrameBlockingColliderCount} labelVisualBlockers={LastLabelBlockingColliderCount} " +
            $"hazardVisualBlockers={LastHazardVisualBlockingColliderCount} " +
            $"concaveMeshTriggerOffenders={LastConcaveMeshTriggerOffenderCount} concaveMeshTriggerFixed={LastConcaveMeshTriggerFixedCount} " +
            $"concaveMeshTriggerProxies={LastConcaveMeshTriggerProxyCount} buildingEntryTriggers={LastBuildingEntryTriggerCount} " +
            $"buildingEntryPhysicalBlockers={LastBuildingEntryPhysicalBlockerCount} " +
            $"invalidZoneBlockersPreserved={LastInvalidZoneBlockersPreserved} unknownBlockersInsidePlayableArea={LastUnknownBlockersInsidePlayableArea} " +
            $"buildingObstacleBoundsFiltered={LastBuildingObstacleBoundsFiltered} buildingObstacleBoundsShrunk={LastBuildingObstacleBoundsShrunk} " +
            $"buildingPrecisionEnabled={LastBuildingPrecisionEnabled} buildingPrecisionCandidates={LastBuildingPrecisionCandidateCount} " +
            $"buildingPrecisionInflatedFound={LastBuildingPrecisionInflatedBoundsFound} buildingPrecisionInflatedSkipped={LastBuildingPrecisionInflatedBoundsSkipped} " +
            $"buildingPrecisionTightProxies={LastBuildingPrecisionTightProxyCount} buildingPrecisionMeshFootprints={LastBuildingPrecisionMeshFootprintCount} " +
            $"buildingPrecisionRendererFallbacks={LastBuildingPrecisionRendererFallbackCount} buildingPrecisionSkippedClusters={LastBuildingPrecisionSkippedClusterRootCount} " +
            $"buildingPrecisionAverageShrinkRatio={LastBuildingPrecisionAverageShrinkRatio:F3} buildingPrecisionMaxWidth={LastBuildingPrecisionMaxWidth:F2} " +
            $"buildingPrecisionMaxDepth={LastBuildingPrecisionMaxDepth:F2} buildingPrecisionColliderHeight={LastBuildingPrecisionColliderHeight:F2} " +
            $"buildingPrecisionTargetClearanceZones={LastBuildingPrecisionTargetClearanceZones} " +
            $"buildingPrecisionTargetClearanceBoundsSplit={LastBuildingPrecisionTargetClearanceBoundsSplit} " +
            $"buildingPrecisionTargetClearanceBoundsRemoved={LastBuildingPrecisionTargetClearanceBoundsRemoved} " +
            $"buildingFinalRefinementEnabled={LastBuildingFinalRefinementEnabled} " +
            $"buildingFinalRefinementProxiesScanned={LastBuildingFinalRefinementProxiesScanned} " +
            $"buildingFinalRefinementOverflowFound={LastBuildingFinalRefinementOverflowFound} " +
            $"buildingFinalRefinementProxiesShrunk={LastBuildingFinalRefinementProxiesShrunk} " +
            $"buildingFinalRefinementProxiesSplit={LastBuildingFinalRefinementProxiesSplit} " +
            $"buildingFinalRefinementSplitPieces={LastBuildingFinalRefinementSplitPiecesCreated} " +
            $"buildingFinalRefinementProxiesDisabled={LastBuildingFinalRefinementProxiesDisabled} " +
            $"buildingFinalRefinementSpawnClearanceZones={LastBuildingFinalRefinementSpawnClearanceZones} " +
            $"buildingPrecisionSampledCorridors={LastBuildingPrecisionSampledCorridorCount} " +
            $"buildingPrecisionUnexpectedCorridorBlockers={LastBuildingPrecisionUnexpectedCorridorBlockers} " +
            $"buildingPrecisionActiveTargetApproachBlocked={LastBuildingPrecisionActiveTargetApproachBlocked} " +
            $"sampledValidPathsPassable={LastSampledValidPathsPassable} " +
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
            $"groundRaiseEnabled={LastGroundCoverRaiseEnabled} groundRaiseOldY={LastGroundCoverRaiseOldY:F2} " +
            $"groundRaiseNewY={LastGroundCoverRaiseNewY:F2} groundRaiseOffset={LastGroundCoverRaiseOffset:F2} " +
            $"groundRaiseSamples={LastGroundCoverRaiseSampleCount} groundRaiseMedianBaseY={LastGroundCoverRaiseMedianBuildingBaseY:F2} " +
            $"groundRaiseAverageBaseY={LastGroundCoverRaiseAverageBuildingBaseY:F2} groundRaiseP25BaseY={LastGroundCoverRaiseP25BuildingBaseY:F2} " +
            $"groundRaiseOutliers={LastGroundCoverRaiseOutlierCount} groundRaiseSkipped={LastGroundCoverRaiseSkippedObjectCount} " +
            $"groundRaiseCapped={LastGroundCoverRaiseCapped} groundRaiseRemainingAvgGap={LastGroundCoverRaiseRemainingAverageGap:F2} " +
            $"groundRaiseRemainingMaxGap={LastGroundCoverRaiseRemainingMaxGap:F2} groundRaiseStatus={SafeLog(LastGroundCoverRaiseStatus)} " +
            $"groundMicroRaiseEnabled={LastGroundMicroRaiseEnabled} groundMicroRaisePreviousY={LastGroundMicroRaisePreviousY:F2} " +
            $"groundMicroRaiseAdditional={LastGroundMicroRaiseAdditionalMeters:F2} groundMicroRaiseNewY={LastGroundMicroRaiseNewY:F2} " +
            $"groundMicroRaiseSupportColliders={LastGroundMicroRaiseAppliedToSupportColliders} " +
            $"groundMicroRaiseStatus={SafeLog(LastGroundMicroRaiseStatus)} " +
            $"groundRaise30Enabled={LastGroundRaise30Enabled} groundRaise30BaselineMode={SafeLog(LastGroundRaise30BaselineMode)} " +
            $"groundRaise30OldGroundY={LastGroundRaise30OldGroundY:F2} groundRaise30OldOffset={LastGroundRaise30OldRaiseOffset:F2} " +
            $"groundRaise30NewGroundY={LastGroundRaise30NewGroundY:F2} groundRaise30NewOffset={LastGroundRaise30NewRaiseOffset:F2} " +
            $"groundRaise30ActualRaiseMeters={LastGroundRaise30ActualRaiseMeters:F2} groundRaise30ActualRaisePercent={LastGroundRaise30ActualRaisePercent:F2} " +
            $"groundRaise30Status={SafeLog(LastGroundRaise30Status)} " +
            $"buildingSnapdownEnabled={LastBuildingSnapdownEnabled} buildingSnapdownScanned={LastBuildingSnapdownScannedCount} " +
            $"floatingBuildingCandidates={LastFloatingBuildingCandidateCount} buildingsSnappedDown={LastBuildingSnapdownMovedCount} " +
            $"buildingSnapdownSkipped={LastBuildingSnapdownSkippedCount} buildingSnapdownRemainingFloating={LastBuildingSnapdownRemainingFloatingCount} " +
            $"buildingSnapdownAverageOffset={LastBuildingSnapdownAverageOffset:F2} buildingSnapdownMaxOffset={LastBuildingSnapdownMaxOffset:F2} " +
            $"buildingSnapdownReferenceY={LastBuildingSnapdownReferenceY:F2} buildingSnapdownThreshold={LastBuildingSnapdownThresholdMeters:F2} " +
            $"buildingSnapdownStatus={SafeLog(LastBuildingSnapdownStatus)}");
    }

    private void RespawnPlayerForNewRun(NewMapPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        Vector3 spawn = ResolveSpawnPosition(LastMapBounds, LastMapBoundsValid, debugDiagnosticsRoot);
        LastFinalSpawnPosition = spawn;
        LastPlayerSpawnGroundDelta = spawn.y - LastRuntimeGroundSurfaceY;
        player.TeleportToSpawn(spawn);
        player.ConfigureGroundSafety(
            spawn,
            LastPlayableBounds,
            LastCircularBoundary,
            LastCircularBoundaryPlayerClampEnabled,
            LastRuntimeGroundSurfaceY,
            safeGroundConfig != null ? safeGroundConfig.fallRecoveryBelowY : -8f,
            safeGroundConfig == null || safeGroundConfig.recoverOutsidePlayableBounds,
            safeGroundConfig != null && safeGroundConfig.logRecoveryEvents);
        Debug.Log(
            $"NewMap randomized player spawn selected source={LastSpawnValidationSource} " +
            $"seed={LastSpawnRandomSeedUsed} deterministic={LastSpawnDeterministicSeedEnabled} " +
            $"position={spawn} fallback={LastSpawnFallbackUsed} fallbackId={LastFallbackSafeSpawnId}");
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
        int rankableGuidanceCount = 0;
        int routeGuideCount = 0;
        int activeTargetsOutsideBoundary = 0;
        int routeGuidesOutsideBoundary = 0;
        foreach (NewMapRuntimeTarget target in controller.RuntimeTargets)
        {
            if (target == null || !target.ActiveInGame)
            {
                continue;
            }

            if (target.SafeFloorAvailable && !target.EntranceBlocked)
            {
                rankableGuidanceCount++;
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

            bool targetInsideBoundary = target.Anchor != null && IsInsideActivePlayableBoundary(target.Anchor.position, 0f);
            if (!targetInsideBoundary)
            {
                activeTargetsOutsideBoundary++;
                if (target.RouteGuide != null || target.RouteGuideFactory != null)
                {
                    routeGuidesOutsideBoundary++;
                }
            }
        }

        LogGameplaySmoke(
            "runtime_target_counts",
            rankableGuidanceCount > 0 &&
            nonOfficialCount > 0 &&
            activeTargetsOutsideBoundary == 0 &&
            routeGuidesOutsideBoundary == 0,
            $"official={officialCount} nonOfficial={nonOfficialCount} rankable={rankableGuidanceCount} routeGuides={routeGuideCount} activeOutsideBoundary={activeTargetsOutsideBoundary} routeGuidesOutsideBoundary={routeGuidesOutsideBoundary} disabledOutsideBoundary={LastActiveTargetsOutsidePlayableBoundaryDisabledCount}");
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
        float tourismWalkSpeed = player != null ? player.WalkSpeedMetersPerSecond : 0f;
        float tourismSprintSpeed = player != null ? player.SprintSpeedMetersPerSecond : 0f;
        bool tourismStaminaEnabled = player != null && player.StaminaEnabled;
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
            LastSpawnValidationPassed && LastSpawnAcceptedCount == 1 && spawnBuildingClear && LastSpawnFinalOverlapCheckCount > 0 && LastSpawnRejectedFinalOverlapCount == 0 && Mathf.Abs(LastPlayerSpawnGroundDelta) <= 0.35f,
            $"source={SafeLog(LastSpawnValidationSource)} attempts={LastSpawnAttemptCount} insideRejected={LastSpawnRejectedInsideBuildingCount} noGroundRejected={LastSpawnRejectedNoGroundCount} finalOverlapChecks={LastSpawnFinalOverlapCheckCount} finalOverlapRejected={LastSpawnRejectedFinalOverlapCount} nearestBuildingDistance={LastNearestBuildingDistance:0.00} fallbackUsed={LastSpawnFallbackUsed}");
        RunRepeatedSpawnSafetyDiagnosticSample(100);

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
        float evacuationWalkSpeed = player != null ? player.WalkSpeedMetersPerSecond : 0f;
        float evacuationSprintSpeed = player != null ? player.SprintSpeedMetersPerSecond : 0f;
        float evacuationMaxStamina = player != null ? player.MaxStamina : 0f;
        float evacuationCurrentStamina = player != null ? player.Stamina : 0f;
        float evacuationSprintMultiplier = player != null ? player.SprintSpeedMultiplierAdditional : 0f;
        LogGameplaySmoke(
            "mode_speed_stamina_rules",
            player != null &&
            Mathf.Abs(tourismWalkSpeed - NewMapRuntimeConstants.TourismWalkSpeed) <= 0.001f &&
            Mathf.Abs(tourismSprintSpeed - NewMapRuntimeConstants.TourismSprintSpeed) <= 0.001f &&
            !tourismStaminaEnabled &&
            player.StaminaEnabled &&
            Mathf.Abs(evacuationWalkSpeed - NewMapRuntimeConstants.EvacuationWalkSpeed) <= 0.001f &&
            Mathf.Abs(evacuationSprintSpeed - 4.59f) <= 0.001f &&
            Mathf.Abs(evacuationMaxStamina - 3500f) <= 0.001f &&
            Mathf.Abs(evacuationSprintMultiplier - 0.918f) <= 0.0001f,
            $"tourismWalk={tourismWalkSpeed:0.000} tourismSprint={tourismSprintSpeed:0.000} tourismStaminaEnabled={tourismStaminaEnabled} evacuationWalk={evacuationWalkSpeed:0.000} evacuationSprint={evacuationSprintSpeed:0.000} evacuationMaxStamina={evacuationMaxStamina:0.0} evacuationStamina={evacuationCurrentStamina:0.0} evacuationSprintMultiplier={evacuationSprintMultiplier:0.0000}");
        bool initialPreWarningStage =
            controller.Stage == NewMapTsunamiStage.PreWarningWait ||
            (controller.Stage == NewMapTsunamiStage.Warning && controller.PreWarningRandomDurationSeconds <= 0.5f);
        LogGameplaySmoke(
            "evacuation_pre_warning_wait",
            controller.Mode == NewMapGameMode.Evacuation &&
            initialPreWarningStage &&
            controller.PreWarningRandomDurationSeconds >= 0f &&
            controller.PreWarningRandomDurationSeconds <= 180f &&
            hazard != null &&
            !hazard.RiskChecksActive &&
            player != null &&
            player.StaminaEnabled,
            $"Evacuation starts in PRE_WARNING_WAIT duration={controller.PreWarningRandomDurationSeconds:0.0}s hazardInactive={hazard != null && !hazard.RiskChecksActive}");

        LogGameplaySmoke(
            "shelter_direct_lines_created",
            controller.ShelterDirectLineCount == rankableGuidanceCount &&
            controller.ShelterDirectLineCount > 0 &&
            controller.CountShelterDirectLineCollidersForDiagnostics() == 0,
            $"lines={controller.ShelterDirectLineCount} rankableTargets={rankableGuidanceCount} colliders={controller.CountShelterDirectLineCollidersForDiagnostics()}");

        LogGameplaySmoke(
            "collision_whitelist_visuals_nonblocking",
            LastCircularBoundaryEnabled &&
            LastPlayableAirWallColliderCount == 0 &&
            LastBoundaryAirWallsPreserved == 0 &&
            LastUnknownBlockersInsidePlayableArea == 0 &&
            LastRouteVisualBlockingColliderCount == 0 &&
            LastGreenFrameBlockingColliderCount == 0 &&
            LastLabelBlockingColliderCount == 0 &&
            LastHazardVisualBlockingColliderCount == 0,
            $"circularBoundary={LastCircularBoundaryEnabled} oldAirWalls={LastPlayableAirWallColliderCount} unknown={LastUnknownBlockersInsidePlayableArea} route={LastRouteVisualBlockingColliderCount} green={LastGreenFrameBlockingColliderCount} label={LastLabelBlockingColliderCount} hazard={LastHazardVisualBlockingColliderCount}");

        LogGameplaySmoke(
            "building_collision_precision_tight_proxies",
            LastBuildingPrecisionEnabled &&
            LastBuildingPrecisionTightProxyCount > 0 &&
            LastBuildingPrecisionMaxWidth <= 60.01f &&
            LastBuildingPrecisionMaxDepth <= 60.01f &&
            LastBuildingFinalRefinementEnabled &&
            player != null &&
            player.BuildingCollisionEnabled,
            $"candidates={LastBuildingPrecisionCandidateCount} inflated={LastBuildingPrecisionInflatedBoundsFound} skipped={LastBuildingPrecisionInflatedBoundsSkipped} tight={LastBuildingPrecisionTightProxyCount} finalShrunk={LastBuildingFinalRefinementProxiesShrunk} finalSplit={LastBuildingFinalRefinementProxiesSplit} maxWidth={LastBuildingPrecisionMaxWidth:0.00} maxDepth={LastBuildingPrecisionMaxDepth:0.00} clearanceZones={LastBuildingPrecisionTargetClearanceZones} clearanceSplits={LastBuildingPrecisionTargetClearanceBoundsSplit} playerBounds={(player != null ? player.BuildingCollisionBoundsCount : 0)}");

        LogGameplaySmoke(
            "building_collision_precision_corridors",
            LastBuildingPrecisionSampledCorridorCount > 0 &&
            LastBuildingPrecisionUnexpectedCorridorBlockers == 0 &&
            LastBuildingPrecisionActiveTargetApproachBlocked == 0,
            $"sampled={LastBuildingPrecisionSampledCorridorCount} unexpected={LastBuildingPrecisionUnexpectedCorridorBlockers} activeTargetBlocked={LastBuildingPrecisionActiveTargetApproachBlocked}");

        if (player != null && LastCircularBoundaryEnabled && LastCircularBoundary.IsValid)
        {
            Vector3 beforeBoundaryProbe = player.transform.position;
            Vector3 outsideCircle = new Vector3(
                LastCircularBoundary.Center.x + LastCircularBoundary.RadiusMeters + 150f,
                beforeBoundaryProbe.y,
                LastCircularBoundary.Center.y);
            player.transform.position = outsideCircle;
            player.MoveForDiagnostics(Vector3.zero, 0f, false);
            bool clampedInside = LastCircularBoundary.ContainsXZ(player.transform.position, 0f);
            player.transform.position = beforeBoundaryProbe;
            Physics.SyncTransforms();
            LogGameplaySmoke(
                "circular_boundary_player_clamp",
                player.CircularBoundaryClampEnabled && clampedInside,
                $"radius={LastCircularBoundary.RadiusMeters:0.0} playerClamp={player.CircularBoundaryClampEnabled} clampedInside={clampedInside}");
        }
        else
        {
            LogGameplaySmoke("circular_boundary_player_clamp", false, "Player or circular boundary missing");
        }

        bool npcInsideCircle = crowd != null && crowd.CircularBoundaryClampEnabled;
        if (npcInsideCircle)
        {
            Vector3[] npcPositions = crowd.GetNpcPositionsForDiagnostics();
            for (int i = 0; i < npcPositions.Length; i++)
            {
                if (!LastCircularBoundary.ContainsXZ(npcPositions[i], 0f))
                {
                    npcInsideCircle = false;
                    break;
                }
            }
        }

        LogGameplaySmoke(
            "circular_boundary_npc_clamp",
            npcInsideCircle,
            $"npcClamp={(crowd != null && crowd.CircularBoundaryClampEnabled)} radius={LastCircularBoundary.RadiusMeters:0.0}");

        bool rankingShown = controller.ToggleShelterRankingForDiagnostics();
        bool rankingHidden = !controller.ToggleShelterRankingForDiagnostics();
        LogGameplaySmoke(
            "r_leaderboard_toggle_show_hide",
            rankingShown && rankingHidden && ui != null && !ui.IsShelterRankingVisible,
            $"shown={rankingShown} hidden={rankingHidden}");

        controller.AdvanceEvacuationTimeForDiagnostics(controller.PreWarningRandomDurationSeconds + 0.1f);
        yield return null;
        LogGameplaySmoke(
            "evacuation_stage1_warning",
            controller.Mode == NewMapGameMode.Evacuation &&
            controller.Stage == NewMapTsunamiStage.Warning &&
            hazard != null &&
            !hazard.RiskChecksActive &&
            player != null &&
            player.StaminaEnabled,
            "Warning starts after PRE_WARNING_WAIT with hazard checks inactive and stamina enabled");

        float configuredWarningSeconds = controller.WarningPhaseSeconds;
        bool warningIsThreeMinutes = Mathf.Abs(configuredWarningSeconds - 180f) <= 0.01f;
        controller.AdvanceEvacuationTimeForDiagnostics(Mathf.Max(0f, configuredWarningSeconds - 1f));
        yield return null;
        bool inactiveBeforeWarningEnds = controller.Stage == NewMapTsunamiStage.Warning &&
            hazard != null &&
            !hazard.RiskChecksActive &&
            !hazard.LightCurtainVisibleForDiagnostics;
        controller.AdvanceEvacuationTimeForDiagnostics(2f);
        yield return null;
        bool activeAfterWarningEnds = controller.Stage == NewMapTsunamiStage.FrontApproaching &&
            hazard != null &&
            hazard.RiskChecksActive &&
            hazard.LightCurtainVisibleForDiagnostics;
        LogGameplaySmoke(
            "tsunami_warning_180s_before_active",
            warningIsThreeMinutes && inactiveBeforeWarningEnds && activeAfterWarningEnds,
            $"warningSeconds={configuredWarningSeconds:0.0} inactiveBeforeEnd={inactiveBeforeWarningEnds} activeAfterEnd={activeAfterWarningEnds}");

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

        NewMapRuntimeTarget touchEntryTarget = official ?? recoveredNonOfficial ?? routeProxy;
        if (touchEntryTarget != null && touchEntryTarget.EntryTrigger != null && player != null)
        {
            controller.StartEvacuationMode();
            controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
            Bounds triggerBounds = touchEntryTarget.EntryTrigger.Bounds;
            player.transform.position = new Vector3(
                triggerBounds.center.x,
                touchEntryTarget.Anchor.position.y + 0.4f,
                triggerBounds.center.z);
            Physics.SyncTransforms();
            yield return null;
            bool touchDetected = controller.RefreshTouchedBuildingForDiagnostics();
            bool entered = controller.TryInteractWithTouchedBuildingForDiagnostics();
            bool completed = controller.CompleteSafeFloorSequenceForDiagnostics();
            LogGameplaySmoke(
                "building_touch_e_entry",
                touchDetected && entered && completed && ui != null && ui.LastResultReason == "safe_floor_reached",
                $"target={touchEntryTarget.Id} touchDetected={touchDetected} finalReason={SafeLog(ui != null ? ui.LastResultReason : string.Empty)}");
        }
        else
        {
            LogGameplaySmoke("building_touch_e_entry", false, "No active target with an entry trigger was available");
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
        Vector3 frontFailurePosition = hazard != null
            ? hazard.GetFloodedSideSamplePointForDiagnostics()
            : (player != null ? player.transform.position + Vector3.back * 200f : Vector3.back * 200f);
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

    private IEnumerator RunNpcLifecycleDiagnosticSmoke(NewMapPlayerController player, NewMapNpcCrowdPrototype crowd)
    {
        if (crowd == null)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(2f);
        Vector3[] positions = crowd.GetNpcPositionsForDiagnostics();
        if (positions != null && positions.Length > 0)
        {
            Vector3 previous = positions[0] + new Vector3(1.5f, 0f, 0f);
            Vector3 candidate = positions[0] + new Vector3(0.1f, 0f, 0f);
            crowd.ResolvePlayerPositionAgainstNpcs(
                previous,
                candidate,
                NewMapPlayerNpcCollisionConfig.Default(),
                out _,
                out _,
                out _,
                out _,
                out _);
        }

        yield return new WaitForSecondsRealtime(8f);
        LogNpcLifecycleSmoke(10, crowd);
        yield return new WaitForSecondsRealtime(20f);
        LogNpcLifecycleSmoke(30, crowd);
        yield return new WaitForSecondsRealtime(30f);
        LogNpcLifecycleSmoke(60, crowd);
        yield return new WaitForSecondsRealtime(120f);
        LogNpcLifecycleSmoke(180, crowd);
    }

    private static void LogNpcLifecycleSmoke(int seconds, NewMapNpcCrowdPrototype crowd)
    {
        if (crowd == null)
        {
            return;
        }

        Debug.Log(
            $"NewMap NPC lifecycle smoke. seconds={seconds} activeNpcCount={crowd.ActiveNpcCount} " +
            $"createdAtStartup={crowd.NpcCreatedAtStartupCount} globalRespawnCount={crowd.GlobalRespawnCount} " +
            $"individualRespawnCount={crowd.IndividualRespawnCount} poolRecycleCount={crowd.PoolRecycleCount} " +
            $"destroyedDuringSmoke={crowd.DestroyedDuringRuntimeCount} instantiateAfterStartup={crowd.InstantiateAfterStartupCount} " +
            $"allStopEventCount={crowd.AllStopEventCount} playerContactEvents={crowd.PlayerContactEventCount} " +
            $"stoppedWithoutReason={crowd.StoppedWithoutReasonCount} moving={crowd.MovingCount} arrived={crowd.ArrivedCount} " +
            $"queued={crowd.QueuedCount} stuck={crowd.StuckCount} static={crowd.StaticProxyCount} " +
            $"averageSpeed={crowd.AverageSpeedMetersPerSecond:F3} allowGlobalRefresh={crowd.AllowGlobalRefresh} " +
            $"globalRespawnIntervalSeconds={crowd.GlobalRespawnIntervalSeconds:F1} collisionWithPlayerDoesNotGlobalPause={crowd.CollisionWithPlayerDoesNotGlobalPause}");
    }

    private static void LogGameplaySmoke(string scenarioId, bool passed, string detail)
    {
        Debug.Log($"NewMap gameplay self-audit smoke: scenario={scenarioId} result={(passed ? "pass" : "fail")} detail={SafeLog(detail)}");
    }

    private void RunRepeatedSpawnSafetyDiagnosticSample(int sampleCount)
    {
        NewMapSpawnConfig config = spawnConfig ?? NewMapSpawnConfig.Default();
        int requested = Mathf.Clamp(sampleCount, 1, 200);
        int accepted = 0;
        int insideBuilding = 0;
        int finalOverlap = 0;
        int outsideBoundary = 0;
        float nearestDistanceMin = 9999f;

        for (int i = 0; i < requested; i++)
        {
            Vector3 sample = ResolveSpawnPosition(LastMapBounds, LastMapBoundsValid, debugDiagnosticsRoot);
            if (LastSpawnValidationPassed)
            {
                accepted++;
            }

            if (IsInsideBuildingBoundsXZ(sample, buildingAvoidanceBounds, 0f) ||
                IsInsideBuildingBoundsXZ(sample, buildingRendererBounds, 0f))
            {
                insideBuilding++;
            }

            if (SpawnCapsuleOverlapsAnyBounds(sample, config.PlayerCapsuleRadiusMeters, config.PlayerCapsuleHeightMeters, buildingAvoidanceBounds) ||
                SpawnCapsuleOverlapsAnyBounds(sample, config.PlayerCapsuleRadiusMeters, config.PlayerCapsuleHeightMeters, buildingRendererBounds))
            {
                finalOverlap++;
            }

            if ((LastPlayableBoundsValid && !LastPlayableBounds.ContainsXZ(sample, config.RejectInsideAirWallMarginMeters)) ||
                (LastCircularBoundaryEnabled && !LastCircularBoundary.ContainsXZ(sample, config.RejectInsideAirWallMarginMeters)))
            {
                outsideBoundary++;
            }

            nearestDistanceMin = Mathf.Min(nearestDistanceMin, CalculateNearestSpawnBuildingDistance(sample, config));
        }

        LogGameplaySmoke(
            "spawn_repeated_100_avoids_buildings",
            accepted == requested &&
            insideBuilding == 0 &&
            finalOverlap == 0 &&
            outsideBoundary == 0 &&
            nearestDistanceMin >= config.minDistanceFromBuildingMeters - 0.01f,
            $"samples={requested} accepted={accepted} insideBuilding={insideBuilding} finalOverlap={finalOverlap} outsideBoundary={outsideBoundary} nearestMin={nearestDistanceMin:0.00}");
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
        buildingRendererBounds.Clear();
        ResetAirwallHardCleanupDiagnostics();
        ResetBuildingFinalRefinementDiagnostics();
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

            if (IsBuildingSnapdownRendererCandidate(renderer))
            {
                buildingRendererBounds.Add(renderer.bounds);
            }

            if (TryBuildConservativeBuildingObstacleBounds(renderer, out Bounds obstacleBounds))
            {
                AddBuildingObstacleBoundsWithFinalRefinement(obstacleBounds);
            }
            else if (IsUsableBuildingAvoidanceBounds(renderer.bounds))
            {
                LastBuildingObstacleBoundsFiltered++;
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
        LastBuildingRendererBoundsCacheCount = buildingRendererBounds.Count;
        LastBuildingBoundsCacheBuilt = LastBuildingBoundsCacheCount > 0;
        RecalculateBuildingPrecisionMaxBounds();

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

        var random = new System.Random(ResolveSpawnRandomSeed(config));
        int maxAttempts = Mathf.Clamp(config.maxSpawnAttempts, 1, 2000);
        float radius = Mathf.Clamp(config.spawnRadiusMeters, 25f, 2500f);
        if (config.randomSpawnEnabled && config.tryRandomBeforeMapCenter)
        {
            if (TryResolveRandomSpawnCandidate(random, basePosition, radius, maxAttempts, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, out Vector3 randomAccepted))
            {
                return randomAccepted;
            }
        }

        Vector3 candidate = new Vector3(basePosition.x, 0f, basePosition.z);
        float candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
        candidate.y = candidateSupportY + GroundSkinOffset;
        if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "map_bounds_center", out Vector3 accepted))
        {
            return accepted;
        }

        if (config.randomSpawnEnabled && !config.tryRandomBeforeMapCenter)
        {
            if (TryResolveRandomSpawnCandidate(random, basePosition, radius, maxAttempts, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, out Vector3 randomAccepted))
            {
                return randomAccepted;
            }
        }

        LastSpawnFallbackUsed = true;
        List<NewMapSafeSpawnPointRecord> safePoints = new List<NewMapSafeSpawnPointRecord>(GetSafeSpawnPointRecords());
        if (config.fallbackSafeSpawnEnabled)
        {
            if (config.randomSpawnEnabled && config.randomizeFallbackSafeSpawnOrder)
            {
                ShuffleSafeSpawnRecords(safePoints, random);
            }

            foreach (NewMapSafeSpawnPointRecord safePoint in safePoints)
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
        }

        if (TryResolveEmergencySafeSpawn(basePosition, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, safePoints, out accepted))
        {
            return accepted;
        }

        LastSpawnValidationPassed = false;
        LastFallbackSafeSpawnId = string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId) ? "none" : config.fallbackSafeSpawnId;
        LastNearestBuildingDistance = CalculateNearestSpawnBuildingDistance(candidate, config);
        LastSpawnValidationSource = "unvalidated_last_resort_support_center";
        LastRuntimeGroundSurfaceY = ResolveLocalSupportSurfaceY(basePosition, supportSurfaceY);
        return new Vector3(basePosition.x, LastRuntimeGroundSurfaceY + GroundSkinOffset, basePosition.z);
    }

    private bool TryResolveEmergencySafeSpawn(
        Vector3 basePosition,
        float supportSurfaceY,
        bool hasSupportSurface,
        Bounds mapBounds,
        bool hasBounds,
        NewMapSpawnConfig config,
        List<NewMapSafeSpawnPointRecord> safePoints,
        out Vector3 accepted)
    {
        accepted = Vector3.zero;
        var anchors = new List<Vector3> { basePosition };
        if (safePoints != null)
        {
            for (int i = 0; i < safePoints.Count; i++)
            {
                NewMapSafeSpawnPointRecord safePoint = safePoints[i];
                if (safePoint != null)
                {
                    anchors.Add(new Vector3(safePoint.position.x, 0f, safePoint.position.z));
                }
            }
        }

        float[] radii = { 12f, 24f, 48f, 96f, 160f, 260f, 420f, 680f, 960f };
        int directions = 16;
        for (int anchorIndex = 0; anchorIndex < anchors.Count; anchorIndex++)
        {
            Vector3 anchor = anchors[anchorIndex];
            for (int radiusIndex = 0; radiusIndex < radii.Length; radiusIndex++)
            {
                float radius = radii[radiusIndex];
                for (int directionIndex = 0; directionIndex < directions; directionIndex++)
                {
                    float angle = Mathf.PI * 2f * directionIndex / directions;
                    Vector3 candidate = anchor + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    float candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
                    candidate.y = candidateSupportY + GroundSkinOffset;
                    if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "emergency_safe_spawn_grid", out accepted))
                    {
                        LastFallbackSafeSpawnId = "emergency_grid";
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool TryResolveRandomSpawnCandidate(
        System.Random random,
        Vector3 basePosition,
        float radius,
        int maxAttempts,
        float supportSurfaceY,
        bool hasSupportSurface,
        Bounds mapBounds,
        bool hasBounds,
        NewMapSpawnConfig config,
        out Vector3 accepted)
    {
        accepted = Vector3.zero;
        while (LastSpawnAttemptCount < maxAttempts)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
            Vector3 candidate = basePosition + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            float candidateSupportY = ResolveLocalSupportSurfaceY(candidate, supportSurfaceY);
            candidate.y = candidateSupportY + GroundSkinOffset;

            if (TryValidateSpawnCandidate(candidate, candidateSupportY, hasSupportSurface, mapBounds, hasBounds, config, "random_playable_support", out accepted))
            {
                return true;
            }
        }

        return false;
    }

    private int ResolveSpawnRandomSeed(NewMapSpawnConfig config)
    {
        config = config ?? NewMapSpawnConfig.Default();
        LastSpawnDeterministicSeedEnabled = config.deterministicSeedEnabled;
        int seed = config.deterministicSeedEnabled
            ? config.spawnRandomSeed
            : NewMapSpawnConfig.CreateSessionRandomSeed(config.spawnRandomSeed);
        LastSpawnRandomSeedUsed = seed;
        return seed;
    }

    private static void ShuffleSafeSpawnRecords(List<NewMapSafeSpawnPointRecord> records, System.Random random)
    {
        if (records == null || random == null)
        {
            return;
        }

        for (int i = records.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            NewMapSafeSpawnPointRecord temp = records[i];
            records[i] = records[j];
            records[j] = temp;
        }
    }

    private float ResolveSupportSurfaceY(Bounds mapBounds, bool hasBounds, Vector3 basePosition, float rayStartY)
    {
        NewMapGameplayGroundCoverConfig coverConfig = groundCoverConfig ?? NewMapGameplayGroundCoverConfig.Default();
        if (coverConfig.enabled && coverConfig.forceFixedCoverY)
        {
            float coverY = ResolveRaisedGroundCoverY(Mathf.Clamp(coverConfig.coverY, -20f, 30f));
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

    private void DisableBuildingVerticalMovesForGroundCoverRaise()
    {
        LastBuildingRoadVerticalOffsetApplied = 0f;
        LastBuildingRoadAlignedRootCount = 0;
        LastBuildingRoadAlignmentStatus = "disabled_by_ground_cover_raise_buildings_fixed";
        ResetBuildingSnapdownDiagnostics();
        LastBuildingSnapdownEnabled = false;
        LastBuildingSnapdownReferenceY = groundCoverConfig != null ? groundCoverConfig.coverY : 0f;
        LastBuildingSnapdownStatus = "disabled_by_ground_cover_raise_buildings_fixed";
    }

    private float ResolveRaisedGroundCoverY(float oldCoverY)
    {
        NewMapGroundCoverRaiseConfig config = groundRaiseConfig ?? NewMapGroundCoverRaiseConfig.Default();
        LastGroundCoverRaiseEnabled = config.enabled;
        LastGroundCoverRaiseOldY = oldCoverY;

        if (!config.enabled)
        {
            LastGroundCoverRaiseNewY = oldCoverY;
            LastGroundCoverRaiseOffset = 0f;
            LastGroundCoverRaiseStatus = "disabled_by_config";
            LastGroundMicroRaiseEnabled = false;
            LastGroundMicroRaisePreviousY = oldCoverY;
            LastGroundMicroRaiseAdditionalMeters = 0f;
            LastGroundMicroRaiseNewY = oldCoverY;
            LastGroundMicroRaiseStatus = "base_raise_disabled";
            return oldCoverY;
        }

        if (LastGroundCoverRaiseStatus != "not_evaluated")
        {
            return LastGroundCoverRaiseNewY;
        }

        List<float> samples = CollectGroundCoverRaiseBuildingBaseSamples(config, out int skipped);
        LastGroundCoverRaiseSkippedObjectCount = skipped;
        LastGroundCoverRaiseSampleCount = samples.Count;
        float selectedOffset;
        if (samples.Count >= config.minimumBuildingBaseSampleCount)
        {
            samples.Sort();
            float p25 = Percentile(samples, 0.25f);
            float median = Percentile(samples, 0.50f);
            float p75 = Percentile(samples, 0.75f);
            float iqr = Mathf.Max(0.01f, p75 - p25);
            float lowFence = p25 - iqr * Mathf.Max(0f, config.outlierIqrMultiplier);
            float highFence = p75 + iqr * Mathf.Max(0f, config.outlierIqrMultiplier);
            var trimmed = new List<float>();
            for (int i = 0; i < samples.Count; i++)
            {
                float sample = samples[i];
                if (sample < lowFence || sample > highFence)
                {
                    LastGroundCoverRaiseOutlierCount++;
                    continue;
                }

                trimmed.Add(sample);
            }

            if (trimmed.Count == 0)
            {
                trimmed.AddRange(samples);
            }

            trimmed.Sort();
            LastGroundCoverRaiseP25BuildingBaseY = Percentile(trimmed, 0.25f);
            LastGroundCoverRaiseMedianBuildingBaseY = Percentile(trimmed, config.selectedPercentile);
            LastGroundCoverRaiseAverageBuildingBaseY = Average(trimmed);
            selectedOffset = Mathf.Max(0f, LastGroundCoverRaiseMedianBuildingBaseY - oldCoverY);
            if (selectedOffset < config.minimumActionableRaiseOffsetMeters && config.fallbackRaiseOffsetMeters > selectedOffset)
            {
                selectedOffset = config.fallbackRaiseOffsetMeters;
                LastGroundCoverRaiseStatus = "sampled_offset_below_actionable_threshold_using_configured_fallback";
            }
            else
            {
                LastGroundCoverRaiseStatus = "computed_from_building_base_samples";
            }
        }
        else
        {
            selectedOffset = Mathf.Max(0f, config.fallbackRaiseOffsetMeters);
            LastGroundCoverRaiseMedianBuildingBaseY = oldCoverY + selectedOffset;
            LastGroundCoverRaiseAverageBuildingBaseY = LastGroundCoverRaiseMedianBuildingBaseY;
            LastGroundCoverRaiseP25BuildingBaseY = LastGroundCoverRaiseMedianBuildingBaseY;
            LastGroundCoverRaiseStatus = "insufficient_building_samples_using_configured_fallback";
        }

        float cappedOffset = Mathf.Clamp(selectedOffset, 0f, Mathf.Max(0f, config.maxRaiseOffsetMeters));
        LastGroundCoverRaiseCapped = Mathf.Abs(cappedOffset - selectedOffset) > 0.001f;
        if (LastGroundCoverRaiseCapped)
        {
            LastGroundCoverRaiseStatus += "_capped";
        }

        float raisedYBeforeMicro = Mathf.Clamp(oldCoverY + cappedOffset, -20f, 30f);
        float microAdditional = ResolveGroundMicroRaiseAdditional(raisedYBeforeMicro);
        float raisedYAfterMicro = Mathf.Clamp(raisedYBeforeMicro + microAdditional, -20f, 30f);
        float raiseOffsetAfterMicro = Mathf.Max(0f, cappedOffset + microAdditional);
        float raisedYAfter30 = ResolveGroundRaise30Y(oldCoverY, raisedYAfterMicro, raiseOffsetAfterMicro);
        LastGroundCoverRaiseOffset = Mathf.Max(0f, raisedYAfter30 - oldCoverY);
        LastGroundCoverRaiseNewY = raisedYAfter30;
        LastSampledBuildingBaseY = LastGroundCoverRaiseMedianBuildingBaseY;
        LastVisualGroundReferenceY = LastGroundCoverRaiseNewY;
        LastVisualGroundSampleValid = true;
        CalculateRemainingBuildingGapAfterRaise(samples, LastGroundCoverRaiseNewY);
        return LastGroundCoverRaiseNewY;
    }

    private float ResolveGroundRaise30Y(float baseGroundY, float currentRaisedY, float currentRaiseOffset)
    {
        NewMapGroundRaise30Config config = groundRaise30Config ?? NewMapGroundRaise30Config.Default();
        LastGroundRaise30Enabled = config.enabled;
        LastGroundRaise30OldGroundY = currentRaisedY;
        LastGroundRaise30OldRaiseOffset = currentRaiseOffset;
        LastGroundRaise30NewGroundY = currentRaisedY;
        LastGroundRaise30NewRaiseOffset = currentRaiseOffset;
        LastGroundRaise30ActualRaiseMeters = 0f;
        LastGroundRaise30ActualRaisePercent = 0f;

        if (!config.enabled)
        {
            LastGroundRaise30BaselineMode = "disabled";
            LastGroundRaise30Status = "disabled_by_config";
            return currentRaisedY;
        }

        float raisePercent = config.RaisePercent;
        float targetY = currentRaisedY;
        float targetOffset = currentRaiseOffset;
        float baselineMagnitude;
        if (config.UseExistingRaiseOffsetBaseline(baseGroundY, currentRaiseOffset))
        {
            LastGroundRaise30BaselineMode = "existing_raise_offset";
            baselineMagnitude = Mathf.Max(0.001f, currentRaiseOffset);
            targetOffset = Mathf.Clamp(
                currentRaiseOffset * (1f + raisePercent),
                0f,
                Mathf.Max(currentRaiseOffset, config.MaxTotalRaiseOffsetMeters));
            targetY = Mathf.Clamp(baseGroundY + targetOffset, -20f, 30f);
            LastGroundRaise30Status = "applied_30_percent_to_existing_raise_offset";
        }
        else if (Mathf.Abs(currentRaisedY) >= config.AbsoluteGroundYThresholdMeters)
        {
            LastGroundRaise30BaselineMode = "absolute_ground_y";
            baselineMagnitude = Mathf.Max(0.001f, Mathf.Abs(currentRaisedY));
            targetY = Mathf.Clamp(currentRaisedY * (1f + raisePercent), -20f, 30f);
            targetOffset = Mathf.Max(0f, targetY - baseGroundY);
            LastGroundRaise30Status = "applied_30_percent_to_absolute_ground_y";
        }
        else
        {
            LastGroundRaise30BaselineMode = "fallback_delta";
            float delta = Mathf.Clamp(
                config.fallbackDeltaMeters,
                config.minFallbackDeltaMeters,
                config.maxFallbackDeltaMeters);
            baselineMagnitude = Mathf.Max(0.001f, delta / Mathf.Max(0.001f, raisePercent));
            targetY = Mathf.Clamp(currentRaisedY + delta, -20f, 30f);
            targetOffset = Mathf.Max(0f, targetY - baseGroundY);
            LastGroundRaise30Status = "applied_fallback_delta_for_near_zero_ground";
        }

        LastGroundRaise30NewGroundY = targetY;
        LastGroundRaise30NewRaiseOffset = targetOffset;
        LastGroundRaise30ActualRaiseMeters = targetY - currentRaisedY;
        LastGroundRaise30ActualRaisePercent = baselineMagnitude > 0.001f
            ? LastGroundRaise30ActualRaiseMeters / baselineMagnitude
            : 0f;

        if (Mathf.Abs(LastGroundRaise30ActualRaiseMeters) <= 0.001f)
        {
            LastGroundRaise30Status += "_unchanged";
        }

        return targetY;
    }

    private float ResolveGroundMicroRaiseAdditional(float previousGroundY)
    {
        NewMapFinalGroundMicroRaiseConfig config = groundMicroRaiseConfig ?? NewMapFinalGroundMicroRaiseConfig.Default();
        LastGroundMicroRaiseEnabled = config.enabled;
        LastGroundMicroRaisePreviousY = previousGroundY;
        LastGroundMicroRaiseAppliedToSupportColliders = config.applyToSupportColliders;

        if (!config.enabled)
        {
            LastGroundMicroRaiseAdditionalMeters = 0f;
            LastGroundMicroRaiseNewY = previousGroundY;
            LastGroundMicroRaiseStatus = "disabled_by_config";
            return 0f;
        }

        if (!config.applyToGroundCover)
        {
            LastGroundMicroRaiseAdditionalMeters = 0f;
            LastGroundMicroRaiseNewY = previousGroundY;
            LastGroundMicroRaiseStatus = "ground_cover_application_disabled";
            return 0f;
        }

        if (!config.applyToSupportColliders)
        {
            LastGroundMicroRaiseAdditionalMeters = 0f;
            LastGroundMicroRaiseNewY = previousGroundY;
            LastGroundMicroRaiseStatus = "support_collider_application_disabled";
            return 0f;
        }

        if (!config.resnapPlayer || !config.resnapNpc || !config.resnapTargets || !config.resnapGreenFrames)
        {
            LastGroundMicroRaiseAdditionalMeters = 0f;
            LastGroundMicroRaiseNewY = previousGroundY;
            LastGroundMicroRaiseStatus = "resnap_alignment_flags_disabled";
            return 0f;
        }

        float additional = Mathf.Clamp(
            Mathf.Max(0f, config.additionalGroundRaiseMeters),
            0f,
            Mathf.Max(0f, config.maxAdditionalRaiseMeters));
        LastGroundMicroRaiseAdditionalMeters = additional;
        LastGroundMicroRaiseNewY = Mathf.Clamp(previousGroundY + additional, -20f, 30f);
        LastGroundMicroRaiseStatus = additional > 0.001f
            ? "applied_to_gameplay_ground_cover_and_support"
            : "enabled_zero_additional_raise";
        return LastGroundMicroRaiseNewY - previousGroundY;
    }

    private List<float> CollectGroundCoverRaiseBuildingBaseSamples(NewMapGroundCoverRaiseConfig config, out int skipped)
    {
        skipped = 0;
        var samples = new List<float>();
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!IsBuildingSnapdownRendererCandidate(renderer))
            {
                skipped++;
                continue;
            }

            Bounds bounds = renderer.bounds;
            if (bounds.size.x < config.minimumBuildingFootprintMeters ||
                bounds.size.z < config.minimumBuildingFootprintMeters ||
                bounds.size.y < config.minimumBuildingHeightMeters)
            {
                skipped++;
                continue;
            }

            samples.Add(bounds.min.y);
        }

        return samples;
    }

    private void CalculateRemainingBuildingGapAfterRaise(List<float> samples, float raisedY)
    {
        LastGroundCoverRaiseRemainingAverageGap = 0f;
        LastGroundCoverRaiseRemainingMaxGap = 0f;
        if (samples == null || samples.Count == 0)
        {
            return;
        }

        float total = 0f;
        int count = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            float gap = Mathf.Max(0f, samples[i] - raisedY);
            total += gap;
            LastGroundCoverRaiseRemainingMaxGap = Mathf.Max(LastGroundCoverRaiseRemainingMaxGap, gap);
            count++;
        }

        LastGroundCoverRaiseRemainingAverageGap = count > 0 ? total / count : 0f;
    }

    private static float Average(List<float> values)
    {
        if (values == null || values.Count == 0)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < values.Count; i++)
        {
            total += values[i];
        }

        return total / values.Count;
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
        LastSpawnFinalOverlapCheckCount = 0;
        LastSpawnRejectedFinalOverlapCount = 0;
        LastSpawnRejectedUnderBuildingOverhangCount = 0;
        LastSpawnFallbackUsed = false;
        LastSpawnValidationPassed = false;
        LastFallbackSafeSpawnId = "none";
        LastSpawnValidationSource = string.Empty;
        LastFinalSpawnPosition = Vector3.zero;
        LastSpawnRandomSeedUsed = 0;
        LastSpawnDeterministicSeedEnabled = false;
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

        float boundaryInset = config.RejectInsideAirWallMarginMeters;
        if (!IsInsidePlayableBounds(grounded, mapBounds, hasBounds) ||
            (LastPlayableBoundsValid && !LastPlayableBounds.ContainsXZ(grounded, boundaryInset)) ||
            (LastCircularBoundaryEnabled && !LastCircularBoundary.ContainsXZ(grounded, boundaryInset)))
        {
            LastSpawnRejectedOutOfBoundsCount++;
            return false;
        }

        float nearestDistance = CalculateNearestSpawnBuildingDistance(grounded, config);
        bool insideProxy = config.useBuildingBoundsRejection && config.useBuildingProxyCache && IsInsideBuildingBoundsXZ(grounded, buildingAvoidanceBounds, 0f);
        bool insideRenderer = config.useBuildingBoundsRejection && config.useRendererBoundsCache && IsInsideBuildingBoundsXZ(grounded, buildingRendererBounds, 0f);
        if (insideProxy || insideRenderer)
        {
            LastNearestBuildingDistance = nearestDistance;
            if (insideRenderer && !insideProxy)
            {
                LastSpawnRejectedUnderBuildingOverhangCount++;
            }

            LastSpawnRejectedInsideBuildingCount++;
            return false;
        }

        if (config.useBuildingBoundsRejection && nearestDistance < config.minDistanceFromBuildingMeters)
        {
            LastNearestBuildingDistance = nearestDistance;
            LastSpawnRejectedTooCloseToBuildingCount++;
            return false;
        }

        if (config.finalOverlapCheck && HasSpawnFinalOverlap(grounded, config))
        {
            LastNearestBuildingDistance = nearestDistance;
            LastSpawnRejectedFinalOverlapCount++;
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

        if (config.useGroundCoverHit && !hasSupportSurface)
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

    private static bool IsInsideBuildingBoundsXZ(Vector3 position, List<Bounds> boundsList, float marginMeters)
    {
        if (boundsList == null || boundsList.Count == 0)
        {
            return false;
        }

        float margin = Mathf.Max(0f, marginMeters);
        for (int i = 0; i < boundsList.Count; i++)
        {
            Bounds bounds = boundsList[i];
            if (position.x >= bounds.min.x - margin &&
                position.x <= bounds.max.x + margin &&
                position.z >= bounds.min.z - margin &&
                position.z <= bounds.max.z + margin)
            {
                return true;
            }
        }

        return false;
    }

    private float CalculateNearestSpawnBuildingDistance(Vector3 position, NewMapSpawnConfig config)
    {
        config = config ?? NewMapSpawnConfig.Default();
        float nearest = 9999f;
        if (config.useBuildingProxyCache)
        {
            nearest = Mathf.Min(nearest, CalculateNearestBuildingDistance(position, buildingAvoidanceBounds));
        }

        if (config.useRendererBoundsCache)
        {
            nearest = Mathf.Min(nearest, CalculateNearestBuildingDistance(position, buildingRendererBounds));
        }

        return nearest;
    }

    private static float CalculateNearestBuildingDistance(Vector3 position, List<Bounds> boundsList)
    {
        if (boundsList == null || boundsList.Count == 0)
        {
            return 9999f;
        }

        float nearest = 9999f;
        for (int i = 0; i < boundsList.Count; i++)
        {
            Bounds bounds = boundsList[i];
            float dx = AxisDistance(position.x, bounds.min.x, bounds.max.x);
            float dz = AxisDistance(position.z, bounds.min.z, bounds.max.z);
            nearest = Mathf.Min(nearest, Mathf.Sqrt(dx * dx + dz * dz));
        }

        return nearest;
    }

    private bool HasSpawnFinalOverlap(Vector3 grounded, NewMapSpawnConfig config)
    {
        LastSpawnFinalOverlapCheckCount++;
        float radius = config.PlayerCapsuleRadiusMeters;
        float height = config.PlayerCapsuleHeightMeters;
        if (config.useBuildingProxyCache && SpawnCapsuleOverlapsAnyBounds(grounded, radius, height, buildingAvoidanceBounds))
        {
            return true;
        }

        return config.useRendererBoundsCache && SpawnCapsuleOverlapsAnyBounds(grounded, radius, height, buildingRendererBounds);
    }

    private static bool SpawnCapsuleOverlapsAnyBounds(Vector3 grounded, float radius, float height, List<Bounds> boundsList)
    {
        if (boundsList == null || boundsList.Count == 0)
        {
            return false;
        }

        float capsuleMinY = grounded.y;
        float capsuleMaxY = grounded.y + Mathf.Max(0.1f, height);
        for (int i = 0; i < boundsList.Count; i++)
        {
            Bounds bounds = boundsList[i];
            if (capsuleMaxY < bounds.min.y || capsuleMinY > bounds.max.y)
            {
                continue;
            }

            float dx = AxisDistance(grounded.x, bounds.min.x, bounds.max.x);
            float dz = AxisDistance(grounded.z, bounds.min.z, bounds.max.z);
            if ((dx * dx + dz * dz) <= radius * radius)
            {
                return true;
            }
        }

        return false;
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

    private bool TryBuildConservativeBuildingObstacleBounds(Renderer renderer, out Bounds obstacleBounds)
    {
        obstacleBounds = default(Bounds);
        if (renderer == null || !IsUsableBuildingAvoidanceBounds(renderer.bounds))
        {
            return false;
        }

        string searchable = BuildRendererSearchText(renderer).ToLowerInvariant();
        if (ContainsSnapdownExcludedText(searchable))
        {
            return false;
        }

        bool buildingLike = IsBuildingRendererCandidate(renderer) ||
            searchable.Contains("bldg") ||
            searchable.Contains("building");
        if (!buildingLike)
        {
            return false;
        }

        LastBuildingPrecisionCandidateCount++;
        NewMapBuildingCollisionPrecisionConfig precision = buildingPrecisionConfig ?? NewMapBuildingCollisionPrecisionConfig.Default();
        LastBuildingPrecisionEnabled = precision.enabled;
        Bounds sourceBounds = renderer.bounds;
        if (!precision.enabled)
        {
            NewMapAirwallHardCleanupConfig legacyConfig = airwallCleanupConfig ?? NewMapAirwallHardCleanupConfig.Default();
            float maxFootprint = Mathf.Max(8f, legacyConfig.buildingObstacleMaxFootprintMeters);
            if (sourceBounds.size.x > maxFootprint || sourceBounds.size.z > maxFootprint)
            {
                LastBuildingObstacleBoundsFiltered++;
                return false;
            }

            obstacleBounds = sourceBounds;
            LastBuildingPrecisionTightProxyCount++;
            return true;
        }

        bool sourceLooksInflated =
            sourceBounds.size.x > precision.LargeBoundsThresholdMeters ||
            sourceBounds.size.z > precision.LargeBoundsThresholdMeters ||
            sourceBounds.size.x > precision.MaxColliderWidthMeters ||
            sourceBounds.size.z > precision.MaxColliderDepthMeters;
        if (sourceLooksInflated)
        {
            LastBuildingPrecisionInflatedBoundsFound++;
        }

        bool usedMeshFootprint = TryBuildProjectedMeshFootprintBounds(renderer, sourceBounds, precision, out Bounds footprintBounds);
        if (usedMeshFootprint)
        {
            LastBuildingPrecisionMeshFootprintCount++;
        }
        else
        {
            LastBuildingPrecisionRendererFallbackCount++;
            footprintBounds = sourceBounds;
        }

        if (footprintBounds.size.x > precision.MaxColliderWidthMeters ||
            footprintBounds.size.z > precision.MaxColliderDepthMeters ||
            footprintBounds.size.x < precision.MinColliderWidthMeters ||
            footprintBounds.size.z < precision.MinColliderWidthMeters)
        {
            LastBuildingPrecisionInflatedBoundsSkipped++;
            LastBuildingObstacleBoundsFiltered++;
            if (sourceLooksInflated)
            {
                LastBuildingPrecisionSkippedClusterRootCount++;
            }

            return false;
        }

        obstacleBounds = BuildTightBuildingFootprintBounds(footprintBounds, sourceBounds, precision);
        LastBuildingObstacleBoundsShrunk++;
        LastBuildingPrecisionTightProxyCount++;
        LastBuildingPrecisionMaxWidth = Mathf.Max(LastBuildingPrecisionMaxWidth, obstacleBounds.size.x);
        LastBuildingPrecisionMaxDepth = Mathf.Max(LastBuildingPrecisionMaxDepth, obstacleBounds.size.z);
        LastBuildingPrecisionColliderHeight = precision.ColliderHeightMeters;

        float sourceArea = Mathf.Max(0.01f, sourceBounds.size.x * sourceBounds.size.z);
        float proxyArea = Mathf.Max(0.01f, obstacleBounds.size.x * obstacleBounds.size.z);
        buildingPrecisionShrinkRatioTotal += Mathf.Clamp(proxyArea / sourceArea, 0f, 10f);
        LastBuildingPrecisionAverageShrinkRatio = buildingPrecisionShrinkRatioTotal / Mathf.Max(1, LastBuildingPrecisionTightProxyCount);
        return true;
    }

    private Bounds BuildTightBuildingFootprintBounds(
        Bounds footprintBounds,
        Bounds sourceBounds,
        NewMapBuildingCollisionPrecisionConfig config)
    {
        float shrink = config.ShrinkFactorXZ;
        NewMapBuildingCollisionFinalRefinementConfig finalRefinement =
            buildingFinalRefinementConfig ?? NewMapBuildingCollisionFinalRefinementConfig.Default();
        if (finalRefinement.enabled)
        {
            bool oversizedFootprint =
                footprintBounds.size.x > finalRefinement.MaxProxySizeMeters ||
                footprintBounds.size.z > finalRefinement.MaxProxySizeMeters;
            float refinedShrink = oversizedFootprint
                ? finalRefinement.NearRoadShrinkFactorXZ
                : finalRefinement.DefaultShrinkFactorXZ;
            if (refinedShrink < shrink - 0.001f)
            {
                LastBuildingFinalRefinementProxiesShrunk++;
            }

            shrink = Mathf.Min(shrink, refinedShrink);
        }

        float width = Mathf.Clamp(
            footprintBounds.size.x * shrink,
            config.MinColliderWidthMeters,
            config.MaxColliderWidthMeters);
        float depth = Mathf.Clamp(
            footprintBounds.size.z * shrink,
            config.MinColliderWidthMeters,
            config.MaxColliderDepthMeters);
        float height = config.ColliderHeightMeters;
        float baseY = sourceBounds.min.y;
        Vector3 center = new Vector3(
            footprintBounds.center.x,
            baseY + height * 0.5f,
            footprintBounds.center.z);
        return new Bounds(center, new Vector3(width, height, depth));
    }

    private static bool TryBuildProjectedMeshFootprintBounds(
        Renderer renderer,
        Bounds sourceBounds,
        NewMapBuildingCollisionPrecisionConfig config,
        out Bounds footprintBounds)
    {
        footprintBounds = default(Bounds);
        MeshFilter filter = renderer.GetComponent<MeshFilter>();
        Mesh mesh = filter != null ? filter.sharedMesh : null;
        if (mesh == null || !mesh.isReadable || mesh.vertexCount < 4)
        {
            return false;
        }

        Vector3[] vertices = mesh.vertices;
        int maxSamples = Mathf.Clamp(config.maxFootprintVertexSamples, 8, 4096);
        int step = Mathf.Max(1, Mathf.CeilToInt(vertices.Length / (float)maxSamples));
        var xs = new List<float>();
        var zs = new List<float>();
        for (int i = 0; i < vertices.Length; i += step)
        {
            Vector3 world = filter.transform.TransformPoint(vertices[i]);
            if (!IsFiniteVector3(world))
            {
                continue;
            }

            xs.Add(world.x);
            zs.Add(world.z);
        }

        if (xs.Count < 4 || zs.Count < 4)
        {
            return false;
        }

        xs.Sort();
        zs.Sort();
        float trim = Mathf.Clamp(config.vertexTrimPercent, 0f, 0.2f);
        float minX = Percentile(xs, trim);
        float maxX = Percentile(xs, 1f - trim);
        float minZ = Percentile(zs, trim);
        float maxZ = Percentile(zs, 1f - trim);
        if (maxX - minX < 0.1f || maxZ - minZ < 0.1f)
        {
            return false;
        }

        footprintBounds = new Bounds(
            new Vector3((minX + maxX) * 0.5f, sourceBounds.center.y, (minZ + maxZ) * 0.5f),
            new Vector3(maxX - minX, sourceBounds.size.y, maxZ - minZ));
        return IsFiniteVector3(footprintBounds.center) && IsFiniteVector3(footprintBounds.size);
    }

    private void NormalizeBuildingPrecisionBoundsToGroundY()
    {
        NewMapBuildingCollisionPrecisionConfig precision = buildingPrecisionConfig ?? NewMapBuildingCollisionPrecisionConfig.Default();
        if (!precision.enabled || !precision.useGroundCoverY || buildingAvoidanceBounds.Count == 0)
        {
            return;
        }

        float height = precision.ColliderHeightMeters;
        float baseY = Mathf.Clamp(LastRuntimeGroundSurfaceY, -20f, 30f);
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds bounds = buildingAvoidanceBounds[i];
            bounds.center = new Vector3(bounds.center.x, baseY + height * 0.5f, bounds.center.z);
            bounds.size = new Vector3(bounds.size.x, height, bounds.size.z);
            buildingAvoidanceBounds[i] = bounds;
        }

        LastBuildingPrecisionColliderHeight = height;
    }

    private void CarveBuildingPrecisionTargetClearances(List<NewMapRuntimeTarget> targets)
    {
        LastBuildingPrecisionTargetClearanceBoundsSplit = 0;
        LastBuildingPrecisionTargetClearanceBoundsRemoved = 0;
        LastBuildingPrecisionTargetClearanceZones = 0;

        NewMapBuildingCollisionPrecisionConfig precision = buildingPrecisionConfig ?? NewMapBuildingCollisionPrecisionConfig.Default();
        if (!precision.enabled || !precision.carveActiveTargetInteractionClearance || targets == null || targets.Count == 0 || buildingAvoidanceBounds.Count == 0)
        {
            return;
        }

        NewMapBuildingCollisionFinalRefinementConfig finalRefinement =
            buildingFinalRefinementConfig ?? NewMapBuildingCollisionFinalRefinementConfig.Default();
        float halfSize = finalRefinement.enabled
            ? Mathf.Max(precision.TargetInteractionClearanceMeters, finalRefinement.NearTargetClearanceMeters)
            : precision.TargetInteractionClearanceMeters;
        float minSize = precision.MinColliderWidthMeters;
        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
        {
            NewMapRuntimeTarget target = targets[targetIndex];
            if (target == null || target.Anchor == null || !target.ActiveInGame)
            {
                continue;
            }

            Vector3 targetPosition = target.Anchor.position;
            if (!IsInsideMovementBoundaryForDiagnostics(targetPosition))
            {
                continue;
            }

            Bounds clearance = new Bounds(
                new Vector3(targetPosition.x, LastRuntimeGroundSurfaceY + precision.ColliderHeightMeters * 0.5f, targetPosition.z),
                new Vector3(halfSize * 2f, precision.ColliderHeightMeters, halfSize * 2f));

            bool touchedAny = false;
            var replacement = new List<Bounds>(buildingAvoidanceBounds.Count + 4);
            for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
            {
                Bounds source = buildingAvoidanceBounds[i];
                if (!IntersectsXZ(source, clearance))
                {
                    replacement.Add(source);
                    continue;
                }

                touchedAny = true;
                int before = replacement.Count;
                AddSplitBuildingBoundsAroundClearance(source, clearance, minSize, replacement);
                int added = replacement.Count - before;
                if (added == 0)
                {
                    LastBuildingPrecisionTargetClearanceBoundsRemoved++;
                }
                else
                {
                    LastBuildingPrecisionTargetClearanceBoundsSplit += added;
                }
            }

            if (touchedAny)
            {
                LastBuildingPrecisionTargetClearanceZones++;
                buildingAvoidanceBounds.Clear();
                buildingAvoidanceBounds.AddRange(replacement);
            }
        }

        LastBuildingBoundsCacheCount = buildingAvoidanceBounds.Count;
        LastBuildingBoundsCacheBuilt = LastBuildingBoundsCacheCount > 0;
        RecalculateBuildingPrecisionMaxBounds();
    }

    private void RecalculateBuildingPrecisionMaxBounds()
    {
        LastBuildingPrecisionMaxWidth = 0f;
        LastBuildingPrecisionMaxDepth = 0f;
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds bounds = buildingAvoidanceBounds[i];
            LastBuildingPrecisionMaxWidth = Mathf.Max(LastBuildingPrecisionMaxWidth, bounds.size.x);
            LastBuildingPrecisionMaxDepth = Mathf.Max(LastBuildingPrecisionMaxDepth, bounds.size.z);
        }
    }

    private void AddBuildingObstacleBoundsWithFinalRefinement(Bounds obstacleBounds)
    {
        NewMapBuildingCollisionFinalRefinementConfig config =
            buildingFinalRefinementConfig ?? NewMapBuildingCollisionFinalRefinementConfig.Default();
        LastBuildingFinalRefinementEnabled = config.enabled;
        LastBuildingFinalRefinementProxiesScanned++;

        if (!config.enabled)
        {
            buildingAvoidanceBounds.Add(obstacleBounds);
            return;
        }

        float maxSize = config.MaxProxySizeMeters;
        bool overflow = obstacleBounds.size.x > maxSize || obstacleBounds.size.z > maxSize;
        if (overflow)
        {
            LastBuildingFinalRefinementOverflowFound++;
        }

        if (overflow && config.splitOversizedProxies)
        {
            int pieces = AddSplitBuildingBoundsByMaxSize(obstacleBounds, maxSize, buildingAvoidanceBounds);
            if (pieces > 0)
            {
                LastBuildingFinalRefinementProxiesSplit++;
                LastBuildingFinalRefinementSplitPiecesCreated += pieces;
                return;
            }
        }

        if (overflow && config.disableProxyIfStillBlocksApproach)
        {
            LastBuildingFinalRefinementProxiesDisabled++;
            LastBuildingObstacleBoundsFiltered++;
            return;
        }

        buildingAvoidanceBounds.Add(obstacleBounds);
    }

    private static int AddSplitBuildingBoundsByMaxSize(Bounds source, float maxSize, List<Bounds> output)
    {
        if (output == null)
        {
            return 0;
        }

        float safeMax = Mathf.Max(5f, maxSize);
        int xPieces = Mathf.Clamp(Mathf.CeilToInt(source.size.x / safeMax), 1, 16);
        int zPieces = Mathf.Clamp(Mathf.CeilToInt(source.size.z / safeMax), 1, 16);
        float pieceWidth = source.size.x / xPieces;
        float pieceDepth = source.size.z / zPieces;
        if (pieceWidth < 0.25f || pieceDepth < 0.25f)
        {
            return 0;
        }

        int added = 0;
        for (int x = 0; x < xPieces; x++)
        {
            for (int z = 0; z < zPieces; z++)
            {
                float centerX = source.min.x + pieceWidth * (x + 0.5f);
                float centerZ = source.min.z + pieceDepth * (z + 0.5f);
                output.Add(new Bounds(
                    new Vector3(centerX, source.center.y, centerZ),
                    new Vector3(pieceWidth, source.size.y, pieceDepth)));
                added++;
            }
        }

        return added;
    }

    private void CarveBuildingPrecisionSpawnClearance(Vector3 spawn)
    {
        LastBuildingFinalRefinementSpawnClearanceZones = 0;
        NewMapBuildingCollisionFinalRefinementConfig finalRefinement =
            buildingFinalRefinementConfig ?? NewMapBuildingCollisionFinalRefinementConfig.Default();
        if (!finalRefinement.enabled || buildingAvoidanceBounds.Count == 0)
        {
            return;
        }

        float halfSize = finalRefinement.NearSpawnClearanceMeters;
        if (halfSize <= 0.001f)
        {
            return;
        }

        NewMapBuildingCollisionPrecisionConfig precision = buildingPrecisionConfig ?? NewMapBuildingCollisionPrecisionConfig.Default();
        Bounds clearance = new Bounds(
            new Vector3(spawn.x, LastRuntimeGroundSurfaceY + precision.ColliderHeightMeters * 0.5f, spawn.z),
            new Vector3(halfSize * 2f, precision.ColliderHeightMeters, halfSize * 2f));

        bool touchedAny = false;
        var replacement = new List<Bounds>(buildingAvoidanceBounds.Count + 4);
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds source = buildingAvoidanceBounds[i];
            if (!IntersectsXZ(source, clearance))
            {
                replacement.Add(source);
                continue;
            }

            touchedAny = true;
            int before = replacement.Count;
            AddSplitBuildingBoundsAroundClearance(source, clearance, precision.MinColliderWidthMeters, replacement);
            int added = replacement.Count - before;
            if (added == 0)
            {
                LastBuildingFinalRefinementProxiesDisabled++;
            }
        }

        if (!touchedAny)
        {
            return;
        }

        LastBuildingFinalRefinementSpawnClearanceZones = 1;
        buildingAvoidanceBounds.Clear();
        buildingAvoidanceBounds.AddRange(replacement);
        LastBuildingBoundsCacheCount = buildingAvoidanceBounds.Count;
        LastBuildingBoundsCacheBuilt = LastBuildingBoundsCacheCount > 0;
        RecalculateBuildingPrecisionMaxBounds();
    }

    private static void AddSplitBuildingBoundsAroundClearance(Bounds source, Bounds clearance, float minSize, List<Bounds> output)
    {
        float minX = source.min.x;
        float maxX = source.max.x;
        float minZ = source.min.z;
        float maxZ = source.max.z;
        float carveMinX = Mathf.Clamp(clearance.min.x, minX, maxX);
        float carveMaxX = Mathf.Clamp(clearance.max.x, minX, maxX);
        float carveMinZ = Mathf.Clamp(clearance.min.z, minZ, maxZ);
        float carveMaxZ = Mathf.Clamp(clearance.max.z, minZ, maxZ);

        AddBuildingBoundsStrip(minX, carveMinX, minZ, maxZ, source, minSize, output);
        AddBuildingBoundsStrip(carveMaxX, maxX, minZ, maxZ, source, minSize, output);
        AddBuildingBoundsStrip(carveMinX, carveMaxX, minZ, carveMinZ, source, minSize, output);
        AddBuildingBoundsStrip(carveMinX, carveMaxX, carveMaxZ, maxZ, source, minSize, output);
    }

    private static void AddBuildingBoundsStrip(float minX, float maxX, float minZ, float maxZ, Bounds source, float minSize, List<Bounds> output)
    {
        float width = maxX - minX;
        float depth = maxZ - minZ;
        if (width < minSize || depth < minSize)
        {
            return;
        }

        output.Add(new Bounds(
            new Vector3((minX + maxX) * 0.5f, source.center.y, (minZ + maxZ) * 0.5f),
            new Vector3(width, source.size.y, depth)));
    }

    private static bool IntersectsXZ(Bounds a, Bounds b)
    {
        return a.min.x < b.max.x &&
            a.max.x > b.min.x &&
            a.min.z < b.max.z &&
            a.max.z > b.min.z;
    }

    private void RunBuildingCollisionPrecisionCorridorDiagnostics(List<NewMapRuntimeTarget> targets, Vector3 spawn)
    {
        LastBuildingPrecisionSampledCorridorCount = 0;
        LastBuildingPrecisionUnexpectedCorridorBlockers = 0;
        LastBuildingPrecisionActiveTargetApproachBlocked = 0;

        NewMapBuildingCollisionPrecisionConfig precision = buildingPrecisionConfig ?? NewMapBuildingCollisionPrecisionConfig.Default();
        if (!precision.enabled || !precision.sampleWalkCorridorValidation || buildingAvoidanceBounds.Count == 0)
        {
            return;
        }

        float clearance = precision.ApproachSampleClearanceMeters;
        Vector3[] directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 sample = spawn + directions[i] * 6f;
            sample.y = LastRuntimeGroundSurfaceY + GroundSkinOffset;
            LastBuildingPrecisionSampledCorridorCount++;
            if (IsInsideBuildingPrecisionBoundsXZ(sample, clearance))
            {
                LastBuildingPrecisionUnexpectedCorridorBlockers++;
            }
        }

        if (targets == null || targets.Count == 0)
        {
            return;
        }

        int inspectedTargets = 0;
        for (int targetIndex = 0; targetIndex < targets.Count && inspectedTargets < 32; targetIndex++)
        {
            NewMapRuntimeTarget target = targets[targetIndex];
            if (target == null || target.Anchor == null || !target.ActiveInGame)
            {
                continue;
            }

            inspectedTargets++;
            bool anyApproachClear = false;
            float[] radii =
            {
                Mathf.Max(4f, precision.ActiveTargetApproachRadiusMeters * 0.5f),
                Mathf.Max(8f, precision.ActiveTargetApproachRadiusMeters),
                Mathf.Max(12f, precision.ActiveTargetApproachRadiusMeters * 1.5f),
                Mathf.Max(18f, precision.ActiveTargetApproachRadiusMeters * 2f)
            };

            for (int radiusIndex = 0; radiusIndex < radii.Length; radiusIndex++)
            {
                float radius = radii[radiusIndex];
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Vector3 sample = target.Anchor.position + directions[directionIndex] * radius;
                    sample.y = LastRuntimeGroundSurfaceY + GroundSkinOffset;
                    if (!IsInsideMovementBoundaryForDiagnostics(sample))
                    {
                        continue;
                    }

                    LastBuildingPrecisionSampledCorridorCount++;
                    if (!IsInsideBuildingPrecisionBoundsXZ(sample, clearance))
                    {
                        anyApproachClear = true;
                    }
                }
            }

            if (!anyApproachClear)
            {
                LastBuildingPrecisionActiveTargetApproachBlocked++;
            }
        }
    }

    private bool IsInsideMovementBoundaryForDiagnostics(Vector3 position)
    {
        if (LastCircularBoundaryEnabled && LastCircularBoundary.IsValid)
        {
            return LastCircularBoundary.ContainsXZ(position, 0f);
        }

        return !LastPlayableBoundsValid || LastPlayableBounds.ContainsXZ(position, 0f);
    }

    private bool IsInsideBuildingPrecisionBoundsXZ(Vector3 position, float marginMeters)
    {
        return IsInsideBuildingBoundsXZ(position, buildingAvoidanceBounds, marginMeters);
    }

    private float ResolvePlayerBuildingCollisionMargin()
    {
        float margin = groundRaiseConfig != null ? groundRaiseConfig.playerBuildingCollisionMarginMeters : 0.35f;
        NewMapAirwallHardCleanupConfig config = airwallCleanupConfig ?? NewMapAirwallHardCleanupConfig.Default();
        if (config.enabled)
        {
            margin = Mathf.Min(margin, config.playerBuildingCollisionMarginMeters);
        }

        return Mathf.Clamp(margin, 0f, 5f);
    }

    private void ResetAirwallHardCleanupDiagnostics()
    {
        LastAirwallHardTotalCollidersScanned = 0;
        LastUnexpectedAirwallBlockersFound = 0;
        LastUnexpectedAirwallBlockersRemoved = 0;
        LastUnexpectedAirwallBlockersResized = 0;
        LastUnexpectedAirwallBlockersConvertedToTrigger = 0;
        LastConcaveMeshTriggerOffenderCount = 0;
        LastConcaveMeshTriggerFixedCount = 0;
        LastConcaveMeshTriggerProxyCount = 0;
        LastBoundaryAirWallsPreserved = 0;
        LastOldRectangularAirWallDisabledCount = 0;
        LastRouteVisualBlockingColliderCount = 0;
        LastGreenFrameBlockingColliderCount = 0;
        LastLabelBlockingColliderCount = 0;
        LastHazardVisualBlockingColliderCount = 0;
        LastInvalidZoneBlockersPreserved = 0;
        LastUnknownBlockersInsidePlayableArea = 0;
        LastBuildingObstacleBoundsFiltered = 0;
        LastBuildingObstacleBoundsShrunk = 0;
        LastBuildingPrecisionEnabled = buildingPrecisionConfig == null || buildingPrecisionConfig.enabled;
        LastBuildingPrecisionCandidateCount = 0;
        LastBuildingPrecisionInflatedBoundsFound = 0;
        LastBuildingPrecisionInflatedBoundsSkipped = 0;
        LastBuildingPrecisionTightProxyCount = 0;
        LastBuildingPrecisionMeshFootprintCount = 0;
        LastBuildingPrecisionRendererFallbackCount = 0;
        LastBuildingPrecisionSkippedClusterRootCount = 0;
        LastBuildingPrecisionAverageShrinkRatio = 0f;
        LastBuildingPrecisionMaxWidth = 0f;
        LastBuildingPrecisionMaxDepth = 0f;
        LastBuildingPrecisionColliderHeight = 0f;
        LastBuildingPrecisionSampledCorridorCount = 0;
        LastBuildingPrecisionUnexpectedCorridorBlockers = 0;
        LastBuildingPrecisionActiveTargetApproachBlocked = 0;
        LastBuildingPrecisionTargetClearanceBoundsSplit = 0;
        LastBuildingPrecisionTargetClearanceBoundsRemoved = 0;
        LastBuildingPrecisionTargetClearanceZones = 0;
        buildingPrecisionShrinkRatioTotal = 0f;
        LastSampledValidPathsPassable = false;
    }

    private void ResetBuildingFinalRefinementDiagnostics()
    {
        LastBuildingFinalRefinementEnabled = buildingFinalRefinementConfig == null || buildingFinalRefinementConfig.enabled;
        LastBuildingFinalRefinementProxiesScanned = 0;
        LastBuildingFinalRefinementOverflowFound = 0;
        LastBuildingFinalRefinementProxiesShrunk = 0;
        LastBuildingFinalRefinementProxiesSplit = 0;
        LastBuildingFinalRefinementSplitPiecesCreated = 0;
        LastBuildingFinalRefinementProxiesDisabled = 0;
        LastBuildingFinalRefinementSpawnClearanceZones = 0;
    }

    private void AuditAndCleanupUnexpectedAirwallColliders()
    {
        NewMapAirwallHardCleanupConfig config = airwallCleanupConfig ?? NewMapAirwallHardCleanupConfig.Default();
        Collider[] colliders = FindObjectsOfType<Collider>(true);
        LastAirwallHardTotalCollidersScanned = colliders.Length;
        if (!config.enabled)
        {
            LastSampledValidPathsPassable =
                LastPlayableBoundsValid &&
                LastCircularBoundaryEnabled &&
                LastPlayableAirWallColliderCount == 0 &&
                LastBuildingPrecisionUnexpectedCorridorBlockers == 0 &&
                LastBuildingPrecisionActiveTargetApproachBlocked == 0;
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                continue;
            }

            string path = GetTransformPath(collider.transform);
            string category = ClassifyBlockingCollider(path, collider);
            bool blocksPlayer = collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger;
            bool insidePlayableArea = IsColliderInsideNormalPlayableArea(collider.bounds, config);
            NeutralizeExistingConcaveMeshTrigger(collider);

            if (IsAllowedPlayerBlockingCategory(category))
            {
                if (category == "map_boundary" && blocksPlayer)
                {
                    LastBoundaryAirWallsPreserved++;
                }

                continue;
            }

            if (category == "old_air_wall")
            {
                if (blocksPlayer)
                {
                    collider.enabled = false;
                    LastUnexpectedAirwallBlockersFound++;
                    LastUnexpectedAirwallBlockersRemoved++;
                    LastOldRectangularAirWallDisabledCount++;
                }

                continue;
            }

            if (!blocksPlayer || !insidePlayableArea)
            {
                continue;
            }

            if (category == "route_line_visual")
            {
                LastRouteVisualBlockingColliderCount++;
            }
            else if (category == "green_frame_visual")
            {
                LastGreenFrameBlockingColliderCount++;
            }
            else if (category == "label_visual")
            {
                LastLabelBlockingColliderCount++;
            }
            else if (category == "hazard_visual")
            {
                LastHazardVisualBlockingColliderCount++;
            }

            if (category == "interaction_trigger" && config.convertInteractionBlockersToTriggers)
            {
                LastUnexpectedAirwallBlockersFound++;
                ConvertOrReplaceBlockingColliderAsTrigger(collider, category);
                continue;
            }

            if (category == "debug_test" && config.disableDebugTestColliders)
            {
                collider.enabled = false;
                LastUnexpectedAirwallBlockersFound++;
                LastUnexpectedAirwallBlockersRemoved++;
                continue;
            }

            if (ShouldDisablePlayerBlockingCollider(category))
            {
                collider.enabled = false;
                LastUnexpectedAirwallBlockersFound++;
                LastUnexpectedAirwallBlockersRemoved++;
                if (category == "unknown")
                {
                    LastUnknownBlockersInsidePlayableArea++;
                }

                continue;
            }

            if (category == "unknown" && config.convertUnknownPlayableBlockersToTriggers)
            {
                LastUnexpectedAirwallBlockersFound++;
                ConvertOrDisableUnexpectedPlayableBlocker(collider);
                continue;
            }

            if (category == "unknown")
            {
                LastUnknownBlockersInsidePlayableArea++;
            }
        }

        LastSampledValidPathsPassable =
            LastPlayableBoundsValid &&
            LastCircularBoundaryEnabled &&
            LastPlayableAirWallColliderCount == 0 &&
            LastUnknownBlockersInsidePlayableArea == 0 &&
            LastBuildingPrecisionUnexpectedCorridorBlockers == 0 &&
            LastBuildingPrecisionActiveTargetApproachBlocked == 0;
    }

    private static bool IsAllowedPlayerBlockingCategory(string category)
    {
        return category == "ground_support" ||
            category == "building_obstacle" ||
            category == "npc_body" ||
            category == "map_boundary" ||
            category == "player_body";
    }

    private static bool ShouldDisablePlayerBlockingCollider(string category)
    {
        return category == "route_line_visual" ||
            category == "green_frame_visual" ||
            category == "shelter_marker_visual" ||
            category == "label_visual" ||
            category == "hazard_visual" ||
            category == "debug_test" ||
            category == "invalid_zone_blocker";
    }

    private void NeutralizeExistingConcaveMeshTrigger(Collider collider)
    {
        MeshCollider meshCollider = collider as MeshCollider;
        if (meshCollider == null || !meshCollider.isTrigger || meshCollider.convex)
        {
            return;
        }

        LastConcaveMeshTriggerOffenderCount++;
        meshCollider.isTrigger = false;
        LastConcaveMeshTriggerFixedCount++;
    }

    private void ConvertOrReplaceBlockingColliderAsTrigger(Collider collider, string category)
    {
        MeshCollider meshCollider = collider as MeshCollider;
        if (meshCollider != null && !meshCollider.convex)
        {
            LastConcaveMeshTriggerOffenderCount++;
            CreatePrimitiveTriggerProxy(collider, category);
            collider.enabled = false;
            LastConcaveMeshTriggerFixedCount++;
            LastUnexpectedAirwallBlockersRemoved++;
            return;
        }

        collider.isTrigger = true;
        LastUnexpectedAirwallBlockersConvertedToTrigger++;
    }

    private void ConvertOrDisableUnexpectedPlayableBlocker(Collider collider)
    {
        MeshCollider meshCollider = collider as MeshCollider;
        if (meshCollider != null && !meshCollider.convex)
        {
            LastConcaveMeshTriggerOffenderCount++;
            collider.enabled = false;
            LastConcaveMeshTriggerFixedCount++;
            LastUnexpectedAirwallBlockersRemoved++;
            return;
        }

        collider.isTrigger = true;
        LastUnexpectedAirwallBlockersConvertedToTrigger++;
    }

    private void CreatePrimitiveTriggerProxy(Collider source, string category)
    {
        if (source == null || !IsFiniteVector3(source.bounds.center) || !IsFiniteVector3(source.bounds.size))
        {
            return;
        }

        GameObject proxy = new GameObject("NewMap_" + category + "_PrimitiveTriggerProxy");
        proxy.transform.SetParent(transform, true);
        proxy.transform.position = source.bounds.center;
        proxy.transform.rotation = Quaternion.identity;
        BoxCollider box = proxy.AddComponent<BoxCollider>();
        box.size = new Vector3(
            Mathf.Max(0.5f, source.bounds.size.x),
            Mathf.Max(0.5f, source.bounds.size.y),
            Mathf.Max(0.5f, source.bounds.size.z));
        box.center = Vector3.zero;
        box.isTrigger = true;
        LastConcaveMeshTriggerProxyCount++;
        LastUnexpectedAirwallBlockersConvertedToTrigger++;
    }

    private bool IsColliderInsideNormalPlayableArea(Bounds bounds, NewMapAirwallHardCleanupConfig config)
    {
        if (!LastPlayableBoundsValid || !IsFiniteVector3(bounds.center) || !IsFiniteVector3(bounds.size))
        {
            return false;
        }

        float margin = Mathf.Max(0f, config.minDistanceFromBoundaryForPlayableCorridorMeters);
        if (LastCircularBoundaryEnabled && LastCircularBoundary.IsValid)
        {
            return LastCircularBoundary.ContainsXZ(bounds.center, margin);
        }

        return LastPlayableBounds.ContainsXZ(bounds.center, margin);
    }

    private static string ClassifyBlockingCollider(string path, Collider collider)
    {
        string lower = (path ?? string.Empty).ToLowerInvariant();
        if (lower.Contains("circularboundary") || lower.Contains("circular_boundary"))
        {
            return "map_boundary";
        }

        if (lower.Contains("playableboundsroot") || lower.Contains("airwall") || lower.Contains("boundaryairwall"))
        {
            return "old_air_wall";
        }

        if (lower.Contains("gameplaygroundcoverroot") || lower.Contains("gameplaysupportroot") || lower.Contains("groundcover"))
        {
            return "ground_support";
        }

        if (lower.Contains("crowdroot") || lower.Contains("newmap_npc"))
        {
            return "npc_body";
        }

        if (lower.Contains("newmap_player") || lower.Contains("playerspawnroot"))
        {
            return "player_body";
        }

        if (lower.Contains("shelterdirectline") || lower.Contains("routeline") || lower.Contains("route_line") || lower.Contains("routeguide"))
        {
            return "route_line_visual";
        }

        if (lower.Contains("greenframeroot") || lower.Contains("greenframe"))
        {
            return "green_frame_visual";
        }

        if (lower.Contains("label") || lower.Contains("namelabel") || lower.Contains("textmesh"))
        {
            return "label_visual";
        }

        if (lower.Contains("hazardvisualroot") || lower.Contains("lightcurtain") || lower.Contains("tsunami") || lower.Contains("debriswarning"))
        {
            return "hazard_visual";
        }

        if (lower.Contains("sheltermarkerroot") || lower.Contains("candidatemarkerroot"))
        {
            return "shelter_marker_visual";
        }

        if (lower.Contains("marker") || lower.Contains("target") || lower.Contains("interaction") || lower.Contains("shelterentrance"))
        {
            return "interaction_trigger";
        }

        if (lower.Contains("debug") || lower.Contains("diagnostic") || lower.Contains("test"))
        {
            return "debug_test";
        }

        if (lower.Contains("bldg") || lower.Contains("building"))
        {
            return "building_obstacle";
        }

        if (lower.Contains("invalid") || lower.Contains("fall") || lower.Contains("blue") || lower.Contains("blocker"))
        {
            return "invalid_zone_blocker";
        }

        return "unknown";
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

    private NewMapCircularBoundary ResolveCircularBoundary(
        Bounds mapBounds,
        bool hasBounds,
        NewMapPlayableBounds playableBounds,
        NewMapCircularBoundaryConfig config)
    {
        config = config ?? NewMapCircularBoundaryConfig.Default();
        Vector2 center;
        string source = config.centerSource ?? "original_map_center";
        if (source == "manual")
        {
            center = new Vector2(config.centerX, config.centerZ);
        }
        else if (hasBounds && IsFiniteVector3(mapBounds.center))
        {
            center = new Vector2(mapBounds.center.x, mapBounds.center.z);
        }
        else if (playableBounds.IsValid)
        {
            center = new Vector2(playableBounds.CenterX, playableBounds.CenterZ);
        }
        else
        {
            NewMapPlayableBounds fallback = NewMapPlayableBounds.DefaultDocumented();
            center = new Vector2(fallback.CenterX, fallback.CenterZ);
        }

        return new NewMapCircularBoundary(
            config.enabled,
            center,
            config.RadiusMeters,
            config.BoundaryHeightMeters,
            config.affectsPlayer,
            config.affectsNpc,
            source);
    }

    private void EnsureCircularBoundaryDiagnostics(Transform root, NewMapCircularBoundary boundary)
    {
        LastPlayableAirWallColliderCount = 0;
        LastPlayableAirWallVisibleRendererCount = 0;
        LastCircularBoundaryDiagnosticColliderCount = 0;
        if (root == null)
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

        if (!boundary.IsValid || circularBoundaryConfig == null || !circularBoundaryConfig.enabled)
        {
            return;
        }

        GameObject marker = new GameObject("P10_CircularBoundary_RuntimeClamp_Diagnostic");
        marker.transform.SetParent(root, true);
        marker.transform.position = new Vector3(boundary.Center.x, 0f, boundary.Center.y);
        marker.SetActive(circularBoundaryConfig.debugVisible);
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
        NewMapPlayableBounds coverBounds = bounds;
        if (LastCircularBoundaryEnabled && LastCircularBoundary.IsValid)
        {
            float radius = LastCircularBoundary.RadiusMeters;
            coverBounds = new NewMapPlayableBounds(
                LastCircularBoundary.Center.x - radius,
                LastCircularBoundary.Center.x + radius,
                LastCircularBoundary.Center.y - radius,
                LastCircularBoundary.Center.y + radius,
                bounds.MarginMeters,
                bounds.BoundaryHeightMeters,
                bounds.BoundaryThicknessMeters);
        }

        Material material = ResolveGameplayGroundCoverMaterial(config, out string materialSource);
        Color materialColor = ReadMaterialColor(material, config.ToColor());
        LastGameplayGroundCoverMaterialName = material != null ? material.name : "missing_material";
        LastGameplayGroundCoverMaterialSource = materialSource;
        LastGameplayGroundCoverMaterialBlueLike = IsBlueLikeColor(materialColor);
        LastGameplayGroundCoverMaterialMagentaLike = IsMagentaLikeColor(materialColor);

        float tileSize = Mathf.Clamp(config.tileSizeMeters, 80f, 900f);
        int maxTiles = Mathf.Clamp(config.maxTileCount, 1, 2000);
        int columns = Mathf.Max(1, Mathf.CeilToInt(coverBounds.Width / tileSize));
        int rows = Mathf.Max(1, Mathf.CeilToInt(coverBounds.Depth / tileSize));
        if (columns * rows > maxTiles)
        {
            float adjusted = Mathf.Sqrt(Mathf.Max(1f, coverBounds.Width * coverBounds.Depth) / maxTiles) * 1.05f;
            tileSize = Mathf.Clamp(adjusted, tileSize, 1200f);
            columns = Mathf.Max(1, Mathf.CeilToInt(coverBounds.Width / tileSize));
            rows = Mathf.Max(1, Mathf.CeilToInt(coverBounds.Depth / tileSize));
        }

        float thickness = Mathf.Clamp(config.thicknessMeters, 0.05f, 2f);
        float overlap = Mathf.Clamp(config.overlapMeters, 0f, 2f);
        int tileIndex = 1;
        for (int row = 0; row < rows; row++)
        {
            float minZ = Mathf.Lerp(coverBounds.MinZ, coverBounds.MaxZ, row / (float)rows);
            float maxZ = Mathf.Lerp(coverBounds.MinZ, coverBounds.MaxZ, (row + 1) / (float)rows);
            for (int column = 0; column < columns; column++)
            {
                float minX = Mathf.Lerp(coverBounds.MinX, coverBounds.MaxX, column / (float)columns);
                float maxX = Mathf.Lerp(coverBounds.MinX, coverBounds.MaxX, (column + 1) / (float)columns);
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
        LastGameplayGroundCoverTotalArea = coverBounds.Width * coverBounds.Depth;
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

    private List<NewMapRuntimeTarget> FilterTargetsByPlayableBoundary(List<NewMapRuntimeTarget> targets)
    {
        LastActiveTargetsInsidePlayableBoundaryCount = 0;
        LastActiveTargetsOutsidePlayableBoundaryDisabledCount = 0;
        LastRouteGuidesSuppressedOutsidePlayableBoundaryCount = 0;

        if (targets == null || targets.Count == 0)
        {
            return new List<NewMapRuntimeTarget>();
        }

        var playableTargets = new List<NewMapRuntimeTarget>(targets.Count);
        for (int i = 0; i < targets.Count; i++)
        {
            NewMapRuntimeTarget target = targets[i];
            if (target == null)
            {
                continue;
            }

            if (target.Anchor == null || !IsInsideActivePlayableBoundary(target.Anchor.position, 0f))
            {
                if (target.RouteGuide != null || target.RouteGuideFactory != null)
                {
                    LastRouteGuidesSuppressedOutsidePlayableBoundaryCount++;
                }

                DisableTargetOutsidePlayableBoundary(target);
                LastActiveTargetsOutsidePlayableBoundaryDisabledCount++;
                continue;
            }

            LastActiveTargetsInsidePlayableBoundaryCount++;
            playableTargets.Add(target);
        }

        return playableTargets;
    }

    private bool IsInsideActivePlayableBoundary(Vector3 position, float insetMeters)
    {
        if (LastCircularBoundaryEnabled && LastCircularBoundary.IsValid)
        {
            return LastCircularBoundary.ContainsXZ(position, insetMeters);
        }

        return !LastPlayableBoundsValid || LastPlayableBounds.ContainsXZ(position, insetMeters);
    }

    private static void DisableTargetOutsidePlayableBoundary(NewMapRuntimeTarget target)
    {
        if (target == null)
        {
            return;
        }

        target.DisabledReason = "disabled_out_of_playable_bounds_2_27km";
        target.GreenFrameFactory = null;
        target.RouteGuideFactory = null;
        target.EntryTrigger = null;

        SetInactive(target.Marker);
        SetInactive(target.GreenFrame);
        SetInactive(target.RouteGuide);
        if (target.Anchor != null)
        {
            target.Anchor.gameObject.SetActive(false);
        }
    }

    private static void SetInactive(GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
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
        float circularDiameter = LastCircularBoundaryEnabled && LastCircularBoundary.IsValid
            ? LastCircularBoundary.DiameterMeters + 100f
            : 0f;
        float width = Mathf.Max(100f, groundConfig.supportSizeX, circularDiameter);
        float depth = Mathf.Max(100f, groundConfig.supportSizeZ, circularDiameter);
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
            targets.Add(CreateOfficialShelterTarget(roots, record, position, bounds));
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
        Vector3 position,
        Bounds buildingBounds)
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
            HasBuildingEntryBounds = true,
            BuildingEntryBounds = buildingBounds,
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

    private void CreateBuildingEntryTriggers(Transform parent, List<NewMapRuntimeTarget> targets, NewMapGameController controller)
    {
        LastBuildingEntryTriggerCount = 0;
        LastBuildingEntryPhysicalBlockerCount = 0;
        if (targets == null || controller == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            NewMapRuntimeTarget target = targets[i];
            if (target == null || !target.ActiveInGame || target.Anchor == null)
            {
                continue;
            }

            NewMapBuildingEntryTrigger trigger;
            if (TryResolveBuildingEntryBounds(target, out Bounds entryBounds))
            {
                trigger = NewMapBuildingEntryTrigger.CreateFromBounds(
                    parent,
                    target,
                    controller,
                    entryBounds,
                    2.5f,
                    8f);
            }
            else
            {
                trigger = NewMapBuildingEntryTrigger.Create(
                    parent,
                    target,
                    controller,
                    ResolveBuildingEntryTriggerRadius(target),
                    6f);
            }
            if (trigger == null)
            {
                continue;
            }

            LastBuildingEntryTriggerCount++;
            if (!trigger.IsTriggerCollider)
            {
                LastBuildingEntryPhysicalBlockerCount++;
            }
        }
    }

    private bool TryResolveBuildingEntryBounds(NewMapRuntimeTarget target, out Bounds bounds)
    {
        bounds = default(Bounds);
        if (target == null || target.Anchor == null)
        {
            return false;
        }

        if (target.HasBuildingEntryBounds && target.BuildingEntryBounds.size.sqrMagnitude > 0.01f)
        {
            bounds = target.BuildingEntryBounds;
            return true;
        }

        if (IsLocalRuntimeTrainingTarget(target))
        {
            return false;
        }

        return TryFindNearestBuildingBounds(target.Anchor.position, 45f, out bounds);
    }

    private bool TryFindNearestBuildingBounds(Vector3 position, float maxDistanceMeters, out Bounds bounds)
    {
        bounds = default(Bounds);
        float bestDistance = Mathf.Max(1f, maxDistanceMeters);
        bool found = false;
        for (int i = 0; i < buildingAvoidanceBounds.Count; i++)
        {
            Bounds candidate = buildingAvoidanceBounds[i];
            Vector3 closest = candidate.ClosestPoint(position);
            float horizontalDistance = Vector2.Distance(
                new Vector2(position.x, position.z),
                new Vector2(closest.x, closest.z));
            if (horizontalDistance > bestDistance)
            {
                continue;
            }

            bestDistance = horizontalDistance;
            bounds = candidate;
            found = true;
        }

        return found;
    }

    private static bool IsLocalRuntimeTrainingTarget(NewMapRuntimeTarget target)
    {
        return target != null && !string.IsNullOrWhiteSpace(target.Id) && target.Id.StartsWith("newmap_proxy_", System.StringComparison.Ordinal);
    }

    private static float ResolveBuildingEntryTriggerRadius(NewMapRuntimeTarget target)
    {
        if (target == null)
        {
            return 5f;
        }

        float baseRadius = Mathf.Max(5f, target.InteractionDistance);
        if (target.IsOfficialShelter)
        {
            return Mathf.Clamp(baseRadius + 4f, 6f, 18f);
        }

        if (!string.IsNullOrWhiteSpace(target.Category) && target.Category.Contains("humanitarian_candidate"))
        {
            return Mathf.Clamp(baseRadius + 4f, 6f, 18f);
        }

        return Mathf.Clamp(baseRadius + 2f, 5f, 14f);
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
public sealed class NewMapGroundCoverRaiseConfig
{
    public bool enabled = true;
    public bool keepImportedBuildingsFixed = true;
    public float fallbackRaiseOffsetMeters = 2.4f;
    public float maxRaiseOffsetMeters = 10f;
    public float selectedPercentile = 0.50f;
    public float outlierIqrMultiplier = 1.5f;
    public int minimumBuildingBaseSampleCount = 16;
    public float minimumActionableRaiseOffsetMeters = 0.5f;
    public float minimumBuildingFootprintMeters = 1.0f;
    public float minimumBuildingHeightMeters = 1.0f;
    public bool playerBuildingCollisionEnabled = true;
    public float playerBuildingCollisionMarginMeters = 0.35f;
    public string strategy = "raise_current_gameplay_ground_cover_to_building_base_samples_not_gis_grade";

    public static NewMapGroundCoverRaiseConfig Default()
    {
        return new NewMapGroundCoverRaiseConfig();
    }

    public static NewMapGroundCoverRaiseConfig Load()
    {
        NewMapGroundCoverRaiseConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_cover_raise_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapGroundCoverRaiseConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap ground-cover raise config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.fallbackRaiseOffsetMeters = Mathf.Clamp(config.fallbackRaiseOffsetMeters, 0f, 10f);
        config.maxRaiseOffsetMeters = Mathf.Clamp(config.maxRaiseOffsetMeters, 0f, 10f);
        config.selectedPercentile = Mathf.Clamp(config.selectedPercentile, 0.25f, 0.75f);
        config.outlierIqrMultiplier = Mathf.Clamp(config.outlierIqrMultiplier, 0f, 4f);
        config.minimumBuildingBaseSampleCount = Mathf.Clamp(config.minimumBuildingBaseSampleCount, 1, 1000);
        config.minimumActionableRaiseOffsetMeters = Mathf.Clamp(config.minimumActionableRaiseOffsetMeters, 0f, 5f);
        config.minimumBuildingFootprintMeters = Mathf.Clamp(config.minimumBuildingFootprintMeters, 0.1f, 20f);
        config.minimumBuildingHeightMeters = Mathf.Clamp(config.minimumBuildingHeightMeters, 0.1f, 20f);
        config.playerBuildingCollisionMarginMeters = Mathf.Clamp(config.playerBuildingCollisionMarginMeters, 0f, 5f);
        config.keepImportedBuildingsFixed = true;
        config.playerBuildingCollisionEnabled = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapFinalGroundMicroRaiseConfig
{
    public bool enabled = true;
    public float additionalGroundRaiseMeters = 0.3f;
    public float maxAdditionalRaiseMeters = 1.0f;
    public bool applyToGroundCover = true;
    public bool applyToSupportColliders = true;
    public bool resnapPlayer = true;
    public bool resnapNpc = true;
    public bool resnapTargets = true;
    public bool resnapGreenFrames = true;

    public static NewMapFinalGroundMicroRaiseConfig Default()
    {
        return new NewMapFinalGroundMicroRaiseConfig();
    }

    public static NewMapFinalGroundMicroRaiseConfig Load()
    {
        NewMapFinalGroundMicroRaiseConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_final_ground_micro_raise_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapFinalGroundMicroRaiseConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap final ground micro-raise config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.additionalGroundRaiseMeters = Mathf.Clamp(config.additionalGroundRaiseMeters, 0f, 1f);
        config.maxAdditionalRaiseMeters = Mathf.Clamp(config.maxAdditionalRaiseMeters, 0f, 1f);
        if (config.additionalGroundRaiseMeters > config.maxAdditionalRaiseMeters)
        {
            config.additionalGroundRaiseMeters = config.maxAdditionalRaiseMeters;
        }

        return config;
    }
}

[System.Serializable]
public sealed class NewMapGroundRaise30Config
{
    public bool enabled = true;
    public float raisePercent = 0.30f;
    public string baselineMode = "existing_raise_offset";
    public float nearZeroGroundYThresholdMeters = 1.0f;
    public float absoluteGroundYThresholdMeters = 1.0f;
    public bool useExistingRaiseOffsetWhenNearZero = true;
    public float minFallbackDeltaMeters = 0.3f;
    public float maxFallbackDeltaMeters = 2.0f;
    public float fallbackDeltaMeters = 0.9f;
    public float maxTotalRaiseOffsetMeters = 10.0f;
    public bool resnapPlayer = true;
    public bool resnapNpc = true;
    public bool resnapTargets = true;
    public bool resnapGreenFrames = true;
    public bool resnapRouteMarkers = true;
    public bool resnapAirWallVerticalRange = true;

    public float RaisePercent => Mathf.Clamp(raisePercent, 0.01f, 1.0f);
    public float NearZeroGroundYThresholdMeters => Mathf.Clamp(nearZeroGroundYThresholdMeters, 0f, 5f);
    public float AbsoluteGroundYThresholdMeters => Mathf.Clamp(absoluteGroundYThresholdMeters, 0f, 5f);
    public float MaxTotalRaiseOffsetMeters => Mathf.Clamp(maxTotalRaiseOffsetMeters, 0.1f, 30f);

    public bool UseExistingRaiseOffsetBaseline(float baseGroundY, float currentRaiseOffset)
    {
        if (string.Equals(baselineMode, "existing_raise_offset", System.StringComparison.OrdinalIgnoreCase))
        {
            return currentRaiseOffset > 0.001f;
        }

        return useExistingRaiseOffsetWhenNearZero &&
            Mathf.Abs(baseGroundY) < NearZeroGroundYThresholdMeters &&
            currentRaiseOffset > 0.001f;
    }

    public static NewMapGroundRaise30Config Default()
    {
        return new NewMapGroundRaise30Config();
    }

    public static NewMapGroundRaise30Config Load()
    {
        NewMapGroundRaise30Config config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_raise_30_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapGroundRaise30Config>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap ground raise 30 percent config could not be loaded; using defaults. {exception.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(config.baselineMode))
        {
            config.baselineMode = "existing_raise_offset";
        }

        config.raisePercent = config.RaisePercent;
        config.nearZeroGroundYThresholdMeters = config.NearZeroGroundYThresholdMeters;
        config.absoluteGroundYThresholdMeters = config.AbsoluteGroundYThresholdMeters;
        config.minFallbackDeltaMeters = Mathf.Clamp(config.minFallbackDeltaMeters, 0f, 5f);
        config.maxFallbackDeltaMeters = Mathf.Clamp(config.maxFallbackDeltaMeters, config.minFallbackDeltaMeters, 5f);
        config.fallbackDeltaMeters = Mathf.Clamp(config.fallbackDeltaMeters, config.minFallbackDeltaMeters, config.maxFallbackDeltaMeters);
        config.maxTotalRaiseOffsetMeters = config.MaxTotalRaiseOffsetMeters;
        config.resnapPlayer = true;
        config.resnapNpc = true;
        config.resnapTargets = true;
        config.resnapGreenFrames = true;
        config.resnapRouteMarkers = true;
        config.resnapAirWallVerticalRange = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapAirwallHardCleanupConfig
{
    public bool enabled = true;
    public bool keepBoundaryAirWalls;
    public bool keepInvalidZoneBlockers;
    public bool convertInteractionBlockersToTriggers = true;
    public bool convertUnknownPlayableBlockersToTriggers = true;
    public bool disableDebugTestColliders = true;
    public float playerBuildingCollisionMarginMeters = 0.05f;
    public float buildingObstacleMaxFootprintMeters = 180f;
    public float buildingObstacleInsetMeters = 0.25f;
    public float minDistanceFromBoundaryForPlayableCorridorMeters = 6f;
    public string strategy = "collision_whitelist_disable_old_airwalls_use_circular_boundary_clamp";

    public static NewMapAirwallHardCleanupConfig Default()
    {
        return new NewMapAirwallHardCleanupConfig();
    }

    public static NewMapAirwallHardCleanupConfig Load()
    {
        NewMapAirwallHardCleanupConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_airwall_hard_cleanup_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapAirwallHardCleanupConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap airwall hard-cleanup config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.enabled = true;
        config.keepBoundaryAirWalls = false;
        config.keepInvalidZoneBlockers = false;
        config.convertInteractionBlockersToTriggers = true;
        config.convertUnknownPlayableBlockersToTriggers = true;
        config.disableDebugTestColliders = true;
        config.playerBuildingCollisionMarginMeters = Mathf.Clamp(config.playerBuildingCollisionMarginMeters, 0f, 0.5f);
        config.buildingObstacleMaxFootprintMeters = Mathf.Clamp(config.buildingObstacleMaxFootprintMeters, 20f, 400f);
        config.buildingObstacleInsetMeters = Mathf.Clamp(config.buildingObstacleInsetMeters, 0f, 2f);
        config.minDistanceFromBoundaryForPlayableCorridorMeters = Mathf.Clamp(config.minDistanceFromBoundaryForPlayableCorridorMeters, 0f, 50f);
        return config;
    }
}

[System.Serializable]
public sealed class NewMapBuildingCollisionPrecisionConfig
{
    public bool enabled = true;
    public string collisionMode = "tight_footprint_box_proxies";
    public float shrinkFactorXZ = 0.90f;
    public float minColliderWidthMeters = 1.0f;
    public float maxColliderWidthMeters = 80.0f;
    public float maxColliderDepthMeters = 80.0f;
    public float colliderHeightMeters = 5.0f;
    public bool useGroundCoverY = true;
    public bool splitLargeBounds = true;
    public float largeBoundsThresholdMeters = 80.0f;
    public bool disableClusterRootColliders = true;
    public bool sampleWalkCorridorValidation = true;
    public bool carveActiveTargetInteractionClearance = true;
    public float vertexTrimPercent = 0.02f;
    public int maxFootprintVertexSamples = 512;
    public float approachSampleClearanceMeters = 0.25f;
    public float activeTargetApproachRadiusMeters = 12.0f;
    public float targetInteractionClearanceMeters = 14.0f;
    public string strategy = "project_renderer_mesh_to_xz_shrink_and_cap_runtime_building_obstacle_bounds_not_gis_grade";

    public float ShrinkFactorXZ => Mathf.Clamp(shrinkFactorXZ, 0.50f, 1.0f);
    public float MinColliderWidthMeters => Mathf.Clamp(minColliderWidthMeters, 0.25f, 20.0f);
    public float MaxColliderWidthMeters => Mathf.Clamp(maxColliderWidthMeters, 5.0f, 120.0f);
    public float MaxColliderDepthMeters => Mathf.Clamp(maxColliderDepthMeters, 5.0f, 120.0f);
    public float ColliderHeightMeters => Mathf.Clamp(colliderHeightMeters, 1.0f, 12.0f);
    public float LargeBoundsThresholdMeters => Mathf.Clamp(largeBoundsThresholdMeters, 10.0f, 200.0f);
    public float ApproachSampleClearanceMeters => Mathf.Clamp(approachSampleClearanceMeters, 0.0f, 3.0f);
    public float ActiveTargetApproachRadiusMeters => Mathf.Clamp(activeTargetApproachRadiusMeters, 4.0f, 40.0f);
    public float TargetInteractionClearanceMeters => Mathf.Clamp(targetInteractionClearanceMeters, 4.0f, 30.0f);

    public static NewMapBuildingCollisionPrecisionConfig Default()
    {
        return new NewMapBuildingCollisionPrecisionConfig();
    }

    public static NewMapBuildingCollisionPrecisionConfig Load()
    {
        NewMapBuildingCollisionPrecisionConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_building_collision_precision_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapBuildingCollisionPrecisionConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap building collision precision config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.enabled = true;
        config.collisionMode = string.IsNullOrWhiteSpace(config.collisionMode)
            ? "tight_footprint_box_proxies"
            : config.collisionMode;
        config.shrinkFactorXZ = config.ShrinkFactorXZ;
        config.minColliderWidthMeters = config.MinColliderWidthMeters;
        config.maxColliderWidthMeters = config.MaxColliderWidthMeters;
        config.maxColliderDepthMeters = config.MaxColliderDepthMeters;
        config.colliderHeightMeters = config.ColliderHeightMeters;
        config.largeBoundsThresholdMeters = config.LargeBoundsThresholdMeters;
        config.vertexTrimPercent = Mathf.Clamp(config.vertexTrimPercent, 0f, 0.2f);
        config.maxFootprintVertexSamples = Mathf.Clamp(config.maxFootprintVertexSamples, 8, 4096);
        config.approachSampleClearanceMeters = config.ApproachSampleClearanceMeters;
        config.activeTargetApproachRadiusMeters = config.ActiveTargetApproachRadiusMeters;
        config.targetInteractionClearanceMeters = config.TargetInteractionClearanceMeters;
        config.carveActiveTargetInteractionClearance = true;
        config.disableClusterRootColliders = true;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapBuildingCollisionFinalRefinementConfig
{
    public bool enabled = true;
    public float defaultShrinkFactorXZ = 0.85f;
    public float nearRoadShrinkFactorXZ = 0.75f;
    public float nearTargetClearanceMeters = 4.0f;
    public float nearSpawnClearanceMeters = 6.0f;
    public float maxProxySizeMeters = 60.0f;
    public bool splitOversizedProxies = true;
    public bool disableProxyIfStillBlocksApproach = true;

    public float DefaultShrinkFactorXZ => Mathf.Clamp(defaultShrinkFactorXZ, 0.50f, 1.0f);
    public float NearRoadShrinkFactorXZ => Mathf.Clamp(nearRoadShrinkFactorXZ, 0.50f, 1.0f);
    public float NearTargetClearanceMeters => Mathf.Clamp(nearTargetClearanceMeters, 0f, 30f);
    public float NearSpawnClearanceMeters => Mathf.Clamp(nearSpawnClearanceMeters, 0f, 30f);
    public float MaxProxySizeMeters => Mathf.Clamp(maxProxySizeMeters, 5f, 120f);

    public static NewMapBuildingCollisionFinalRefinementConfig Default()
    {
        return new NewMapBuildingCollisionFinalRefinementConfig();
    }

    public static NewMapBuildingCollisionFinalRefinementConfig Load()
    {
        NewMapBuildingCollisionFinalRefinementConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_building_collision_final_refinement.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapBuildingCollisionFinalRefinementConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap building collision final refinement config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.defaultShrinkFactorXZ = config.DefaultShrinkFactorXZ;
        config.nearRoadShrinkFactorXZ = config.NearRoadShrinkFactorXZ;
        config.nearTargetClearanceMeters = config.NearTargetClearanceMeters;
        config.nearSpawnClearanceMeters = config.NearSpawnClearanceMeters;
        config.maxProxySizeMeters = config.MaxProxySizeMeters;
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
    public float minDistanceFromBuildingMeters = 4f;
    public int maxSpawnAttempts = 500;
    public float playerCapsuleRadiusMeters = 0.35f;
    public float playerCapsuleHeightMeters = 1.8f;
    public bool finalOverlapCheck = true;
    public bool fallbackSafeSpawnEnabled = true;
    public bool useBuildingBoundsRejection = true;
    public bool useBuildingProxyCache = true;
    public bool useRendererBoundsCache = true;
    public bool useGroundProbe = true;
    public bool useGroundSupportFallback = true;
    public bool useGroundCoverHit = true;
    public string fallbackSafeSpawnId = "newmap_safe_spawn_01";
    public int spawnRandomSeed = 20260529;
    public bool deterministicSeedEnabled = false;
    public bool tryRandomBeforeMapCenter = true;
    public bool randomizeFallbackSafeSpawnOrder = true;
    public float minDistanceFromAirWallMeters = 3f;
    public float rejectInsideAirWallMarginMeters = 3f;

    public float PlayerCapsuleRadiusMeters => Mathf.Clamp(playerCapsuleRadiusMeters, 0.1f, 2f);
    public float PlayerCapsuleHeightMeters => Mathf.Clamp(playerCapsuleHeightMeters, 0.5f, 4f);
    public float RejectInsideAirWallMarginMeters => Mathf.Clamp(
        Mathf.Max(minDistanceFromAirWallMeters, rejectInsideAirWallMarginMeters),
        0f,
        100f);

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
        config.rejectInsideAirWallMarginMeters = Mathf.Clamp(config.rejectInsideAirWallMarginMeters, 0f, 50f);
        config.playerCapsuleRadiusMeters = config.PlayerCapsuleRadiusMeters;
        config.playerCapsuleHeightMeters = config.PlayerCapsuleHeightMeters;
        config.maxSpawnAttempts = Mathf.Clamp(config.maxSpawnAttempts, 1, 2000);
        config.finalOverlapCheck = true;
        config.fallbackSafeSpawnEnabled = true;
        config.useBuildingProxyCache = true;
        config.useRendererBoundsCache = true;
        config.useGroundCoverHit = true;
        if (string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId))
        {
            config.fallbackSafeSpawnId = "newmap_safe_spawn_01";
        }

        return config;
    }

    public static int CreateSessionRandomSeed(int configuredSeed)
    {
        unchecked
        {
            int seed = configuredSeed;
            seed = (seed * 397) ^ System.Environment.TickCount;
            seed = (seed * 397) ^ System.Guid.NewGuid().GetHashCode();
            seed = (seed * 397) ^ (int)(System.DateTime.UtcNow.Ticks & 0x7fffffff);
            return seed == 0 ? 1 : seed;
        }
    }

    public int ResolveSeedForDiagnostics()
    {
        return deterministicSeedEnabled ? spawnRandomSeed : CreateSessionRandomSeed(spawnRandomSeed);
    }
}

[System.Serializable]
public struct NewMapCircularBoundary
{
    public bool Enabled;
    public Vector2 Center;
    public float RadiusMeters;
    public float BoundaryHeightMeters;
    public bool AffectsPlayer;
    public bool AffectsNpc;
    public string CenterSource;

    public NewMapCircularBoundary(
        bool enabled,
        Vector2 center,
        float radiusMeters,
        float boundaryHeightMeters,
        bool affectsPlayer,
        bool affectsNpc,
        string centerSource)
    {
        Enabled = enabled;
        Center = center;
        RadiusMeters = Mathf.Max(1f, radiusMeters);
        BoundaryHeightMeters = Mathf.Max(1f, boundaryHeightMeters);
        AffectsPlayer = affectsPlayer;
        AffectsNpc = affectsNpc;
        CenterSource = centerSource ?? "original_map_center";
    }

    public bool IsValid => Enabled && RadiusMeters > 1f;
    public float DiameterMeters => RadiusMeters * 2f;

    public bool ContainsXZ(Vector3 position, float insetMeters = 0f)
    {
        if (!IsValid)
        {
            return false;
        }

        float radius = Mathf.Max(0.1f, RadiusMeters - Mathf.Max(0f, insetMeters));
        Vector2 delta = new Vector2(position.x - Center.x, position.z - Center.y);
        return delta.sqrMagnitude <= radius * radius && position.y >= -50f && position.y <= BoundaryHeightMeters + 50f;
    }

    public Vector3 ClampXZ(Vector3 position, float insetMeters = 0f)
    {
        if (!IsValid)
        {
            return position;
        }

        float radius = Mathf.Max(0.1f, RadiusMeters - Mathf.Max(0f, insetMeters));
        Vector2 delta = new Vector2(position.x - Center.x, position.z - Center.y);
        float magnitude = delta.magnitude;
        if (magnitude <= radius)
        {
            return position;
        }

        Vector2 direction = magnitude > 0.0001f ? delta / magnitude : Vector2.right;
        Vector2 clamped = Center + direction * radius;
        return new Vector3(clamped.x, position.y, clamped.y);
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
public sealed class NewMapCircularBoundaryConfig
{
    public bool enabled = true;
    public string centerSource = "original_map_center";
    public float centerX;
    public float centerZ;
    public float radiusMeters = 2270f;
    public float boundaryHeightMeters = 300f;
    public string boundaryMode = "runtime_circular_clamp";
    public bool visibleInNormalMode;
    public bool debugVisible;
    public bool affectsPlayer = true;
    public bool affectsNpc = true;

    public float RadiusMeters => Mathf.Clamp(radiusMeters, 1f, 10000f);
    public float BoundaryHeightMeters => Mathf.Clamp(boundaryHeightMeters, 5f, 1000f);

    public static NewMapCircularBoundaryConfig Default()
    {
        return new NewMapCircularBoundaryConfig();
    }

    public static NewMapCircularBoundaryConfig Load()
    {
        NewMapCircularBoundaryConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_circular_boundary_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapCircularBoundaryConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap circular boundary config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.centerSource = string.IsNullOrWhiteSpace(config.centerSource) ? "original_map_center" : config.centerSource;
        config.radiusMeters = config.RadiusMeters;
        config.boundaryHeightMeters = config.BoundaryHeightMeters;
        config.boundaryMode = string.IsNullOrWhiteSpace(config.boundaryMode) ? "runtime_circular_clamp" : config.boundaryMode;
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
