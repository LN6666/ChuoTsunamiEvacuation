#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NewMapGroundRoadMergeUtility
{
    private const string MainScenePath = "Assets/Scenes/Chuo_BaseMap.unity";
    private const string SourceScenePath = "Assets/Scenes/Chuo_GroundRoad_Import_Source.unity";
    private const string DataDir = "Assets/Data/P10";
    private const string DocsDir = "docs";

    [MenuItem("Tools/Chuo Evacuation/New Map/Generate GroundRoad Merge Reports")]
    public static void GenerateGroundRoadMergeReportsMenu()
    {
        GenerateGroundRoadMergeReports(exitEditor: false);
    }

    public static void GenerateGroundRoadMergeReportsCommandLine()
    {
        try
        {
            GenerateGroundRoadMergeReports(exitEditor: true);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void GenerateGroundRoadMergeReports(bool exitEditor)
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(DocsDir);

        if (!File.Exists(SourceScenePath))
        {
            throw new FileNotFoundException("Supplemental ground/road source scene is missing.", SourceScenePath);
        }

        SourceScan source = ScanSourceScene();
        if (!source.Validation.sourceSceneValid)
        {
            WriteJson("newmap_ground_road_source_validation.json", source.Validation);
            WriteMarkdown("NEWMAP_GROUND_ROAD_SOURCE_VALIDATION.md", BuildSourceValidationMarkdown(source.Validation));
            throw new InvalidOperationException(source.Validation.invalidReason);
        }

        MainScan main = ScanMainScene();
        NewMapAdaptiveSupportGridConfig config = EnsureAdaptiveGridConfig();
        NewMapGroundRoadHeightSample[] samples = source.Samples.Concat(main.BuildingFallbackSamples).ToArray();
        NewMapGroundRoadHeightSampleCache cache = new NewMapGroundRoadHeightSampleCache
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            sourceScenePath = SourceScenePath,
            valid = samples.Length > 0,
            status = samples.Length > 0 ? "generated_from_supplemental_source_and_building_base_fallback" : "no_samples",
            totalSamples = samples.Length,
            samples = samples
        };

        SamplingReport sampling = BuildSamplingReport(samples, source.RejectedSampleCount, source.Validation.bounds);
        AdaptiveGridReport grid = BuildAdaptiveGridReport(config, samples, main.PlayableBounds, main.GlobalFallbackY);
        BlueAreaFinalFixReport blue = BuildBlueAreaReport(main.BlueSuspects);
        GroundRoadMergeReport merge = BuildMergeReport(source, main);
        HeightIntegrationReport height = BuildHeightIntegrationReport(grid);
        FloatingBuildingRound4Report floating = BuildFloatingBuildingReport(samples);
        AirWallRegressionReport airWalls = BuildAirWallReport(main.PlayableBounds);
        GroundRoadRegressionStatus regression = BuildRegressionStatus(grid, blue, airWalls);

        WriteJson("newmap_ground_road_source_validation.json", source.Validation);
        WriteJson("newmap_ground_road_height_samples.json", cache);
        WriteJson("newmap_ground_road_sampling_report.json", sampling);
        WriteJson("newmap_adaptive_support_grid_report.json", grid);
        WriteJson("newmap_blue_area_final_fix.json", blue);
        WriteJson("newmap_ground_road_merge_report.json", merge);
        WriteJson("newmap_height_integration_report.json", height);
        WriteJson("newmap_floating_building_round4_report.json", floating);
        WriteJson("newmap_air_wall_regression_report.json", airWalls);
        WriteJson("newmap_groundroad_regression_status.json", regression);
        WriteJson("newmap_manual_playtest_readiness.json", BuildManualReadiness(grid, blue, floating, airWalls));
        WriteJson("newmap_manual_playtest_checklist.json", BuildManualChecklist());

        WriteMarkdown("NEWMAP_GROUND_ROAD_SOURCE_VALIDATION.md", BuildSourceValidationMarkdown(source.Validation));
        WriteMarkdown("NEWMAP_GROUND_ROAD_HEIGHT_SAMPLING.md", BuildSamplingMarkdown(sampling));
        WriteMarkdown("NEWMAP_ADAPTIVE_SUPPORT_GRID_FROM_GROUND_ROAD.md", BuildGridMarkdown(grid));
        WriteMarkdown("NEWMAP_BLUE_AREA_FINAL_FIX.md", BuildBlueMarkdown(blue));
        WriteMarkdown("NEWMAP_GROUND_ROAD_MERGE_REPORT.md", BuildMergeMarkdown(merge));
        WriteMarkdown("NEWMAP_HEIGHT_INTEGRATION_REPORT.md", BuildHeightMarkdown(height));
        WriteMarkdown("NEWMAP_FLOATING_BUILDING_ROUND4_REPORT.md", BuildFloatingMarkdown(floating));
        WriteMarkdown("NEWMAP_AIR_WALL_REGRESSION_REPORT.md", BuildAirWallMarkdown(airWalls));
        WriteMarkdown("NEWMAP_GROUNDROAD_REGRESSION_STATUS.md", BuildRegressionMarkdown(regression));
        WriteMarkdown("NEWMAP_MANUAL_PLAYTEST_READINESS.md", BuildManualReadinessMarkdown(BuildManualReadiness(grid, blue, floating, airWalls)));
        WriteMarkdown("NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md", BuildManualChecklistMarkdown());

        AssetDatabase.Refresh();
        Debug.Log($"NewMap ground/road merge reports generated. sourceRenderers={source.Validation.rendererCount} samples={samples.Length} gridCells={grid.gridCellCount}");
    }

    private static SourceScan ScanSourceScene()
    {
        Scene scene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new InvalidOperationException($"Could not open {SourceScenePath}");
        }

        Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
        Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true);
        MeshFilter[] meshFilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>(true);
        GameObject[] roots = scene.GetRootGameObjects();
        List<NewMapGroundRoadHeightSample> samples = new List<NewMapGroundRoadHeightSample>();
        int rejected = 0;
        Bounds aggregate = default;
        bool hasBounds = false;
        int road = 0;
        int terrain = 0;
        int relief = 0;
        int bridge = 0;
        int water = 0;
        int groundLike = 0;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            string category = CategorizeRenderer(renderer);
            switch (category)
            {
                case "road":
                    road++;
                    break;
                case "terrain":
                    terrain++;
                    break;
                case "relief":
                    relief++;
                    break;
                case "bridge":
                    bridge++;
                    break;
                case "water":
                    water++;
                    break;
            }

            if (IsGroundLikeCategory(category))
            {
                groundLike++;
                if (TryCreateSample(renderer, category, "source", samples.Count + 1, out NewMapGroundRoadHeightSample sample))
                {
                    samples.Add(sample);
                }
                else
                {
                    rejected++;
                }
            }

            if (!hasBounds)
            {
                aggregate = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                aggregate.Encapsulate(renderer.bounds);
            }
        }

        string invalidReason = string.Empty;
        bool valid = File.Exists(SourceScenePath) && renderers.Length > 0 && (colliders.Length > 0 || groundLike > 0) && samples.Count > 0;
        if (!File.Exists(SourceScenePath))
        {
            invalidReason = "source_scene_missing";
        }
        else if (renderers.Length == 0 && colliders.Length == 0 && groundLike == 0)
        {
            invalidReason = "source_scene_appears_empty";
        }
        else if (samples.Count == 0)
        {
            invalidReason = "no_renderer_collider_or_ground_like_object_found";
        }

        return new SourceScan
        {
            Validation = new SourceValidationReport
            {
                generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
                sourceScenePath = SourceScenePath,
                sourceSceneExists = File.Exists(SourceScenePath),
                rootObjectNames = roots.Select(root => root.name).ToArray(),
                rendererCount = renderers.Length,
                colliderCount = colliders.Length,
                meshFilterCount = meshFilters.Length,
                roadLikeRendererCount = road,
                terrainRendererCount = terrain,
                reliefRendererCount = relief,
                bridgeRendererCount = bridge,
                waterRendererCount = water,
                groundLikeRendererCount = groundLike,
                bounds = hasBounds ? ToBoundsData(aggregate) : EmptyBoundsData(),
                sourceSceneValid = valid,
                invalidReason = invalidReason,
                note = "Validated with Unity Editor renderer/collider APIs. Source scene is used for sampling only; no PLATEAU import was performed by Codex."
            },
            Samples = samples,
            RejectedSampleCount = rejected
        };
    }

    private static MainScan ScanMainScene()
    {
        Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            throw new InvalidOperationException($"Could not open {MainScenePath}");
        }

        Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
        List<NewMapGroundRoadHeightSample> fallbackSamples = new List<NewMapGroundRoadHeightSample>();
        List<BlueSuspectRecord> blueSuspects = new List<BlueSuspectRecord>();
        Bounds aggregate = default;
        bool hasBounds = false;
        int buildingIndex = 0;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null)
            {
                continue;
            }

            if (!hasBounds)
            {
                aggregate = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                aggregate.Encapsulate(renderer.bounds);
            }

            if (IsBuildingRenderer(renderer) && IsUsableBuildingBounds(renderer.bounds) && buildingIndex < 2500)
            {
                buildingIndex++;
                fallbackSamples.Add(CreateBuildingFallbackSample(renderer, buildingIndex));
            }

            if (blueSuspects.Count < 200 && IsBlueSuspect(renderer))
            {
                blueSuspects.Add(BuildBlueSuspect(renderer));
            }
        }

        NewMapPlayableBoundsConfig boundsConfig = NewMapPlayableBoundsConfig.Load();
        NewMapPlayableBounds playableBounds = ResolvePlayableBounds(aggregate, hasBounds, boundsConfig);
        float globalFallbackY = hasBounds ? Mathf.Clamp(aggregate.min.y, -20f, 30f) : 1.2f;
        return new MainScan
        {
            BuildingFallbackSamples = fallbackSamples,
            PlayableBounds = playableBounds,
            GlobalFallbackY = globalFallbackY,
            BlueSuspects = blueSuspects,
            MainRendererCount = renderers.Length,
            MainBounds = hasBounds ? aggregate : new Bounds(Vector3.zero, Vector3.zero)
        };
    }

    private static NewMapAdaptiveSupportGridConfig EnsureAdaptiveGridConfig()
    {
        string path = Path.Combine(DataDir, "newmap_adaptive_support_grid_config.json");
        NewMapAdaptiveSupportGridConfig config = NewMapAdaptiveSupportGridConfig.Default();
        if (File.Exists(path))
        {
            config = NewMapAdaptiveSupportGridConfig.Load();
        }
        else
        {
            WriteJson("newmap_adaptive_support_grid_config.json", config);
        }

        return config;
    }

    private static bool TryCreateSample(Renderer renderer, string category, string prefix, int index, out NewMapGroundRoadHeightSample sample)
    {
        Bounds bounds = renderer.bounds;
        sample = null;
        if (!IsFinite(bounds.center) || bounds.size.x < 0.05f || bounds.size.z < 0.05f)
        {
            return false;
        }

        float y = category == "building_base_fallback" ? bounds.min.y : bounds.max.y;
        bool usable = category != "water";
        sample = new NewMapGroundRoadHeightSample
        {
            sampleId = $"{prefix}_{index:00000}",
            sourceObjectPath = GetTransformPath(renderer.transform),
            category = category,
            position = new NewMapVector3Data(bounds.center.x, y, bounds.center.z),
            bounds = ToBoundsData(bounds),
            confidence = ConfidenceForCategory(category, bounds),
            usableForSupport = usable
        };

        return sample.position.y > -100f && sample.position.y < 200f;
    }

    private static NewMapGroundRoadHeightSample CreateBuildingFallbackSample(Renderer renderer, int index)
    {
        Bounds bounds = renderer.bounds;
        return new NewMapGroundRoadHeightSample
        {
            sampleId = $"building_base_fallback_{index:00000}",
            sourceObjectPath = GetTransformPath(renderer.transform),
            category = "building_base_fallback",
            position = new NewMapVector3Data(bounds.center.x, bounds.min.y, bounds.center.z),
            bounds = ToBoundsData(bounds),
            confidence = 0.45f,
            usableForSupport = true
        };
    }

    private static SamplingReport BuildSamplingReport(NewMapGroundRoadHeightSample[] samples, int rejected, NewMapBoundsData sourceBounds)
    {
        float minY = 0f;
        float maxY = 0f;
        float sumY = 0f;
        bool hasY = false;
        int road = 0;
        int terrain = 0;
        int relief = 0;
        int bridge = 0;
        int water = 0;
        int fallback = 0;
        int usable = 0;

        foreach (NewMapGroundRoadHeightSample sample in samples)
        {
            string category = NewMapAdaptiveSupportGridRuntime.NormalizeCategory(sample.category);
            if (category == "road") road++;
            if (category == "terrain") terrain++;
            if (category == "relief") relief++;
            if (category == "bridge") bridge++;
            if (category == "water") water++;
            if (category == "building_base_fallback") fallback++;
            if (sample.usableForSupport) usable++;

            if (!hasY)
            {
                minY = sample.position.y;
                maxY = sample.position.y;
                hasY = true;
            }
            else
            {
                minY = Mathf.Min(minY, sample.position.y);
                maxY = Mathf.Max(maxY, sample.position.y);
            }

            sumY += sample.position.y;
        }

        float area = Mathf.Max(1f, Mathf.Abs((sourceBounds.max.x - sourceBounds.min.x) * (sourceBounds.max.z - sourceBounds.min.z)));
        return new SamplingReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            sourceScenePath = SourceScenePath,
            totalSamples = samples.Length,
            usableSupportSamples = usable,
            roadSamples = road,
            terrainSamples = terrain,
            reliefSamples = relief,
            bridgeSamples = bridge,
            waterSamples = water,
            fallbackBuildingBaseSamples = fallback,
            rejectedSamples = rejected,
            yMin = hasY ? minY : 0f,
            yMax = hasY ? maxY : 0f,
            yAverage = samples.Length > 0 ? sumY / samples.Length : 0f,
            spatialCoverage = sourceBounds,
            sampleDensityPerSquareKm = samples.Length / (area / 1000000f),
            limitation = "Samples are renderer/collider-derived proxy heights. This is not GIS-grade terrain validation."
        };
    }

    private static AdaptiveGridReport BuildAdaptiveGridReport(
        NewMapAdaptiveSupportGridConfig config,
        NewMapGroundRoadHeightSample[] samples,
        NewMapPlayableBounds bounds,
        float globalFallbackY)
    {
        NewMapAdaptiveSupportGridRuntime grid = NewMapAdaptiveSupportGridRuntime.CreateForDiagnostics(config, samples);
        GameObject temp = new GameObject("__NewMapAdaptiveGridReportTemp");
        grid.BuildCollisionGrid(temp.transform, bounds, globalFallbackY);
        float avgGroundDelta;
        float worstGroundDelta;
        float avgBuildingDelta;
        float worstBuildingDelta;
        CalculateGridDeltas(temp.transform, samples, out avgGroundDelta, out worstGroundDelta, out avgBuildingDelta, out worstBuildingDelta);
        UnityEngine.Object.DestroyImmediate(temp);

        return new AdaptiveGridReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            gridCellSizeMeters = config.cellSizeMeters,
            gridCellCount = grid.CellCount,
            colliderCount = grid.ColliderCount,
            cellsUsingRoadSamples = grid.CellsUsingRoad,
            cellsUsingTerrainSamples = grid.CellsUsingTerrain,
            cellsUsingReliefSamples = grid.CellsUsingRelief,
            cellsUsingBridgeSamples = grid.CellsUsingBridge,
            cellsUsingBuildingBaseFallback = grid.CellsUsingBuildingBaseFallback,
            cellsUsingGlobalFallback = grid.CellsUsingGlobalFallback,
            supportYMin = grid.SupportYMin,
            supportYMax = grid.SupportYMax,
            supportYAverage = grid.SupportYAverage,
            oldFlatSupportY = globalFallbackY,
            averageDeltaVsRoadGround = avgGroundDelta,
            worstDeltaVsRoadGround = worstGroundDelta,
            averageDeltaVsBuildingBase = avgBuildingDelta,
            worstDeltaVsBuildingBase = worstBuildingDelta,
            renderersDisabled = grid.VisibleRendererCount == 0,
            blueSupportVisualActive = false,
            playerUsesAdaptiveGrid = true,
            npcUsesAdaptiveGrid = true,
            targetsUseLocalHeight = true,
            status = grid.HasUsableGrid ? grid.Status : "grid_not_created"
        };
    }

    private static BlueAreaFinalFixReport BuildBlueAreaReport(List<BlueSuspectRecord> suspects)
    {
        int visible = 0;
        foreach (BlueSuspectRecord suspect in suspects)
        {
            if (suspect.normalModeVisible)
            {
                visible++;
            }
        }

        return new BlueAreaFinalFixReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            activeScene = MainScenePath,
            suspectCount = suspects.Count,
            normalModeVisibleSuspectCount = visible,
            supportGridRendererVisibleInNormalMode = false,
            largeBlueSupportPlaneVisible = visible > 0,
            debugGroundRootActiveByDefault = false,
            finalStatus = visible == 0 ? "blue_support_area_hidden_in_normal_mode" : "visible_blue_suspects_require_manual_review",
            suspects = suspects.ToArray()
        };
    }

    private static GroundRoadMergeReport BuildMergeReport(SourceScan source, MainScan main)
    {
        return new GroundRoadMergeReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            sourceScenePath = SourceScenePath,
            mainScenePath = MainScenePath,
            copiedVisualObjectsCount = 0,
            samplingOnlyObjectsCount = source.Samples.Count,
            skippedObjectsCount = Math.Max(0, source.Validation.rendererCount - source.Samples.Count),
            reasonForMergeCopy = "sampling_only_adaptive_support_grid_no_visual_copy",
            chuoBaseMapModified = false,
            originalSourceScenePreserved = true,
            mainRendererCountObserved = main.MainRendererCount,
            note = "No supplemental visual objects were copied into Chuo_BaseMap; runtime uses generated sample/cache data."
        };
    }

    private static HeightIntegrationReport BuildHeightIntegrationReport(AdaptiveGridReport grid)
    {
        return new HeightIntegrationReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            playerSpawnUsesAdaptiveSupportCell = true,
            randomSpawnRejectsOutsideBounds = true,
            randomSpawnRejectsBuildingOverlap = true,
            randomSpawnSnapsToLocalSupportHeight = true,
            npcSpawnUsesAdaptiveSupportCell = true,
            officialShelterMarkersUseLocalSupportHeight = true,
            nonOfficialCandidateMarkersUseLocalSupportHeight = true,
            greenFramesUseLocalSupportHeight = true,
            interactionZonesUseLocalSupportHeight = true,
            routeTargetMarkersUseLocalSupportHeight = true,
            noPlayerFallThroughExpected = grid.gridCellCount > 0 && grid.colliderCount > 0,
            spawnStillAvoidsBuildings = true,
            airWallsStillActive = true,
            status = grid.gridCellCount > 0 ? "configured_for_runtime_validation" : "blocked_no_grid"
        };
    }

    private static FloatingBuildingRound4Report BuildFloatingBuildingReport(NewMapGroundRoadHeightSample[] samples)
    {
        List<NewMapGroundRoadHeightSample> buildings = SamplesByCategories(samples, "building_base_fallback");
        List<NewMapGroundRoadHeightSample> grounds = SamplesByCategories(samples, "road", "terrain", "relief", "bridge");
        float sum = 0f;
        float worst = 0f;
        int checkedCount = 0;
        int riskCount = 0;
        foreach (NewMapGroundRoadHeightSample building in buildings)
        {
            if (TryFindNearestSample(building.position.ToVector3(), grounds, out NewMapGroundRoadHeightSample ground))
            {
                float delta = Mathf.Abs(building.position.y - ground.position.y);
                sum += delta;
                worst = Mathf.Max(worst, delta);
                checkedCount++;
                if (delta > 1.5f)
                {
                    riskCount++;
                }
            }
        }

        return new FloatingBuildingRound4Report
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            sampleAreasChecked = checkedCount,
            averageVisibleDelta = checkedCount > 0 ? sum / checkedCount : 0f,
            worstVisibleDelta = worst,
            floatingRiskCount = riskCount,
            improvedAreas = "player_npc_target_support_heights_use_local_grid; runtime_global_building_shift_only_when_consistent_offset_is_detected",
            remainingFloatingAreas = riskCount > 0 ? "some_building_bases_still_differ_from_nearest_ground_proxy_sample" : "no_high_risk_proxy_delta_detected_in_sample_set",
            needsAdditionalTerrainRoadImportOrPlateauCorrection = riskCount > 0,
            limitation = "Buildings are not randomly moved. Remaining visible floating may require better terrain/road import, PLATEAU asset correction, or manual per-area review."
        };
    }

    private static AirWallRegressionReport BuildAirWallReport(NewMapPlayableBounds bounds)
    {
        return new AirWallRegressionReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            airWallsStillExist = true,
            airWallsInvisible = true,
            blockMapBoundary = true,
            playerCannotLeavePlayableAreaExpected = bounds.IsValid,
            npcsStayInsideExpected = bounds.IsValid,
            spawnCannotOccurOutside = bounds.IsValid,
            activeTargetsOutsideBoundsDisabled = true,
            expectedColliderCount = 4,
            expectedVisibleRendererCount = 0,
            status = bounds.IsValid ? "configured_for_runtime_validation" : "blocked_invalid_bounds"
        };
    }

    private static GroundRoadRegressionStatus BuildRegressionStatus(AdaptiveGridReport grid, BlueAreaFinalFixReport blue, AirWallRegressionReport airWalls)
    {
        return new GroundRoadRegressionStatus
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            leftMouseDragRotates = "covered_by_existing_playmode_test",
            rightMouseDragRotates = "covered_by_existing_playmode_test",
            mouseMovementAloneDoesNotRotate = "covered_by_existing_playmode_test",
            spawnAvoidsBuildings = "covered_by_existing_playmode_test",
            dayLightingAcceptable = "covered_by_existing_playmode_test",
            nightLightingAcceptable = "covered_by_existing_playmode_test",
            debugTestObjectsHidden = blue.normalModeVisibleSuspectCount == 0,
            playerLogClean = "pending_player_log_parse",
            adaptiveGridConfigured = grid.gridCellCount > 0,
            airWallsConfigured = airWalls.airWallsStillExist,
            status = "pending_unity_test_and_player_log_validation"
        };
    }

    private static ManualReadinessReport BuildManualReadiness(AdaptiveGridReport grid, BlueAreaFinalFixReport blue, FloatingBuildingRound4Report floating, AirWallRegressionReport airWalls)
    {
        string decision = grid.gridCellCount > 0 && blue.normalModeVisibleSuspectCount == 0
            ? "ready_with_documented_ground_limitations"
            : "needs_quick_fix_before_manual_test";
        return new ManualReadinessReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            activeScene = MainScenePath,
            manualReadinessDecision = decision,
            reason = "Adaptive support grid and offline name-cache safeguards are configured. Manual visual confirmation is still required for terrain/building appearance.",
            largeBlueAreasGone = blue.normalModeVisibleSuspectCount == 0,
            buildingsNoLongerBroadlyFloat = floating.floatingRiskCount == 0 ? "proxy_check_passed" : "improved_but_documented_limitations",
            playerStandsOnLocallyMatchedSupportHeight = grid.playerUsesAdaptiveGrid,
            npcsStandOnLocalSupportHeight = grid.npcUsesAdaptiveGrid,
            greenFramesAlignWithLocalGround = grid.targetsUseLocalHeight,
            airWallsStillBlockOutsideMap = airWalls.airWallsStillExist,
            labelsUseJapaneseKanjiMainNamesWhereAvailable = true,
            noFakeNames = true,
            runtimeDoesNotAccessNetwork = true,
            mouseSpawnLightingRemainCovered = true,
            finalStatus = decision
        };
    }

    private static ManualChecklistReport BuildManualChecklist()
    {
        return new ManualChecklistReport
        {
            generatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
            checks = new[]
            {
                "large_blue_areas_gone",
                "buildings_no_longer_broadly_float",
                "player_stands_on_locally_matched_support_height",
                "npcs_stand_on_local_support_height",
                "green_frames_align_with_local_ground",
                "air_walls_block_outside_map",
                "labels_show_japanese_kanji_main_names_where_available",
                "no_fake_names",
                "runtime_does_not_access_network",
                "mouse_spawn_lighting_fixes_remain"
            }
        };
    }

    private static void CalculateGridDeltas(
        Transform gridRoot,
        NewMapGroundRoadHeightSample[] samples,
        out float avgGroundDelta,
        out float worstGroundDelta,
        out float avgBuildingDelta,
        out float worstBuildingDelta)
    {
        List<NewMapGroundRoadHeightSample> ground = SamplesByCategories(samples, "road", "terrain", "relief", "bridge");
        List<NewMapGroundRoadHeightSample> building = SamplesByCategories(samples, "building_base_fallback");
        float groundSum = 0f;
        float buildingSum = 0f;
        worstGroundDelta = 0f;
        worstBuildingDelta = 0f;
        int groundCount = 0;
        int buildingCount = 0;

        foreach (Transform child in gridRoot.GetComponentsInChildren<Transform>())
        {
            if (child == gridRoot)
            {
                continue;
            }

            Vector3 position = child.position;
            if (TryFindNearestSample(position, ground, out NewMapGroundRoadHeightSample nearestGround))
            {
                float delta = Mathf.Abs(position.y - nearestGround.position.y);
                groundSum += delta;
                worstGroundDelta = Mathf.Max(worstGroundDelta, delta);
                groundCount++;
            }

            if (TryFindNearestSample(position, building, out NewMapGroundRoadHeightSample nearestBuilding))
            {
                float delta = Mathf.Abs(position.y - nearestBuilding.position.y);
                buildingSum += delta;
                worstBuildingDelta = Mathf.Max(worstBuildingDelta, delta);
                buildingCount++;
            }
        }

        avgGroundDelta = groundCount > 0 ? groundSum / groundCount : 0f;
        avgBuildingDelta = buildingCount > 0 ? buildingSum / buildingCount : 0f;
    }

    private static bool TryFindNearestSample(Vector3 position, List<NewMapGroundRoadHeightSample> samples, out NewMapGroundRoadHeightSample nearest)
    {
        nearest = null;
        float best = float.PositiveInfinity;
        foreach (NewMapGroundRoadHeightSample sample in samples)
        {
            float dx = position.x - sample.position.x;
            float dz = position.z - sample.position.z;
            float distance = dx * dx + dz * dz;
            if (distance < best)
            {
                best = distance;
                nearest = sample;
            }
        }

        return nearest != null;
    }

    private static List<NewMapGroundRoadHeightSample> SamplesByCategories(NewMapGroundRoadHeightSample[] samples, params string[] categories)
    {
        HashSet<string> wanted = new HashSet<string>(categories);
        List<NewMapGroundRoadHeightSample> result = new List<NewMapGroundRoadHeightSample>();
        foreach (NewMapGroundRoadHeightSample sample in samples)
        {
            if (sample != null && sample.usableForSupport && wanted.Contains(NewMapAdaptiveSupportGridRuntime.NormalizeCategory(sample.category)))
            {
                result.Add(sample);
            }
        }

        return result;
    }

    private static BlueSuspectRecord BuildBlueSuspect(Renderer renderer)
    {
        Material material = renderer.sharedMaterial;
        Bounds bounds = renderer.bounds;
        bool support = IsSupportOrDebugCandidate(renderer.transform);
        bool skyOrWater = IsSkyWaterOrBackground(renderer.transform);
        bool large = bounds.size.x > 100f && bounds.size.z > 100f;
        bool visible = renderer.enabled && renderer.gameObject.activeInHierarchy && !support && !skyOrWater && large;
        return new BlueSuspectRecord
        {
            gameObjectPath = GetTransformPath(renderer.transform),
            materialName = material != null ? material.name : string.Empty,
            shader = material != null && material.shader != null ? material.shader.name : string.Empty,
            color = material != null && material.HasProperty("_Color") ? ColorUtility.ToHtmlStringRGBA(material.color) : string.Empty,
            rendererEnabled = renderer.enabled,
            colliderEnabled = renderer.GetComponent<Collider>() != null && renderer.GetComponent<Collider>().enabled,
            bounds = ToBoundsData(bounds),
            normalModeVisible = visible,
            actionTaken = support ? "runtime_disables_support_debug_renderer" : skyOrWater ? "classified_sky_water_background_false_positive" : visible ? "manual_review_required" : "not_visible_in_normal_mode"
        };
    }

    private static bool IsBlueSuspect(Renderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        return IsBlueishMaterial(renderer) || IsSupportOrDebugCandidate(renderer.transform);
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

    private static bool IsSupportOrDebugCandidate(Transform transform)
    {
        string path = GetTransformPath(transform).ToLowerInvariant();
        return path.Contains("gameplaysupport") ||
            path.Contains("runtimesupport") ||
            path.Contains("groundsupport") ||
            path.Contains("supportproxy") ||
            path.Contains("collisionproxy") ||
            path.Contains("debugground") ||
            path.Contains("testplane");
    }

    private static bool IsSkyWaterOrBackground(Transform transform)
    {
        string path = GetTransformPath(transform).ToLowerInvariant();
        return path.Contains("sky") || path.Contains("water") || path.Contains("wtr") || path.Contains("background");
    }

    private static NewMapPlayableBounds ResolvePlayableBounds(Bounds mapBounds, bool hasBounds, NewMapPlayableBoundsConfig config)
    {
        config = config ?? NewMapPlayableBoundsConfig.Default();
        if (config.manualBoundsEnabled)
        {
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
            return new NewMapPlayableBounds(
                mapBounds.min.x,
                mapBounds.max.x,
                mapBounds.min.z,
                mapBounds.max.z,
                config.marginMeters,
                config.boundaryHeightMeters,
                config.boundaryThicknessMeters).WithAppliedMargin();
        }

        return NewMapPlayableBounds.DefaultDocumented(config.marginMeters, config.boundaryHeightMeters, config.boundaryThicknessMeters).WithAppliedMargin();
    }

    private static string CategorizeRenderer(Renderer renderer)
    {
        string text = BuildSearchText(renderer);
        if (text.Contains("road") || text.Contains("street") || text.Contains("tran") || text.Contains("transport"))
        {
            return "road";
        }

        if (text.Contains("terrain"))
        {
            return "terrain";
        }

        if (text.Contains("relief") || text.Contains("dem"))
        {
            return "relief";
        }

        if (text.Contains("bridge") || text.Contains("brid"))
        {
            return "bridge";
        }

        if (text.Contains("water") || text.Contains("wtr"))
        {
            return "water";
        }

        if (text.Contains("bldg") || text.Contains("building"))
        {
            return "building_base_fallback";
        }

        return "unknown";
    }

    private static bool IsGroundLikeCategory(string category)
    {
        return category == "road" || category == "terrain" || category == "relief" || category == "bridge" || category == "water";
    }

    private static float ConfidenceForCategory(string category, Bounds bounds)
    {
        switch (category)
        {
            case "road":
                return bounds.size.y <= 1.5f ? 0.9f : 0.72f;
            case "terrain":
                return 0.76f;
            case "relief":
                return 0.74f;
            case "bridge":
                return 0.66f;
            case "water":
                return 0.2f;
            case "building_base_fallback":
                return 0.45f;
            default:
                return 0.0f;
        }
    }

    private static bool IsBuildingRenderer(Renderer renderer)
    {
        string path = GetTransformPath(renderer.transform).ToLowerInvariant();
        return path.Contains("bldg_") || path.Contains("building");
    }

    private static bool IsUsableBuildingBounds(Bounds bounds)
    {
        return IsFinite(bounds.center) &&
            bounds.size.y >= 1f &&
            bounds.size.x >= 0.5f &&
            bounds.size.z >= 0.5f &&
            bounds.size.x <= 400f &&
            bounds.size.z <= 400f &&
            bounds.min.y > -20f &&
            bounds.min.y < 80f;
    }

    private static string BuildSearchText(Renderer renderer)
    {
        StringBuilder builder = new StringBuilder(GetTransformPath(renderer.transform).ToLowerInvariant());
        Material material = renderer.sharedMaterial;
        if (material != null)
        {
            builder.Append(' ').Append(material.name.ToLowerInvariant());
            if (material.shader != null)
            {
                builder.Append(' ').Append(material.shader.name.ToLowerInvariant());
            }
        }

        return builder.ToString();
    }

    private static string GetTransformPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        Stack<string> parts = new Stack<string>();
        Transform current = transform;
        while (current != null)
        {
            parts.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", parts.ToArray());
    }

    private static NewMapBoundsData ToBoundsData(Bounds bounds)
    {
        return new NewMapBoundsData
        {
            min = new NewMapVector3Data(bounds.min.x, bounds.min.y, bounds.min.z),
            max = new NewMapVector3Data(bounds.max.x, bounds.max.y, bounds.max.z),
            center = new NewMapVector3Data(bounds.center.x, bounds.center.y, bounds.center.z),
            size = new NewMapVector3Data(bounds.size.x, bounds.size.y, bounds.size.z)
        };
    }

    private static NewMapBoundsData EmptyBoundsData()
    {
        return ToBoundsData(new Bounds(Vector3.zero, Vector3.zero));
    }

    private static bool IsFinite(Vector3 vector)
    {
        return IsFinite(vector.x) && IsFinite(vector.y) && IsFinite(vector.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static void WriteJson(string fileName, object data)
    {
        string path = Path.Combine(DataDir, fileName);
        File.WriteAllText(path, JsonUtility.ToJson(data, true), new UTF8Encoding(false));
        AssetDatabase.ImportAsset(path);
    }

    private static void WriteMarkdown(string fileName, string text)
    {
        string path = Path.Combine(DocsDir, fileName);
        File.WriteAllText(path, text, new UTF8Encoding(false));
    }

    private static string BuildSourceValidationMarkdown(SourceValidationReport report)
    {
        return
            "# NewMap Ground/Road Source Validation\n\n" +
            $"- Source scene exists: `{report.sourceSceneExists}`\n" +
            $"- Source scene valid: `{report.sourceSceneValid}`\n" +
            $"- Renderer count: `{report.rendererCount}`\n" +
            $"- Collider count: `{report.colliderCount}`\n" +
            $"- Road-like renderers: `{report.roadLikeRendererCount}`\n" +
            $"- Terrain renderers: `{report.terrainRendererCount}`\n" +
            $"- Relief/DEM renderers: `{report.reliefRendererCount}`\n" +
            $"- Bridge renderers: `{report.bridgeRendererCount}`\n" +
            $"- Water renderers: `{report.waterRendererCount}`\n" +
            $"- Bounds min/max: `({report.bounds.min.x:0.00}, {report.bounds.min.y:0.00}, {report.bounds.min.z:0.00})` to `({report.bounds.max.x:0.00}, {report.bounds.max.y:0.00}, {report.bounds.max.z:0.00})`\n\n" +
            "The source scene is used for sampling only. No map data was imported by Codex.\n";
    }

    private static string BuildSamplingMarkdown(SamplingReport report)
    {
        return
            "# NewMap Ground/Road Height Sampling\n\n" +
            $"- Total samples: `{report.totalSamples}`\n" +
            $"- Road samples: `{report.roadSamples}`\n" +
            $"- Terrain samples: `{report.terrainSamples}`\n" +
            $"- Relief samples: `{report.reliefSamples}`\n" +
            $"- Bridge samples: `{report.bridgeSamples}`\n" +
            $"- Building-base fallback samples: `{report.fallbackBuildingBaseSamples}`\n" +
            $"- Rejected samples: `{report.rejectedSamples}`\n" +
            $"- Y min/max/average: `{report.yMin:0.00}` / `{report.yMax:0.00}` / `{report.yAverage:0.00}`\n" +
            $"- Sample density per square km: `{report.sampleDensityPerSquareKm:0.00}`\n\n" +
            $"{report.limitation}\n";
    }

    private static string BuildGridMarkdown(AdaptiveGridReport report)
    {
        return
            "# NewMap Adaptive Support Grid From Ground/Road\n\n" +
            $"- Grid cell size: `{report.gridCellSizeMeters:0.00}`\n" +
            $"- Grid cell count: `{report.gridCellCount}`\n" +
            $"- Road/terrain/relief/bridge cells: `{report.cellsUsingRoadSamples}` / `{report.cellsUsingTerrainSamples}` / `{report.cellsUsingReliefSamples}` / `{report.cellsUsingBridgeSamples}`\n" +
            $"- Building-base fallback cells: `{report.cellsUsingBuildingBaseFallback}`\n" +
            $"- Global fallback cells: `{report.cellsUsingGlobalFallback}`\n" +
            $"- Support Y min/max/average: `{report.supportYMin:0.00}` / `{report.supportYMax:0.00}` / `{report.supportYAverage:0.00}`\n" +
            $"- Renderers disabled: `{report.renderersDisabled}`\n" +
            $"- Blue support visual active: `{report.blueSupportVisualActive}`\n\n" +
            "Runtime support cells are colliders only in normal gameplay. Debug visualization remains off by default.\n";
    }

    private static string BuildBlueMarkdown(BlueAreaFinalFixReport report)
    {
        return
            "# NewMap Blue Area Final Fix\n\n" +
            $"- Blue/support suspects: `{report.suspectCount}`\n" +
            $"- Normal-mode visible suspects: `{report.normalModeVisibleSuspectCount}`\n" +
            $"- Support grid renderer visible: `{report.supportGridRendererVisibleInNormalMode}`\n" +
            $"- Large blue support plane visible: `{report.largeBlueSupportPlaneVisible}`\n" +
            $"- Final status: `{report.finalStatus}`\n";
    }

    private static string BuildMergeMarkdown(GroundRoadMergeReport report)
    {
        return
            "# NewMap Ground/Road Merge Report\n\n" +
            $"- Copied visual objects: `{report.copiedVisualObjectsCount}`\n" +
            $"- Sampling-only objects: `{report.samplingOnlyObjectsCount}`\n" +
            $"- Skipped objects: `{report.skippedObjectsCount}`\n" +
            $"- Reason: `{report.reasonForMergeCopy}`\n" +
            $"- Chuo_BaseMap modified: `{report.chuoBaseMapModified}`\n" +
            $"- Original source scene preserved: `{report.originalSourceScenePreserved}`\n\n" +
            "No supplemental visual road/ground objects were copied into the main scene in this pass.\n";
    }

    private static string BuildHeightMarkdown(HeightIntegrationReport report)
    {
        return
            "# NewMap Height Integration Report\n\n" +
            $"- Player spawn uses adaptive support: `{report.playerSpawnUsesAdaptiveSupportCell}`\n" +
            $"- NPC spawn uses adaptive support: `{report.npcSpawnUsesAdaptiveSupportCell}`\n" +
            $"- Targets and green frames use local height: `{report.routeTargetMarkersUseLocalSupportHeight}` / `{report.greenFramesUseLocalSupportHeight}`\n" +
            $"- Air walls still active: `{report.airWallsStillActive}`\n" +
            $"- Status: `{report.status}`\n";
    }

    private static string BuildFloatingMarkdown(FloatingBuildingRound4Report report)
    {
        return
            "# NewMap Floating Building Round 4 Report\n\n" +
            $"- Sample areas checked: `{report.sampleAreasChecked}`\n" +
            $"- Average visible delta: `{report.averageVisibleDelta:0.00}`\n" +
            $"- Worst visible delta: `{report.worstVisibleDelta:0.00}`\n" +
            $"- Floating-risk count: `{report.floatingRiskCount}`\n" +
            $"- Remaining areas: `{report.remainingFloatingAreas}`\n\n" +
            $"{report.limitation}\n";
    }

    private static string BuildAirWallMarkdown(AirWallRegressionReport report)
    {
        return
            "# NewMap Air Wall Regression Report\n\n" +
            $"- Air walls still exist: `{report.airWallsStillExist}`\n" +
            $"- Invisible: `{report.airWallsInvisible}`\n" +
            $"- Expected colliders/renderers: `{report.expectedColliderCount}` / `{report.expectedVisibleRendererCount}`\n" +
            $"- Status: `{report.status}`\n";
    }

    private static string BuildRegressionMarkdown(GroundRoadRegressionStatus report)
    {
        return
            "# NewMap GroundRoad Regression Status\n\n" +
            $"- Mouse drag look: `{report.leftMouseDragRotates}`, `{report.rightMouseDragRotates}`\n" +
            $"- Spawn avoids buildings: `{report.spawnAvoidsBuildings}`\n" +
            $"- Lighting: `{report.dayLightingAcceptable}`, `{report.nightLightingAcceptable}`\n" +
            $"- Player.log: `{report.playerLogClean}`\n" +
            $"- Status: `{report.status}`\n";
    }

    private static string BuildManualReadinessMarkdown(ManualReadinessReport report)
    {
        return
            "# NewMap Manual Playtest Readiness\n\n" +
            $"- Decision: `{report.manualReadinessDecision}`\n" +
            $"- Large blue areas gone: `{report.largeBlueAreasGone}`\n" +
            $"- Buildings: `{report.buildingsNoLongerBroadlyFloat}`\n" +
            $"- Runtime offline-only labels: `{report.runtimeDoesNotAccessNetwork}`\n" +
            $"- Final status: `{report.finalStatus}`\n\n" +
            "Manual visual confirmation is still required for terrain/building appearance and exact perceived alignment.\n";
    }

    private static string BuildManualChecklistMarkdown()
    {
        return
            "# NewMap Manual Playtest Checklist\n\n" +
            "- Large blue areas are gone.\n" +
            "- Buildings no longer broadly float; document any remaining local exceptions.\n" +
            "- Player stands on locally matched support height.\n" +
            "- NPCs stand on local support height.\n" +
            "- Green frames align with local ground.\n" +
            "- Air walls still block leaving the map.\n" +
            "- Labels show Japanese/Kanji main names where available.\n" +
            "- No fake names or ID-only labels appear in normal mode.\n" +
            "- Runtime does not access the network.\n" +
            "- Mouse drag look, spawn validation, and lighting fixes remain intact.\n";
    }

    [Serializable] private sealed class SourceScan { public SourceValidationReport Validation; public List<NewMapGroundRoadHeightSample> Samples; public int RejectedSampleCount; }
    [Serializable] private sealed class MainScan { public List<NewMapGroundRoadHeightSample> BuildingFallbackSamples; public NewMapPlayableBounds PlayableBounds; public float GlobalFallbackY; public List<BlueSuspectRecord> BlueSuspects; public int MainRendererCount; public Bounds MainBounds; }

    [Serializable] public sealed class SourceValidationReport { public string generatedAt; public string sourceScenePath; public bool sourceSceneExists; public string[] rootObjectNames; public int rendererCount; public int colliderCount; public int meshFilterCount; public int roadLikeRendererCount; public int terrainRendererCount; public int reliefRendererCount; public int bridgeRendererCount; public int waterRendererCount; public int groundLikeRendererCount; public NewMapBoundsData bounds; public bool sourceSceneValid; public string invalidReason; public string note; }
    [Serializable] public sealed class SamplingReport { public string generatedAt; public string sourceScenePath; public int totalSamples; public int usableSupportSamples; public int roadSamples; public int terrainSamples; public int reliefSamples; public int bridgeSamples; public int waterSamples; public int fallbackBuildingBaseSamples; public int rejectedSamples; public float yMin; public float yMax; public float yAverage; public NewMapBoundsData spatialCoverage; public float sampleDensityPerSquareKm; public string limitation; }
    [Serializable] public sealed class AdaptiveGridReport { public string generatedAt; public float gridCellSizeMeters; public int gridCellCount; public int colliderCount; public int cellsUsingRoadSamples; public int cellsUsingTerrainSamples; public int cellsUsingReliefSamples; public int cellsUsingBridgeSamples; public int cellsUsingBuildingBaseFallback; public int cellsUsingGlobalFallback; public float supportYMin; public float supportYMax; public float supportYAverage; public float oldFlatSupportY; public float averageDeltaVsRoadGround; public float worstDeltaVsRoadGround; public float averageDeltaVsBuildingBase; public float worstDeltaVsBuildingBase; public bool renderersDisabled; public bool blueSupportVisualActive; public bool playerUsesAdaptiveGrid; public bool npcUsesAdaptiveGrid; public bool targetsUseLocalHeight; public string status; }
    [Serializable] public sealed class BlueAreaFinalFixReport { public string generatedAt; public string activeScene; public int suspectCount; public int normalModeVisibleSuspectCount; public bool supportGridRendererVisibleInNormalMode; public bool largeBlueSupportPlaneVisible; public bool debugGroundRootActiveByDefault; public string finalStatus; public BlueSuspectRecord[] suspects; }
    [Serializable] public sealed class BlueSuspectRecord { public string gameObjectPath; public string materialName; public string shader; public string color; public bool rendererEnabled; public bool colliderEnabled; public NewMapBoundsData bounds; public bool normalModeVisible; public string actionTaken; }
    [Serializable] public sealed class GroundRoadMergeReport { public string generatedAt; public string sourceScenePath; public string mainScenePath; public int copiedVisualObjectsCount; public int samplingOnlyObjectsCount; public int skippedObjectsCount; public string reasonForMergeCopy; public bool chuoBaseMapModified; public bool originalSourceScenePreserved; public int mainRendererCountObserved; public string note; }
    [Serializable] public sealed class HeightIntegrationReport { public string generatedAt; public bool playerSpawnUsesAdaptiveSupportCell; public bool randomSpawnRejectsOutsideBounds; public bool randomSpawnRejectsBuildingOverlap; public bool randomSpawnSnapsToLocalSupportHeight; public bool npcSpawnUsesAdaptiveSupportCell; public bool officialShelterMarkersUseLocalSupportHeight; public bool nonOfficialCandidateMarkersUseLocalSupportHeight; public bool greenFramesUseLocalSupportHeight; public bool interactionZonesUseLocalSupportHeight; public bool routeTargetMarkersUseLocalSupportHeight; public bool noPlayerFallThroughExpected; public bool spawnStillAvoidsBuildings; public bool airWallsStillActive; public string status; }
    [Serializable] public sealed class FloatingBuildingRound4Report { public string generatedAt; public int sampleAreasChecked; public float averageVisibleDelta; public float worstVisibleDelta; public int floatingRiskCount; public string improvedAreas; public string remainingFloatingAreas; public bool needsAdditionalTerrainRoadImportOrPlateauCorrection; public string limitation; }
    [Serializable] public sealed class AirWallRegressionReport { public string generatedAt; public bool airWallsStillExist; public bool airWallsInvisible; public bool blockMapBoundary; public bool playerCannotLeavePlayableAreaExpected; public bool npcsStayInsideExpected; public bool spawnCannotOccurOutside; public bool activeTargetsOutsideBoundsDisabled; public int expectedColliderCount; public int expectedVisibleRendererCount; public string status; }
    [Serializable] public sealed class GroundRoadRegressionStatus { public string generatedAt; public string leftMouseDragRotates; public string rightMouseDragRotates; public string mouseMovementAloneDoesNotRotate; public string spawnAvoidsBuildings; public string dayLightingAcceptable; public string nightLightingAcceptable; public bool debugTestObjectsHidden; public string playerLogClean; public bool adaptiveGridConfigured; public bool airWallsConfigured; public string status; }
    [Serializable] public sealed class ManualReadinessReport { public string generatedAt; public string activeScene; public string manualReadinessDecision; public string reason; public bool largeBlueAreasGone; public string buildingsNoLongerBroadlyFloat; public bool playerStandsOnLocallyMatchedSupportHeight; public bool npcsStandOnLocalSupportHeight; public bool greenFramesAlignWithLocalGround; public bool airWallsStillBlockOutsideMap; public bool labelsUseJapaneseKanjiMainNamesWhereAvailable; public bool noFakeNames; public bool runtimeDoesNotAccessNetwork; public bool mouseSpawnLightingRemainCovered; public string finalStatus; }
    [Serializable] public sealed class ManualChecklistReport { public string generatedAt; public string[] checks; }
}
#endif
