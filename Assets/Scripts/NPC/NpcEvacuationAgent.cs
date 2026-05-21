using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcEvacuationAgent : MonoBehaviour
{
    public static bool AffectsPlayerSuccessFailure => false;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.6f;
    [SerializeField] private bool autoSelectOnStart = true;
    [SerializeField] private bool faceMovementDirection = true;
    [SerializeField] private bool disableBlockingPhysics = true;
    [SerializeField] private NpcTargetScoringSettings scoringSettings = new NpcTargetScoringSettings();

    private readonly List<NpcEvacuationTargetInfo> availableTargets = new List<NpcEvacuationTargetInfo>();
    private NpcEvacuationState currentState = NpcEvacuationState.Idle;
    private NpcEvacuationTargetInfo currentTarget;
    private float currentTargetScore = float.PositiveInfinity;

    public NpcEvacuationState CurrentState => currentState;
    public NpcEvacuationTargetInfo CurrentTarget => currentTarget;
    public float CurrentTargetScore => currentTargetScore;
    public int TargetCount => availableTargets.Count;

    private void Awake()
    {
        if (disableBlockingPhysics)
        {
            EnsureNonBlockingPhysics();
        }
    }

    private void Start()
    {
        if (autoSelectOnStart && currentState == NpcEvacuationState.Idle && availableTargets.Count > 0)
        {
            SelectTarget();
        }
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        arrivalDistance = Mathf.Max(0.01f, arrivalDistance);
    }

    public void ConfigureMovement(float speed, float arriveDistance)
    {
        moveSpeed = Mathf.Max(0f, speed);
        arrivalDistance = Mathf.Max(0.01f, arriveDistance);
    }

    public void ConfigureTargets(IEnumerable<NpcEvacuationTargetInfo> targets, bool selectImmediately)
    {
        availableTargets.Clear();
        if (targets != null)
        {
            foreach (NpcEvacuationTargetInfo target in targets)
            {
                if (target != null)
                {
                    availableTargets.Add(target);
                }
            }
        }

        currentTarget = null;
        currentTargetScore = float.PositiveInfinity;
        currentState = NpcEvacuationState.Idle;

        if (selectImmediately)
        {
            SelectTarget();
        }
    }

    public bool SelectTarget()
    {
        currentState = NpcEvacuationState.SelectingTarget;

        if (!NpcTargetScorer.TrySelectBestTarget(
            transform.position,
            availableTargets,
            scoringSettings,
            out currentTarget,
            out currentTargetScore))
        {
            currentTarget = null;
            currentTargetScore = float.PositiveInfinity;
            currentState = NpcEvacuationState.FailedNoTarget;
            return false;
        }

        currentState = IsAtCurrentTarget() ? NpcEvacuationState.Arrived : NpcEvacuationState.MovingToTarget;
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (currentState != NpcEvacuationState.MovingToTarget)
        {
            return;
        }

        if (currentTarget == null)
        {
            currentState = NpcEvacuationState.FailedNoTarget;
            return;
        }

        if (IsAtCurrentTarget())
        {
            currentState = NpcEvacuationState.Arrived;
            return;
        }

        float safeDeltaTime = Mathf.Max(0f, deltaTime);
        if (safeDeltaTime <= 0f || moveSpeed <= 0f)
        {
            return;
        }

        Vector3 before = transform.position;
        Vector3 targetPosition = currentTarget.GetCurrentPosition();
        transform.position = Vector3.MoveTowards(before, targetPosition, moveSpeed * safeDeltaTime);

        Vector3 movement = transform.position - before;
        if (faceMovementDirection && movement.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
        }

        if (IsAtCurrentTarget())
        {
            currentState = NpcEvacuationState.Arrived;
        }
    }

    public void EnsureNonBlockingPhysics()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody body in rigidbodies)
        {
            if (body == null)
            {
                continue;
            }

            body.isKinematic = true;
            body.detectCollisions = false;
            body.useGravity = false;
        }
    }

    public string GetDebugText()
    {
        string targetText = currentTarget != null ? currentTarget.GetDisplayName() : "none";
        return $"NPC: {currentState}\nTarget: {targetText}";
    }

    private bool IsAtCurrentTarget()
    {
        if (currentTarget == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, currentTarget.GetCurrentPosition()) <= arrivalDistance;
    }
}
