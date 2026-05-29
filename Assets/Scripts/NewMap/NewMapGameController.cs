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
    private NewMapRuntimeTarget touchedBuildingTarget;
    private NewMapRuntimeTarget activeSequenceTarget;
    private System.Action<NewMapPlayerController> respawnPlayerForRun;
    private NewMapTsunamiModeHotfixConfig tsunamiConfig;
    private bool paused;
    private bool resultLocked;
    private bool safeFloorSequenceActive;
    private bool skipNextResetRespawn;
    private float modeElapsedSeconds;
    private float safeFloorRemainingSeconds;
    private string diagnostics = string.Empty;
    private float warningStartedAtSeconds = -1f;
    private float activeTsunamiStartedAtSeconds = -1f;
    private float lastFailureAtSeconds = -1f;
    private string lastFailureCode = string.Empty;

    public NewMapGameMode Mode => mode;
    public NewMapTsunamiStage Stage => stage;
    public NewMapWeatherPreset Weather => weather;
    public bool IsPaused => paused;
    public int ActiveTargetCount => targets.Count;
    public bool SafeFloorSequenceActive => safeFloorSequenceActive;
    public float WarningPhaseSeconds => stage1WarningSeconds;
    public float WarningStartedAtSeconds => warningStartedAtSeconds;
    public float ActiveTsunamiStartedAtSeconds => activeTsunamiStartedAtSeconds;
    public float LastFailureAtSeconds => lastFailureAtSeconds;
    public string LastFailureCode => lastFailureCode;
    public NewMapRuntimeTarget CurrentEnterableBuilding => touchedBuildingTarget != null && touchedBuildingTarget.ActiveInGame ? touchedBuildingTarget : null;
    public IEnumerable<NewMapRuntimeTarget> RuntimeTargets => targets;

    public void Configure(
        NewMapPlayerController playerController,
        NewMapRuntimeUI runtimeUi,
        NewMapLightingController lightingController,
        NewMapHazardController hazardController,
        NewMapNpcCrowdPrototype crowdPrototype,
        IEnumerable<NewMapRuntimeTarget> runtimeTargets,
        string startupDiagnostics,
        System.Action<NewMapPlayerController> respawnHandler = null,
        NewMapTsunamiModeHotfixConfig tsunamiHotfixConfig = null)
    {
        player = playerController;
        ui = runtimeUi;
        lighting = lightingController;
        hazard = hazardController;
        crowd = crowdPrototype;
        diagnostics = startupDiagnostics ?? string.Empty;
        respawnPlayerForRun = respawnHandler;
        tsunamiConfig = tsunamiHotfixConfig ?? NewMapTsunamiModeHotfixConfig.Load();
        stage1WarningSeconds = tsunamiConfig.WarningPhaseSeconds;
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
        skipNextResetRespawn = true;
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
        touchedBuildingTarget = null;
        warningStartedAtSeconds = -1f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
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
        touchedBuildingTarget = null;
        activeSequenceTarget = null;
        warningStartedAtSeconds = 0f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
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
        Debug.Log($"NewMap Evacuation Mode started. Stage 1 warning is active for {stage1WarningSeconds:0.0}s; light curtain and hazard checks are hidden/ignored.");
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
        if (skipNextResetRespawn)
        {
            skipNextResetRespawn = false;
        }
        else
        {
            RespawnPlayerForNewRun("reset_to_start_menu");
        }
        mode = NewMapGameMode.None;
        stage = NewMapTsunamiStage.Inactive;
        modeElapsedSeconds = 0f;
        paused = false;
        resultLocked = false;
        safeFloorSequenceActive = false;
        activeSequenceTarget = null;
        touchedBuildingTarget = null;
        nearestTarget = null;
        warningStartedAtSeconds = -1f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
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

    private void RespawnPlayerForNewRun(string reason)
    {
        if (player == null || respawnPlayerForRun == null)
        {
            return;
        }

        respawnPlayerForRun(player);
        Debug.Log($"NewMap player respawned for {reason} position={player.transform.position}");
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
        activeTsunamiStartedAtSeconds = modeElapsedSeconds;
        hazard?.SetStage(stage);
        SetTargetGuidanceVisible(true);
        Debug.Log($"NewMap tsunami Stage 2 FrontApproaching started at t={modeElapsedSeconds:0.0}s. Light curtain is visible and hazard checks are active.");
    }

    private void UpdateInteraction()
    {
        RefreshTouchedBuildingFromOverlap();
        nearestTarget = FindNearestTarget();
        ui?.ShowInteraction(CurrentEnterableBuilding, mode);

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

    private void RefreshTouchedBuildingFromOverlap()
    {
        if (player == null)
        {
            return;
        }

        CharacterController character = player.GetComponent<CharacterController>();
        Bounds playerBounds = character != null
            ? character.bounds
            : new Bounds(player.transform.position + Vector3.up * 0.9f, new Vector3(0.7f, 1.8f, 0.7f));
        if (touchedBuildingTarget != null &&
            touchedBuildingTarget.ActiveInGame &&
            touchedBuildingTarget.EntryTrigger != null &&
            touchedBuildingTarget.EntryTrigger.Bounds.Intersects(playerBounds))
        {
            return;
        }

        touchedBuildingTarget = null;
        for (int i = 0; i < targets.Count; i++)
        {
            NewMapRuntimeTarget target = targets[i];
            if (target == null || !target.ActiveInGame || target.EntryTrigger == null)
            {
                continue;
            }

            if (target.EntryTrigger.Bounds.Intersects(playerBounds))
            {
                touchedBuildingTarget = target;
                return;
            }
        }
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
        NewMapRuntimeTarget target = CurrentEnterableBuilding;
        if (target == null)
        {
            nearestTarget = FindNearestTarget();
            target = CurrentEnterableBuilding;
        }

        if (target == null || resultLocked || safeFloorSequenceActive || mode == NewMapGameMode.None)
        {
            Debug.Log("NewMap building entry rejected reason=no_current_enterable_building");
            return false;
        }

        Debug.Log($"NewMap E pressed for building id={target.Id} name={target.DisplayName} official={target.IsOfficialShelter}");
        TryInteract(target);
        return true;
    }

    public bool TryInteractWithTouchedBuildingForDiagnostics()
    {
        return TryInteractWithNearestTargetFromInput();
    }

    public bool TrySetTouchedBuildingForDiagnostics(string targetId, bool touching)
    {
        NewMapRuntimeTarget target = targets.Find(candidate => candidate != null && candidate.Id == targetId && candidate.ActiveInGame);
        if (target == null)
        {
            return false;
        }

        NotifyBuildingEntryTouch(target, touching);
        return true;
    }

    public void NotifyBuildingEntryTouch(NewMapRuntimeTarget target, bool touching)
    {
        if (target == null || !target.ActiveInGame)
        {
            return;
        }

        if (touching)
        {
            touchedBuildingTarget = target;
            return;
        }

        if (touchedBuildingTarget == target)
        {
            touchedBuildingTarget = null;
        }
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
        if (forcedStage == NewMapTsunamiStage.Warning)
        {
            warningStartedAtSeconds = Mathf.Max(0f, modeElapsedSeconds);
            activeTsunamiStartedAtSeconds = -1f;
        }
        else if (forcedStage == NewMapTsunamiStage.FrontApproaching && activeTsunamiStartedAtSeconds < 0f)
        {
            activeTsunamiStartedAtSeconds = Mathf.Max(0f, modeElapsedSeconds);
        }

        hazard?.SetStage(forcedStage);
        SetTargetGuidanceVisible(mode == NewMapGameMode.Evacuation && forcedStage == NewMapTsunamiStage.FrontApproaching);
    }

    public void AdvanceEvacuationTimeForDiagnostics(float seconds)
    {
        if (mode != NewMapGameMode.Evacuation)
        {
            return;
        }

        modeElapsedSeconds += Mathf.Max(0f, seconds);
        UpdateEvacuationStages();
        hazard?.Tick(Mathf.Max(0f, seconds));
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
        touchedBuildingTarget = null;
        lastFailureCode = code ?? string.Empty;
        lastFailureAtSeconds = modeElapsedSeconds;
        player?.SetControlEnabled(false);
        string stageNote = activeTsunamiStartedAtSeconds >= 0f
            ? $"Failure after active tsunami start at t={activeTsunamiStartedAtSeconds:0.0}s."
            : "Failure happened before active tsunami start.";
        ui?.ShowResult(false, code, $"{reason}\n{stageNote}\nUse Retry / Restart to return to the start menu.");
        Debug.Log($"NewMap failure outcome: {code} - {reason} failureTime={lastFailureAtSeconds:0.0}s activeStart={activeTsunamiStartedAtSeconds:0.0}s");
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
        if (mode == NewMapGameMode.Evacuation && stage == NewMapTsunamiStage.Warning)
        {
            builder.AppendLine($"Warning phase: {Mathf.Max(0f, stage1WarningSeconds - modeElapsedSeconds):0.0}s until tsunami start");
        }
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
