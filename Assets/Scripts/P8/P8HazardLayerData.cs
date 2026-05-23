using System;

[Serializable]
public class P8HazardLayerData
{
    public const bool HazardDataIsGameplayReadOnly = true;

    public string scenarioId = string.Empty;
    public string sourceMode = string.Empty;
    public string hazardLayerVersion = string.Empty;
    public float timeOriginSeconds;
    public string[] scienceLayerFields = new string[0];
    public string[] visualLayerFields = new string[0];
    public P8HazardFeature[] features = new P8HazardFeature[0];
    public P8EvidenceSource[] evidenceSources = new P8EvidenceSource[0];
    public string notes = string.Empty;
}

[Serializable]
public class P8HazardFeature
{
    public string featureId = string.Empty;
    public string sourceMode = string.Empty;
    public string geometryType = string.Empty;
    public float arrivalTimeSeconds;
    public float inundationDepthMeters;
    public float waterLevelMeters;
    public float tsunamiHeightMeters;
    public P8BoundaryPoint[] inundationBoundary = new P8BoundaryPoint[0];
    public float hazardIntensity;
    public float confidence;
    public string evidenceSourceId = string.Empty;
    public string notes = string.Empty;
    public float visualHeightMeters;
    public bool visualHeightIsCinematicOnly;
    public string boundaryIsEvidenceBasedOrPrototype = string.Empty;
    public P8AffectedInfrastructureTypes affectedInfrastructureTypes = new P8AffectedInfrastructureTypes();
    public string buildingDamageState = string.Empty;
    public string collapseProxyState = string.Empty;
    public float collapseProbability;
    public int collapseRandomSeed;
    public bool hazardDrivenCollapse;
}

[Serializable]
public class P8BoundaryPoint
{
    public float x;
    public float y;
}

[Serializable]
public class P8AffectedInfrastructureTypes
{
    public bool roads;
    public bool buildings;
    public bool bridges;
    public bool underground;
    public bool entrances;
    public bool waterfront;
    public bool open_space;
}

[Serializable]
public class P8EvidenceSource
{
    public string evidenceSourceId = string.Empty;
    public string sourceMode = string.Empty;
    public string sourceCategory = string.Empty;
    public string title = string.Empty;
    public string url = string.Empty;
    public string reviewedStatus = string.Empty;
    public string notes = string.Empty;
}
