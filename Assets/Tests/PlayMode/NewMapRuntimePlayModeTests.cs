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
        Assert.IsTrue(bootstrap.LastRuntimeCollisionSupportProxyActive, "Final NewMap manual test uses the documented runtime collision support proxy.");
        Assert.IsFalse(bootstrap.LastRuntimeCollisionSupportRendererVisible, "Manual-test support proxy must be invisible.");
        Assert.LessOrEqual(Mathf.Abs(player.transform.position.y - bootstrap.LastRuntimeGroundSurfaceY), 0.35f, "Player/support surface must align with the visible map ground height tolerance.");
        Assert.LessOrEqual(bootstrap.LastSupportToVisualGroundDelta, 0.35f, "Round-2 support surface should align to the sampled visual building/ground base.");
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
        Assert.GreaterOrEqual(bootstrap.LastNearestBuildingDistance, NewMapSpawnConfig.Default().minDistanceFromBuildingMeters - 0.01f);
        Assert.LessOrEqual(Mathf.Abs(player.transform.position.y - bootstrap.LastRuntimeGroundSurfaceY), 0.35f);
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
        Assert.NotNull(player);
        Assert.NotNull(controller);
        Assert.NotNull(crowd);

        controller.StartTourismMode();
        yield return null;

        Assert.AreEqual(160, crowd.RequestedNpcCount, "NPC requested count should be 20x the previous base count of 8 before cap.");
        Assert.LessOrEqual(crowd.SpawnedNpcCount, 300);
        Assert.AreEqual(crowd.CappedNpcCount, crowd.SpawnedNpcCount);
        Assert.GreaterOrEqual(crowd.UsedSectorCount, 12);
        Assert.GreaterOrEqual(crowd.UsedRingCount, 4);

        Vector3[] positions = crowd.GetNpcPositionsForDiagnostics();
        Assert.AreEqual(crowd.SpawnedNpcCount, positions.Length);
        foreach (Vector3 position in positions)
        {
            Vector3 delta = position - player.transform.position;
            delta.y = 0f;
            Assert.LessOrEqual(delta.magnitude, 1000.5f);
            Assert.GreaterOrEqual(delta.magnitude, crowd.MinDistanceFromPlayerMeters - 0.5f);
            Assert.Greater(position.y, -5f);
            Assert.Less(position.y, 8f);
        }

        Vector3[] deterministicA = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(player.transform.position, NewMapNpcDistributionConfig.Default());
        Vector3[] deterministicB = NewMapNpcCrowdPrototype.GenerateDistributionForDiagnostics(player.transform.position, NewMapNpcDistributionConfig.Default());
        Assert.AreEqual(deterministicA.Length, deterministicB.Length);
        for (int i = 0; i < deterministicA.Length; i += 17)
        {
            Assert.AreEqual(deterministicA[i].x, deterministicB[i].x, 0.001f);
            Assert.AreEqual(deterministicA[i].z, deterministicB[i].z, 0.001f);
        }

        controller.StartEvacuationMode();
        controller.ForceStageForDiagnostics(NewMapTsunamiStage.FrontApproaching);
        yield return null;
        NewMapRuntimeTarget crowdTarget = controller.RuntimeTargets.First(target => target.Id == "newmap_proxy_crowd_delay");
        float delay = crowd.GetDelayForTarget(crowdTarget);
        Assert.GreaterOrEqual(delay, 0f);
        Assert.LessOrEqual(delay, 8f);
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
