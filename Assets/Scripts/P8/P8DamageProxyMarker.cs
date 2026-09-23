using UnityEngine;

public class P8DamageProxyMarker : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;
    public const bool UsesPhysicsCollapse = false;
    public const bool UsesDebrisSimulation = false;

    [SerializeField] private string markerId = string.Empty;
    [SerializeField] private P8InfrastructureCategory category = P8InfrastructureCategory.Building;
    [SerializeField] private P8InfrastructureDamageState currentState = P8InfrastructureDamageState.NoDamage;
    [SerializeField] private bool nonOfficialWarningRequired;
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private Color normalColor = new Color(0.4f, 0.8f, 0.45f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.72f, 0.18f, 1f);
    [SerializeField] private Color restrictedColor = new Color(1f, 0.35f, 0.18f, 1f);
    [SerializeField] private Color collapsedProxyColor = new Color(0.8f, 0.05f, 0.05f, 1f);

    private Quaternion baseRotation;

    public string MarkerId => string.IsNullOrWhiteSpace(markerId) ? name : markerId;
    public P8InfrastructureCategory Category => category;
    public P8InfrastructureDamageState CurrentState => currentState;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public bool IsOfficialShelter => isOfficialShelter;

    private void Awake()
    {
        baseRotation = transform.localRotation;
    }

    public void ApplyDamageState(P8InfrastructureDamageState state)
    {
        currentState = state;
        transform.localRotation = state == P8InfrastructureDamageState.CollapsedProxyVisual
            ? baseRotation * Quaternion.Euler(0f, 0f, 15f)
            : baseRotation;
        ApplyColor(ColorForState(state));
    }

    public void ConfigureForTests(
        string id,
        P8InfrastructureCategory markerCategory,
        bool requiresNonOfficialWarning,
        bool officialShelter)
    {
        markerId = id ?? string.Empty;
        category = markerCategory;
        nonOfficialWarningRequired = requiresNonOfficialWarning;
        isOfficialShelter = officialShelter;
    }

    public static P8DamageProxyMarker CreateCubeMarkerForTests(string name, P8InfrastructureCategory markerCategory)
    {
        GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        markerObject.name = name;
        Collider collider = markerObject.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        P8DamageProxyMarker marker = markerObject.AddComponent<P8DamageProxyMarker>();
        marker.ConfigureForTests(name, markerCategory, false, false);
        return marker;
    }

    private Color ColorForState(P8InfrastructureDamageState state)
    {
        switch (state)
        {
            case P8InfrastructureDamageState.CollapsedProxyVisual:
                return collapsedProxyColor;
            case P8InfrastructureDamageState.EntranceBlockedProxy:
            case P8InfrastructureDamageState.RoadRestrictedProxy:
            case P8InfrastructureDamageState.BridgeRestrictedProxy:
            case P8InfrastructureDamageState.UndergroundAvoidProxy:
            case P8InfrastructureDamageState.BuildingDamagedProxy:
            case P8InfrastructureDamageState.InaccessibleProxy:
                return restrictedColor;
            case P8InfrastructureDamageState.Warning:
            case P8InfrastructureDamageState.LowFloorInundationWarning:
            case P8InfrastructureDamageState.ManualReviewRequired:
                return warningColor;
            default:
                return normalColor;
        }
    }

    private void ApplyColor(Color color)
    {
        Renderer markerRenderer = GetComponent<Renderer>();
        if (markerRenderer == null)
        {
            return;
        }

        markerRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        markerRenderer.sharedMaterial.color = color;
    }
}
