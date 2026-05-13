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

    private bool isMoving;
    private bool waitingForManualStart;
    private bool hasReportedPlayer;
    private bool hasReportedActiveShelter;
    private float elapsedSeconds;
    private Vector3 fallbackStartPosition;
    private Vector3 fallbackEndPosition;

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

    private void BeginMovement()
    {
        elapsedSeconds = 0f;
        isMoving = true;
        waitingForManualStart = false;
        ResetContactReports();

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
