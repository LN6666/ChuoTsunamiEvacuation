using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class NewMapRuntimePlayModeTests
{
    [TearDown]
    public void TearDown()
    {
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
        Assert.AreEqual(0, controller.RuntimeTargets.Count(target => target.IsOfficialShelter), "Official shelters must not be active without a verified scene GML anchor.");

        controller.StartTourismMode();
        float startY = player.transform.position.y;
        yield return new WaitForSeconds(2f);
        Assert.Greater(player.transform.position.y, startY - 8f, "Player should not fall endlessly through the map/support proxy.");
        Assert.AreEqual(0, player.FallRecoveryCount, "Normal spawn grounding should not need fall recovery.");
        Assert.IsTrue(bootstrap.LastRuntimeCollisionSupportProxyActive, "Final NewMap manual test uses the documented runtime collision support proxy.");
        Assert.IsFalse(bootstrap.LastMeshColliderDisableComplete, "Scene MeshCollider shutdown should not run at player startup because it caused the Pre2 spike.");
        Assert.AreEqual(0, bootstrap.LastDisabledSceneMeshColliderCount, "Scene MeshColliders should remain untouched during player startup.");
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
