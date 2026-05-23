using System;

[Serializable]
public class P8RiskFrontConfig
{
    public string scenarioId = string.Empty;
    public string sourceMode = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string configVersion = string.Empty;
    public string hazardLayerVersion = string.Empty;
    public string geometryType = string.Empty;
    public float timeOriginSeconds;
    public bool riskFrontEnabledInP8A;
    public float visualHeightMeters;
    public bool visualHeightIsCinematicOnly;
    public string boundaryIsEvidenceBasedOrPrototype = string.Empty;
    public string visualLayerPurpose = string.Empty;
    public string[] scienceLayerFields = new string[0];
    public string[] visualLayerFields = new string[0];
    public bool p8bVisualSceneObjectsImplemented;
    public bool p8cInfrastructureInteractionImplemented;
    public bool p8dCollapseProxyGameplayImplemented;
    public P8RiskFrontPerformanceSettings performanceSettings = new P8RiskFrontPerformanceSettings();
    public bool manualSampleIsOfficial;
    public string notes = string.Empty;
}

[Serializable]
public class P8InfrastructureHazardInteractionConfig
{
    public string scenarioId = string.Empty;
    public string sourceMode = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string configVersion = string.Empty;
    public bool interactionEnabledInP8A;
    public P8AffectedInfrastructureTypes affectedInfrastructureTypes = new P8AffectedInfrastructureTypes();
    public string roadInteractionMode = string.Empty;
    public string buildingInteractionMode = string.Empty;
    public string bridgeInteractionMode = string.Empty;
    public string undergroundInteractionMode = string.Empty;
    public string entranceInteractionMode = string.Empty;
    public bool collapseProxyEnabledInP8A;
    public bool collapseGameplayEnabledInP8A;
    public string buildingDamageState = string.Empty;
    public string collapseProxyState = string.Empty;
    public float collapseProbability;
    public int collapseRandomSeed;
    public bool hazardDrivenCollapse;
    public bool manualSampleIsOfficial;
    public string notes = string.Empty;
}
