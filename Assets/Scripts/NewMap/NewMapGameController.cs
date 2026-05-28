using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class NewMapGameController : MonoBehaviour
{
    [SerializeField] private float stage1WarningSeconds = 12f;

    private readonly List<NewMapRuntimeTarget> targets = new List<NewMapRuntimeTarget>();
    private NewMapPlayerController player;
    private NewMapRuntimeUI ui;
    private NewMapLightingController lighting;
    private NewMapHazardController hazard;
    private NewMapNpcCrowdPrototype crowd;
    private NewMapGameMode mode = NewMapGameMode.None;
    private NewMapWeatherPreset weather = NewMapWeatherPreset.ClearDay;
    private NewMapTsunamiStage stage = NewMapTsunamiStage.Inactive;
    private NewMapRuntimeTarget nearestTarget;
    private NewMapRuntimeTarget activeSequenceTarget;
    private bool paused;
    private bool resultLocked;
    private bool safeFloorSequenceActive;
    private float modeElapsedSeconds;
    private float safeFloorRemainingSeconds;
    private string diagnostics = string.Empty;

    public NewMapGameMode Mode => mode;
    public NewMapTsunamiStage Stage => stage;
    public NewMapWeatherPreset Weather => weather;
    public bool IsPaused => paused;
    public int ActiveTargetCount => targets.Count;
    public bool SafeFloorSequenceActive => safeFloorSequenceActive;
    public IEnumerable<NewMapRuntimeTarget> RuntimeTargets => targets;

    public void Configure(
        NewMapPlayerController playerController,
        NewMapRuntimeUI runtimeUi,
        NewMapLightingController lightingController,
        NewMapHazardController hazardController,
        NewMapNpcCrowdPrototype crowdPrototype,
        IEnumerable<NewMapRuntimeTarget> runtimeTargets,
        string startupDiagnostics)
    {
        player = playerController;
        ui = runtimeUi;
        lighting = lightingController;
        hazard = hazardController;
        crowd = crowdPrototype;
        diagnostics = startupDiagnostics ?? string.Empty;
        targets.Clear();
        if (runtimeTargets != null)
        {
            targets.AddRange(runtimeTargets);
        }

        ui.TourismRequested += StartTourismMode;
        ui.EvacuationRequested += StartEvacuationMode;
        ui.ResumeRequested += () => SetPaused(false);
        ui.ResetRequested += ResetToStartMenu;
        ui.ForceQuitRequested += ForceQuit;
        ui.WeatherRequested += SetWeather;
        ResetToStartMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && mode != NewMapGameMode.None)
        {
            SetPaused(!paused);
        }

        if (paused || mode == NewMapGameMode.None)
        {
            return;
        }

        modeElapsedSeconds += Time.deltaTime;

        if (mode == NewMapGameMode.Evacuation)
        {
            UpdateEvacuationStages();
            hazard?.Tick(Time.deltaTime);
            if (!resultLocked && player != null && hazard != null)
            {
                if (hazard.IsPlayerReachedByFront(player.transform.position))
                {
                    Fail("tsunami_front_contact", "The Stage 2 risk front reached the player.");
                    return;
                }

                if (hazard.IsPlayerInDebrisExposure(player.transform.position, Time.deltaTime, out string debrisReason))
                {
                    Fail("collapse_debris_exposure", debrisReason);
                    return;
                }
            }
        }

        UpdateInteraction();
        UpdateSafeFloorSequence();
        UpdateHud();
    }

    public void StartTourismMode()
    {
        mode = NewMapGameMode.Tourism;
        stage = NewMapTsunamiStage.Inactive;
        modeElapsedSeconds = 0f;
        resultLocked = false;
        safeFloorSequenceActive = false;
        activeSequenceTarget = null;
        safeFloorRemainingSeconds = 0f;
        paused = false;
        player?.SetMode(mode, weather);
        player?.SetControlEnabled(true);
        hazard?.SetStage(NewMapTsunamiStage.Inactive);
        crowd?.SetCrowdFailuresEnabled(false);
        SetTargetGuidanceVisible(false);
        ui?.HideResult();
        ui?.ShowHud();
        Debug.Log("NewMap Tourism Mode started. Hazards, crowd failure, collapse/debris failure, and stamina drain are disabled.");
    }

    public void StartEvacuationMode()
    {
        mode = NewMapGameMode.Evacuation;
        stage = NewMapTsunamiStage.Warning;
        modeElapsedSeconds = 0f;
        resultLocked = false;
        safeFloorSequenceActive = false;
        safeFloorRemainingSeconds = 0f;
        paused = false;
        player?.SetMode(mode, weather);
        player?.ResetStamina();
        player?.SetControlEnabled(true);
        hazard?.SetStage(NewMapTsunamiStage.Warning);
        crowd?.SetCrowdFailuresEnabled(true);
        SetTargetGuidanceVisible(false);
        ui?.HideResult();
        ui?.ShowHud();
        Debug.Log("NewMap Evacuation Mode started. Stage 1 warning is active; light curtain and hazard checks are hidden/ignored.");
    }

    public void SetWeather(NewMapWeatherPreset preset)
    {
        weather = preset;
        lighting?.ApplyWeather(weather);
        player?.SetMode(mode == NewMapGameMode.None ? NewMapGameMode.Tourism : mode, weather);
        UpdateHud();
    }

    public void ResetToStartMenu()
    {
        mode = NewMapGameMode.None;
        stage = NewMapTsunamiStage.Inactive;
        modeElapsedSeconds = 0f;
        paused = false;
        resultLocked = false;
        safeFloorSequenceActive = false;
        activeSequenceTarget = null;
        nearestTarget = null;
        player?.SetMode(NewMapGameMode.Tourism, weather);
        player?.SetControlEnabled(false);
        hazard?.SetStage(NewMapTsunamiStage.Inactive);
        crowd?.SetCrowdFailuresEnabled(false);
        SetTargetGuidanceVisible(false);
        ui?.ShowStartMenu();
    }

    public void SetPaused(bool value)
    {
        paused = value;
        player?.SetControlEnabled(!paused && mode != NewMapGameMode.None && !safeFloorSequenceActive);
        ui?.SetPauseVisible(paused);
    }

    private void ForceQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateEvacuationStages()
    {
        if (stage != NewMapTsunamiStage.Warning || modeElapsedSeconds < stage1WarningSeconds)
        {
            return;
        }

        stage = NewMapTsunamiStage.FrontApproaching;
        hazard?.SetStage(stage);
        SetTargetGuidanceVisible(true);
        Debug.Log("NewMap tsunami Stage 2 FrontApproaching started. Light curtain is visible and hazard checks are active.");
    }

    private void UpdateInteraction()
    {
        nearestTarget = FindNearestTarget();
        ui?.ShowInteraction(nearestTarget, mode);

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        TryInteractWithNearestTargetFromInput();
    }

    private NewMapRuntimeTarget FindNearestTarget()
    {
        if (player == null)
        {
            return null;
        }

        NewMapRuntimeTarget best = null;
        float bestDistance = float.MaxValue;
        foreach (NewMapRuntimeTarget target in targets)
        {
            if (target == null || !target.ActiveInGame)
            {
                continue;
            }

            float distance = Vector3.Distance(player.transform.position, target.Anchor.position);
            if (distance <= target.InteractionDistance && distance < bestDistance)
            {
                best = target;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void TryInteract(NewMapRuntimeTarget target)
    {
        if (mode == NewMapGameMode.Tourism)
        {
            string inspectionNote = target.IsOfficialShelter
                ? "Official shelter anchor verified on Chuo_BaseMap. No official route is claimed."
                : (target.NonOfficialWarningRequired
                    ? "Non-official candidate. This is not a safety approval."
                    : "Runtime training target.");
            ui?.ShowResult(
                true,
                "Tourism inspection",
                $"{target.DisplayName}\n{inspectionNote}\nThis is map exploration mode. No evacuation success/failure is applied.");
            return;
        }

        if (target.EntranceBlocked)
        {
            Fail("entrance_blocked", $"{target.DisplayName}: entrance is blocked in this runtime test condition.");
            return;
        }

        if (!target.SafeFloorAvailable)
        {
            Fail("safe_floor_unavailable", $"{target.DisplayName}: safe-floor proxy reports no usable vertical evacuation path.");
            return;
        }

        float crowdDelay = crowd != null ? crowd.GetDelayForTarget(target) : 0f;
        safeFloorRemainingSeconds = Mathf.Max(0.5f, target.ClimbSeconds + crowdDelay);
        safeFloorSequenceActive = true;
        activeSequenceTarget = target;
        player?.SetControlEnabled(false);
        string reason = target.IsOfficialShelter ? "Entering official shelter anchor" : "Entering shelter proxy";
        string sourceNote = target.IsOfficialShelter
            ? "Official Chuo shelter anchor verified by exact PLATEAU GML object name. Safe-floor timing is a gameplay prototype; no official route is claimed."
            : (target.Category != null && target.Category.Contains("humanitarian_candidate")
                ? "Non-official humanitarian candidate. This is not a safety approval."
                : "Non-official runtime training target. This is not a safety approval.");
        string routeNote = target.RouteGuide != null || target.RouteGuideFactory != null
            ? "Route guidance is estimated prototype guidance only; it is not an official evacuation route."
            : "No official evacuation route is claimed.";
        ui?.ShowResult(
            true,
            reason,
            $"{target.DisplayName}\n{sourceNote}\n{routeNote}\nCrowd delay: {crowdDelay:0.0}s.");
    }

    public bool TryInteractWithNearestTargetFromInput()
    {
        if (nearestTarget == null)
        {
            nearestTarget = FindNearestTarget();
        }

        if (nearestTarget == null || resultLocked || safeFloorSequenceActive || mode == NewMapGameMode.None)
        {
            return false;
        }

        TryInteract(nearestTarget);
        return true;
    }

    public bool TryInteractForDiagnostics(string targetId)
    {
        NewMapRuntimeTarget target = targets.Find(candidate => candidate != null && candidate.Id == targetId && candidate.ActiveInGame);
        if (target == null || resultLocked || safeFloorSequenceActive || mode == NewMapGameMode.None)
        {
            return false;
        }

        TryInteract(target);
        return true;
    }

    public void ForceStageForDiagnostics(NewMapTsunamiStage forcedStage)
    {
        stage = forcedStage;
        hazard?.SetStage(forcedStage);
        SetTargetGuidanceVisible(mode == NewMapGameMode.Evacuation && forcedStage == NewMapTsunamiStage.FrontApproaching);
    }

    public bool TryApplyDebrisExposureForDiagnostics(float exposureSeconds)
    {
        if (mode != NewMapGameMode.Evacuation ||
            stage != NewMapTsunamiStage.FrontApproaching ||
            player == null ||
            hazard == null ||
            resultLocked)
        {
            return false;
        }

        float remaining = Mathf.Max(0f, exposureSeconds);
        string reason = string.Empty;
        while (remaining > 0f)
        {
            float step = Mathf.Min(1f, remaining);
            if (hazard.IsPlayerInDebrisExposure(player.transform.position, step, out reason))
            {
                Fail("collapse_debris_exposure", reason);
                return true;
            }

            remaining -= step;
        }

        return false;
    }

    public bool TryApplyTsunamiFrontForDiagnostics(Vector3 playerPosition)
    {
        if (mode != NewMapGameMode.Evacuation ||
            stage != NewMapTsunamiStage.FrontApproaching ||
            hazard == null ||
            resultLocked)
        {
            return false;
        }

        if (!hazard.IsPlayerReachedByFront(playerPosition))
        {
            return false;
        }

        if (player != null)
        {
            player.transform.position = playerPosition;
        }

        Fail("tsunami_front_contact", "The Stage 2 risk front reached the player.");
        return true;
    }

    public bool CompleteSafeFloorSequenceForDiagnostics()
    {
        if (!safeFloorSequenceActive || resultLocked)
        {
            return false;
        }

        safeFloorRemainingSeconds = 0f;
        UpdateSafeFloorSequence();
        return !safeFloorSequenceActive && resultLocked;
    }

    private void SetTargetGuidanceVisible(bool visible)
    {
        foreach (NewMapRuntimeTarget target in targets)
        {
            if (target == null)
            {
                continue;
            }

            bool activeVisible = visible && target.ActiveInGame;
            if (activeVisible)
            {
                target.EnsureGuidanceVisuals();
            }

            if (target.GreenFrame != null)
            {
                target.GreenFrame.SetActive(activeVisible);
            }

            if (target.RouteGuide != null)
            {
                target.RouteGuide.SetActive(activeVisible);
            }
        }
    }

    private void UpdateSafeFloorSequence()
    {
        if (!safeFloorSequenceActive)
        {
            return;
        }

        safeFloorRemainingSeconds -= Time.deltaTime;
        if (safeFloorRemainingSeconds > 0f)
        {
            return;
        }

        safeFloorSequenceActive = false;
        resultLocked = true;
        player?.SetControlEnabled(false);
        string detail = activeSequenceTarget != null && activeSequenceTarget.IsOfficialShelter
            ? "Reached the verified official shelter anchor before the Stage 2 risk front arrived. Route geometry remains unclaimed because no WGS84-to-Unity transform is proven."
            : "Reached the runtime safe-floor proxy before the Stage 2 risk front arrived.";
        activeSequenceTarget = null;
        ui?.ShowResult(
            true,
            "safe_floor_reached",
            detail);
    }

    private void Fail(string code, string reason)
    {
        resultLocked = true;
        safeFloorSequenceActive = false;
        activeSequenceTarget = null;
        player?.SetControlEnabled(false);
        ui?.ShowResult(false, code, reason);
        Debug.Log($"NewMap failure outcome: {code} - {reason}");
    }

    private void UpdateHud()
    {
        if (ui == null)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine(mode == NewMapGameMode.Tourism ? "Tourism Mode / 観光モード" : "Evacuation Mode / 避難モード");
        builder.AppendLine($"Stage: {stage}");
        builder.AppendLine($"Weather: {NewMapRuntimeConstants.GetWeatherLabel(weather)} x{NewMapRuntimeConstants.GetWeatherModifier(weather):0.00}");
        if (player != null)
        {
            builder.AppendLine($"Walk/Sprint: {player.WalkSpeedMetersPerSecond:0.00} / {player.SprintSpeedMetersPerSecond:0.00} m/s");
            builder.AppendLine(player.StaminaEnabled ? $"Stamina: {player.Stamina:0}" : "Stamina: disabled");
        }

        if (crowd != null)
        {
            builder.AppendLine($"NPCs: {crowd.ActiveNpcCount}/{crowd.NpcCap} | Crowd delay: {crowd.CurrentCongestionDelaySeconds:0.0}s");
        }

        if (safeFloorSequenceActive)
        {
            builder.AppendLine($"Safe-floor proxy: {safeFloorRemainingSeconds:0.0}s remaining");
        }

        if (!string.IsNullOrWhiteSpace(diagnostics))
        {
            builder.AppendLine(diagnostics);
        }

        ui.SetHud(builder.ToString());
    }
}
