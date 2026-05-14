using System.Collections.Generic;
using UnityEngine;

public class EvacuationGameManager : MonoBehaviour
{
    public enum GameState
    {
        WaitingToStart,
        PreEvent,
        Playing,
        Climbing,
        Succeeded,
        Failed
    }

    [Header("Scene References")]
    [SerializeField] private SimplePlayerController playerController;
    [SerializeField] private TsunamiCountdownManager countdownManager;
    [SerializeField] private MovingTsunamiWall tsunamiWall;
    [SerializeField] private ClimbSimulation climbSimulation;
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private ResultPanelController resultPanelController;

    [Header("Startup")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private KeyCode eventStartKey = KeyCode.T;
    [SerializeField] private string preEventMessage = "Move: WASD / Arrow Keys\nSprint: Left or Right Shift\nCamera: Mouse drag or Q/E\nStart tsunami test: T\nReach green shelter and press E";
    [SerializeField] private string startMessage = "Tsunami warning issued. Find a usable evacuation building.";

    private GameState currentState = GameState.WaitingToStart;
    private BuildingShelter selectedShelter;
    private ShelterEntranceTrigger activeShelterEntrance;
    private float gameStartTime;
    private string lastFailureReason;
    private GameConfigLoader.TsunamiEventConfig tsunamiEventConfig;
    private GameConfigLoader.AntiCampingConfig antiCampingConfig;
    private ScenarioPresetLoader.ActiveScenario activeScenario;
    private ResultMetrics resultMetrics;
    private bool randomStartScheduled;
    private float randomStartTime;
    private readonly HashSet<string> campedShelterIds = new HashSet<string>();
    private ShelterEntranceTrigger campingTrackedEntrance;
    private float campingTrackedSeconds;

    public GameState CurrentState => currentState;
    public bool IsGameplayActive => currentState == GameState.Playing || currentState == GameState.Climbing;
    public bool IsClimbing => currentState == GameState.Climbing;
    public ShelterEntranceTrigger ActiveShelterEntrance => activeShelterEntrance;
    public string ActiveScenarioId => activeScenario != null ? activeScenario.activeScenarioId : ScenarioPresetLoader.DefaultScenarioId;

    private void Awake()
    {
        SetPlayerControlEnabled(false, GameState.WaitingToStart);
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.HideClimbProgress();
        resultPanelController?.Hide();
    }

    private void Start()
    {
        if (startOnAwake)
        {
            StartGame();
        }
    }

    private void Update()
    {
        if (currentState != GameState.PreEvent)
        {
            return;
        }

        if (Input.GetKeyDown(eventStartKey))
        {
            if (tsunamiEventConfig == null || tsunamiEventConfig.manualStartEnabled)
            {
                StartEvacuationEvent();
            }
            else
            {
                Debug.LogWarning("Manual tsunami start was ignored because manualStartEnabled is false in tsunami_event_config.json.");
            }
        }

        if (randomStartScheduled && Time.time >= randomStartTime)
        {
            Debug.Log("Random tsunami warning start triggered by tsunami_event_config.json.");
            StartEvacuationEvent();
        }
    }

    public void StartGame()
    {
        if (currentState != GameState.WaitingToStart)
        {
            return;
        }

        currentState = GameState.PreEvent;
        LoadMilestoneConfig();
        gameStartTime = 0f;
        selectedShelter = null;
        activeShelterEntrance = null;
        lastFailureReason = string.Empty;
        resultMetrics = CreateBaseResultMetrics();
        randomStartScheduled = false;
        campingTrackedEntrance = null;
        campingTrackedSeconds = 0f;
        campedShelterIds.Clear();

        Debug.Log(GetStartupLogMessage());
        SetPlayerControlEnabled(true, currentState);
        gameUIManager?.ShowWarning(preEventMessage);
        gameUIManager?.HideClimbProgress();
        gameUIManager?.SetCountdownWaiting();
        resultPanelController?.Hide();

        countdownManager?.PrepareWaiting();
        tsunamiWall?.ResetToStart();
        tsunamiWall?.StopMovement();
        ScheduleRandomStartIfEnabled();
    }

    public void StartEvacuationEvent()
    {
        if (currentState != GameState.PreEvent)
        {
            return;
        }

        currentState = GameState.Playing;
        gameStartTime = Time.time;
        randomStartScheduled = false;
        campingTrackedEntrance = null;
        campingTrackedSeconds = 0f;

        if (resultMetrics == null)
        {
            resultMetrics = CreateBaseResultMetrics();
        }

        resultMetrics.warningStartTime = Time.time;
        resultMetrics.evacuationCountdownSeconds = GetEvacuationCountdownSeconds();

        Debug.Log("Tsunami warning event starts.");
        SetPlayerControlEnabled(true, currentState);
        gameUIManager?.ShowWarning(GetWarningMessage());
        countdownManager?.StartCountdown();
        tsunamiWall?.StartMovement();
    }

    public void HandleCountdownExpired()
    {
        TriggerFailure("Time ran out before reaching safe shelter.");
    }

    public void HandlePlayerEnteredRiskZone(string reason)
    {
        ReportPlayerReachedByRisk(string.IsNullOrWhiteSpace(reason) ? "Tsunami risk reached the player." : reason);
    }

    public void TryEnterShelter(BuildingShelter shelter)
    {
        TryEnterShelter(shelter, null);
    }

    public void TryEnterShelter(BuildingShelter shelter, ShelterEntranceTrigger shelterEntrance)
    {
        if (currentState == GameState.PreEvent)
        {
            Debug.Log("Shelter entry ignored before tsunami warning. Press T to start the evacuation event first.");
            return;
        }

        if (currentState != GameState.Playing)
        {
            return;
        }

        if (shelter == null)
        {
            TriggerFailure("Shelter data was missing.");
            return;
        }

        ApplyShelterConfig(shelter);
        selectedShelter = shelter;
        activeShelterEntrance = shelterEntrance;
        CaptureShelterMetrics(shelter);

        if (IsShelterBlockedByCamping(shelter))
        {
            if (resultMetrics != null)
            {
                resultMetrics.wasShelterBlockedByCampingRule = true;
            }

            Debug.LogWarning($"Anti-camping blocked shelter '{shelter.ShelterId}' for this round.");
            TriggerFailure("This shelter was blocked because the player camped near it before the tsunami warning.");
            return;
        }

        if (!shelter.CanUse(out string failureReason))
        {
            TriggerFailure(failureReason);
            return;
        }

        currentState = GameState.Climbing;
        Debug.Log($"Climb starts at shelter: {shelter.ShelterName}.");
        if (resultMetrics != null)
        {
            resultMetrics.climbStartTime = Time.time;
        }

        SetPlayerControlEnabled(false, currentState);
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.ShowWarning("Entering shelter. Climb to a safe floor.");

        if (climbSimulation == null)
        {
            TriggerFailure("Climb simulation is not assigned.");
            return;
        }

        climbSimulation.StartClimb(shelter, shelterEntrance, this);
    }

    public void ReportPlayerReachedByRisk(string reason)
    {
        if (currentState == GameState.WaitingToStart || currentState == GameState.PreEvent)
        {
            Debug.Log("Risk contact ignored before tsunami warning event starts.");
            return;
        }

        if (currentState == GameState.Succeeded || currentState == GameState.Failed)
        {
            return;
        }

        if (resultMetrics != null)
        {
            resultMetrics.tsunamiArrivalTime = Time.time;
        }

        TriggerFailure(string.IsNullOrWhiteSpace(reason) ? "Tsunami risk reached the player." : reason);
    }

    public void ReportShelterEntranceReachedByRisk(ShelterEntranceTrigger shelterEntrance)
    {
        if (currentState != GameState.Climbing || shelterEntrance == null || shelterEntrance != activeShelterEntrance)
        {
            return;
        }

        Debug.Log("Failure triggered during climb because risk reached shelter.");
        if (resultMetrics != null)
        {
            resultMetrics.tsunamiArrivalTime = Time.time;
        }

        TriggerFailure("The tsunami risk reached the shelter entrance before you reached a safe floor.");
    }

    public bool IsActiveShelterEntrance(ShelterEntranceTrigger shelterEntrance)
    {
        return currentState == GameState.Climbing && shelterEntrance != null && shelterEntrance == activeShelterEntrance;
    }

    public void HandleClimbProgress(float normalizedProgress)
    {
        gameUIManager?.SetClimbProgress(normalizedProgress);
    }

    public void HandleClimbCompleted(BuildingShelter shelter)
    {
        if (currentState != GameState.Climbing)
        {
            return;
        }

        selectedShelter = shelter != null ? shelter : selectedShelter;
        currentState = GameState.Succeeded;
        activeShelterEntrance = null;
        CaptureShelterMetrics(selectedShelter);
        if (resultMetrics != null)
        {
            resultMetrics.success = true;
            resultMetrics.failureReason = "Reached a safe floor before the risk boundary arrived.";
            resultMetrics.climbCompleteTime = Time.time;
            resultMetrics.resultTime = Time.time;
        }

        Debug.Log("Climb completes.");
        Debug.Log("Success triggered.");
        SetPlayerControlEnabled(false, currentState);
        countdownManager?.StopCountdown();
        tsunamiWall?.StopMovement();
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.HideClimbProgress();

        if (resultMetrics != null)
        {
            resultPanelController?.Show(resultMetrics);
        }
        else
        {
            resultPanelController?.ShowSuccess(
                GetShelterName(),
                GetElapsedTime(),
                "Reached a safe floor before the risk boundary arrived.");
        }
    }

    public void TriggerFailure(string reason)
    {
        if (currentState == GameState.Failed || currentState == GameState.Succeeded)
        {
            return;
        }

        currentState = GameState.Failed;
        lastFailureReason = string.IsNullOrWhiteSpace(reason) ? "Evacuation failed." : reason;
        activeShelterEntrance = null;
        CaptureShelterMetrics(selectedShelter);
        if (resultMetrics != null)
        {
            resultMetrics.success = false;
            resultMetrics.failureReason = lastFailureReason;
            resultMetrics.resultTime = Time.time;
        }

        Debug.Log($"Failure triggered: {lastFailureReason}");

        SetPlayerControlEnabled(false, currentState);
        countdownManager?.StopCountdown();
        tsunamiWall?.StopMovement();
        climbSimulation?.CancelClimb();
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.HideClimbProgress();

        if (resultMetrics != null)
        {
            resultPanelController?.Show(resultMetrics);
        }
        else
        {
            resultPanelController?.ShowFailure(GetShelterName(), GetElapsedTime(), lastFailureReason);
        }
    }

    public void ApplyShelterConfig(BuildingShelter shelter)
    {
        if (shelter == null)
        {
            return;
        }

        ShelterDataLoader.ShelterData shelterData = ShelterDataLoader.GetShelterOrDefault(shelter.ShelterId);
        shelter.ApplyShelterData(shelterData);
    }

    public void UpdateShelterEntranceProximity(ShelterEntranceTrigger shelterEntrance, float deltaTime)
    {
        if (currentState != GameState.PreEvent ||
            antiCampingConfig == null ||
            !antiCampingConfig.antiCampingEnabled ||
            shelterEntrance == null)
        {
            return;
        }

        BuildingShelter shelter = shelterEntrance.Shelter;
        if (shelter == null)
        {
            return;
        }

        ApplyShelterConfig(shelter);

        if (campingTrackedEntrance != shelterEntrance)
        {
            campingTrackedEntrance = shelterEntrance;
            campingTrackedSeconds = 0f;
        }

        campingTrackedSeconds += Mathf.Max(0f, deltaTime);

        if (campingTrackedSeconds < antiCampingConfig.preWarningCampingThresholdSeconds)
        {
            return;
        }

        if (campedShelterIds.Add(shelter.ShelterId))
        {
            if (resultMetrics != null)
            {
                resultMetrics.wasCampingDetected = true;
            }

            Debug.LogWarning(
                $"Anti-camping detected pre-warning camping near shelter '{shelter.ShelterId}' for " +
                $"{campingTrackedSeconds:0.#} seconds.");
        }
    }

    public void HandleShelterEntranceExit(ShelterEntranceTrigger shelterEntrance)
    {
        if (campingTrackedEntrance == shelterEntrance)
        {
            campingTrackedEntrance = null;
            campingTrackedSeconds = 0f;
        }
    }

    private void LoadMilestoneConfig()
    {
        tsunamiEventConfig = GameConfigLoader.LoadTsunamiEventConfig();
        if (tsunamiEventConfig == null)
        {
            Debug.LogWarning("Tsunami event config failed to load. Using hard-coded safe defaults.");
            tsunamiEventConfig = CreateDefaultTsunamiEventConfig();
        }

        antiCampingConfig = GameConfigLoader.LoadAntiCampingConfig();
        if (antiCampingConfig == null)
        {
            Debug.LogWarning("Anti-camping config failed to load. Using hard-coded safe defaults.");
            antiCampingConfig = GameConfigLoader.CreateDefaultAntiCampingConfig();
        }

        activeScenario = ScenarioPresetLoader.LoadActiveScenario();
        ScenarioPresetLoader.ApplyScenarioOverrides(activeScenario, tsunamiEventConfig, antiCampingConfig);
        ShelterDataLoader.SetRuntimeOverrides(
            ScenarioPresetLoader.GetShelterOverrides(activeScenario),
            ActiveScenarioId);

        countdownManager?.SetCountdownSeconds(tsunamiEventConfig.evacuationCountdownSeconds);
        tsunamiWall?.SetDurationSeconds(tsunamiEventConfig.wallMoveDurationSeconds);

        Debug.Log(
            $"Active scenario: {ActiveScenarioId} - " +
            $"{(activeScenario != null ? activeScenario.displayName : "Default")}.");
    }

    private static GameConfigLoader.TsunamiEventConfig CreateDefaultTsunamiEventConfig()
    {
        return GameConfigLoader.CreateDefaultTsunamiEventConfig();
    }

    private void ScheduleRandomStartIfEnabled()
    {
        if (tsunamiEventConfig == null || !tsunamiEventConfig.randomStartEnabled)
        {
            return;
        }

        float delay = UnityEngine.Random.Range(
            tsunamiEventConfig.randomStartMinSeconds,
            tsunamiEventConfig.randomStartMaxSeconds);
        randomStartTime = Time.time + delay;
        randomStartScheduled = true;
        Debug.Log($"Random tsunami warning start scheduled in {delay:0.#} seconds.");
    }

    private string GetStartupLogMessage()
    {
        bool manualEnabled = tsunamiEventConfig == null || tsunamiEventConfig.manualStartEnabled;
        bool randomEnabled = tsunamiEventConfig != null && tsunamiEventConfig.randomStartEnabled;

        if (manualEnabled && randomEnabled)
        {
            return $"Game starts in PreEvent state. Press {eventStartKey} to start the tsunami warning event, or wait for the configured random warning.";
        }

        if (manualEnabled)
        {
            return $"Game starts in PreEvent state. Press {eventStartKey} to start the tsunami warning event.";
        }

        if (randomEnabled)
        {
            return "Game starts in PreEvent state. Waiting for the configured random tsunami warning event.";
        }

        return "Game starts in PreEvent state. No tsunami warning start mode is enabled in config.";
    }

    private string GetWarningMessage()
    {
        if (tsunamiEventConfig != null && !string.IsNullOrWhiteSpace(tsunamiEventConfig.warningMessage))
        {
            return tsunamiEventConfig.warningMessage;
        }

        return startMessage;
    }

    private float GetEvacuationCountdownSeconds()
    {
        return tsunamiEventConfig != null ? tsunamiEventConfig.evacuationCountdownSeconds : 60f;
    }

    private bool IsShelterBlockedByCamping(BuildingShelter shelter)
    {
        if (shelter == null ||
            antiCampingConfig == null ||
            !antiCampingConfig.antiCampingEnabled ||
            !antiCampingConfig.blockCampedShelterForRound)
        {
            return false;
        }

        return campedShelterIds.Contains(shelter.ShelterId);
    }

    private ResultMetrics CreateBaseResultMetrics()
    {
        return new ResultMetrics
        {
            evacuationCountdownSeconds = GetEvacuationCountdownSeconds(),
            activeScenarioId = ActiveScenarioId,
            activeScenarioName = activeScenario != null ? activeScenario.displayName : "Default"
        };
    }

    private void CaptureShelterMetrics(BuildingShelter shelter)
    {
        if (resultMetrics == null || shelter == null)
        {
            return;
        }

        resultMetrics.selectedShelterId = shelter.ShelterId;
        resultMetrics.selectedShelterName = shelter.ShelterName;
        resultMetrics.shelterRank = shelter.ShelterRank;
        resultMetrics.isOfficialShelter = shelter.IsOfficialShelter;
        resultMetrics.shelterEntryTime = resultMetrics.shelterEntryTime <= 0f ? Time.time : resultMetrics.shelterEntryTime;
        resultMetrics.entryDelaySeconds = shelter.EntryDelaySeconds;
        resultMetrics.climbTimeSeconds = shelter.ClimbTimeSeconds;
        resultMetrics.crowdingDelaySeconds = shelter.CrowdingDelaySeconds;
        resultMetrics.wasCampingDetected = resultMetrics.wasCampingDetected || campedShelterIds.Contains(shelter.ShelterId);
    }

    private void SetPlayerControlEnabled(bool isEnabled, GameState reasonState)
    {
        if (playerController != null)
        {
            playerController.SetControlEnabled(isEnabled, reasonState.ToString());
        }
    }

    private string GetShelterName()
    {
        return selectedShelter != null ? selectedShelter.ShelterName : "No shelter selected";
    }

    private float GetElapsedTime()
    {
        return gameStartTime > 0f ? Time.time - gameStartTime : 0f;
    }
}
