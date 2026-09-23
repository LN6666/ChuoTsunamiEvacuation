using UnityEngine;

public class P8InfrastructureHazardMarker : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;

    [SerializeField] private string markerId = string.Empty;
    [SerializeField] private P8InfrastructureCategory category = P8InfrastructureCategory.Road;
    [SerializeField] private bool proxyBased = true;
    [SerializeField] private bool useExplicitHazardCoordinates;
    [SerializeField] private float hazardXMeters;
    [SerializeField] private float hazardYMeters;

    public string MarkerId => string.IsNullOrWhiteSpace(markerId) ? name : markerId;
    public P8InfrastructureCategory Category => category;
    public bool IsProxyBased => proxyBased;

    public P8InfrastructureHazardEvaluationInput CreateEvaluationInput()
    {
        return new P8InfrastructureHazardEvaluationInput
        {
            targetId = MarkerId,
            category = category,
            worldPosition = transform.position,
            isProxy = proxyBased,
            hasExplicitHazardCoordinates = useExplicitHazardCoordinates,
            hazardXMeters = useExplicitHazardCoordinates ? hazardXMeters : transform.position.x,
            hazardYMeters = useExplicitHazardCoordinates ? hazardYMeters : transform.position.z
        };
    }

    public void ConfigureForTests(
        string id,
        P8InfrastructureCategory markerCategory,
        bool isProxy,
        bool explicitCoordinates,
        float xMeters,
        float yMeters)
    {
        markerId = id ?? string.Empty;
        category = markerCategory;
        proxyBased = isProxy;
        useExplicitHazardCoordinates = explicitCoordinates;
        hazardXMeters = xMeters;
        hazardYMeters = yMeters;
    }
}
