using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MovingTsunamiWall : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private EvacuationGameManager gameManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float durationSeconds = 180f;
    [SerializeField] private bool resetToStartOnPlay = true;
    [SerializeField] private bool requireManualStartKey;
    [SerializeField] private KeyCode manualStartKey = KeyCode.T;
    [SerializeField] private bool enableRiskFrontCheck = true;
    [SerializeField] private float riskFrontGraceDistance = 0.1f;

    private bool isMoving;
    private bool waitingForManualStart;
    private bool hasReportedPlayer;
    private bool hasReportedActiveShelter;
    private bool hasWarnedMissingGameManager;
    private bool hasWarnedMissingRiskAxis;
    private bool hasWarnedMissingPlayer;
    private float elapsedSeconds;
    private Vector3 fallbackStartPosition;
    private Vector3 fallbackEndPosition;
    private Transform cachedPlayerTransform;

    public float DurationSeconds => durationSeconds;

    private void Awake()
    {
        ConfigureTriggerPhysics();
        fallbackStartPosition = transform.position;
        fallbackEndPosition = transform.position + transform.forward * 100f;
    }

    private void Reset()
    {
        ConfigureTriggerPhysics();
    }

    private void Update()
    {
        if (waitingForManualStart && Input.GetKeyDown(manualStartKey))
        {
            BeginMovement();
        }

        if (!isMoving)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        float duration = Mathf.Max(0.01f, durationSeconds);
        float t = Mathf.Clamp01(elapsedSeconds / duration);
        transform.position = Vector3.Lerp(GetStartPosition(), GetEndPosition(), t);
        CheckRiskFrontFailures();

        if (t >= 1f)
        {
            isMoving = false;
        }
    }

    public void StartMovement()
    {
        elapsedSeconds = 0f;
        ResetContactReports();

        if (resetToStartOnPlay)
        {
            transform.position = GetStartPosition();
        }

        if (requireManualStartKey)
        {
            isMoving = false;
            waitingForManualStart = true;
            Debug.Log($"TsunamiWall is waiting for manual start. Press {manualStartKey} to start the tsunami test.");
            return;
        }

        BeginMovement();
    }

    public void StopMovement()
    {
        isMoving = false;
        waitingForManualStart = false;
    }

    public void ResetToStart()
    {
        elapsedSeconds = 0f;
        isMoving = false;
        waitingForManualStart = false;
        ResetContactReports();
        transform.position = GetStartPosition();
    }

    public void SetDurationSeconds(float seconds)
    {
        durationSeconds = Mathf.Max(0.1f, seconds);
    }

    private void BeginMovement()
    {
        elapsedSeconds = 0f;
        isMoving = true;
        waitingForManualStart = false;
        ResetContactReports();
        hasWarnedMissingRiskAxis = false;
        hasWarnedMissingPlayer = false;
        hasWarnedMissingGameManager = false;

        if (resetToStartOnPlay)
        {
            transform.position = GetStartPosition();
        }

        Debug.Log("TsunamiWall movement started.");
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTriggerContact(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleTriggerContact(other);
    }

    private void HandleTriggerContact(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (hasReportedPlayer)
            {
                return;
            }

            if (gameManager != null &&
                (gameManager.CurrentState == EvacuationGameManager.GameState.Succeeded ||
                 gameManager.CurrentState == EvacuationGameManager.GameState.Failed))
            {
                return;
            }

            hasReportedPlayer = true;
            Debug.Log("TsunamiWall touched Player.");
            gameManager?.ReportPlayerReachedByRisk("The tsunami risk boundary reached the player.");
            return;
        }

        ShelterEntranceTrigger shelterEntrance = other.GetComponentInParent<ShelterEntranceTrigger>();
        if (shelterEntrance != null && gameManager != null && gameManager.IsActiveShelterEntrance(shelterEntrance))
        {
            if (hasReportedActiveShelter)
            {
                return;
            }

            hasReportedActiveShelter = true;
            Debug.Log("TsunamiWall reached active shelter entrance.");
            gameManager.ReportShelterEntranceReachedByRisk(shelterEntrance);
        }
    }

    private void CheckRiskFrontFailures()
    {
        if (!enableRiskFrontCheck || gameManager == null)
        {
            if (enableRiskFrontCheck && gameManager == null && !hasWarnedMissingGameManager)
            {
                hasWarnedMissingGameManager = true;
                Debug.LogWarning("Tsunami risk-front check could not run because GameManager is not assigned.", this);
            }

            return;
        }

        if (gameManager.CurrentState == EvacuationGameManager.GameState.Succeeded ||
            gameManager.CurrentState == EvacuationGameManager.GameState.Failed ||
            gameManager.CurrentState == EvacuationGameManager.GameState.WaitingToStart ||
            gameManager.CurrentState == EvacuationGameManager.GameState.PreEvent)
        {
            return;
        }

        if (!TryGetHorizontalRiskAxis(out Vector3 startPosition, out Vector3 riskDirection))
        {
            return;
        }

        ShelterEntranceTrigger activeShelterEntrance = gameManager.ActiveShelterEntrance;
        if (!hasReportedActiveShelter &&
            gameManager.CurrentState == EvacuationGameManager.GameState.Climbing &&
            activeShelterEntrance != null &&
            IsOnFloodedSide(activeShelterEntrance.transform.position, startPosition, riskDirection))
        {
            hasReportedActiveShelter = true;
            Debug.Log("Tsunami risk front passed active shelter entrance.");
            gameManager.ReportShelterEntranceReachedByRisk(activeShelterEntrance);
            return;
        }

        Transform playerTransform = GetPlayerTransform();
        if (!hasReportedPlayer &&
            playerTransform != null &&
            IsOnFloodedSide(playerTransform.position, startPosition, riskDirection))
        {
            hasReportedPlayer = true;
            Debug.Log("Tsunami risk front passed Player.");
            gameManager.ReportPlayerReachedByRisk("The tsunami risk front passed the player.");
        }
    }

    private bool TryGetHorizontalRiskAxis(out Vector3 startPosition, out Vector3 riskDirection)
    {
        startPosition = FlattenToGround(GetStartPosition());
        Vector3 endPosition = FlattenToGround(GetEndPosition());
        Vector3 movement = endPosition - startPosition;

        if (movement.sqrMagnitude <= 0.0001f)
        {
            riskDirection = Vector3.zero;

            if (!hasWarnedMissingRiskAxis)
            {
                hasWarnedMissingRiskAxis = true;
                Debug.LogWarning("Tsunami risk-front check could not run because start and end positions are missing or identical.", this);
            }

            return false;
        }

        riskDirection = movement.normalized;
        return true;
    }

    private bool IsOnFloodedSide(Vector3 targetPosition, Vector3 startPosition, Vector3 riskDirection)
    {
        float frontDistance = Vector3.Dot(FlattenToGround(transform.position) - startPosition, riskDirection);
        float targetDistance = Vector3.Dot(FlattenToGround(targetPosition) - startPosition, riskDirection);
        return targetDistance <= frontDistance + Mathf.Max(0f, riskFrontGraceDistance);
    }

    private Transform GetPlayerTransform()
    {
        if (cachedPlayerTransform != null)
        {
            return cachedPlayerTransform;
        }

        try
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                cachedPlayerTransform = playerObject.transform;
                return cachedPlayerTransform;
            }
        }
        catch (UnityException exception)
        {
            if (!hasWarnedMissingPlayer)
            {
                hasWarnedMissingPlayer = true;
                Debug.LogWarning($"Tsunami risk-front check could not find player tag '{playerTag}'. {exception.Message}", this);
            }

            return null;
        }

        if (!hasWarnedMissingPlayer)
        {
            hasWarnedMissingPlayer = true;
            Debug.LogWarning($"Tsunami risk-front check could not find a GameObject tagged '{playerTag}'.", this);
        }

        return null;
    }

    private static Vector3 FlattenToGround(Vector3 position)
    {
        position.y = 0f;
        return position;
    }

    private void ResetContactReports()
    {
        hasReportedPlayer = false;
        hasReportedActiveShelter = false;
    }

    private void ConfigureTriggerPhysics()
    {
        Collider wallCollider = GetComponent<Collider>();
        if (wallCollider != null)
        {
            wallCollider.isTrigger = true;
        }

        Rigidbody wallRigidbody = GetComponent<Rigidbody>();
        if (wallRigidbody != null)
        {
            wallRigidbody.isKinematic = true;
            wallRigidbody.useGravity = false;
        }
    }

    private Vector3 GetStartPosition()
    {
        return startPoint != null ? startPoint.position : fallbackStartPosition;
    }

    private Vector3 GetEndPosition()
    {
        return endPoint != null ? endPoint.position : fallbackEndPosition;
    }
}
