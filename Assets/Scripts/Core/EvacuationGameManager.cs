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

    public GameState CurrentState => currentState;
    public bool IsGameplayActive => currentState == GameState.Playing || currentState == GameState.Climbing;
    public bool IsClimbing => currentState == GameState.Climbing;

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
        if (currentState == GameState.PreEvent && Input.GetKeyDown(eventStartKey))
        {
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
        gameStartTime = 0f;
        selectedShelter = null;
        activeShelterEntrance = null;
        lastFailureReason = string.Empty;

        Debug.Log("Game starts in PreEvent state. Press T to start the tsunami warning event.");
        SetPlayerControlEnabled(true, currentState);
        gameUIManager?.ShowWarning(preEventMessage);
        gameUIManager?.HideClimbProgress();
        gameUIManager?.SetCountdownWaiting();
        resultPanelController?.Hide();

        countdownManager?.PrepareWaiting();
        tsunamiWall?.ResetToStart();
        tsunamiWall?.StopMovement();
    }

    public void StartEvacuationEvent()
    {
        if (currentState != GameState.PreEvent)
        {
            return;
        }

        currentState = GameState.Playing;
        gameStartTime = Time.time;

        Debug.Log("Tsunami warning event starts.");
        SetPlayerControlEnabled(true, currentState);
        gameUIManager?.ShowWarning(startMessage);
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

        selectedShelter = shelter;
        activeShelterEntrance = shelterEntrance;

        if (!shelter.CanUse(out string failureReason))
        {
            TriggerFailure(failureReason);
            return;
        }

        currentState = GameState.Climbing;
        Debug.Log($"Climb starts at shelter: {shelter.ShelterName}.");
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

        TriggerFailure(string.IsNullOrWhiteSpace(reason) ? "Tsunami risk reached the player." : reason);
    }

    public void ReportShelterEntranceReachedByRisk(ShelterEntranceTrigger shelterEntrance)
    {
        if (currentState != GameState.Climbing || shelterEntrance == null || shelterEntrance != activeShelterEntrance)
        {
            return;
        }

        Debug.Log("Failure triggered during climb because risk reached shelter.");
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
        Debug.Log("Climb completes.");
        Debug.Log("Success triggered.");
        SetPlayerControlEnabled(false, currentState);
        countdownManager?.StopCountdown();
        tsunamiWall?.StopMovement();
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.HideClimbProgress();

        resultPanelController?.ShowSuccess(
            GetShelterName(),
            GetElapsedTime(),
            "Reached a safe floor before the risk boundary arrived.");
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
        Debug.Log($"Failure triggered: {lastFailureReason}");

        SetPlayerControlEnabled(false, currentState);
        countdownManager?.StopCountdown();
        tsunamiWall?.StopMovement();
        climbSimulation?.CancelClimb();
        gameUIManager?.HideInteractionPrompt();
        gameUIManager?.HideClimbProgress();

        resultPanelController?.ShowFailure(GetShelterName(), GetElapsedTime(), lastFailureReason);
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
