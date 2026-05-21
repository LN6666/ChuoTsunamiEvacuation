using UnityEngine;

public class NavigationTargetInfo
{
    public string targetId = string.Empty;
    public string displayName = string.Empty;
    public Vector3 worldPosition;
    public bool hasWorldPosition;
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public float routeDistanceMeters = -1f;
    public float estimatedRouteTimeSeconds = -1f;
    public string routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel;
    public string osmAttribution = string.Empty;
    public string routeStatusNote = NavigationGuidanceCalculator.RouteLineRenderingDisabledWarning;

    public bool HasRouteDistanceMeters => IsUsableNonNegativeFloat(routeDistanceMeters);
    public bool HasEstimatedRouteTimeSeconds => IsUsableNonNegativeFloat(estimatedRouteTimeSeconds);
    public bool HasOsmAttribution => !string.IsNullOrWhiteSpace(osmAttribution);

    public static NavigationTargetInfo FromTransform(Transform targetTransform, string fallbackName = "")
    {
        if (targetTransform == null)
        {
            return null;
        }

        return new NavigationTargetInfo
        {
            targetId = targetTransform.name,
            displayName = FirstNonEmpty(fallbackName, targetTransform.name),
            worldPosition = targetTransform.position,
            hasWorldPosition = true,
            isOfficialShelter = false
        };
    }

    public static bool TryFromShelter(BuildingShelter shelter, out NavigationTargetInfo targetInfo)
    {
        targetInfo = null;
        if (shelter == null)
        {
            return false;
        }

        targetInfo = new NavigationTargetInfo
        {
            targetId = shelter.ShelterId,
            displayName = shelter.ShelterName,
            worldPosition = shelter.transform.position,
            hasWorldPosition = true,
            isOfficialShelter = shelter.IsOfficialShelter
        };

        ApplyRealQualifiedMetadata(shelter, targetInfo);
        ApplyHumanitarianMetadata(shelter, targetInfo);
        return true;
    }

    private static void ApplyRealQualifiedMetadata(BuildingShelter shelter, NavigationTargetInfo targetInfo)
    {
        RealQualifiedShelterMetadata metadata = shelter.GetComponent<RealQualifiedShelterMetadata>();
        if (metadata == null)
        {
            return;
        }

        targetInfo.targetId = FirstNonEmpty(metadata.GameplayShelterId, targetInfo.targetId);
        targetInfo.displayName = FirstNonEmpty(metadata.DisplayName, targetInfo.displayName);
        targetInfo.routeDistanceMeters = metadata.RouteDistanceMeters;
        targetInfo.estimatedRouteTimeSeconds = metadata.EstimatedRouteTimeSeconds;
        targetInfo.routeDisclaimer = FirstNonEmpty(metadata.RouteDisclaimer, P5CStaticDataLoader.EstimatedPrototypeRouteLabel);
        targetInfo.osmAttribution = metadata.Attribution;
    }

    private static void ApplyHumanitarianMetadata(BuildingShelter shelter, NavigationTargetInfo targetInfo)
    {
        HumanitarianCandidateMetadata metadata = shelter.GetComponent<HumanitarianCandidateMetadata>();
        if (metadata == null)
        {
            return;
        }

        targetInfo.isHumanitarianCandidate = true;
        targetInfo.targetId = FirstNonEmpty(metadata.CandidateId, targetInfo.targetId);
        targetInfo.displayName = FirstNonEmpty(metadata.BuildingName, targetInfo.displayName);
    }

    private static bool IsUsableNonNegativeFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
