using System.Collections.Generic;
using System.IO;
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
    private NewMapShelterDirectLineController shelterDirectLines;
    private NewMapGameMode mode = NewMapGameMode.None;
    private NewMapWeatherPreset weather = NewMapWeatherPreset.ClearDay;
    private NewMapTsunamiStage stage = NewMapTsunamiStage.Inactive;
    private NewMapRuntimeTarget nearestTarget;
    private NewMapRuntimeTarget touchedBuildingTarget;
    private NewMapRuntimeTarget activeSequenceTarget;
    private System.Action<NewMapPlayerController> respawnPlayerForRun;
    private NewMapTsunamiModeHotfixConfig tsunamiConfig;
    private NewMapLeaderboardToggleConfig leaderboardToggleConfig;
    private bool paused;
    private bool resultLocked;
    private bool safeFloorSequenceActive;
    private bool skipNextResetRespawn;
    private float modeElapsedSeconds;
    private float safeFloorRemainingSeconds;
    private string diagnostics = string.Empty;
    private float preWarningStartedAtSeconds = -1f;
    private float preWarningRandomDurationSeconds;
    private float warningStartedAtSeconds = -1f;
    private float activeTsunamiStartedAtSeconds = -1f;
    private float lastFailureAtSeconds = -1f;
    private float rankingRefreshTimer;
    private string lastFailureCode = string.Empty;

    public NewMapGameMode Mode => mode;
    public NewMapTsunamiStage Stage => stage;
    public NewMapWeatherPreset Weather => weather;
    public bool IsPaused => paused;
    public int ActiveTargetCount => targets.Count;
    public bool SafeFloorSequenceActive => safeFloorSequenceActive;
    public float WarningPhaseSeconds => stage1WarningSeconds;
    public float PreWarningStartedAtSeconds => preWarningStartedAtSeconds;
    public float PreWarningRandomDurationSeconds => preWarningRandomDurationSeconds;
    public float PreWarningRandomMaxSeconds => tsunamiConfig != null ? tsunamiConfig.PreWarningRandomMaxSeconds : 180f;
    public float WarningStartedAtSeconds => warningStartedAtSeconds;
    public float ActiveTsunamiStartedAtSeconds => activeTsunamiStartedAtSeconds;
    public float LastFailureAtSeconds => lastFailureAtSeconds;
    public string LastFailureCode => lastFailureCode;
    public int ShelterDirectLineCount => shelterDirectLines != null ? shelterDirectLines.LineCount : 0;
    public string NearestShelterLineTargetId => shelterDirectLines != null ? shelterDirectLines.NearestTargetId : string.Empty;
    public string LastShelterRankingText => shelterDirectLines != null ? shelterDirectLines.LastRankingText : string.Empty;
    public NewMapRuntimeTarget CurrentEnterableBuilding => touchedBuildingTarget != null && touchedBuildingTarget.ActiveInGame ? touchedBuildingTarget : null;
    public IEnumerable<NewMapRuntimeTarget> RuntimeTargets => targets;

    public void Configure(
        NewMapPlayerController playerController,
        NewMapRuntimeUI runtimeUi,
        NewMapLightingController lightingController,
        NewMapHazardController hazardController,
        NewMapNpcCrowdPrototype crowdPrototype,
        NewMapShelterDirectLineController directLineController,
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
        shelterDirectLines = directLineController;
        diagnostics = startupDiagnostics ?? string.Empty;
        respawnPlayerForRun = respawnHandler;
        tsunamiConfig = tsunamiHotfixConfig ?? NewMapTsunamiModeHotfixConfig.Load();
        leaderboardToggleConfig = NewMapLeaderboardToggleConfig.Load();
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
            shelterDirectLines?.Tick(Time.deltaTime, !resultLocked);
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
        else
        {
            shelterDirectLines?.SetVisible(false);
        }

        UpdateShelterRanking();

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
        preWarningStartedAtSeconds = -1f;
        preWarningRandomDurationSeconds = 0f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
        safeFloorRemainingSeconds = 0f;
        paused = false;
        player?.SetMode(mode, weather);
        player?.SetControlEnabled(true);
        hazard?.SetStage(NewMapTsunamiStage.Inactive);
        crowd?.SetCrowdFailuresEnabled(false);
        shelterDirectLines?.SetVisible(false);
        SetTargetGuidanceVisible(false);
        ui?.HideShelterRanking();
        ui?.HideResult();
        ui?.ShowHud();
        Debug.Log("NewMap Tourism Mode started. Hazards, crowd failure, collapse/debris failure, and stamina drain are disabled.");
    }

    public void StartEvacuationMode()
    {
        mode = NewMapGameMode.Evacuation;
        stage = NewMapTsunamiStage.PreWarningWait;
        modeElapsedSeconds = 0f;
        resultLocked = false;
        safeFloorSequenceActive = false;
        touchedBuildingTarget = null;
        activeSequenceTarget = null;
        preWarningStartedAtSeconds = 0f;
        preWarningRandomDurationSeconds = tsunamiConfig != null ? tsunamiConfig.ResolvePreWarningWaitSeconds() : 0f;
        warningStartedAtSeconds = -1f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
        safeFloorRemainingSeconds = 0f;
        rankingRefreshTimer = 0f;
        paused = false;
        player?.SetMode(mode, weather);
        player?.ResetStamina();
        player?.SetControlEnabled(true);
        hazard?.SetStage(NewMapTsunamiStage.PreWarningWait);
        crowd?.SetCrowdFailuresEnabled(true);
        shelterDirectLines?.SetVisible(true);
        shelterDirectLines?.ForceRefresh();
        SetTargetGuidanceVisible(false);
        ui?.HideShelterRanking();
        ui?.HideResult();
        ui?.ShowHud();
        Debug.Log(
            $"NewMap pre_warning_start_time={preWarningStartedAtSeconds:0.0} " +
            $"pre_warning_random_duration_seconds={preWarningRandomDurationSeconds:0.0} " +
            $"pre_warning_random_max_seconds={PreWarningRandomMaxSeconds:0.0} phase=PRE_WARNING_WAIT " +
            "lightCurtainVisible=false hazardChecksActive=false");
        UpdateEvacuationStages();
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
        preWarningStartedAtSeconds = -1f;
        preWarningRandomDurationSeconds = 0f;
        warningStartedAtSeconds = -1f;
        activeTsunamiStartedAtSeconds = -1f;
        lastFailureAtSeconds = -1f;
        lastFailureCode = string.Empty;
        player?.SetMode(NewMapGameMode.Tourism, weather);
        player?.SetControlEnabled(false);
        hazard?.SetStage(NewMapTsunamiStage.Inactive);
        crowd?.SetCrowdFailuresEnabled(false);
        shelterDirectLines?.SetVisible(false);
        SetTargetGuidanceVisible(false);
        ui?.HideShelterRanking();
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
        if (stage == NewMapTsunamiStage.PreWarningWait &&
            modeElapsedSeconds >= preWarningStartedAtSeconds + preWarningRandomDurationSeconds)
        {
            StartWarningPhase(preWarningStartedAtSeconds + preWarningRandomDurationSeconds);
        }

        if (stage != NewMapTsunamiStage.Warning ||
            warningStartedAtSeconds < 0f ||
            modeElapsedSeconds - warningStartedAtSeconds < stage1WarningSeconds)
        {
            return;
        }

        StartActiveTsunamiPhase(warningStartedAtSeconds + stage1WarningSeconds);
    }

    private void StartWarningPhase(float startTimeSeconds)
    {
        stage = NewMapTsunamiStage.Warning;
        warningStartedAtSeconds = Mathf.Max(0f, startTimeSeconds);
        hazard?.SetStage(NewMapTsunamiStage.Warning);
        Debug.Log(
            $"NewMap warning_start_time={warningStartedAtSeconds:0.0} " +
            $"pre_warning_start_time={preWarningStartedAtSeconds:0.0} " +
            $"pre_warning_random_duration_seconds={preWarningRandomDurationSeconds:0.0} " +
            $"warning_duration_seconds={stage1WarningSeconds:0.0} phase=WARNING " +
            "lightCurtainVisible=false hazardChecksActive=false");
    }

    private void StartActiveTsunamiPhase(float startTimeSeconds)
    {
        stage = NewMapTsunamiStage.FrontApproaching;
        activeTsunamiStartedAtSeconds = Mathf.Max(0f, startTimeSeconds);
        hazard?.SetStage(stage);
        SetTargetGuidanceVisible(true);
        string startSide = hazard != null ? hazard.TsunamiStartSide : (tsunamiConfig != null ? tsunamiConfig.NormalizedTsunamiStartSide : "south");
        string direction = hazard != null ? hazard.TsunamiDirection.ToString("F2") : Vector3.forward.ToString("F2");
        Debug.Log(
            $"NewMap tsunami_active_start_time={activeTsunamiStartedAtSeconds:0.0} " +
            $"warning_start_time={warningStartedAtSeconds:0.0} " +
            $"warning_duration_seconds={stage1WarningSeconds:0.0} " +
            $"active_tsunami_start_side={startSide} active_tsunami_direction={direction} " +
            "phase=ACTIVE_TSUNAMI active tsunami state entered lightCurtainVisible=true hazardChecksActive=true");
    }

    private void UpdateShelterRanking()
    {
        if (shelterDirectLines == null || ui == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleShelterRankingPanelFromInput();
            return;
        }

        if (!ui.IsShelterRankingVisible)
        {
            return;
        }

        rankingRefreshTimer -= Time.deltaTime;
        if (rankingRefreshTimer <= 0f)
        {
            RefreshShelterRankingPanel();
        }
    }

    private void RefreshShelterRankingPanel()
    {
        if (shelterDirectLines == null || ui == null)
        {
            return;
        }

        shelterDirectLines.ForceRefresh();
        ui.ShowShelterRanking(shelterDirectLines.LastRankingText);
        rankingRefreshTimer = shelterDirectLines.RankingAutoRefreshIntervalSeconds;
    }

    private void ToggleShelterRankingPanelFromInput()
    {
        if (!CanToggleShelterRankingFromInput())
        {
            return;
        }

        if (ui.IsShelterRankingVisible)
        {
            ui.HideShelterRanking();
            return;
        }

        RefreshShelterRankingPanel();
    }

    private bool CanToggleShelterRankingFromInput()
    {
        if (leaderboardToggleConfig != null && !leaderboardToggleConfig.enabled)
        {
            return false;
        }

        if (ui == null || shelterDirectLines == null || mode == NewMapGameMode.None)
        {
            return false;
        }

        bool hideWhenMenuOpen = leaderboardToggleConfig == null || leaderboardToggleConfig.hideWhenMenuOpen;
        if (hideWhenMenuOpen && ui.IsStartMenuVisible)
        {
            return false;
        }

        if (ui.IsPauseVisible || ui.IsRulesVisible)
        {
            return false;
        }

        bool hideWhenResultOpen = leaderboardToggleConfig == null || leaderboardToggleConfig.hideWhenResultPanelOpen;
        if (hideWhenResultOpen && ui.IsResultVisible)
        {
            return false;
        }

        return true;
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

        Bounds playerBounds = GetPlayerEntryBounds();
        if (touchedBuildingTarget != null &&
            touchedBuildingTarget.ActiveInGame &&
            touchedBuildingTarget.EntryTrigger != null &&
            touchedBuildingTarget.EntryTrigger.OverlapsPlayer(playerBounds, player.transform.position))
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

            if (target.EntryTrigger.OverlapsPlayer(playerBounds, player.transform.position))
            {
                touchedBuildingTarget = target;
                return;
            }
        }
    }

    private Bounds GetPlayerEntryBounds()
    {
        if (player == null)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        CharacterController character = player.GetComponent<CharacterController>();
        return character != null
            ? character.bounds
            : new Bounds(player.transform.position + Vector3.up * 0.9f, new Vector3(0.7f, 1.8f, 0.7f));
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
        RefreshTouchedBuildingFromOverlap();
        NewMapRuntimeTarget target = CurrentEnterableBuilding;

        if (target == null || resultLocked || safeFloorSequenceActive || mode == NewMapGameMode.None)
        {
            ui?.ShowNoEnterableBuildingPrompt(mode);
            string position = player != null ? player.transform.position.ToString("F2") : "missing_player";
            Debug.Log($"NewMap building entry rejected reason=no_current_enterable_building playerPosition={position}");
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

    public bool RefreshTouchedBuildingForDiagnostics()
    {
        RefreshTouchedBuildingFromOverlap();
        return CurrentEnterableBuilding != null;
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
        else if (forcedStage == NewMapTsunamiStage.PreWarningWait)
        {
            preWarningStartedAtSeconds = Mathf.Max(0f, modeElapsedSeconds);
            warningStartedAtSeconds = -1f;
            activeTsunamiStartedAtSeconds = -1f;
        }
        else if (forcedStage == NewMapTsunamiStage.FrontApproaching && activeTsunamiStartedAtSeconds < 0f)
        {
            if (warningStartedAtSeconds < 0f)
            {
                warningStartedAtSeconds = Mathf.Max(0f, modeElapsedSeconds);
            }

            activeTsunamiStartedAtSeconds = Mathf.Max(0f, modeElapsedSeconds);
        }

        hazard?.SetStage(forcedStage);
        SetTargetGuidanceVisible(mode == NewMapGameMode.Evacuation && forcedStage == NewMapTsunamiStage.FrontApproaching);
        shelterDirectLines?.SetVisible(mode == NewMapGameMode.Evacuation);
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

    public void RefreshShelterDirectLinesForDiagnostics()
    {
        shelterDirectLines?.SetVisible(mode == NewMapGameMode.Evacuation);
        shelterDirectLines?.ForceRefresh();
    }

    public bool ShowShelterRankingForDiagnostics()
    {
        if (shelterDirectLines == null || ui == null)
        {
            return false;
        }

        RefreshShelterRankingPanel();
        return ui.IsShelterRankingVisible;
    }

    public bool ToggleShelterRankingForDiagnostics()
    {
        ToggleShelterRankingPanelFromInput();
        return ui != null && ui.IsShelterRankingVisible;
    }

    public void HideShelterRankingForDiagnostics()
    {
        ui?.HideShelterRanking();
    }

    public NewMapShelterLineSnapshot[] GetShelterRankingForDiagnostics()
    {
        return shelterDirectLines != null
            ? shelterDirectLines.GetSortedSnapshots()
            : new NewMapShelterLineSnapshot[0];
    }

    public bool TryGetShelterLineColorForDiagnostics(string targetId, out Color color)
    {
        color = Color.clear;
        return shelterDirectLines != null && shelterDirectLines.TryGetLineColorForDiagnostics(targetId, out color);
    }

    public int CountShelterDirectLineCollidersForDiagnostics()
    {
        return shelterDirectLines != null ? shelterDirectLines.CountLineCollidersForDiagnostics() : 0;
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
        shelterDirectLines?.SetVisible(false);
        ui?.HideShelterRanking();
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
        shelterDirectLines?.SetVisible(false);
        ui?.HideShelterRanking();
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
        if (mode == NewMapGameMode.Evacuation && stage == NewMapTsunamiStage.PreWarningWait)
        {
            builder.AppendLine($"PRE_WARNING_WAIT: warning pending in {Mathf.Max(0f, preWarningStartedAtSeconds + preWarningRandomDurationSeconds - modeElapsedSeconds):0.0}s");
        }
        else if (mode == NewMapGameMode.Evacuation && stage == NewMapTsunamiStage.Warning)
        {
            builder.AppendLine($"TSUNAMI WARNING / PRE-ALERT: {Mathf.Max(0f, stage1WarningSeconds - (modeElapsedSeconds - warningStartedAtSeconds)):0.0}s until tsunami start");
        }
        else if (mode == NewMapGameMode.Evacuation && stage == NewMapTsunamiStage.FrontApproaching)
        {
            builder.AppendLine("ACTIVE TSUNAMI: coastal-side light curtain is advancing");
        }
        if (mode != NewMapGameMode.None && ShelterDirectLineCount > 0)
        {
            builder.AppendLine($"Shelter ranking: {ShelterDirectLineCount} targets | R toggle");
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

[System.Serializable]
public sealed class NewMapLeaderboardToggleConfig
{
    public bool enabled = true;
    public string toggleKey = "R";
    public bool hideWhenMenuOpen = true;
    public bool hideWhenResultPanelOpen = true;
    public bool showOfficialShelterRank = true;
    public bool showNonOfficialCandidateRank = true;
    public bool showPrototypeGuidance = true;
    public bool preserveWarnings = true;

    public static NewMapLeaderboardToggleConfig Default()
    {
        return new NewMapLeaderboardToggleConfig();
    }

    public static NewMapLeaderboardToggleConfig Load()
    {
        NewMapLeaderboardToggleConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_leaderboard_toggle_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapLeaderboardToggleConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap leaderboard toggle config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.toggleKey = string.IsNullOrWhiteSpace(config.toggleKey) ? "R" : config.toggleKey;
        return config;
    }
}
