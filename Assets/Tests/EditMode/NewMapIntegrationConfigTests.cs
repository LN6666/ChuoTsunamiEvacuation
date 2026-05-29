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
            "Data/P10/newmap_name_enrichment_report.json"
        };

        foreach (string relativePath in requiredPaths)
        {
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            Assert.IsTrue(File.Exists(fullPath), $"{relativePath} must exist for manual-blocker preflight.");
        }

        string config = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_npc_distribution_config.json"));
        StringAssert.Contains("\"npcCountMultiplier\": 20", config);
        StringAssert.Contains("\"distributionRadiusMeters\": 1000", config);
        StringAssert.Contains("\"maxNpcCount\": 300", config);
        StringAssert.Contains("\"npcDistributionSeed\": 20260529", config);

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
        Assert.IsFalse(labelConfig.showBuildingNames, "Generic building labels should remain hidden unless a reliable cache/source enables them.");
        Assert.IsTrue(labelConfig.showRoadNames);
        Assert.IsFalse(labelConfig.showIdOnlyLabelsInDebug);
        Assert.IsFalse(labelConfig.runtimeNetworkRequestsAllowed);
        Assert.LessOrEqual(labelConfig.maxVisibleLabels, 80);
        Assert.GreaterOrEqual(labelConfig.labelUpdateIntervalSeconds, 0.25f);

        string support = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_support_surface_visibility_status.json"));
        StringAssert.Contains("\"supportRendererAllowedInNormalMode\": false", support);
        StringAssert.Contains("\"blueDebugGroundMaterialAllowedInNormalMode\": false", support);

        string labelSource = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_object_name_label_source_report.json"));
        StringAssert.Contains("no_source_name_available_for_generic_building_or_road_names", labelSource);
        StringAssert.Contains("No fabricated road/building names", labelSource);

        string enrichmentConfig = File.ReadAllText(Path.Combine(Application.dataPath, "Data/P10/newmap_name_enrichment_config.json"));
        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", enrichmentConfig);
        StringAssert.Contains("\"rateLimitSeconds\": 1.1", enrichmentConfig);
        StringAssert.Contains("\"maxQueriesPerRun\": 200", enrichmentConfig);

        string labelRuntimeSource = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/NewMap/NewMapNameLabelController.cs"));
        Assert.IsFalse(labelRuntimeSource.Contains("UnityWebRequest"));
        Assert.IsFalse(labelRuntimeSource.Contains("HttpClient"));
        Assert.IsFalse(labelRuntimeSource.Contains("nominatim"));
    }

    [Test]
    public void GroundRoadAdaptiveSupportGridConfigAndReportsStayInvisibleAndLocal()
    {
        string sourceValidationPath = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_road_source_validation.json");
        string samplingPath = Path.Combine(Application.dataPath, "Data/P10/newmap_ground_road_sampling_report.json");
        string gridConfigPath = Path.Combine(Application.dataPath, "Data/P10/newmap_adaptive_support_grid_config.json");
        string gridReportPath = Path.Combine(Application.dataPath, "Data/P10/newmap_adaptive_support_grid_report.json");
        string bluePath = Path.Combine(Application.dataPath, "Data/P10/newmap_blue_area_final_fix.json");
        string heightPath = Path.Combine(Application.dataPath, "Data/P10/newmap_height_integration_report.json");
        Assert.IsTrue(File.Exists(sourceValidationPath), "Ground/road source validation JSON must exist.");
        Assert.IsTrue(File.Exists(samplingPath), "Ground/road sampling report JSON must exist.");
        Assert.IsTrue(File.Exists(gridConfigPath), "Adaptive support grid config JSON must exist.");
        Assert.IsTrue(File.Exists(gridReportPath), "Adaptive support grid report JSON must exist.");
        Assert.IsTrue(File.Exists(bluePath), "Blue area final fix JSON must exist.");
        Assert.IsTrue(File.Exists(heightPath), "Height integration report JSON must exist.");

        string sourceValidation = File.ReadAllText(sourceValidationPath);
        string sampling = File.ReadAllText(samplingPath);
        string gridConfig = File.ReadAllText(gridConfigPath);
        string gridReport = File.ReadAllText(gridReportPath);
        string blue = File.ReadAllText(bluePath);
        string height = File.ReadAllText(heightPath);

        StringAssert.Contains("\"sourceSceneValid\": true", sourceValidation);
        StringAssert.Contains("\"groundLikeRendererCount\": 20", sourceValidation);
        StringAssert.Contains("\"reliefSamples\": 20", sampling);
        StringAssert.Contains("not GIS-grade", sampling);
        StringAssert.Contains("\"enabled\": true", gridConfig);
        StringAssert.Contains("\"debugVisualizationEnabled\": false", gridConfig);
        StringAssert.Contains("\"rendererEnabledInNormalMode\": false", gridConfig);
        StringAssert.Contains("\"gridCellCount\": 510", gridReport);
        StringAssert.Contains("\"colliderCount\": 510", gridReport);
        StringAssert.Contains("\"cellsUsingReliefSamples\": 163", gridReport);
        StringAssert.Contains("\"cellsUsingBuildingBaseFallback\": 226", gridReport);
        StringAssert.Contains("\"renderersDisabled\": true", gridReport);
        StringAssert.Contains("\"blueSupportVisualActive\": false", gridReport);
        StringAssert.Contains("\"playerUsesAdaptiveGrid\": true", gridReport);
        StringAssert.Contains("\"npcUsesAdaptiveGrid\": true", gridReport);
        StringAssert.Contains("\"targetsUseLocalHeight\": true", gridReport);
        StringAssert.Contains("\"normalModeVisibleSuspectCount\": 0", blue);
        StringAssert.Contains("\"supportGridRendererVisibleInNormalMode\": false", blue);
        StringAssert.Contains("\"playerSpawnUsesAdaptiveSupportCell\": true", height);
        StringAssert.Contains("\"greenFramesUseLocalSupportHeight\": true", height);
    }

    [Test]
    public void AdaptiveSupportGridRuntimeBuildsVariedInvisibleColliderCells()
    {
        GameObject root = new GameObject("AdaptiveSupportGridTestRoot");
        try
        {
            NewMapAdaptiveSupportGridConfig config = NewMapAdaptiveSupportGridConfig.Default();
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
        string labelConfigPath = Path.Combine(Application.dataPath, "Data/P10/newmap_name_label_config.json");
        Assert.IsTrue(File.Exists(cachePath), "Name cache JSON must exist.");
        Assert.IsTrue(File.Exists(enrichmentReportPath), "Name enrichment report JSON must exist.");
        Assert.IsTrue(File.Exists(labelConfigPath), "Name label config JSON must exist.");

        string cache = File.ReadAllText(cachePath);
        string enrichment = File.ReadAllText(enrichmentReportPath);
        string labelConfig = File.ReadAllText(labelConfigPath);

        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", cache);
        StringAssert.Contains("\u8056\u8def\u52a0\u30ac\u30fc\u30c7\u30f3\u30bf\u30ef\u30fc", cache);
        Assert.IsFalse(cache.Contains("\"idOnly\": true"), "ID-only cache entries must not be normal labels.");
        Assert.IsFalse(cache.Contains("address_only"), "Address-only reverse geocode strings must not become labels.");
        StringAssert.Contains("\"status\": \"completed\"", enrichment);
        StringAssert.Contains("\"onlineQueriesAttempted\": 0", enrichment);
        StringAssert.Contains("\"runtimeNetworkRequestsAllowed\": false", labelConfig);
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
        Assert.AreEqual(160, first.Length);
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

        Assert.GreaterOrEqual(sectors.Count, 20);
        Assert.GreaterOrEqual(rings.Count, 5);
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
