using System.IO;
using NUnit.Framework;
using UnityEngine;

public class NewMapIntegrationConfigTests
{
    [Test]
    public void WeatherAndModeSpeedsMatchNewMapPolicy()
    {
        Assert.AreEqual(1.0f, NewMapRuntimeConstants.EvacuationWalkSpeed);
        Assert.AreEqual(5.0f, NewMapRuntimeConstants.EvacuationSprintSpeed);
        Assert.AreEqual(2.0f, NewMapRuntimeConstants.TourismWalkSpeed);
        Assert.AreEqual(10.0f, NewMapRuntimeConstants.TourismSprintSpeed);
        Assert.AreEqual(1.0f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.ClearDay));
        Assert.AreEqual(0.75f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.RainyDay));
        Assert.AreEqual(0.85f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.NightClear));
        Assert.AreEqual(0.65f, NewMapRuntimeConstants.GetWeatherModifier(NewMapWeatherPreset.NightRain));
    }

    [Test]
    public void TsunamiModeHotfixConfigsAreExplicit()
    {
        string staminaPath = Path.Combine(Application.dataPath, "Data/P10/newmap_player_stamina_config.json");
        string tsunamiPath = Path.Combine(Application.dataPath, "Data/P10/newmap_tsunami_mode_hotfix_config.json");
        string spawnPath = Path.Combine(Application.dataPath, "Data/P10/newmap_spawn_config.json");
        Assert.IsTrue(File.Exists(staminaPath), "100x stamina config must exist.");
        Assert.IsTrue(File.Exists(tsunamiPath), "Tsunami hotfix config must exist.");
        Assert.IsTrue(File.Exists(spawnPath), "Spawn config must exist.");

        NewMapPlayerStaminaConfig stamina = NewMapPlayerStaminaConfig.Load();
        Assert.AreEqual(100f, stamina.baselineMaxStamina, 0.001f);
        Assert.AreEqual(100f, stamina.staminaMultiplier, 0.001f);
        Assert.AreEqual(10000f, stamina.MaxStamina, 0.001f);

        NewMapTsunamiModeHotfixConfig tsunami = NewMapTsunamiModeHotfixConfig.Load();
        Assert.AreEqual("south", tsunami.NormalizedTsunamiStartSide);
        Assert.GreaterOrEqual(tsunami.WarningPhaseSeconds, 1f);
        Assert.GreaterOrEqual(tsunami.CurtainHeightMeters, 1000f);
        Assert.GreaterOrEqual(tsunami.MinimumCurtainLengthMeters, 1500f);
        Assert.IsFalse(tsunami.CurtainThicknessMeters <= 0f);

        NewMapSpawnConfig spawn = NewMapSpawnConfig.Load();
        Assert.IsTrue(spawn.randomSpawnEnabled);
        Assert.IsTrue(spawn.tryRandomBeforeMapCenter);
        Assert.IsTrue(spawn.randomizeFallbackSafeSpawnOrder);
        Assert.IsFalse(spawn.deterministicSeedEnabled, "Normal player sessions must not use the fixed diagnostic seed.");
        spawn.deterministicSeedEnabled = true;
        spawn.spawnRandomSeed = 2468;
        Assert.AreEqual(2468, spawn.ResolveSeedForDiagnostics());
    }

    [Test]
    public void NewMapStatusFilesUseStrictCompletionStatuses()
    {
        string matrixPath = Path.Combine(Application.dataPath, "Data/P10/newmap_p2_p10_full_completion_matrix.json");
        Assert.IsTrue(File.Exists(matrixPath), "Completion matrix JSON must exist.");
        string matrix = File.ReadAllText(matrixPath);
        Assert.IsFalse(matrix.Contains("basic " + "complete"));
        Assert.IsFalse(matrix.Contains("mostly " + "done"));
        Assert.IsFalse(matrix.Contains("proxy" + "-ready"));
        StringAssert.Contains("completed_with_documented_runtime_proxy", matrix);
        StringAssert.Contains("disabled_missing_from_new_map", matrix);
    }

    [Test]
    public void MissingOldTargetsAreNotActive()
    {
        string targetStatusPath = Path.Combine(Application.dataPath, "Data/P10/newmap_target_remap_status.json");
        Assert.IsTrue(File.Exists(targetStatusPath), "Target remap status JSON must exist.");
        string status = File.ReadAllText(targetStatusPath);
        Assert.IsFalse(status.Contains("\"sourcePhase\":\"P5\"") && status.Contains("\"activeInGame\":true"));
        StringAssert.Contains("disabled_missing_from_new_map", status);
        StringAssert.Contains("newmap_proxy_safe_floor", status);
    }

    [Test]
    public void NonMemoryFinalReportsRecordAnchorFitAndRouteValidation()
    {
        string anchorPath = Path.Combine(Application.dataPath, "Data/P10/newmap_official_shelter_anchor_final_check.json");
        string transformPath = Path.Combine(Application.dataPath, "Data/P10/newmap_coordinate_transform_anchor_fit.json");
        string routePath = Path.Combine(Application.dataPath, "Data/P10/newmap_route_geometry_final_validation.json");
        Assert.IsTrue(File.Exists(anchorPath), "Official anchor final check JSON must exist.");
        Assert.IsTrue(File.Exists(transformPath), "Coordinate transform anchor-fit JSON must exist.");
        Assert.IsTrue(File.Exists(routePath), "Route geometry final validation JSON must exist.");

        string anchor = File.ReadAllText(anchorPath);
        string transform = File.ReadAllText(transformPath);
        string route = File.ReadAllText(routePath);

        StringAssert.Contains("\"activeOfficialShelterCount\": 15", anchor);
        StringAssert.Contains("\"activeOfficialCountEqualsVerifiedAnchors\": true", anchor);
        StringAssert.Contains("transform_validated_from_official_anchors", transform);
        StringAssert.Contains("\"controlPointCount\": 15", transform);
        StringAssert.Contains("\"validatedOnNewChuoBaseMapCount\": 60", route);
        StringAssert.Contains("\"officialRouteClaimCount\": 0", route);
        StringAssert.Contains("not a GIS-grade", transform);
    }

    [Test]
    public void NonMemoryFinalReportsKeepRoutesAndDisabledTargetsHonest()
    {
        string routePath = Path.Combine(Application.dataPath, "Data/P10/newmap_route_geometry_final_validation.json");
        string p5Path = Path.Combine(Application.dataPath, "Data/P10/newmap_p5_route_candidate_final_status.json");
        string disabledPath = Path.Combine(Application.dataPath, "Data/P10/newmap_disabled_targets_final.json");
        Assert.IsTrue(File.Exists(routePath), "Route validation JSON must exist.");
        Assert.IsTrue(File.Exists(p5Path), "P5 final status JSON must exist.");
        Assert.IsTrue(File.Exists(disabledPath), "Disabled-target final JSON must exist.");

        string route = File.ReadAllText(routePath);
        string p5 = File.ReadAllText(p5Path);
        string disabled = File.ReadAllText(disabledPath);

        Assert.IsFalse(route.Contains("\"isOfficialEvacuationRoute\": true"), "No route record may claim official evacuation route status.");
        StringAssert.Contains("\"runtimeOldRouteLineSpawnCount\": 0", route);
        StringAssert.Contains("not an official evacuation route", p5);
        StringAssert.Contains("\"preflightMustFailIfDisabledTargetActive\": true", disabled);
        Assert.IsFalse(disabled.Contains("\"activeInGame\": true"), "Disabled-target report must not contain active records.");
    }

    [Test]
    public void RemainingHardeningRecoversNonOfficialCandidatesWithWarnings()
    {
        string recoveryPath = Path.Combine(Application.dataPath, "Data/P10/newmap_non_official_candidate_recovery.json");
        string activePath = Path.Combine(Application.dataPath, "Data/P10/newmap_active_target_final_report.json");
        string greenPath = Path.Combine(Application.dataPath, "Data/P10/newmap_candidate_green_frame_final_status.json");
        Assert.IsTrue(File.Exists(recoveryPath), "Non-official candidate recovery JSON must exist.");
        Assert.IsTrue(File.Exists(activePath), "Active target report JSON must exist.");
        Assert.IsTrue(File.Exists(greenPath), "Green-frame status JSON must exist.");

        string recovery = File.ReadAllText(recoveryPath);
        string active = File.ReadAllText(activePath);
        string green = File.ReadAllText(greenPath);

        StringAssert.Contains("\"totalCandidateRecordsLoaded\": 110", recovery);
        StringAssert.Contains("\"finalActiveNonOfficialCandidateCount\": 78", recovery);
        StringAssert.Contains("\"activeExactGmlAnchorCount\": 61", recovery);
        StringAssert.Contains("\"activeNearestBuildingAnchorCount\": 3", recovery);
        StringAssert.Contains("\"activeCoordinateProxyAnchorCount\": 14", recovery);
        StringAssert.Contains("\"disabledOutOfNewMapCount\": 32", recovery);
        StringAssert.Contains("\"nonOfficialWarningRequired\": true", recovery);
        StringAssert.Contains("\"safeApprovedByDefault\": false", recovery);
        Assert.IsFalse(recovery.Contains("\"isOfficialShelter\": true"), "Recovered non-official candidates must not become official shelters.");

        StringAssert.Contains("\"activeNonOfficialHumanitarianCandidateCount\": 78", active);
        StringAssert.Contains("\"activeNonOfficialTrainingTargetCount\": 82", active);
        StringAssert.Contains("\"nonOfficialTargetLabeledOfficialCount\": 0", active);
        StringAssert.Contains("\"disabledSelectableCount\": 0", active);
        StringAssert.Contains("\"resultPanelWarningTextExists\": true", green);
    }

    [Test]
    public void ManualBlockerFixReportsAndNpcDistributionConfigExist()
    {
        string[] requiredPaths =
        {
            "Data/P10/newmap_manual_blocker_audit.json",
            "Data/P10/newmap_mouse_look_camera_status.json",
            "Data/P10/newmap_debug_object_cleanup_report.json",
            "Data/P10/newmap_lighting_visual_status.json",
            "Data/P10/newmap_material_visual_quality_status.json",
            "Data/P10/newmap_ground_visual_alignment_status.json",
            "Data/P10/newmap_building_clipping_status.json",
            "Data/P10/newmap_post_visual_fix_gameplay_status.json",
            "Data/P10/newmap_npc_distribution_config.json",
            "Data/P10/newmap_npc_distribution_report.json",
            "Data/P10/newmap_mouse_drag_look_config.json",
            "Data/P10/newmap_mouse_drag_look_status.json",
            "Data/P10/newmap_ground_height_realignment_round2.json",
            "Data/P10/newmap_building_material_texture_audit_round2.json",
            "Data/P10/newmap_lighting_profiles.json",
            "Data/P10/newmap_night_lighting_correction.json",
            "Data/P10/newmap_debug_cleanup_round2.json",
            "Data/P10/newmap_visual_round2_gameplay_regression.json",
            "Data/P10/newmap_spawn_config.json",
            "Data/P10/newmap_safe_spawn_points.json",
            "Data/P10/newmap_spawn_validation_report.json",
            "Data/P10/newmap_building_bounds_cache_status.json",
            "Data/P10/newmap_blue_ground_diagnosis.json",
            "Data/P10/newmap_support_surface_visibility_status.json",
            "Data/P10/newmap_ground_visual_alignment_round3.json",
            "Data/P10/newmap_road_visual_sanity_round3.json",
            "Data/P10/newmap_building_floating_round3.json",
            "Data/P10/newmap_playable_bounds_config.json",
            "Data/P10/newmap_playable_bounds_report.json",
            "Data/P10/newmap_object_name_label_source_report.json",
            "Data/P10/newmap_name_label_config.json",
            "Data/P10/newmap_name_label_cache.json",
            "Data/P10/newmap_name_cache.json",
            "Data/P10/newmap_name_label_runtime_report.json",
            "Data/P10/newmap_name_enrichment_config.json",
            "Data/P10/newmap_name_enrichment_report.json",
            "Data/P10/newmap_name_cache_coverage_audit.json",
            "Data/P10/newmap_name_enrichment_query_list.json",
            "Data/P10/newmap_name_normalization_report.json",
            "Data/P10/newmap_gameplay_ground_cover_config.json",
            "Data/P10/newmap_ground_cover_material_report.json",
            "Data/P10/newmap_gameplay_ground_cover_report.json",
            "Data/P10/newmap_ground_raise_alignment_report.json",
            "Data/P10/newmap_full_fall_prevention_report.json",
            "Data/P10/newmap_blue_area_cover_status.json",
            "Data/P10/newmap_ground_cover_spawn_npc_target_status.json",
            "Data/P10/newmap_floating_building_snapdown_config.json",
            "Data/P10/newmap_floating_building_snapdown_candidates.json",
            "Data/P10/newmap_floating_building_snapdown_report.json",
            "Data/P10/newmap_target_height_after_building_snapdown.json",
            "Data/P10/newmap_building_snapdown_visual_validation.json",
            "Data/P10/newmap_npc_100x_distribution_report.json",
            "Data/P10/newmap_npc_crowd_100x_gameplay_status.json",
            "Data/P10/newmap_npc_100x_performance_report.json",
            "Data/P10/newmap_building_snap_npc100x_regression.json",
            "Data/P10/newmap_ground_cover_raise_config.json",
            "Data/P10/newmap_ground_cover_raise_report.json",
            "Data/P10/newmap_ground_raise_runtime_resnap_status.json",
            "Data/P10/newmap_building_floating_after_ground_raise.json",
            "Data/P10/newmap_name_normalization_rules.json",
            "Data/P10/newmap_player_building_collision_report.json",
            "Data/P10/newmap_npc_building_collision_report.json",
            "Data/P10/newmap_npc_movement_config.json",
            "Data/P10/newmap_npc_continuous_movement_report.json",
            "Data/P10/newmap_unexpected_airwall_hard_audit.json",
            "Data/P10/newmap_airwall_hard_cleanup_report.json",
            "Data/P10/newmap_airwall_hard_cleanup_config.json",
            "Data/P10/newmap_player_npc_collision_config.json",
            "Data/P10/newmap_player_npc_collision_report.json",
            "Data/P10/newmap_npc_collision_regression_after_player_collision.json",
            "Data/P10/newmap_airwall_npc_label_regression.json",
            "Data/P10/newmap_player_stamina_config.json",
            "Data/P10/newmap_tsunami_mode_hotfix_config.json"
        };

        foreach (string relativePath in requiredPaths)
        {
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            Assert.IsTrue(File.Exists(fullPath), $"{relativePath} must exist for manual-blocker preflight.");
        }

        string config = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_distribution_config.json"));
        StringAssert.Contains("\"npcCountMultiplier\": 100", config);
        StringAssert.Contains("\"distributionRadiusMeters\": 1000", config);
        StringAssert.Contains("\"maxNpcCount\": 800", config);
        StringAssert.Contains("\"npcDistributionSeed\": 20260529", config);
        StringAssert.Contains("\"useSectorDistribution\": true", config);
        StringAssert.Contains("\"sectorCount\": 32", config);
        StringAssert.Contains("\"ringCount\": 6", config);
        StringAssert.Contains("\"avoidBuildings\": true", config);
        StringAssert.Contains("\"usePooling\": true", config);
        StringAssert.Contains("\"farNpcStaticProxyMode\": true", config);

        string movement = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_movement_config.json"));
        StringAssert.Contains("\"continuousMovementEnabled\": true", movement);
        StringAssert.Contains("\"stuckRecoveryEnabled\": true", movement);
        StringAssert.Contains("\"buildingAvoidanceEnabled\": true", movement);
        StringAssert.Contains("\"farNpcStaticProxyMode\": false", movement);

        string playerNpcCollision = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_player_npc_collision_config.json"));
        StringAssert.Contains("\"enabled\": true", playerNpcCollision);
        StringAssert.Contains("\"mode\": \"soft_blocking_with_near_capsules\"", playerNpcCollision);
        StringAssert.Contains("\"preventDirectOverlap\": true", playerNpcCollision);

        string mouse = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_mouse_drag_look_config.json"));
        StringAssert.Contains("\"lookRequiresMouseButton\": true", mouse);
        StringAssert.Contains("\"lookMouseButton\": \"RightMouse\"", mouse);
        StringAssert.Contains("\"allowedButtons\"", mouse);
        StringAssert.Contains("\"LeftMouse\"", mouse);
        StringAssert.Contains("\"RightMouse\"", mouse);
        StringAssert.Contains("\"cursorVisibleWhenNotDragging\": true", mouse);

        string spawn = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_spawn_config.json"));
        StringAssert.Contains("\"spawnMode\": \"road_or_playable_ground_only\"", spawn);
        StringAssert.Contains("\"useBuildingBoundsRejection\": true", spawn);
        StringAssert.Contains("\"fallbackSafeSpawnId\": \"newmap_safe_spawn_01\"", spawn);
        StringAssert.Contains("\"minDistanceFromAirWallMeters\": 2.0", spawn);

        string readiness = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_manual_playtest_readiness.json"));
        StringAssert.Contains("\"manualReadinessDecision\"", readiness);
    }

    [Test]
    public void GroundRaiseNameCollisionAndNpc100xReportsAreConstrained()
    {
        string snapConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_floating_building_snapdown_config.json"));
        string snapReport = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_floating_building_snapdown_report.json"));
        string targetReport = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_target_height_after_building_snapdown.json"));
        string raiseConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_ground_cover_raise_config.json"));
        string raiseReport = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_ground_cover_raise_report.json"));
        string playerCollision = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_player_building_collision_report.json"));
        string npcMovement = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_continuous_movement_report.json"));
        string npcReport = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_100x_distribution_report.json"));
        string npcGameplay = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_crowd_100x_gameplay_status.json"));
        string bootstrap = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapRuntimeBootstrap.cs"));
        string npc = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNpcCrowdPrototype.cs"));

        string compactSnapConfig = snapConfig.Replace(" ", string.Empty);
        string compactSnapReport = snapReport.Replace(" ", string.Empty);
        string compactTargetReport = targetReport.Replace(" ", string.Empty);
        string compactRaiseConfig = raiseConfig.Replace(" ", string.Empty);
        string compactRaiseReport = raiseReport.Replace(" ", string.Empty);
        string compactPlayerCollision = playerCollision.Replace(" ", string.Empty);
        string compactNpcMovement = npcMovement.Replace(" ", string.Empty);
        string compactNpcReport = npcReport.Replace(" ", string.Empty);
        string compactNpcGameplay = npcGameplay.Replace(" ", string.Empty);

        StringAssert.Contains("\"floatingGapThresholdMeters\":0.5", compactSnapConfig);
        StringAssert.Contains("\"maxSnapdownMeters\":8.0", compactSnapConfig);
        StringAssert.Contains("\"notGisGradeTerrainAccuracy\":true", compactSnapReport);
        StringAssert.Contains("\"markerGreenFrameRealignmentStatus\"", snapReport);
        StringAssert.Contains("\"greenFramesAlignToGroundCover\":true", compactTargetReport);
        StringAssert.Contains("\"keepImportedBuildingsFixed\":true", compactRaiseConfig);
        StringAssert.Contains("\"maxRaiseOffsetMeters\":10.0", compactRaiseConfig);
        StringAssert.Contains("\"buildingsMoved\":false", compactRaiseReport);
        StringAssert.Contains("\"playerBuildingCollisionEnabled\":true", compactPlayerCollision);
        StringAssert.Contains("\"continuousMovementEnabled\":true", compactNpcMovement);
        StringAssert.Contains("\"stoppedWithoutReasonCount\":0", compactNpcMovement);
        StringAssert.Contains("\"npcCountMultiplier\":100", compactNpcReport);
        StringAssert.Contains("\"requestedNpcCount\":800", compactNpcReport);
        StringAssert.Contains("\"maxNpcCount\":800", compactNpcReport);
        StringAssert.Contains("\"avoidBuildings\":true", compactNpcReport);
        StringAssert.Contains("\"usePooling\":true", compactNpcReport);
        StringAssert.Contains("\"evacuationCrowdDelayBounded\":true", compactNpcGameplay);
        StringAssert.Contains("ApplyFloatingBuildingSnapdownToGameplayGroundCover", bootstrap);
        StringAssert.Contains("DisableBuildingVerticalMovesForGroundCoverRaise", bootstrap);
        StringAssert.Contains("ResolveRaisedGroundCoverY", bootstrap);
        StringAssert.Contains("ContainsSnapdownExcludedText", bootstrap);
        StringAssert.Contains("groundRaiseStatus", bootstrap);
        StringAssert.Contains("IsInsideBuildingBounds", npc);
        StringAssert.Contains("NewMapNpcMovementState", npc);
        StringAssert.Contains("StoppedWithoutReasonCount", npc);
        StringAssert.Contains("ResolvePlayerPositionAgainstNpcs", npc);
        StringAssert.Contains("CapsuleCollider", npc);
    }

    [Test]
    public void AirwallHardCleanupAndPlayerNpcSoftBlockingAreConfigured()
    {
        string audit = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_unexpected_airwall_hard_audit.json"));
        string cleanup = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_airwall_hard_cleanup_report.json"));
        string collisionConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_player_npc_collision_config.json"));
        string collisionReport = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_player_npc_collision_report.json"));
        string bootstrap = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapRuntimeBootstrap.cs"));
        string player = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapPlayerController.cs"));
        string npc = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNpcCrowdPrototype.cs"));

        string compactAudit = audit.Replace(" ", string.Empty);
        string compactCleanup = cleanup.Replace(" ", string.Empty);
        string compactCollisionConfig = collisionConfig.Replace(" ", string.Empty);
        string compactCollisionReport = collisionReport.Replace(" ", string.Empty);

        StringAssert.Contains("\"boundaryAirWallsPreserved\":true", compactCleanup);
        StringAssert.Contains("\"debugTestCollidersInactiveInNormalMode\":true", compactCleanup);
        StringAssert.Contains("\"buildingObstacleBoundsShrunk\":", compactAudit);
        StringAssert.Contains("\"enabled\":true", compactCollisionConfig);
        StringAssert.Contains("\"preventDirectOverlap\":true", compactCollisionConfig);
        StringAssert.Contains("\"playerCannotPassStraightThroughNearNpc\":true", compactCollisionReport);
        StringAssert.Contains("\"physicsExplosionRisk\":false", compactCollisionReport);
        StringAssert.Contains("AuditAndCleanupUnexpectedAirwallColliders", bootstrap);
        StringAssert.Contains("TryBuildConservativeBuildingObstacleBounds", bootstrap);
        StringAssert.Contains("ResolvePlayerBuildingCollisionMargin", bootstrap);
        StringAssert.Contains("ConfigurePlayerNpcCollision", player);
        StringAssert.Contains("ApplyPlayerNpcCollisionCorrection", player);
        StringAssert.Contains("ResolvePlayerPositionAgainstNpcs", npc);
    }

    [Test]
    public void RuntimeErrorNpcLifecycleAndExpandedLabelsAreConfigured()
    {
        string concaveAudit = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_concave_mesh_trigger_audit.json"));
        string concaveFix = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_concave_mesh_trigger_fix.json"));
        string lifecycleConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_lifecycle_config.json"));
        string lifecycleAudit = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_lifecycle_deadlock_audit.json"));
        string contactFix = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_player_collision_deadlock_fix.json"));
        string labelAudit = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_building_road_label_coverage_audit.json"));
        string labelRuntime = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_building_road_label_runtime_report.json"));
        string nameConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_name_label_config.json"));
        string bootstrap = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapRuntimeBootstrap.cs"));
        string npc = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNpcCrowdPrototype.cs"));
        string labels = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNameLabelController.cs"));

        string compactConcaveAudit = concaveAudit.Replace(" ", string.Empty);
        string compactConcaveFix = concaveFix.Replace(" ", string.Empty);
        string compactLifecycleConfig = lifecycleConfig.Replace(" ", string.Empty);
        string compactLifecycleAudit = lifecycleAudit.Replace(" ", string.Empty);
        string compactContactFix = contactFix.Replace(" ", string.Empty);
        string compactLabelAudit = labelAudit.Replace(" ", string.Empty);
        string compactLabelRuntime = labelRuntime.Replace(" ", string.Empty);
        string compactNameConfig = nameConfig.Replace(" ", string.Empty);

        StringAssert.Contains("\"sceneConcaveMeshTriggerOffenders\":0", compactConcaveAudit);
        StringAssert.Contains("\"runtimeFixImplemented\":true", compactConcaveFix);
        StringAssert.Contains("\"allowGlobalRefresh\":false", compactLifecycleConfig);
        StringAssert.Contains("\"globalRespawnIntervalSeconds\":0.0", compactLifecycleConfig);
        StringAssert.Contains("\"collisionWithPlayerDoesNotGlobalPause\":true", compactLifecycleConfig);
        StringAssert.Contains("\"globalRespawnCount\":0", compactLifecycleAudit);
        StringAssert.Contains("\"collisionAffectsOnlyLocalPair\":true", compactContactFix);
        StringAssert.Contains("\"ordinaryBuildingLabels\":117", compactLabelAudit);
        StringAssert.Contains("\"roadLabels\":180", compactLabelAudit);
        StringAssert.Contains("\"ordinaryBuildingLabelsAvailable\":117", compactLabelRuntime);
        StringAssert.Contains("\"maxVisibleLabels\":180", compactNameConfig);
        StringAssert.Contains("\"maxVisibleBuildingLabels\":80", compactNameConfig);
        StringAssert.Contains("\"maxVisibleRoadLabels\":50", compactNameConfig);
        StringAssert.Contains("NeutralizeExistingConcaveMeshTrigger", bootstrap);
        StringAssert.Contains("CreatePrimitiveTriggerProxy", bootstrap);
        StringAssert.Contains("RunNpcLifecycleDiagnosticSmoke", bootstrap);
        StringAssert.Contains("SetReferenceTransform", npc);
        StringAssert.Contains("ResolveNpcTargetAvoidingPlayerContact", npc);
        StringAssert.Contains("PreventAllStopDeadlock", npc);
        StringAssert.Contains("maxCachedLabelSources = 800", labels);
    }

    [Test]
    public void Round3SupportBoundsAndNameLabelsStayInvisibleOfflineAndCapped()
    {
        NewMapPlayableBoundsConfig bounds = NewMapPlayableBoundsConfig.Default();
        Assert.IsTrue(bounds.enabled);
        Assert.IsTrue(bounds.autoDetectFromMapBounds);
        Assert.IsFalse(bounds.debugVisualizationEnabled);
        Assert.Greater(bounds.boundaryHeightMeters, 10f);
        Assert.Greater(bounds.boundaryThicknessMeters, 0f);

        NewMapPlayableBounds documented = NewMapPlayableBounds.DefaultDocumented().WithAppliedMargin();
        Assert.IsTrue(documented.IsValid);
        Assert.IsTrue(documented.ContainsXZ(new Vector3(0f, 0.04f, 0f)));
        Assert.IsFalse(documented.ContainsXZ(new Vector3(5000f, 0.04f, 0f)));

        NewMapNameLabelConfig labelConfig = NewMapNameLabelConfig.Default();
        Assert.IsTrue(labelConfig.enabled);
        Assert.IsTrue(labelConfig.showOfficialShelterNames);
        Assert.IsTrue(labelConfig.showNonOfficialCandidateNames);
        Assert.IsTrue(labelConfig.showBuildingNames, "Reliable cache-backed building labels should be visible.");
        Assert.IsTrue(labelConfig.showRoadNames);
        Assert.IsFalse(labelConfig.showIdOnlyLabelsInDebug);
        Assert.IsFalse(labelConfig.runtimeNetworkRequestsAllowed);
        Assert.LessOrEqual(labelConfig.maxVisibleLabels, 180);
        Assert.GreaterOrEqual(labelConfig.labelUpdateIntervalSeconds, 0.25f);

        string support = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_support_surface_visibility_status.json"));
        StringAssert.Contains("\"supportRendererAllowedInNormalMode\": false", support);
        StringAssert.Contains("\"blueDebugGroundMaterialAllowedInNormalMode\": false", support);

        string cache = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_name_cache.json"));
        StringAssert.Contains("source_project_and_local_osm_names_available", cache);
        StringAssert.Contains("東京駅", cache);
        StringAssert.Contains("昭和通り", cache);

        string enrichmentConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_name_enrichment_config.json"));
        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", enrichmentConfig);
        StringAssert.Contains("\"rateLimitSeconds\": 1.1", enrichmentConfig);
        StringAssert.Contains("\"maxQueriesPerRun\": 500", enrichmentConfig);
        StringAssert.Contains("\"queryBuildingsNearGameplayArea\": true", enrichmentConfig);
        StringAssert.Contains("\"allowOnlineLookup\": true", enrichmentConfig);

        string labelRuntimeSource = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNameLabelController.cs"));
        Assert.IsFalse(labelRuntimeSource.Contains("UnityWebRequest"));
        Assert.IsFalse(labelRuntimeSource.Contains("HttpClient"));
        Assert.IsFalse(labelRuntimeSource.Contains("nominatim"));
    }

    [Test]
    public void GroundRoadRollbackDisablesAdaptiveGridAndConfiguresSafeGround()
    {
        string sourceValidationPath = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_road_source_validation.json");
        string samplingPath = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_road_sampling_report.json");
        string gridConfigPath = Path.Combine(Application.dataPath, "Data/P10/newmap_adaptive_support_grid_config.json");
        string safeGroundPath = Path.Combine(Application.dataPath, "Data/P10/newmap_safe_ground_config.json");
        string safeGroundReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_safe_ground_report.json");
        string bluePath = Path.Combine(Application.dataPath, "Data/P10/newmap_blue_area_hard_removal.json");
        string fallPath = Path.Combine(Application.dataPath, "Data/P10/newmap_fall_out_prevention_report.json");
        Assert.IsTrue(File.Exists(sourceValidationPath), "Ground/road source validation JSON must exist.");
        Assert.IsTrue(File.Exists(samplingPath), "Ground/road sampling report JSON must exist.");
        Assert.IsTrue(File.Exists(gridConfigPath), "Adaptive support grid config JSON must exist.");
        Assert.IsTrue(File.Exists(safeGroundPath), "Safe ground config JSON must exist.");
        Assert.IsTrue(File.Exists(safeGroundReportPath), "Safe ground report JSON must exist.");
        Assert.IsTrue(File.Exists(bluePath), "Blue hard-removal JSON must exist.");
        Assert.IsTrue(File.Exists(fallPath), "Fall-out prevention report JSON must exist.");

        string sourceValidation = File.ReadAllText(sourceValidationPath);
        string sampling = File.ReadAllText(samplingPath);
        string gridConfig = File.ReadAllText(gridConfigPath);
        string safeGround = File.ReadAllText(safeGroundPath);
        string safeGroundReport = File.ReadAllText(safeGroundReportPath);
        string blue = File.ReadAllText(bluePath);
        string fall = File.ReadAllText(fallPath);
        string compactSafeGround = safeGround.Replace(" ", string.Empty);
        string compactSafeGroundReport = safeGroundReport.Replace(" ", string.Empty);
        string compactBlue = blue.Replace(" ", string.Empty);
        string compactFall = fall.Replace(" ", string.Empty);

        StringAssert.Contains("\"sourceSceneValid\": true", sourceValidation);
        StringAssert.Contains("\"groundLikeRendererCount\": 20", sourceValidation);
        StringAssert.Contains("\"roadLikeRendererCount\": 0", sourceValidation);
        StringAssert.Contains("\"reliefSamples\": 20", sampling);
        StringAssert.Contains("not GIS-grade", sampling);
        StringAssert.Contains("\"enabled\": false", gridConfig);
        StringAssert.Contains("\"debugVisualizationEnabled\": false", gridConfig);
        StringAssert.Contains("\"rendererEnabledInNormalMode\": false", gridConfig);
        StringAssert.Contains("\"forceFixedSupportY\":true", compactSafeGround);
        StringAssert.Contains("\"supportY\":0", compactSafeGround);
        StringAssert.Contains("\"supportColliderCount\":", compactSafeGroundReport);
        Assert.IsFalse(compactSafeGroundReport.Contains("\"supportColliderCount\":0"));
        StringAssert.Contains("\"supportRendererHidden\":true", compactSafeGroundReport);
        StringAssert.Contains("\"adaptiveSupportGridDisabled\":true", compactSafeGroundReport);
        StringAssert.Contains("\"normalModeVisibleBlueSupportCount\":0", compactBlue);
        StringAssert.Contains("\"supportGridRendererActive\":false", compactBlue);
        StringAssert.Contains("\"fallRecoveryEnabled\":true", compactFall);
        StringAssert.Contains("\"playerCannotFallOutOfMap\":true", compactFall);
    }

    [Test]
    public void GroundCoverConfigAndReportsRequireVisibleRoadLikeColliders()
    {
        string configPath = Path.Combine(Application.dataPath, "Data/P10/newmap_gameplay_ground_cover_config.json");
        string materialReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_cover_material_report.json");
        string coverReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_gameplay_ground_cover_report.json");
        string fullFallPath = Path.Combine(Application.dataPath, "Data/P10/newmap_full_fall_prevention_report.json");
        string blueCoverPath = Path.Combine(Application.dataPath, "Data/P10/newmap_blue_area_cover_status.json");
        string materialPath = Path.Combine(Application.dataPath, "Resources/NewMap/P10_NewMap_RoadGroundCover.mat");

        Assert.IsTrue(File.Exists(configPath), "Ground cover config JSON must exist.");
        Assert.IsTrue(File.Exists(materialReportPath), "Ground cover material report must exist.");
        Assert.IsTrue(File.Exists(coverReportPath), "Gameplay ground cover report must exist.");
        Assert.IsTrue(File.Exists(fullFallPath), "Full fall prevention report must exist.");
        Assert.IsTrue(File.Exists(blueCoverPath), "Blue area cover status must exist.");
        Assert.IsTrue(File.Exists(materialPath), "Local road-like material asset must exist.");

        string config = File.ReadAllText(configPath);
        string material = File.ReadAllText(materialReportPath);
        string cover = File.ReadAllText(coverReportPath);
        string fall = File.ReadAllText(fullFallPath);
        string blue = File.ReadAllText(blueCoverPath);
        string bootstrap = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapRuntimeBootstrap.cs"));
        string compactConfig = config.Replace(" ", string.Empty);
        string compactMaterial = material.Replace(" ", string.Empty);
        string compactCover = cover.Replace(" ", string.Empty);
        string compactFall = fall.Replace(" ", string.Empty);
        string compactBlue = blue.Replace(" ", string.Empty);

        StringAssert.Contains("\"rendererEnabledInNormalMode\":true", compactConfig);
        StringAssert.Contains("\"colliderEnabled\":true", compactConfig);
        StringAssert.Contains("\"coverY\":0", compactConfig);
        StringAssert.Contains("\"opaque\":true", compactMaterial);
        StringAssert.Contains("\"notBlue\":true", compactMaterial);
        StringAssert.Contains("\"notMagenta\":true", compactMaterial);
        StringAssert.Contains("\"allTilesHaveColliders\":true", compactCover);
        StringAssert.Contains("\"allTilesRenderInNormalMode\":true", compactCover);
        StringAssert.Contains("\"notGisGradeTerrainAccuracy\":true", compactCover);
        StringAssert.Contains("\"allVisibleGroundCoverTilesHaveColliders\":true", compactFall);
        StringAssert.Contains("\"playerCannotFallThroughGroundCover\":true", compactFall);
        StringAssert.Contains("\"blueFallThroughAreasInsidePlayableBoundsCovered\":true", compactBlue);
        StringAssert.Contains("EnsureGameplayGroundCover", bootstrap);
        StringAssert.Contains("GameplayGroundCoverRoot", bootstrap);
        StringAssert.Contains("Resources.Load<Material>(\"NewMap/P10_NewMap_RoadGroundCover\")", bootstrap);
    }

    [Test]
    public void AdaptiveSupportGridRuntimeBuildsVariedInvisibleColliderCells()
    {
        GameObject root = new GameObject("AdaptiveSupportGridTestRoot");
        try
        {
            NewMapAdaptiveSupportGridConfig config = NewMapAdaptiveSupportGridConfig.Default();
            config.enabled = true;
            config.cellSizeMeters = 10f;
            config.maxGridCells = 20;
            config.nearestSampleRadiusMeters = 12f;
            config.minSampleConfidence = 0.5f;
            config.useBuildingBaseFallback = true;
            config.useGlobalFallback = true;

            NewMapGroundRoadHeightSample[] samples =
            {
                CreateSupportSample("road_01", "road", -15f, 1f, 0f, 0.95f),
                CreateSupportSample("relief_01", "relief", 15f, 5f, 0f, 0.90f),
                CreateSupportSample("building_01", "building_base_fallback", 30f, 2f, 0f, 0.80f)
            };

            NewMapAdaptiveSupportGridRuntime runtime = NewMapAdaptiveSupportGridRuntime.CreateForDiagnostics(config, samples);
            runtime.BuildCollisionGrid(root.transform, new NewMapPlayableBounds(-20f, 20f, -5f, 5f, 0f, 80f, 4f), -3f);

            Assert.IsTrue(runtime.Enabled);
            Assert.IsTrue(runtime.HasUsableGrid);
            Assert.AreEqual(2, runtime.CellCount);
            Assert.AreEqual(runtime.CellCount, runtime.ColliderCount);
            Assert.AreEqual(0, runtime.VisibleRendererCount);
            Assert.Greater(runtime.CellsUsingRoad, 0);
            Assert.Greater(runtime.CellsUsingRelief, 0);
            Assert.AreEqual(0, runtime.CellsUsingGlobalFallback);
            Assert.AreEqual(1f, runtime.SupportYMin, 0.001f);
            Assert.AreEqual(5f, runtime.SupportYMax, 0.001f);
            Assert.AreEqual(1f, runtime.ResolveSupportY(new Vector3(-15f, 0f, 0f), -3f, out string roadSource), 0.001f);
            Assert.AreEqual("road", roadSource);
            Assert.AreEqual(5f, runtime.ResolveSupportY(new Vector3(15f, 0f, 0f), -3f, out string reliefSource), 0.001f);
            Assert.AreEqual("relief", reliefSource);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void NameCacheRuntimeIsOfflineJapaneseMainNameOnly()
    {
        string cachePath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_cache.json");
        string enrichmentReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_enrichment_report.json");
        string coverageAuditPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_cache_coverage_audit.json");
        string queryListPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_enrichment_query_list.json");
        string normalizationReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_normalization_report.json");
        string labelConfigPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_label_config.json");
        Assert.IsTrue(File.Exists(cachePath), "Name cache JSON must exist.");
        Assert.IsTrue(File.Exists(enrichmentReportPath), "Name enrichment report JSON must exist.");
        Assert.IsTrue(File.Exists(coverageAuditPath), "Name cache coverage audit JSON must exist.");
        Assert.IsTrue(File.Exists(queryListPath), "Name enrichment query-list JSON must exist.");
        Assert.IsTrue(File.Exists(normalizationReportPath), "Name normalization report JSON must exist.");
        Assert.IsTrue(File.Exists(labelConfigPath), "Name label config JSON must exist.");

        string cache = File.ReadAllText(cachePath);
        string enrichment = File.ReadAllText(enrichmentReportPath);
        string coverageAudit = File.ReadAllText(coverageAuditPath);
        string queryList = File.ReadAllText(queryListPath);
        string normalizationReport = File.ReadAllText(normalizationReportPath);
        string labelConfig = File.ReadAllText(labelConfigPath);

        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", cache);
        StringAssert.Contains("\"finalDisplayName\"", cache);
        StringAssert.Contains("\u8056\u8def\u52a0\u30ac\u30fc\u30c7\u30f3\u30bf\u30ef\u30fc", cache);
        Assert.IsFalse(cache.Contains("\"idOnly\": true"), "ID-only cache entries must not be normal labels.");
        Assert.IsFalse(cache.Contains("address_only"), "Address-only reverse geocode strings must not become labels.");
        StringAssert.Contains("\"finalStatus\": \"completed\"", enrichment);
        Assert.IsFalse(enrichment.Contains("\"onlineQueriesAttempted\": 0"), "This pass must actually run online preprocessing lookup or clearly report failure.");
        StringAssert.Contains("\"maxQueriesPerRun\": 500", enrichment);
        StringAssert.Contains("\"onlineQueriesSucceeded\":", enrichment);
        StringAssert.Contains("\"namesNewlyAdded\":", enrichment);
        StringAssert.Contains("\"hardRuleStatus\": \"online_queries_executed\"", coverageAudit);
        StringAssert.Contains("\"queryCount\":", queryList);
        StringAssert.Contains("\"enabledForOnlineLookupCount\":", queryList);
        StringAssert.Contains("\"visibleAddressLikeLabelCount\": 0", normalizationReport);
        StringAssert.Contains("\"visibleIdOnlyLabelCount\": 0", normalizationReport);
        StringAssert.Contains("\"visibleLowConfidenceLabelCount\": 0", normalizationReport);
        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", labelConfig);
        StringAssert.Contains("\"showBuildingNames\": true", labelConfig);
        StringAssert.Contains("\"showIdOnlyLabelsInDebug\": false", labelConfig);
    }

    [Test]
    public void Round2MouseGroundLightingConfigDefaultsMatchManualFeedback()
    {
        NewMapMouseDragLookConfig mouse = NewMapMouseDragLookConfig.Default();
        Assert.IsTrue(mouse.enabled);
        Assert.IsTrue(mouse.lookRequiresMouseButton);
        Assert.AreEqual(1, NewMapMouseDragLookConfig.ParseMouseButtonIndex(mouse.lookMouseButton));
        CollectionAssert.AreEquivalent(
            new[] { 0, 1 },
            NewMapMouseDragLookConfig.ParseAllowedMouseButtonIndices(mouse.allowedButtons, mouse.lookMouseButton));
        Assert.GreaterOrEqual(mouse.pitchMin, -70f);
        Assert.LessOrEqual(mouse.pitchMax, 80f);

        NewMapLightingProfilesConfig lighting = NewMapLightingProfilesConfig.Default();
        Assert.Greater(lighting.clear_day.directionalLightIntensity, 1.0f);
        Assert.Greater(lighting.clear_day.SkyBrightness, 0.6f);
        Assert.Less(lighting.night_clear.SkyBrightness, 0.12f);
        Assert.Greater(lighting.night_clear.fillLightIntensity, 0.1f);
        Assert.Greater(lighting.night_clear.ambientIntensity, 0.8f);

        string ground = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_ground_height_realignment_round2.json"));
        StringAssert.Contains("\"supportToVisualToleranceMeters\": 0.35", ground);
        StringAssert.Contains("\"runtimeGroundReference\": \"renderer_bounds_low_percentile_building_base\"", ground);
    }

    [Test]
    public void NpcDistributionPurePlanIsDeterministicAndBroad()
    {
        NewMapNpcDistributionConfig config = NewMapNpcDistributionConfig.Default();
        Vector3 center = new Vector3(0f, 0.04f, 0f);
        Vector3[] first = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(center, config);
        Vector3[] second = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(center, config);
        Assert.AreEqual(800, first.Length);
        Assert.AreEqual(first.Length, second.Length);

        var sectors = new System.Collections.Generic.HashSet<int>();
        var rings = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < first.Length; i++)
        {
            Assert.AreEqual(first[i].x, second[i].x, 0.001f);
            Assert.AreEqual(first[i].z, second[i].z, 0.001f);
            Vector3 delta = first[i] - center;
            delta.y = 0f;
            Assert.GreaterOrEqual(delta.magnitude, config.minDistanceFromPlayerMeters - 0.01f);
            Assert.LessOrEqual(delta.magnitude, config.distributionRadiusMeters + 0.01f);

            float angle = Mathf.Atan2(delta.z, delta.x);
            if (angle < 0f)
            {
                angle += Mathf.PI * 2f;
            }

            sectors.Add(Mathf.FloorToInt(angle / (Mathf.PI * 2f / config.sectorCount)));
            float normalized = Mathf.InverseLerp(config.minDistanceFromPlayerMeters, config.distributionRadiusMeters, delta.magnitude);
            rings.Add(Mathf.Clamp(Mathf.FloorToInt(normalized * config.ringCount), 0, config.ringCount - 1));
        }

        Assert.GreaterOrEqual(sectors.Count, 28);
        Assert.GreaterOrEqual(rings.Count, 6);
    }

    private static NewMapGroundRoadHeightSample CreateSupportSample(string id, string category, float x, float y, float z, float confidence)
    {
        return new NewMapGroundRoadHeightSample
        {
            sampleId = id,
            sourceObjectPath = "diagnostic/" + id,
            category = category,
            position = new NewMapVector3Data(x, y, z),
            confidence = confidence,
            usableForSupport = true
        };
    }
}
