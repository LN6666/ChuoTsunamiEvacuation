using System;
using UnityEngine;

public class P9CrowdRuntimeAgent : MonoBehaviour
{
    public const bool AffectsPlayerSuccessFailure = P9RuntimePolicy.CanCausePlayerFailureInP9B;
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ClaimsRealCrowdModel = false;

    [SerializeField] private string agentId = string.Empty;
    [SerializeField] private string sourceSpawnId = string.Empty;
    [SerializeField] private string targetProxyId = string.Empty;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float speedMetersPerSecond = 1.2f;
    [SerializeField] private bool simulateMovementInUpdate;
    [SerializeField] private bool reachedTarget;
    [SerializeField] private float elapsedSeconds;
    [SerializeField] private float estimatedEvacuationTimeSeconds;

    public string AgentId => agentId;
    public string SourceSpawnId => sourceSpawnId;
    public string TargetProxyId => targetProxyId;
    public Vector3 TargetPosition => targetPosition;
    public float SpeedMetersPerSecond => Mathf.Max(0f, speedMetersPerSecond);
    public bool SimulateMovementInUpdate => simulateMovementInUpdate;
    public bool ReachedTarget => reachedTarget;
    public float ElapsedSeconds => Mathf.Max(0f, elapsedSeconds);
    public float EstimatedEvacuationTimeSeconds => Mathf.Max(0f, estimatedEvacuationTimeSeconds);

    private void Update()
    {
        if (simulateMovementInUpdate)
        {
            StepSimulation(Time.deltaTime);
        }
    }

    public void Configure(
        string id,
        string spawnId,
        string targetId,
        Vector3 target,
        float speed,
        bool simulateInUpdate)
    {
        agentId = id ?? string.Empty;
        sourceSpawnId = spawnId ?? string.Empty;
        targetProxyId = targetId ?? string.Empty;
        targetPosition = target;
        speedMetersPerSecond = Mathf.Max(0f, speed);
        simulateMovementInUpdate = simulateInUpdate;
        reachedTarget = false;
        elapsedSeconds = 0f;
        estimatedEvacuationTimeSeconds = EstimateTravelTime(transform.position, targetPosition, SpeedMetersPerSecond);
    }

    public void StepSimulation(float deltaTime)
    {
        if (reachedTarget)
        {
            return;
        }

        elapsedSeconds += Mathf.Max(0f, deltaTime);
        float step = SpeedMetersPerSecond * Mathf.Max(0f, deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        reachedTarget = Vector3.Distance(transform.position, targetPosition) <= 0.05f;
    }

    public P9CrowdRuntimeAgentSnapshot CreateSnapshot()
    {
        return new P9CrowdRuntimeAgentSnapshot
        {
            agentId = agentId ?? string.Empty,
            sourceSpawnId = sourceSpawnId ?? string.Empty,
            targetProxyId = targetProxyId ?? string.Empty,
            position = transform.position,
            targetPosition = targetPosition,
            reachedTarget = reachedTarget,
            estimatedEvacuationTimeSeconds = EstimatedEvacuationTimeSeconds,
            elapsedSeconds = ElapsedSeconds,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure
        };
    }

    private static float EstimateTravelTime(Vector3 start, Vector3 target, float speed)
    {
        if (speed <= 0f)
        {
            return 0f;
        }

        return Vector3.Distance(start, target) / speed;
    }
}

[Serializable]
public class P9CrowdRuntimeAgentSnapshot
{
    public string agentId = string.Empty;
    public string sourceSpawnId = string.Empty;
    public string targetProxyId = string.Empty;
    public Vector3 position;
    public Vector3 targetPosition;
    public bool reachedTarget;
    public float estimatedEvacuationTimeSeconds;
    public float elapsedSeconds;
    public bool affectsGameplaySuccessFailure;
}
