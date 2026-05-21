using System.Text;
using UnityEngine;

public static class NavigationGuidanceCalculator
{
    public const float DefaultWalkingSpeedMetersPerSecond = 1.2f;
    public const string MissingTargetWarning = "Navigation target unavailable. Guidance hidden.";
    public const string EstimatedPrototypeRouteWarning = "Estimated prototype route";
    public const string NotOfficialNavigationWarning = "Not official navigation";
    public const string NotOfficialEvacuationGuidanceWarning = "Not official evacuation guidance";
    public const string RouteLineRenderingDisabledWarning =
        "Route line rendering disabled until coordinate transform is validated";
    public const string HumanitarianCandidateWarning =
        "Humanitarian candidates are not official designated shelters";

    public static Vector3 CalculateHorizontalDirection(Vector3 fromWorldPosition, Vector3 toWorldPosition)
    {
        Vector3 delta = toWorldPosition - fromWorldPosition;
        delta.y = 0f;

        if (!IsFinite(delta) || delta.sqrMagnitude <= 0.000001f)
        {
            return Vector3.zero;
        }

        return delta.normalized;
    }

    public static float CalculateWorldDistanceMeters(Vector3 fromWorldPosition, Vector3 toWorldPosition)
    {
        if (!IsFinite(fromWorldPosition) || !IsFinite(toWorldPosition))
        {
            return -1f;
        }

        return Vector3.Distance(fromWorldPosition, toWorldPosition);
    }

    public static float CalculateEstimatedTimeSeconds(
        float unityDistanceMeters,
        NavigationTargetInfo targetInfo,
        float walkingSpeedMetersPerSecond,
        out bool usedRouteMetadata)
    {
        usedRouteMetadata = false;

        if (targetInfo != null && targetInfo.HasEstimatedRouteTimeSeconds)
        {
            usedRouteMetadata = true;
            return targetInfo.estimatedRouteTimeSeconds;
        }

        float distanceForEstimate = unityDistanceMeters;
        if (targetInfo != null && targetInfo.HasRouteDistanceMeters)
        {
            distanceForEstimate = targetInfo.routeDistanceMeters;
            usedRouteMetadata = true;
        }

        return CalculateDistanceBasedEstimateSeconds(distanceForEstimate, walkingSpeedMetersPerSecond);
    }

    public static float CalculateDistanceBasedEstimateSeconds(
        float distanceMeters,
        float walkingSpeedMetersPerSecond)
    {
        if (!IsUsableNonNegativeFloat(distanceMeters))
        {
            return -1f;
        }

        float safeSpeed = IsUsablePositiveFloat(walkingSpeedMetersPerSecond)
            ? walkingSpeedMetersPerSecond
            : DefaultWalkingSpeedMetersPerSecond;
        return distanceMeters / safeSpeed;
    }

    public static NavigationGuidanceResult BuildGuidance(
        Vector3 playerWorldPosition,
        NavigationTargetInfo targetInfo,
        float walkingSpeedMetersPerSecond)
    {
        var result = new NavigationGuidanceResult();

        if (targetInfo == null || !targetInfo.hasWorldPosition)
        {
            return result;
        }

        result.hasTarget = true;
        result.targetId = targetInfo.targetId ?? string.Empty;
        result.targetName = string.IsNullOrWhiteSpace(targetInfo.displayName)
            ? result.targetId
            : targetInfo.displayName.Trim();
        result.targetWorldPosition = targetInfo.worldPosition;
        result.directionToTarget = CalculateHorizontalDirection(playerWorldPosition, targetInfo.worldPosition);
        result.distanceMeters = CalculateWorldDistanceMeters(playerWorldPosition, targetInfo.worldPosition);
        result.estimatedTimeSeconds = CalculateEstimatedTimeSeconds(
            result.distanceMeters,
            targetInfo,
            walkingSpeedMetersPerSecond,
            out result.usedRouteMetadata);
        result.distanceText = FormatDistance(result.distanceMeters);
        result.estimatedTimeText = FormatEstimatedTime(result.estimatedTimeSeconds, result.usedRouteMetadata);
        result.statusText = BuildWarningText(targetInfo, true);
        return result;
    }

    public static string BuildWarningText(NavigationTargetInfo targetInfo, bool includeRouteLineRenderingWarning)
    {
        var builder = new StringBuilder();
        AppendLine(builder, EstimatedPrototypeRouteWarning);
        AppendLine(builder, NotOfficialNavigationWarning);
        AppendLine(builder, NotOfficialEvacuationGuidanceWarning);

        if (targetInfo != null && !string.IsNullOrWhiteSpace(targetInfo.routeDisclaimer))
        {
            AppendLine(builder, targetInfo.routeDisclaimer);
        }
        else
        {
            AppendLine(builder, P5CStaticDataLoader.EstimatedPrototypeRouteLabel);
        }

        if (includeRouteLineRenderingWarning)
        {
            string note = targetInfo != null && !string.IsNullOrWhiteSpace(targetInfo.routeStatusNote)
                ? targetInfo.routeStatusNote
                : RouteLineRenderingDisabledWarning;
            AppendLine(builder, note);
        }

        if (targetInfo != null && targetInfo.isHumanitarianCandidate)
        {
            AppendLine(builder, HumanitarianCandidateWarning);
        }

        if (targetInfo != null && targetInfo.HasOsmAttribution)
        {
            AppendLine(builder, $"OSM/ODbL attribution: {targetInfo.osmAttribution.Trim()}");
        }

        return builder.ToString().TrimEnd();
    }

    public static bool ContainsUnsafeOfficialNavigationClaim(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string lower = text.ToLowerInvariant();
        return ContainsUnsafePhrase(lower, "official navigation") ||
            ContainsUnsafePhrase(lower, "official evacuation guidance") ||
            ContainsUnsafePhrase(lower, "official evacuation route") ||
            ContainsUnsafePhrase(lower, "approved navigation");
    }

    public static string FormatDistance(float distanceMeters)
    {
        if (!IsUsableNonNegativeFloat(distanceMeters))
        {
            return "Distance unavailable";
        }

        return distanceMeters >= 1000f
            ? $"{distanceMeters / 1000f:0.00} km"
            : $"{distanceMeters:0.#} m";
    }

    public static string FormatEstimatedTime(float seconds, bool usedRouteMetadata)
    {
        if (!IsUsableNonNegativeFloat(seconds))
        {
            return "Estimated time unavailable";
        }

        string source = usedRouteMetadata ? "route metadata" : "walking-speed estimate";
        return $"{FormatTime(seconds)} ({source})";
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return minutes > 0 ? $"{minutes}m {remainingSeconds:00}s" : $"{remainingSeconds}s";
    }

    private static bool ContainsUnsafePhrase(string lowerText, string phrase)
    {
        int searchIndex = 0;
        while (searchIndex < lowerText.Length)
        {
            int index = lowerText.IndexOf(phrase, searchIndex, System.StringComparison.Ordinal);
            if (index < 0)
            {
                return false;
            }

            int prefixStart = Mathf.Max(0, index - 12);
            string prefix = lowerText.Substring(prefixStart, index - prefixStart);
            if (!prefix.Contains("not ") && !prefix.Contains("not an ") && !prefix.Contains("not a "))
            {
                return true;
            }

            searchIndex = index + phrase.Length;
        }

        return false;
    }

    private static void AppendLine(StringBuilder builder, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine(value.Trim());
        }
    }

    private static bool IsUsableNonNegativeFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static bool IsUsablePositiveFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }

    private static bool IsFinite(Vector3 value)
    {
        return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }
}
