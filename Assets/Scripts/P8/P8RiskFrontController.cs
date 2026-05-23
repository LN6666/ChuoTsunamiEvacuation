using UnityEngine;

public class P8RiskFrontController : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool AppliesInfrastructureHazardInteraction = false;
    public const bool AppliesCollapseProxy = false;
    public const bool RequiresP9Systems = false;
    public const bool RequiresChuoBaseMap = false;

    [SerializeField] private bool autoLoadFromP8Data = true;
    [SerializeField] private bool autoAdvanceInPlayMode = true;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private P8RiskFrontLightCurtainRenderer lightCurtainRenderer;
    [SerializeField] private P8RiskFrontTimeDriver timeDriver;
    [SerializeField] private P8RiskFrontDebugStatus debugStatus;

    private P8HazardLayerData hazardLayer;
    private P8RiskFrontConfig riskFrontConfig;
    private P8RiskFrontVisualConfig visualConfig;
    private float lastMeshBuildTime = -999f;
    private bool initialized;

    public bool IsFailSafeHidden { get; private set; } = true;
    public bool IsVisualVisible => lightCurtainRenderer != null && lightCurtainRenderer.IsVisible;
    public string LastStatus { get; private set; } = "Not initialized.";
    public P8RiskFrontVisualConfig VisualConfig => visualConfig;

    private void Awake()
    {
        EnsureRuntimeObjects();
    }

    private void Start()
    {
        if (autoLoadFromP8Data)
        {
            LoadFromP8Data();
        }
    }

    private void Update()
    {
        if (!initialized || visualConfig == null || !visualConfig.visualEnabled)
        {
            return;
        }

        if (autoAdvanceInPlayMode && timeDriver != null)
        {
            timeDriver.Advance(Time.deltaTime);
        }

        float interval = Mathf.Max(0.02f, visualConfig.meshRebuildIntervalSeconds);
        if (Time.unscaledTime - lastMeshBuildTime < interval)
        {
            return;
        }

        ForceRefreshVisual(timeDriver != null ? timeDriver.CurrentTimeSeconds : 0f);
        lastMeshBuildTime = Time.unscaledTime;
    }

    public bool LoadFromP8Data()
    {
        P8HazardLayerLoadResult hazardResult = P8HazardLayerLoader.LoadSampleHazardLayer();
        P8RiskFrontConfigLoadResult configResult = P8HazardLayerLoader.LoadRiskFrontConfig();

        if (!hazardResult.success || !configResult.success)
        {
            HideFailSafe("P8-B risk front hidden because P8-A hazard/config data did not validate.");
            return false;
        }

        return Initialize(hazardResult.data, configResult.config);
    }

    public bool Initialize(P8HazardLayerData hazardData, P8RiskFrontConfig config)
    {
        hazardLayer = hazardData;
        riskFrontConfig = config;
        visualConfig = P8RiskFrontVisualConfig.FromRiskFrontConfig(config);
        EnsureRuntimeObjects();

        if (timeDriver != null)
        {
            timeDriver.SetAutoAdvance(false);
            timeDriver.Configure(visualConfig);
            timeDriver.SetTime(config != null ? config.timeOriginSeconds : 0f);
        }

        if (hazardLayer == null || riskFrontConfig == null || visualConfig == null || !visualConfig.isValid || !visualConfig.visualEnabled)
        {
            HideFailSafe(visualConfig != null ? visualConfig.validationMessage : "Missing P8-B visual configuration.");
            initialized = false;
            return false;
        }

        initialized = true;
        return ForceRefreshVisual(timeDriver != null ? timeDriver.CurrentTimeSeconds : riskFrontConfig.timeOriginSeconds);
    }

    public bool ForceRefreshVisual(float simulationTimeSeconds)
    {
        if (hazardLayer == null || visualConfig == null)
        {
            HideFailSafe("Missing P8-B hazard or visual config.");
            return false;
        }

        P8RiskFrontCurveResult curve = P8RiskFrontCurveGenerator.Generate(hazardLayer, visualConfig, simulationTimeSeconds);
        if (!curve.success)
        {
            HideFailSafe(curve.summary);
            return false;
        }

        EnsureRuntimeObjects();
        lightCurtainRenderer.UpdateCurtain(curve.visualBoundary, visualConfig);
        IsFailSafeHidden = false;
        LastStatus = curve.summary + " selectedFeature=" + curve.selectedFeatureId;
        if (debugStatus != null)
        {
            debugStatus.SetStatus(LastStatus, false);
        }

        return true;
    }

    public void SetVisualVisible(bool visible)
    {
        EnsureRuntimeObjects();
        lightCurtainRenderer.SetVisible(visible);
        IsFailSafeHidden = !visible;
    }

    private void HideFailSafe(string reason)
    {
        EnsureRuntimeObjects();
        lightCurtainRenderer.SetVisible(false);
        IsFailSafeHidden = true;
        LastStatus = reason ?? "P8-B risk front hidden fail-safe.";
        if (debugStatus != null)
        {
            debugStatus.SetStatus(LastStatus, true);
        }
    }

    private void EnsureRuntimeObjects()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        if (lightCurtainRenderer == null)
        {
            lightCurtainRenderer = visualRoot.GetComponent<P8RiskFrontLightCurtainRenderer>();
            if (lightCurtainRenderer == null)
            {
                lightCurtainRenderer = visualRoot.gameObject.AddComponent<P8RiskFrontLightCurtainRenderer>();
            }
        }

        if (timeDriver == null)
        {
            timeDriver = GetComponent<P8RiskFrontTimeDriver>();
            if (timeDriver == null)
            {
                timeDriver = gameObject.AddComponent<P8RiskFrontTimeDriver>();
            }
        }

        timeDriver.SetAutoAdvance(false);

        if (debugStatus == null)
        {
            debugStatus = GetComponent<P8RiskFrontDebugStatus>();
            if (debugStatus == null)
            {
                debugStatus = gameObject.AddComponent<P8RiskFrontDebugStatus>();
            }
        }
    }
}
