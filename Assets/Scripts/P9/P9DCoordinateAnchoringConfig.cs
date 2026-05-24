using System;

[Serializable]
public class P9DCoordinateAnchoringConfig
{
    public string schemaVersion = string.Empty;
    public string coordinatePolicy = "coordinate_based_proxy_nearest_match";
    public string coordinateReferenceSystem = "EPSG:4326";
    public string projectionMode = "local_linear_proxy";
    public float originLatitude = 35.67f;
    public float originLongitude = 139.77f;
    public float metersPerDegreeLatitude = 111320f;
    public float metersPerDegreeLongitude = 91000f;
    public float minLatitude = 35.63f;
    public float maxLatitude = 35.71f;
    public float minLongitude = 139.73f;
    public float maxLongitude = 139.82f;
    public float exactDistanceMeters = 1f;
    public float highConfidenceDistanceMeters = 15f;
    public float mediumConfidenceDistanceMeters = 50f;
    public float lowConfidenceDistanceMeters = 100f;
    public float maxNearestMatchDistanceMeters = 120f;
    public bool exactPlateauObjectIdentityProofAvailable;
    public bool coordinateBasedProxyRequired = true;
    public bool nearestMatchAllowed = true;
    public bool claimsOfficialRouteStatus;
    public bool claimsExactPlateauObjectBinding;
    public bool p8HandoffConsumed = true;
    public string notes = string.Empty;
    public P9DCoordinateAnchorRecord[] anchors = Array.Empty<P9DCoordinateAnchorRecord>();

    public bool IsProxyPolicySafe()
    {
        return coordinateBasedProxyRequired &&
            nearestMatchAllowed &&
            !claimsOfficialRouteStatus &&
            !claimsExactPlateauObjectBinding;
    }
}

[Serializable]
public class P9DCoordinateAnchorCollection
{
    public string schemaVersion = string.Empty;
    public P9DCoordinateAnchorRecord[] anchors = Array.Empty<P9DCoordinateAnchorRecord>();
}

[Serializable]
public class P9DCoordinateAnchorRecord
{
    public string anchorId = string.Empty;
    public string sourceId = string.Empty;
    public string anchorType = string.Empty;
    public string displayName = string.Empty;
    public string coordinateSource = string.Empty;
    public string coordinateSystem = "EPSG:4326";
    public bool hasCoordinate = true;
    public float latitude;
    public float longitude;
    public P9Vector3Data fallbackProxyPosition = new P9Vector3Data();
    public string requestedBindingStatus = string.Empty;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool nonOfficialWarningRequired;
    public bool safeApprovedByDefault;
    public bool routeIsOfficial;
    public bool routeEstimatedPrototypeGuidance = true;
    public string hazardStatus = "unknown";
    public string damageStatus = "unknown";
    public string blockageStatus = "unknown";
    public string lowFloorWarningStatus = "unknown";
    public string notes = string.Empty;

    public bool IsHumanitarianWarningSafe()
    {
        return !isHumanitarianCandidate || (!isOfficialShelter && nonOfficialWarningRequired && !safeApprovedByDefault);
    }
}
