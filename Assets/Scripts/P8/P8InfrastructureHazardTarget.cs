using UnityEngine;

public class P8InfrastructureHazardTarget : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;
    public const bool ImplementsCollapseProxy = false;

    [SerializeField] private string targetId = string.Empty;
    [SerializeField] private P8InfrastructureCategory category = P8InfrastructureCategory.Road;
    [SerializeField] private bool proxyBased = true;
    [SerializeField] private bool useExplicitHazardCoordinates;
    [SerializeField] private float hazardXMeters;
    [SerializeField] private float hazardYMeters;
    [SerializeField] private bool evaluateOnStart;
    [SerializeField] private float simulationTimeSeconds;
    [SerializeField] private bool conservativeFallbackWhenSpatialDataMissing = true;

    public P8InfrastructureHazardState CurrentState { get; private set; } = P8InfrastructureHazardState.Safe;
    public P8InfrastructureHazardEvaluation LastEvaluation { get; private set; }
    public string TargetId => string.IsNullOrWhiteSpace(targetId) ? name : targetId;
    public P8InfrastructureCategory Category => category;
    public bool IsProxyBased => proxyBased;

    private void Start()
    {
        if (evaluateOnStart)
        {
            P8HazardLayerLoadResult loaded = P8HazardLayerLoader.LoadSampleHazardLayer();
            Evaluate(loaded.success ? loaded.data : null, simulationTimeSeconds);
        }
    }

    public P8InfrastructureHazardEvaluation Evaluate(P8HazardLayerData hazardLayer, float timeSeconds)
    {
        LastEvaluation = P8InfrastructureHazardEvaluator.Evaluate(CreateEvaluationInput(), hazardLayer, timeSeconds);
        CurrentState = LastEvaluation != null ? LastEvaluation.state : P8InfrastructureHazardState.Safe;
        return LastEvaluation;
    }

    public void ApplyEvaluation(P8InfrastructureHazardEvaluation evaluation)
    {
        LastEvaluation = evaluation;
        CurrentState = evaluation != null ? evaluation.state : P8InfrastructureHazardState.Safe;
    }

    public P8InfrastructureHazardEvaluationInput CreateEvaluationInput()
    {
        return new P8InfrastructureHazardEvaluationInput
        {
            targetId = TargetId,
            category = category,
            worldPosition = transform.position,
            isProxy = proxyBased,
            hasExplicitHazardCoordinates = useExplicitHazardCoordinates,
            hazardXMeters = useExplicitHazardCoordinates ? hazardXMeters : transform.position.x,
            hazardYMeters = useExplicitHazardCoordinates ? hazardYMeters : transform.position.z,
            conservativeFallbackWhenSpatialDataMissing = conservativeFallbackWhenSpatialDataMissing
        };
    }

    public void ConfigureForTests(
        string id,
        P8InfrastructureCategory targetCategory,
        bool isProxy,
        bool explicitCoordinates,
        float xMeters,
        float yMeters)
    {
        targetId = id ?? string.Empty;
        category = targetCategory;
        proxyBased = isProxy;
        useExplicitHazardCoordinates = explicitCoordinates;
        hazardXMeters = xMeters;
        hazardYMeters = yMeters;
    }

    public void SetSimulationTimeForTests(float timeSeconds)
    {
        simulationTimeSeconds = Mathf.Max(0f, timeSeconds);
    }
}
