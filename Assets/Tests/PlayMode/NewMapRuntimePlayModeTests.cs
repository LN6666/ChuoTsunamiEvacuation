using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class NewMapRuntimePlayModeTests
{
    [SetUp]
    public void SetUp()
    {
        NewMapRuntimeBootstrap.EnableLocalTrainingProxyTargetsForDiagnostics = true;
    }

    [TearDown]
    public void TearDown()
    {
        NewMapRuntimeBootstrap.EnableLocalTrainingProxyTargetsForDiagnostics = false;

        foreach (NewMapRuntimeBootstrap bootstrap in Object.FindObjectsOfType<NewMapRuntimeBootstrap>())
        {
            Object.DestroyImmediate(bootstrap.gameObject);
        }

        foreach (NewMapPlayerController player in Object.FindObjectsOfType<NewMapPlayerController>())
        {
            Object.DestroyImmediate(player.gameObject);
        }

        foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
        {
            Object.DestroyImmediate(canvas.gameObject);
        }

        foreach (NewMapHazardController hazard in Object.FindObjectsOfType<NewMapHazardController>())
        {
            Object.DestroyImmediate(hazard.gameObject);
        }

        foreach (NewMapNpcCrowdPrototype crowd in Object.FindObjectsOfType<NewMapNpcCrowdPrototype>())
        {
            Object.DestroyImmediate(crowd.gameObject);
        }

        foreach (EventSystem eventSystem in Object.FindObjectsOfType<EventSystem>())
        {
            Object.DestroyImmediate(eventSystem.gameObject);
        }

        GameObject officialAnchorFixture = GameObject.Find("bldg_25d370de-2c35-457b-b756-3444a3d02eb3");
        if (officialAnchorFixture != null)
        {
            Object.DestroyImmediate(officialAnchorFixture);
        }

        GameObject spawnOverlapFixture = GameObject.Find("bldg_spawn_overlap_rejection_fixture");
        if (spawnOverlapFixture != null)
        {
            Object.DestroyImmediate(spawnOverlapFixture);
        }

        GameObject snapdownFixture = GameObject.Find("bldg_snapdown_fixture_root");
        if (snapdownFixture != null)
        {
            Object.DestroyImmediate(snapdownFixture);
        }

        GameObject nonBuildingSnapdownFixture = GameObject.Find("road_snapdown_nonbuilding_fixture");
        if (nonBuildingSnapdownFixture != null)
        {
            Object.DestroyImmediate(nonBuildingSnapdownFixture);
        }

        string[] runtimeRootNames =
        {
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

        foreach (GameObject root in Object.FindObjectsOfType<GameObject>(true))
        {
            if (root != null && runtimeRootNames.Contains(root.name))
            {
                Object.DestroyImmediate(root);
            }
        }
    }

    [UnityTest]
    public IEnumerator RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough()
    {
        NewMapRuntimeBootstrap bootstrap = NewMapRuntimeBootstrap.CreateForCurrentScene();
        Assert.NotNull(bootstrap);

        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);
        Assert.IsTrue(player.HasActiveCamera);
        Assert.IsFalse(player.ControlEnabled);
        Assert.IsFalse(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);
        Assert.AreEqual(0, controller.RuntimeTargets.Count(target => target.IsOfficialShelter), "Official shelters must not be active without a verified scene GML anchor.");

        controller.StartTourismMode();
        float startY = player.transform.position.y;
        yield return new WaitForSeconds(2f);
        Assert.Greater(player.transform.position.y, startY - 8f, "Player should not fall endlessly through the map/support proxy.");
        Assert.AreEqual(0, player.FallRecoveryCount, "Normal spawn grounding should not need fall recovery.");
        Assert.IsTrue(bootstrap.LastRuntimeCollisionSupportProxyActive, "Final NewMap manual test uses the documented runtime collision support.");
        Assert.IsTrue(bootstrap.LastRuntimeCollisionSupportColliderActive, "Gameplay ground cover/support must keep enabled colliders for movement/spawn support.");
        Assert.IsFalse(bootstrap.LastRuntimeCollisionSupportRendererVisible, "Old support/debug proxy renderers must stay invisible.");
        Assert.AreEqual(0, bootstrap.LastVisibleSupportRendererCount, "No blue/debug support renderer may remain visible in normal mode.");
        Assert.IsFalse(bootstrap.LastAdaptiveSupportGridEnabled, "Failed relief-based adaptive support grid must be disabled by default.");
        Assert.IsFalse(bootstrap.LastAdaptiveSupportGridActive, "Adaptive support grid must not be the rollback runtime collision support.");
        Assert.AreEqual(0, bootstrap.LastAdaptiveSupportGridCellCount);
        Assert.AreEqual(0, bootstrap.LastAdaptiveSupportGridColliderCount);
        Assert.AreEqual(0, bootstrap.LastAdaptiveSupportGridVisibleRendererCount);
        Assert.IsTrue(bootstrap.LastSafeGroundEnabled, "Ground cover must create the safe gameplay support surface.");
        Assert.IsTrue(bootstrap.LastGameplayGroundCoverActive, "Visible road-like gameplay ground cover must be active.");
        Assert.Greater(bootstrap.LastGameplayGroundCoverTileCount, 0);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverTileCount, bootstrap.LastGameplayGroundCoverColliderCount);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverTileCount, bootstrap.LastGameplayGroundCoverVisibleRendererCount);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverColliderCount, bootstrap.LastSafeGroundColliderCount);
        Assert.IsFalse(bootstrap.LastGameplayGroundCoverMaterialBlueLike);
        Assert.IsFalse(bootstrap.LastGameplayGroundCoverMaterialMagentaLike);
        Assert.AreEqual(1f, bootstrap.LastGameplayGroundCoverOpacity, 0.001f);
        Assert.IsTrue(bootstrap.LastFallOutPreventionEnabled);
        Assert.AreEqual(0, bootstrap.LastVisibleLargeBlueGroundRendererCount);
        Assert.IsTrue(bootstrap.LastPlayableBoundsValid, "Playable bounds should be resolved for Chuo_BaseMap.");
        Assert.AreEqual(4, bootstrap.LastPlayableAirWallColliderCount, "Invisible north/south/east/west air walls should be created.");
        Assert.AreEqual(0, bootstrap.LastPlayableAirWallVisibleRendererCount, "Air walls must not render in normal player mode.");
        Assert.IsTrue(bootstrap.LastPlayableBounds.ContainsXZ(player.transform.position, 0f), "Player spawn must remain inside playable bounds.");
        Assert.LessOrEqual(Mathf.Abs(player.transform.position.y - bootstrap.LastGameplayGroundCoverY), 0.35f, "Player must stand on the visible gameplay ground cover.");
        Assert.LessOrEqual(bootstrap.LastPlayerSpawnGroundDelta, 0.35f, "Player spawn should sit near the aligned support surface.");
        Assert.IsFalse(bootstrap.LastMeshColliderDisableComplete, "Scene MeshCollider shutdown should not run at player startup because it caused the Pre2 spike.");
        Assert.AreEqual(0, bootstrap.LastDisabledSceneMeshColliderCount, "Scene MeshColliders should remain untouched during player startup.");
    }

    [UnityTest]
    public IEnumerator RuntimeMouseDragLookRequiresButtonAndRestoresAfterPause()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);

        controller.StartTourismMode();
        yield return null;
        Assert.IsTrue(player.ControlEnabled);
        Assert.IsTrue(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);
        Assert.IsFalse(player.IsMouseLookDragging);
        Assert.IsTrue(player.LookRequiresMouseButton);
        CollectionAssert.AreEquivalent(new[] { "LeftMouse", "RightMouse" }, player.AllowedLookMouseButtonNames);
        float yaw = player.CurrentYaw;
        float pitch = player.CurrentPitch;
        Assert.IsFalse(player.ApplyLookInputForDiagnostics(4f, -3f, false));
        Assert.AreEqual(yaw, player.CurrentYaw, 0.001f);
        Assert.AreEqual(pitch, player.CurrentPitch, 0.001f);
        Assert.IsFalse(player.WantsLockedCursor);
        Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
        Assert.IsTrue(Cursor.visible);
        Assert.IsTrue(player.ApplyLookInputForDiagnostics(4f, -3f, "LeftMouse"));
        Assert.AreNotEqual(yaw, player.CurrentYaw);
        Assert.AreNotEqual(pitch, player.CurrentPitch);
        Assert.IsTrue(player.IsMouseLookDragging);
        Assert.IsTrue(player.WantsLockedCursor);
        Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState);
        Assert.IsFalse(Cursor.visible);
        Assert.GreaterOrEqual(player.CurrentPitch, player.MinPitch);
        Assert.LessOrEqual(player.CurrentPitch, player.MaxPitch);
        player.ReleaseLookDragForDiagnostics();
        Assert.IsFalse(player.IsMouseLookDragging);
        Assert.IsFalse(player.WantsLockedCursor);
        Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
        Assert.IsTrue(Cursor.visible);
        yaw = player.CurrentYaw;
        pitch = player.CurrentPitch;
        Assert.IsTrue(player.ApplyLookInputForDiagnostics(4f, -3f, "RightMouse"));
        Assert.AreNotEqual(yaw, player.CurrentYaw);
        Assert.AreNotEqual(pitch, player.CurrentPitch);
        player.ReleaseLookDragForDiagnostics();

        controller.SetPaused(true);
        yield return null;
        Assert.IsFalse(player.ControlEnabled);
        Assert.IsFalse(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);

        controller.SetPaused(false);
        yield return null;
        Assert.IsTrue(player.ControlEnabled);
        Assert.IsTrue(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);

        controller.ResetToStartMenu();
        yield return null;
        Assert.IsFalse(player.ControlEnabled);
        Assert.IsFalse(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);

        controller.StartEvacuationMode();
        yield return null;
        Assert.IsTrue(player.ControlEnabled);
        Assert.IsTrue(player.MouseLookEnabled);
        Assert.IsFalse(player.WantsLockedCursor);
        yaw = player.CurrentYaw;
        Assert.IsTrue(player.ApplyLookInputForDiagnostics(3f, 0f, "LeftMouse"));
        Assert.AreNotEqual(yaw, player.CurrentYaw);
        yaw = player.CurrentYaw;
        Assert.IsTrue(player.ApplyLookInputForDiagnostics(3f, 0f, "RightMouse"));
        Assert.AreNotEqual(yaw, player.CurrentYaw);
    }

    [UnityTest]
    public IEnumerator RuntimeFallOutPreventionRecoversPlayerToSafeGround()
    {
        NewMapRuntimeBootstrap bootstrap = NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(bootstrap);
        Assert.NotNull(player);
        Assert.NotNull(controller);

        controller.StartTourismMode();
        yield return null;
        Vector3 safe = player.SafeRecoveryPosition;
        player.transform.position = new Vector3(safe.x, player.FallRecoveryThresholdY - 20f, safe.z);
        Assert.IsTrue(player.TryRecoverForDiagnostics());

        Assert.AreEqual(1, player.FallRecoveryCount);
        Assert.AreEqual("below_fall_threshold", player.LastFallRecoveryReason);
        Assert.LessOrEqual(Mathf.Abs(player.transform.position.y - safe.y), 0.1f);
        Assert.IsTrue(bootstrap.LastPlayableBounds.ContainsXZ(player.transform.position, 0f));
    }

    [UnityTest]
    public IEnumerator RuntimeSpawnValidationRejectsBuildingOverlapAndUsesPlayableSupport()
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = "bldg_spawn_overlap_rejection_fixture";
        building.transform.position = new Vector3(0f, 1f, 0f);
        building.transform.localScale = new Vector3(30f, 2f, 30f);

        NewMapRuntimeBootstrap bootstrap = NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        Assert.NotNull(bootstrap);
        Assert.NotNull(player);

        yield return null;

        Assert.IsTrue(bootstrap.LastSpawnValidationPassed, "Spawn must pass the road/playable-ground validator before the player is created.");
        Assert.AreEqual(1, bootstrap.LastSpawnAcceptedCount);
        Assert.GreaterOrEqual(bootstrap.LastSpawnAttemptCount, 1);
        Assert.GreaterOrEqual(bootstrap.LastSpawnRejectedInsideBuildingCount, 1, "The map-bounds center fixture should be rejected as inside a building.");
        Assert.GreaterOrEqual(bootstrap.LastBuildingBoundsCacheCount, 1);
        Assert.IsFalse(bootstrap.LastAdaptiveSupportGridActive, "Spawn validation must not use the failed adaptive relief grid in rollback mode.");
        Assert.IsTrue(bootstrap.LastSafeGroundEnabled);
        Assert.GreaterOrEqual(bootstrap.LastNearestBuildingDistance, NewMapSpawnConfig.Default().minDistanceFromBuildingMeters - 0.01f);
        Assert.LessOrEqual(Mathf.Abs(player.transform.position.y - bootstrap.LastRuntimeGroundSurfaceY), 0.5f);
        Bounds buildingBounds = building.GetComponent<Renderer>().bounds;
        Assert.IsFalse(
            player.transform.position.x >= buildingBounds.min.x &&
            player.transform.position.x <= buildingBounds.max.x &&
            player.transform.position.z >= buildingBounds.min.z &&
            player.transform.position.z <= buildingBounds.max.z,
            "Validated spawn may not overlap building renderer bounds in X/Z.");

        Object.DestroyImmediate(building);
    }

    [UnityTest]
    public IEnumerator RuntimeLightingModesAreExplicitAndReversible()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapLightingController lighting = Object.FindObjectOfType<NewMapLightingController>();
        Assert.NotNull(controller);
        Assert.NotNull(lighting);
        Assert.IsTrue(lighting.ClearDayConfigured);
        Assert.Greater(lighting.DirectionalLightIntensity, 1.0f);
        float clearAmbient = lighting.AmbientSkyBrightness;
        float clearSky = lighting.SkyBrightness;

        controller.SetWeather(NewMapWeatherPreset.NightClear);
        yield return null;
        Assert.Less(lighting.DirectionalLightIntensity, 0.5f);
        Assert.Less(lighting.AmbientSkyBrightness, clearAmbient);
        Assert.Less(lighting.SkyBrightness, 0.12f);
        Assert.Less(lighting.SkyBrightness, clearSky);
        Assert.Greater(lighting.FillLightIntensity, 0.1f);
        Assert.Greater(lighting.NightBuildingReadabilityScore, 0.35f);

        controller.SetWeather(NewMapWeatherPreset.ClearDay);
        yield return null;
        Assert.Greater(lighting.DirectionalLightIntensity, 1.0f);
        Assert.GreaterOrEqual(lighting.AmbientSkyBrightness, clearAmbient - 0.01f);
        Assert.GreaterOrEqual(lighting.SkyBrightness, clearSky - 0.01f);
    }

    [UnityTest]
    public IEnumerator RuntimeProductionModeHidesDebugLocalTrainingTargets()
    {
        NewMapRuntimeBootstrap.EnableLocalTrainingProxyTargetsForDiagnostics = false;
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(controller);

        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.Id.StartsWith("newmap_proxy_")), "Manual production mode must not show local training proxy targets.");
        GameObject diagnosticsRoot = Object.FindObjectsOfType<GameObject>(true).FirstOrDefault(candidate => candidate.name == "DebugDiagnosticsRoot");
        Assert.NotNull(diagnosticsRoot);
        Assert.IsFalse(diagnosticsRoot.activeSelf, "DebugDiagnosticsRoot is inactive by default for manual/player mode.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator RuntimePlayerSupportsSustainedMovementSimulation()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);

        controller.StartTourismMode();
        Vector3 start = player.transform.position;
        for (int i = 0; i < 600; i++)
        {
            Vector3 direction = i < 300 ? Vector3.forward : Vector3.right;
            player.MoveForDiagnostics(direction, 0.1f, sprint: i % 2 == 0);
            if (i % 60 == 0)
            {
                yield return null;
            }
        }
        yield return null;

        Assert.Greater(Vector3.Distance(start, player.transform.position), 10f);
        Assert.AreEqual(0, player.FallRecoveryCount, "60-second diagnostic movement route should not trigger fall recovery.");
        Assert.Greater(player.transform.position.y, start.y - 8f);
    }

    [UnityTest]
    public IEnumerator RuntimeNpcDistributionUsesConfiguredWideDeterministicSpread()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapNpcCrowdPrototype crowd = Object.FindObjectOfType<NewMapNpcCrowdPrototype>();
        NewMapRuntimeBootstrap bootstrap = Object.FindObjectOfType<NewMapRuntimeBootstrap>();
        Assert.NotNull(player);
        Assert.NotNull(controller);
        Assert.NotNull(crowd);
        Assert.NotNull(bootstrap);

        controller.StartTourismMode();
        yield return null;

        Assert.AreEqual(800, crowd.RequestedNpcCount, "NPC requested count should be 100x the baseline count of 8 before cap.");
        Assert.LessOrEqual(crowd.SpawnedNpcCount, 800);
        Assert.AreEqual(crowd.CappedNpcCount, crowd.SpawnedNpcCount);
        Assert.AreEqual(800, crowd.CappedNpcCount);
        Assert.GreaterOrEqual(crowd.UsedSectorCount, 24);
        Assert.GreaterOrEqual(crowd.UsedRingCount, 5);
        Assert.IsTrue(crowd.AvoidBuildingsEnabled);
        Assert.IsTrue(crowd.UsePoolingEnabled);
        Assert.IsFalse(crowd.FarNpcStaticProxyModeEnabled, "Continuous movement validation disables static far proxies so NPCs do not silently freeze.");

        Vector3[] positions = crowd.GetNpcPositionsForDiagnostics();
        Assert.AreEqual(crowd.SpawnedNpcCount, positions.Length);
        foreach (Vector3 position in positions)
        {
            Vector3 delta = position - player.transform.position;
            delta.y = 0f;
            Assert.LessOrEqual(delta.magnitude, 1000.5f);
            Assert.GreaterOrEqual(delta.magnitude, crowd.MinDistanceFromPlayerMeters - 0.5f);
            Assert.GreaterOrEqual(position.y, bootstrap.LastRuntimeGroundSurfaceY - 0.75f);
            Assert.LessOrEqual(position.y, bootstrap.LastRuntimeGroundSurfaceY + 1.5f);
            Assert.IsTrue(crowd.RuntimePlayableBounds.ContainsXZ(position, 0f), "NPC positions must stay inside the playable air-wall bounds.");
        }

        Vector3[] deterministicA = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(player.transform.position, NewMapNpcDistributionConfig.Default());
        Vector3[] deterministicB = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(player.transform.position, NewMapNpcDistributionConfig.Default());
        Assert.AreEqual(deterministicA.Length, deterministicB.Length);
        for (int i = 0; i < deterministicA.Length; i += 17)
        {
            Assert.AreEqual(deterministicA[i].x, deterministicB[i].x, 0.001f);
            Assert.AreEqual(deterministicA[i].z, deterministicB[i].z, 0.001f);
        }

        for (int i = 0; i < 20; i++)
        {
            yield return null;
        }

        Assert.AreEqual(0, crowd.StoppedWithoutReasonCount, "NPCs may be moving, arrived, queued, or recovering, but not silently stopped.");
        Assert.GreaterOrEqual(crowd.MovingCount + crowd.ArrivedCount + crowd.QueuedCount + crowd.StuckCount + crowd.StaticProxyCount, 1);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        NewMapRuntimeTarget crowdTarget = controller.RuntimeTargets.First(target => target.Id == "newmap_proxy_crowd_delay");
        float delay = crowd.GetDelayForTarget(crowdTarget);
        Assert.GreaterOrEqual(delay, 0f);
        Assert.LessOrEqual(delay, 8f);
    }

    [UnityTest]
    public IEnumerator RuntimeGroundRaiseKeepsBuildingsFixedAndRaisesCover()
    {
        GameObject floatingBuilding = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floatingBuilding.name = "bldg_snapdown_fixture_root";
        floatingBuilding.transform.position = new Vector3(45f, 3f, 45f);
        floatingBuilding.transform.localScale = new Vector3(8f, 2f, 8f);

        GameObject nonBuilding = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nonBuilding.name = "road_snapdown_nonbuilding_fixture";
        nonBuilding.transform.position = new Vector3(70f, 3f, 45f);
        nonBuilding.transform.localScale = new Vector3(8f, 2f, 8f);

        NewMapRuntimeBootstrap bootstrap = NewMapRuntimeBootstrap.CreateForCurrentScene();
        yield return null;

        Assert.NotNull(bootstrap);
        Assert.IsTrue(bootstrap.LastGroundCoverRaiseEnabled);
        Assert.Greater(bootstrap.LastGroundCoverRaiseOffset, 0.1f);
        Assert.Greater(bootstrap.LastGameplayGroundCoverY, bootstrap.LastGroundCoverRaiseOldY);
        Assert.IsFalse(bootstrap.LastBuildingSnapdownEnabled, "Ground raise pass keeps imported buildings fixed and disables runtime snapdown.");
        Assert.AreEqual(0, bootstrap.LastBuildingSnapdownMovedCount);
        Assert.AreEqual(3f, floatingBuilding.transform.position.y, 0.01f, "Buildings must remain fixed as visual reference in this pass.");
        Assert.AreEqual(3f, nonBuilding.transform.position.y, 0.01f, "Road/ground-like non-building objects must not move.");
    }

    [UnityTest]
    public IEnumerator RuntimeNameLabelsUseOfflineRealSourcesOnly()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapNameLabelController labels = Object.FindObjectOfType<NewMapNameLabelController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(labels);
        Assert.NotNull(controller);

        yield return null;

        Assert.IsFalse(labels.RuntimeNetworkRequestsAllowed, "Runtime labels must not perform online geocoding/name lookup.");
        Assert.IsFalse(labels.SceneWideMetadataScanPerformed, "Runtime labels should not full-scan the PLATEAU scene every frame.");
        Assert.IsFalse(labels.IdOnlyLabelsVisibleInNormalMode, "ID-only labels stay debug-only.");
        Assert.IsTrue(labels.NameCacheLoaded, "Runtime labels must load the generated local cache.");
        Assert.GreaterOrEqual(labels.NameCacheRecordCount, 100);
        Assert.GreaterOrEqual(labels.ReliableCacheLabelCount, 50);
        Assert.Greater(labels.BuildingNameLabelCount, 0, "Reliable cache-backed building names should be shown.");
        Assert.Greater(labels.RoadNameLabelCount, 0, "Reliable cache-backed road names should be shown.");
        Assert.AreEqual(0, labels.IdOnlyLabelCount, "ID-only labels must stay hidden in normal mode.");
        Assert.AreEqual(0, labels.LowConfidenceHiddenCount, "Low-confidence names should be omitted before runtime display.");
        StringAssert.Contains("source_or_cached_names_available", labels.SourceNameAvailabilityStatus);
        Assert.Greater(labels.NonOfficialCandidateLabelCount, 0, "Existing non-official candidate dataset names should be label sources.");
        Assert.LessOrEqual(labels.ActiveLabelCount, NewMapNameLabelConfig.Default().maxVisibleLabels);
        Assert.IsTrue(controller.RuntimeTargets.Any(target => target.NonOfficialWarningRequired), "Label test expects non-official warning targets to remain active.");
    }

    [UnityTest]
    public IEnumerator RuntimeSafeGroundAlignsPlayerNpcsTargetsAndGuidance()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapNpcCrowdPrototype crowd = Object.FindObjectOfType<NewMapNpcCrowdPrototype>();
        NewMapRuntimeBootstrap bootstrap = Object.FindObjectOfType<NewMapRuntimeBootstrap>();
        Assert.NotNull(player);
        Assert.NotNull(controller);
        Assert.NotNull(crowd);
        Assert.NotNull(bootstrap);

        Assert.IsFalse(bootstrap.LastAdaptiveSupportGridActive);
        Assert.IsTrue(bootstrap.LastSafeGroundEnabled);
        Assert.IsTrue(bootstrap.LastGameplayGroundCoverActive);
        Assert.Greater(bootstrap.LastGameplayGroundCoverTileCount, 0);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverTileCount, bootstrap.LastGameplayGroundCoverColliderCount);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverTileCount, bootstrap.LastGameplayGroundCoverVisibleRendererCount);
        Assert.AreEqual(bootstrap.LastGameplayGroundCoverColliderCount, bootstrap.LastSafeGroundColliderCount);
        Assert.AreEqual(0, bootstrap.LastAdaptiveSupportGridVisibleRendererCount);
        Assert.GreaterOrEqual(player.transform.position.y, bootstrap.LastRuntimeGroundSurfaceY - 0.75f);
        Assert.LessOrEqual(player.transform.position.y, bootstrap.LastRuntimeGroundSurfaceY + 1.5f);
        Assert.IsTrue(bootstrap.LastPlayableBounds.ContainsXZ(player.transform.position, 0f));

        controller.StartEvacuationMode();
        yield return null;
        Vector3[] npcPositions = crowd.GetNpcPositionsForDiagnostics();
        Assert.Greater(npcPositions.Length, 0);
        foreach (Vector3 position in npcPositions.Take(20))
        {
            Assert.GreaterOrEqual(position.y, bootstrap.LastRuntimeGroundSurfaceY - 0.75f);
            Assert.LessOrEqual(position.y, bootstrap.LastRuntimeGroundSurfaceY + 1.5f);
            Assert.IsTrue(crowd.RuntimePlayableBounds.ContainsXZ(position, 0f));
        }

        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;

        int activeTargetChecks = 0;
        int greenFrameChecks = 0;
        foreach (NewMapRuntimeTarget target in controller.RuntimeTargets.Where(target => target != null && target.ActiveInGame))
        {
            Assert.NotNull(target.Anchor);
            Assert.GreaterOrEqual(target.Anchor.position.y, bootstrap.LastRuntimeGroundSurfaceY - 0.75f);
            Assert.LessOrEqual(target.Anchor.position.y, bootstrap.LastRuntimeGroundSurfaceY + 1.5f);
            activeTargetChecks++;

            if (target.GreenFrame != null && target.GreenFrame.activeSelf)
            {
                Assert.LessOrEqual(Mathf.Abs(target.GreenFrame.transform.position.y - (target.Anchor.position.y + 0.06f)), 0.25f);
                greenFrameChecks++;
            }
        }

        Assert.Greater(activeTargetChecks, 0);
        Assert.Greater(greenFrameChecks, 0, "Stage 2 should show green frames aligned to target/local support height.");
        Assert.AreEqual(0, bootstrap.LastActiveTargetHeightOffsetViolations, "Active official/candidate target anchors must remain aligned to the gameplay ground cover.");
        Assert.AreEqual(0, bootstrap.LastPlayableAirWallVisibleRendererCount);
        Assert.AreEqual(4, bootstrap.LastPlayableAirWallColliderCount);
    }

    [UnityTest]
    public IEnumerator RuntimeModesApplySpeedAndStaminaRules()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        Assert.NotNull(player);
        Assert.NotNull(controller);

        controller.SetWeather(NewMapWeatherPreset.ClearDay);
        controller.StartTourismMode();
        yield return null;
        Assert.AreEqual(2.0f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(10.0f, player.SprintSpeedMetersPerSecond, 0.001f);
        Assert.IsFalse(player.StaminaEnabled);

        controller.StartEvacuationMode();
        yield return null;
        Assert.AreEqual(1.0f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(5.0f, player.SprintSpeedMetersPerSecond, 0.001f);
        Assert.IsTrue(player.StaminaEnabled);

        controller.SetWeather(NewMapWeatherPreset.NightRain);
        yield return null;
        Assert.AreEqual(0.65f, player.WalkSpeedMetersPerSecond, 0.001f);
        Assert.AreEqual(3.25f, player.SprintSpeedMetersPerSecond, 0.001f);
    }

    [UnityTest]
    public IEnumerator RuntimeUiPauseRulesAndStageGuidanceWork()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);
        Assert.IsTrue(ui.IsStartMenuVisible);

        ui.ToggleRules();
        yield return null;
        Assert.IsTrue(ui.IsRulesVisible);
        Assert.IsTrue(ui.RulesPanelHasScrollRect);

        controller.StartEvacuationMode();
        yield return null;
        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf));
        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.RouteGuide != null && target.RouteGuide.activeSelf));
        NewMapHazardController hazard = Object.FindObjectOfType<NewMapHazardController>();
        Assert.NotNull(hazard);
        Assert.IsFalse(hazard.Stage2VisualsBuiltForDiagnostics, "Stage 1 Warning should not build the light curtain/debris visual set at startup.");

        controller.SetPaused(true);
        yield return null;
        Assert.IsTrue(controller.IsPaused);
        Assert.IsTrue(ui.IsPauseVisible);
        ui.ResumeRequested?.Invoke();
        yield return null;
        Assert.IsFalse(controller.IsPaused);
        Assert.IsFalse(ui.IsPauseVisible);

        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf));
        Assert.IsTrue(controller.RuntimeTargets.Any(target => target.RouteGuide != null && target.RouteGuide.activeSelf));
        Assert.IsTrue(hazard.Stage2VisualsBuiltForDiagnostics);
        Assert.IsTrue(hazard.LightCurtainVisibleForDiagnostics);

        controller.StartTourismMode();
        yield return null;
        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf));
        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.RouteGuide != null && target.RouteGuide.activeSelf));
    }

    [UnityTest]
    public IEnumerator RuntimeProxyInteractionsProduceExpectedResultCodes()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);

        controller.StartTourismMode();
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_blocked_entrance"));
        Assert.AreEqual("Tourism inspection", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_blocked_entrance"));
        Assert.AreEqual("entrance_blocked", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_no_safe_floor"));
        Assert.AreEqual("safe_floor_unavailable", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_safe_floor"));
        Assert.AreEqual("Entering shelter proxy", ui.LastResultReason);
        yield return new WaitForSeconds(14f);
        Assert.AreEqual("safe_floor_reached", ui.LastResultReason);
    }

    [UnityTest]
    public IEnumerator RuntimeRecoveredNonOfficialCandidatesRemainWarningOnly()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);

        NewMapRuntimeTarget recovered = controller.RuntimeTargets.FirstOrDefault(target => target.Id == "p8_plateau_highrise_candidate_001");
        Assert.NotNull(recovered, "Recovered P8/P9 non-official candidate should be loaded from the runtime candidate cache.");
        Assert.IsFalse(recovered.IsOfficialShelter);
        Assert.IsTrue(recovered.NonOfficialWarningRequired);
        Assert.IsFalse(recovered.SafeApprovedByDefault);
        StringAssert.Contains("humanitarian_candidate", recovered.Category);
        Assert.GreaterOrEqual(controller.RuntimeTargets.Count(target => !target.IsOfficialShelter), 82);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.NotNull(recovered.GreenFrame, "Recovered active candidate should lazily create a green frame at Stage 2.");
        Assert.IsTrue(recovered.GreenFrame.activeSelf);

        Assert.IsTrue(controller.TryInteractForDiagnostics(recovered.Id));
        Assert.AreEqual("Entering shelter proxy", ui.LastResultReason);
        StringAssert.Contains("Non-official humanitarian candidate", ui.LastResultDetail);
        StringAssert.Contains("not a safety approval", ui.LastResultDetail);
        StringAssert.Contains("No official evacuation route is claimed", ui.LastResultDetail);
    }

    [UnityTest]
    public IEnumerator RuntimeGameplaySelfAuditFlowsAreReachable()
    {
        GameObject officialAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        officialAnchor.name = "bldg_25d370de-2c35-457b-b756-3444a3d02eb3";
        officialAnchor.transform.position = new Vector3(24f, 3f, 18f);
        officialAnchor.transform.localScale = new Vector3(4f, 6f, 4f);

        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapHazardController hazard = Object.FindObjectOfType<NewMapHazardController>();
        NewMapNpcCrowdPrototype crowd = Object.FindObjectOfType<NewMapNpcCrowdPrototype>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);
        Assert.NotNull(player);
        Assert.NotNull(hazard);
        Assert.NotNull(crowd);
        Assert.IsTrue(ui.IsStartMenuVisible);
        Assert.IsTrue(player.HasActiveCamera);
        Assert.NotNull(player.transform.Find("PlayerVisual"));
        Assert.NotNull(GameObject.Find("TourismButton"));
        Assert.NotNull(GameObject.Find("EvacuationButton"));
        Assert.NotNull(GameObject.Find("EnglishButton"));
        Assert.NotNull(GameObject.Find("JapaneseButton"));
        Assert.NotNull(Object.FindObjectsOfType<Transform>(true).FirstOrDefault(transform => transform.name == "ForceQuitButton"));

        NewMapRuntimeTarget officialTarget = controller.RuntimeTargets.FirstOrDefault(target => target.Id == "chuo_official_emergency_001");
        NewMapRuntimeTarget nonOfficialTarget = controller.RuntimeTargets.FirstOrDefault(target => target.Id == "p8_plateau_highrise_candidate_001");
        NewMapRuntimeTarget routeTarget = controller.RuntimeTargets.FirstOrDefault(target => target.Id == "newmap_proxy_safe_floor");
        Assert.NotNull(officialTarget);
        Assert.NotNull(nonOfficialTarget);
        Assert.NotNull(routeTarget);
        Assert.AreEqual(1, controller.RuntimeTargets.Count(target => target.IsOfficialShelter));
        Assert.GreaterOrEqual(controller.RuntimeTargets.Count(target => !target.IsOfficialShelter), 82);

        controller.StartTourismMode();
        yield return null;
        Assert.AreEqual(NewMapGameMode.Tourism, controller.Mode);
        Assert.AreEqual(NewMapTsunamiStage.Inactive, controller.Stage);
        Assert.IsFalse(player.StaminaEnabled);
        Assert.IsFalse(hazard.RiskChecksActive);
        Assert.AreEqual(0f, crowd.CurrentCongestionDelaySeconds, 0.001f);
        Assert.IsTrue(controller.TryInteractForDiagnostics(nonOfficialTarget.Id));
        Assert.AreEqual("Tourism inspection", ui.LastResultReason);
        StringAssert.Contains("Non-official candidate", ui.LastResultDetail);
        StringAssert.Contains("not a safety approval", ui.LastResultDetail);

        controller.StartEvacuationMode();
        yield return null;
        Assert.AreEqual(NewMapTsunamiStage.Warning, controller.Stage);
        Assert.IsTrue(player.StaminaEnabled);
        Assert.Greater(crowd.ActiveNpcCount, 0, "Evacuation Mode should lazily build visible NPC humanoids.");
        Assert.IsFalse(hazard.LightCurtainVisibleForDiagnostics);
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(hazard.RiskChecksActive);
        Assert.IsTrue(hazard.LightCurtainVisibleForDiagnostics);
        Assert.IsTrue(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf));

        Assert.IsTrue(controller.TryInteractForDiagnostics(officialTarget.Id));
        string officialEntryDetail = ui.LastResultDetail;
        Assert.IsTrue(controller.CompleteSafeFloorSequenceForDiagnostics());
        Assert.AreEqual("safe_floor_reached", ui.LastResultReason);
        StringAssert.Contains("Official Chuo shelter anchor", officialEntryDetail);
        StringAssert.Contains("no official route is claimed", officialEntryDetail);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics(nonOfficialTarget.Id));
        string nonOfficialEntryDetail = ui.LastResultDetail;
        Assert.IsTrue(controller.CompleteSafeFloorSequenceForDiagnostics());
        Assert.AreEqual("safe_floor_reached", ui.LastResultReason);
        StringAssert.Contains("Non-official humanitarian candidate", nonOfficialEntryDetail);
        StringAssert.Contains("not a safety approval", nonOfficialEntryDetail);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics(routeTarget.Id));
        StringAssert.Contains("estimated prototype guidance", ui.LastResultDetail);
        StringAssert.Contains("not an official evacuation route", ui.LastResultDetail);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_blocked_entrance"));
        Assert.AreEqual("entrance_blocked", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_no_safe_floor"));
        Assert.AreEqual("safe_floor_unavailable", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        player.transform.position = hazard.DebrisCenterForDiagnostics;
        Assert.IsTrue(controller.TryApplyDebrisExposureForDiagnostics(5f));
        Assert.AreEqual("collapse_debris_exposure", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryApplyTsunamiFrontForDiagnostics(player.transform.position + new Vector3(-200f, 0f, 0f)));
        Assert.AreEqual("tsunami_front_contact", ui.LastResultReason);

        Assert.IsFalse(controller.TryInteractForDiagnostics("disabled_out_of_new_map_candidate_probe"));
    }

    [UnityTest]
    public IEnumerator RuntimeP9HardeningScenariosProduceRequiredOutcomes()
    {
        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        NewMapPlayerController player = Object.FindObjectOfType<NewMapPlayerController>();
        NewMapHazardController hazard = Object.FindObjectOfType<NewMapHazardController>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);
        Assert.NotNull(player);
        Assert.NotNull(hazard);

        controller.StartTourismMode();
        yield return null;
        Assert.AreEqual(NewMapTsunamiStage.Inactive, controller.Stage, "tourism_free_roam_no_failure keeps tsunami inactive.");
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_safe_floor"));
        Assert.AreEqual("Tourism inspection", ui.LastResultReason);

        controller.StartEvacuationMode();
        yield return null;
        Assert.AreEqual(NewMapTsunamiStage.Warning, controller.Stage, "warning_before_front starts in Stage 1 Warning.");
        Assert.IsFalse(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf));

        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.RuntimeTargets.Any(target => target.GreenFrame != null && target.GreenFrame.activeSelf), "green_frame_after_front shows guidance only in Stage 2.");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_crowd_delay"), "crowd_delay_scenario uses the dedicated active target.");
        StringAssert.Contains("Crowd delay:", ui.LastResultDetail);
        StringAssert.DoesNotContain("Crowd delay: 0.0s", ui.LastResultDetail);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_blocked_entrance"));
        Assert.AreEqual("entrance_blocked", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_no_safe_floor"));
        Assert.AreEqual("safe_floor_unavailable", ui.LastResultReason);

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        player.transform.position = hazard.DebrisCenterForDiagnostics;
        Assert.IsTrue(controller.TryApplyDebrisExposureForDiagnostics(5f));
        Assert.AreEqual("collapse_debris_exposure", ui.LastResultReason);

        controller.StartTourismMode();
        Assert.IsTrue(controller.TryInteractForDiagnostics("newmap_proxy_safe_floor"));
        Assert.AreEqual("Tourism inspection", ui.LastResultReason);
        player.transform.position = hazard.DebrisCenterForDiagnostics;
        yield return new WaitForSeconds(1f);
        Assert.AreEqual("Tourism inspection", ui.LastResultReason, "collapse_disabled_success keeps tourism mode free of debris failure.");

        Assert.IsFalse(controller.RuntimeTargets.Any(target => target != null && !target.ActiveInGame), "disabled_targets_not_spawned keeps inactive records out of runtime targets.");
    }

    [UnityTest]
    public IEnumerator RuntimeOfficialShelterRequiresVerifiedGmlAnchor()
    {
        GameObject officialAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        officialAnchor.name = "bldg_25d370de-2c35-457b-b756-3444a3d02eb3";
        officialAnchor.transform.position = new Vector3(24f, 3f, 18f);
        officialAnchor.transform.localScale = new Vector3(4f, 6f, 4f);

        NewMapRuntimeBootstrap.CreateForCurrentScene();
        NewMapGameController controller = Object.FindObjectOfType<NewMapGameController>();
        NewMapRuntimeUI ui = Object.FindObjectOfType<NewMapRuntimeUI>();
        Assert.NotNull(controller);
        Assert.NotNull(ui);

        NewMapRuntimeTarget officialTarget = controller.RuntimeTargets.FirstOrDefault(target => target.Id == "chuo_official_emergency_001");
        Assert.NotNull(officialTarget);
        Assert.IsTrue(officialTarget.IsOfficialShelter);
        Assert.IsFalse(officialTarget.NonOfficialWarningRequired);
        Assert.IsNull(officialTarget.RouteGuide, "Official shelter activation must not create an official-looking route line.");

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;

        Assert.IsTrue(controller.TryInteractForDiagnostics("chuo_official_emergency_001"));
        Assert.AreEqual("Entering official shelter anchor", ui.LastResultReason);
        StringAssert.Contains("no official route is claimed", ui.LastResultDetail);
    }
}
