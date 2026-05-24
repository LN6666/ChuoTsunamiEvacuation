using System;
using UnityEngine;

[Serializable]
public class P9DCoordinateAnchoringResult
{
    public bool success;
    public string anchorId = string.Empty;
    public string sourceId = string.Empty;
    public string anchorType = string.Empty;
    public string coordinateSource = string.Empty;
    public bool coordinateValid;
    public bool coordinateBasedProxy;
    public bool nearestMatchUsed;
    public bool exactPlateauObjectIdentityProven;
    public bool routeIsOfficial;
    public bool routeEstimatedPrototypeGuidance = true;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool nonOfficialWarningRequired;
    public bool safeApprovedByDefault;
    public float latitude;
    public float longitude;
    public Vector3 unityPosition;
    public string anchoringStatus = string.Empty;
    public string confidence = string.Empty;
    public float matchedDistanceMeters;
    public string matchedAnchorId = string.Empty;
    public string fallbackReason = string.Empty;
    public string hazardStatus = "unknown";
    public string damageStatus = "unknown";
    public string blockageStatus = "unknown";
    public string lowFloorWarningStatus = "unknown";
    public string warningText = string.Empty;
    public string summary = string.Empty;
}

[Serializable]
public class P9DAnchoringReport
{
    public string schemaVersion = "p9d.anchoring_report.runtime.v1";
    public string coordinatePolicy = "coordinate_based_proxy_nearest_match";
    public int totalAnchors;
    public int successfulAnchors;
    public int coordinateAnchoredCount;
    public int nearestMatchCount;
    public int fallbackMarkerOnlyCount;
    public int rejectedCount;
    public int humanitarianCandidateTotal;
    public int humanitarianCandidateAnchored;
    public int humanitarianCandidateWarningRequiredCount;
    public int namedHumanitarianCandidateCount;
    public int idOnlyHumanitarianCandidateCount;
    public int officialShelterAnchorCount;
    public int entranceProxyAnchorCount;
    public int routeProxyAnchorCount;
    public int hazardLookupAnchorCount;
    public bool allHumanitarianCandidatesRemainNonOfficial;
    public bool allHumanitarianCandidatesRequireWarning;
    public bool routesRemainEstimatedPrototypeGuidance = true;
    public bool exactPlateauObjectIdentityClaimed;
    public bool p8HandoffConsumed;
    public P9DCoordinateAnchoringResult[] results = Array.Empty<P9DCoordinateAnchoringResult>();
    public string summary = string.Empty;
}
