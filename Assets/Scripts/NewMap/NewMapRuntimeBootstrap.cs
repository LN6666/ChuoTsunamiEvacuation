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

    private readonly List<Bounds> buildingAvoidanceBounds = new List<Bounds>();
    private NewMapSpawnConfig spawnConfig;
    private NewMapSafeSpawnPointDataset safeSpawnDataset;

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
        PrepareManualTestRoots(roots);
        Physics.SyncTransforms();
        LastMapBoundsValid = TryResolveRuntimeMapBounds(out Bounds mapBounds, out int rendererCount, out int colliderCount);
        LastMapBounds = mapBounds;
        LastRendererCount = rendererCount;
        LastColliderCount = colliderCount;
        long boundsMs = stopwatch.ElapsedMilliseconds;

        LastOldRuntimeGroundSurfaceY = 0f;
        Vector3 spawn = ResolveSpawnPosition(mapBounds, LastMapBoundsValid, roots["DebugDiagnosticsRoot"]);
        LastFinalSpawnPosition = spawn;
        LastPlayerSpawnGroundDelta = spawn.y - LastRuntimeGroundSurfaceY;
        EnsureRuntimeCollisionSupportProxy(roots["GameplaySupportRoot"], new Vector3(spawn.x, LastRuntimeGroundSurfaceY, spawn.z));
        Physics.SyncTransforms();
        if (SuppressSceneMeshCollidersForManualTest)
        {
            StartCoroutine(DisableSceneMeshCollidersStaged());
        }
        long spawnSupportMs = stopwatch.ElapsedMilliseconds - boundsMs;

        NewMapPlayerController player = NewMapPlayerController.Create(roots["PlayerSpawnRoot"], spawn);
        NewMapRuntimeUI.EnsureRuntimeEventSystem();
        NewMapRuntimeUI ui = NewMapRuntimeUI.Create(roots["UIAnchorRoot"]);
        NewMapLightingController lighting = NewMapLightingController.Create(roots["RuntimeSystemsRoot"]);
        NewMapHazardController hazard = NewMapHazardController.Create(roots["HazardVisualRoot"], roots["CollapseDebrisRoot"], spawn);
        NewMapNpcCrowdPrototype crowd = NewMapNpcCrowdPrototype.Create(roots["CrowdRoot"], spawn);
        NewMapPerformanceProbe.Create(roots["PerformanceMetricsRoot"]);
        long systemsMs = stopwatch.ElapsedMilliseconds - boundsMs - spawnSupportMs;
        List<NewMapRuntimeTarget> targets = CreateVerifiedOfficialShelterTargets(roots);
        targets.AddRange(CreateRecoveredNonOfficialCandidateTargets(roots, spawn));
        if (ShouldEnableLocalTrainingProxyTargets())
        {
            targets.AddRange(CreateLocalRuntimeTargets(roots, spawn));
        }

        CalculateActiveTargetHeightOffsets(targets);
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
            $"spawnX={LastFinalSpawnPosition.x:F2} spawnY={LastFinalSpawnPosition.y:F2} spawnZ={LastFinalSpawnPosition.z:F2}");
    }

    private static void PrepareManualTestRoots(Dictionary<string, Transform> roots)
    {
        if (roots.TryGetValue("GameplaySupportRoot", out Transform supportRoot) && supportRoot != null)
        {
            supportRoot.gameObject.SetActive(true);
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
        string ground = LastUsedGroundSupportProxy
            ? "Ground: invisible runtime support proxy aligned to the round-2 fallback visual height"
            : "Ground: invisible runtime support proxy aligned to sampled visual building/ground base";
        string collisionProxy = LastRuntimeCollisionSupportProxyActive
            ? " | Runtime collision support proxy active and renderer hidden; scene MeshCollider shutdown is disabled at player startup"
            : string.Empty;
        return $"{ground}{collisionProxy} | Old P3/P5 targets disabled unless remapped.";
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
        bool hasSupportSurface = LastVisualGroundSampleValid || hasBounds || LastUsedGroundSupportProxy;

        Vector3 candidate = new Vector3(basePosition.x, supportSurfaceY + GroundSkinOffset, basePosition.z);
        if (TryValidateSpawnCandidate(candidate, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, "map_bounds_center", out Vector3 accepted))
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
            candidate.y = supportSurfaceY + GroundSkinOffset;

            if (TryValidateSpawnCandidate(candidate, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, "random_playable_support", out accepted))
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

            candidate = new Vector3(safePoint.position.x, supportSurfaceY + GroundSkinOffset, safePoint.position.z);
            if (TryValidateSpawnCandidate(candidate, supportSurfaceY, hasSupportSurface, mapBounds, hasBounds, config, "fallback_safe_spawn:" + safePoint.id, out accepted))
            {
                LastFallbackSafeSpawnId = safePoint.id;
                return accepted;
            }
        }

        LastSpawnValidationPassed = false;
        LastFallbackSafeSpawnId = string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId) ? "none" : config.fallbackSafeSpawnId;
        LastNearestBuildingDistance = CalculateNearestBuildingDistance(candidate);
        LastSpawnValidationSource = "unvalidated_last_resort_support_center";
        return new Vector3(basePosition.x, supportSurfaceY + GroundSkinOffset, basePosition.z);
    }

    private float ResolveSupportSurfaceY(Bounds mapBounds, bool hasBounds, Vector3 basePosition, float rayStartY)
    {
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

        if (!TryResolveGroundedSpawn(candidate, supportSurfaceY, hasSupportSurface, config, out Vector3 grounded))
        {
            LastSpawnRejectedNoGroundCount++;
            return false;
        }

        if (!IsInsidePlayableBounds(grounded, mapBounds, hasBounds))
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
        return true;
    }

    private static bool TryResolveGroundedSpawn(
        Vector3 candidate,
        float supportSurfaceY,
        bool hasSupportSurface,
        NewMapSpawnConfig config,
        out Vector3 grounded)
    {
        grounded = new Vector3(candidate.x, supportSurfaceY + GroundSkinOffset, candidate.z);
        if (!IsFiniteVector3(grounded) || supportSurfaceY < -20f || supportSurfaceY > 30f)
        {
            return false;
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
                position.z <= bounds.max.z &&
                position.y >= bounds.min.y - 2f &&
                position.y <= bounds.max.y + 2f)
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
            if (position.y < bounds.min.y - 3f || position.y > bounds.max.y + 3f)
            {
                continue;
            }

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
                name.Contains("GameplaySupport"))
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
            case "PerformanceMetricsRoot":
                return true;
            default:
                return false;
        }
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

            float offset = Mathf.Abs(target.Anchor.position.y - (LastRuntimeGroundSurfaceY + GroundSkinOffset));
            LastMaxActiveTargetHeightOffset = Mathf.Max(LastMaxActiveTargetHeightOffset, offset);
            if (offset > 2.5f)
            {
                LastActiveTargetHeightOffsetViolations++;
            }
        }
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
        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.name = "NewMap_RuntimeGroundSupport_DocumentedProxy";
        support.transform.SetParent(parent, true);
        support.transform.position = center - Vector3.up * 0.25f;
        support.transform.localScale = new Vector3(6000f, 0.5f, 6000f);
        Renderer renderer = support.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        LastRuntimeCollisionSupportRendererVisible = renderer != null && renderer.enabled;
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

    private static List<NewMapRuntimeTarget> CreateVerifiedOfficialShelterTargets(Dictionary<string, Transform> roots)
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

            targets.Add(CreateOfficialShelterTarget(roots, record, position));
        }

        return targets;
    }

    private static List<NewMapRuntimeTarget> CreateRecoveredNonOfficialCandidateTargets(Dictionary<string, Transform> roots, Vector3 spawn)
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

            position.y = spawn.y;
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

    private static List<NewMapRuntimeTarget> CreateLocalRuntimeTargets(Dictionary<string, Transform> roots, Vector3 spawn)
    {
        var targets = new List<NewMapRuntimeTarget>
        {
            CreateTarget(
                roots,
                "newmap_proxy_safe_floor",
                "Local Training Proxy - Safe Floor",
                spawn,
                spawn + new Vector3(12f, 0f, 10f),
                false,
                false,
                true,
                "Runtime safe-floor proxy: E starts vertical evacuation and can succeed."),
            CreateTarget(
                roots,
                "newmap_proxy_blocked_entrance",
                "Local Training Proxy - Blocked Entrance",
                spawn,
                spawn + new Vector3(18f, 0f, -7f),
                false,
                true,
                true,
                "Runtime entrance-blocked proxy: E triggers entrance_blocked result."),
            CreateTarget(
                roots,
                "newmap_proxy_no_safe_floor",
                "Local Training Proxy - No Safe Floor",
                spawn,
                spawn + new Vector3(-13f, 0f, 11f),
                false,
                false,
                false,
                "Runtime safe-floor failure proxy: E triggers safe_floor_unavailable result."),
            CreateTarget(
                roots,
                "newmap_proxy_crowd_delay",
                "Local Training Proxy - Crowd Delay",
                spawn,
                spawn + new Vector3(8f, 0f, 4f),
                false,
                false,
                true,
                "Runtime crowd-delay proxy: E starts safe-floor climb with local NPC congestion delay.")
        };

        return targets;
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
        config.maxSpawnAttempts = Mathf.Clamp(config.maxSpawnAttempts, 1, 2000);
        if (string.IsNullOrWhiteSpace(config.fallbackSafeSpawnId))
        {
            config.fallbackSafeSpawnId = "newmap_safe_spawn_01";
        }

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
}
