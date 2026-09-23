using System;

[Serializable]
public class P8HazardLayerData
{
    public const bool HazardDataIsGameplayReadOnly = true;

    public string scenarioId = string.Empty;
    public string sourceMode = string.Empty;
    public string sourceCategory = string.Empty;
    public string hazardLayerVersion = string.Empty;
    public string evidenceRegistryFile = string.Empty;
    public string p8cGateDecision = string.Empty;
    public string spatialExtractionStatus = string.Empty;
    public string extractionStatus = string.Empty;
    public string extractionMethod = string.Empty;
    public bool officialMetropolitanEvidenceIdentified;
    public bool completeOfficialSpatialLayerExtracted;
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
    public string scenarioName = string.Empty;
    public string sourceMode = string.Empty;
    public string sourceCategory = string.Empty;
    public string geometryType = string.Empty;
    public string extractionStatus = string.Empty;
    public string extractionMethod = string.Empty;
    public float arrivalTimeSeconds;
    public string arrivalTimeStatus = string.Empty;
    public float inundationDepthMeters;
    public float maxInundationDepthMeters;
    public float averageInundationDepthMeters;
    public float waterLevelMeters;
    public string waterLevelStatus = string.Empty;
    public float tsunamiHeightMeters;
    public float maxTsunamiHeightMeters;
    public string inundationDepthStatus = string.Empty;
    public string boundaryStatus = string.Empty;
    public string spatialExtractionStatus = string.Empty;
    public int spatialSampleCount;
    public float minArrivalTime1cmSeconds;
    public float minArrivalTime30cmSeconds;
    public float maxArrivalTimeMaxWaterLevelSeconds;
    public P8BoundaryPoint[] inundationBoundary = new P8BoundaryPoint[0];
    public P8HazardSpatialSample[] spatialSamples = new P8HazardSpatialSample[0];
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
public class P8HazardSpatialSample
{
    public float xMeters;
    public float yMeters;
    public float longitude;
    public float latitude;
    public float inundationDepthMeters;
    public float tsunamiHeightMeters;
    public float arrivalTime1cmSeconds;
    public float arrivalTime30cmSeconds;
    public float arrivalTime1mSeconds;
    public float arrivalTimeMaxWaterLevelSeconds;
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
    public bool shelter_proxy;
    public bool navigation_target_proxy;
    public bool humanitarian_candidate_proxy;
    public bool highrise_candidate_marker;
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
    public string accessMethod = string.Empty;
    public bool requiresTokenOrLogin;
    public string expectedFileType = string.Empty;
    public string notes = string.Empty;
}
