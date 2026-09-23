using UnityEngine;

public class P9DCoordinateAnchor : MonoBehaviour
{
    [SerializeField] private string anchorId = string.Empty;
    [SerializeField] private string sourceId = string.Empty;
    [SerializeField] private string anchorType = string.Empty;
    [SerializeField] private string anchoringStatus = string.Empty;
    [SerializeField] private string confidence = string.Empty;
    [SerializeField] private bool coordinateBasedProxy = true;
    [SerializeField] private bool exactPlateauObjectIdentityProven;
    [SerializeField] private bool routeIsOfficial;
    [SerializeField] private bool routeEstimatedPrototypeGuidance = true;
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool isHumanitarianCandidate;
    [SerializeField] private bool nonOfficialWarningRequired;
    [SerializeField] private string warningText = string.Empty;

    public string AnchorId => anchorId;
    public string SourceId => sourceId;
    public string AnchorType => anchorType;
    public string AnchoringStatus => anchoringStatus;
    public string Confidence => confidence;
    public bool CoordinateBasedProxy => coordinateBasedProxy;
    public bool ExactPlateauObjectIdentityProven => exactPlateauObjectIdentityProven;
    public bool RouteIsOfficial => routeIsOfficial;
    public bool RouteEstimatedPrototypeGuidance => routeEstimatedPrototypeGuidance;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool IsHumanitarianCandidate => isHumanitarianCandidate;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public string WarningText => warningText;

    public void ApplyResult(P9DCoordinateAnchoringResult result)
    {
        if (result == null)
        {
            return;
        }

        anchorId = result.anchorId ?? string.Empty;
        sourceId = result.sourceId ?? string.Empty;
        anchorType = result.anchorType ?? string.Empty;
        anchoringStatus = result.anchoringStatus ?? string.Empty;
        confidence = result.confidence ?? string.Empty;
        coordinateBasedProxy = result.coordinateBasedProxy;
        exactPlateauObjectIdentityProven = result.exactPlateauObjectIdentityProven;
        routeIsOfficial = result.routeIsOfficial;
        routeEstimatedPrototypeGuidance = result.routeEstimatedPrototypeGuidance;
        isOfficialShelter = result.isOfficialShelter;
        isHumanitarianCandidate = result.isHumanitarianCandidate;
        nonOfficialWarningRequired = result.nonOfficialWarningRequired;
        warningText = result.warningText ?? string.Empty;
        transform.position = result.unityPosition;
    }

    public P9DCoordinateAnchoringResult CreateSnapshot()
    {
        return new P9DCoordinateAnchoringResult
        {
            success = true,
            anchorId = anchorId ?? string.Empty,
            sourceId = sourceId ?? string.Empty,
            anchorType = anchorType ?? string.Empty,
            anchoringStatus = anchoringStatus ?? string.Empty,
            confidence = confidence ?? string.Empty,
            coordinateBasedProxy = coordinateBasedProxy,
            exactPlateauObjectIdentityProven = exactPlateauObjectIdentityProven,
            routeIsOfficial = routeIsOfficial,
            routeEstimatedPrototypeGuidance = routeEstimatedPrototypeGuidance,
            isOfficialShelter = isOfficialShelter,
            isHumanitarianCandidate = isHumanitarianCandidate,
            nonOfficialWarningRequired = nonOfficialWarningRequired,
            warningText = warningText ?? string.Empty,
            unityPosition = transform.position
        };
    }
}
