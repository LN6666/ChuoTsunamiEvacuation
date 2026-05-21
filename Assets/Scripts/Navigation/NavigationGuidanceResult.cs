using UnityEngine;

public class NavigationGuidanceResult
{
    public bool hasTarget;
    public string targetId = string.Empty;
    public string targetName = string.Empty;
    public Vector3 targetWorldPosition;
    public Vector3 directionToTarget;
    public float distanceMeters = -1f;
    public float estimatedTimeSeconds = -1f;
    public bool usedRouteMetadata;
    public string distanceText = "Distance unavailable";
    public string estimatedTimeText = "Estimated time unavailable";
    public string statusText = NavigationGuidanceCalculator.MissingTargetWarning;
}
