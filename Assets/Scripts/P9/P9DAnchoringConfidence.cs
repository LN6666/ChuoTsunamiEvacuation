public enum P9DAnchoringConfidence
{
    Exact,
    High,
    Medium,
    Low,
    Fallback,
    Rejected
}

public static class P9DAnchoringTokens
{
    public const string AnchoredToCoordinate = "anchored_to_coordinate";
    public const string AnchoredToNearestBuildingProxy = "anchored_to_nearest_building_proxy";
    public const string AnchoredToNearestRoadProxy = "anchored_to_nearest_road_proxy";
    public const string AnchoredToEntranceProxy = "anchored_to_entrance_proxy";
    public const string AnchoredToHazardGrid = "anchored_to_hazard_grid";
    public const string FallbackMarkerOnly = "fallback_marker_only";
    public const string RejectedInvalidCoordinate = "rejected_invalid_coordinate";
    public const string RejectedOutOfBounds = "rejected_out_of_bounds";
    public const string RejectedDistanceThresholdExceeded = "rejected_distance_threshold_exceeded";

    public static string ToToken(P9DAnchoringConfidence confidence)
    {
        switch (confidence)
        {
            case P9DAnchoringConfidence.Exact:
                return "exact";
            case P9DAnchoringConfidence.High:
                return "high";
            case P9DAnchoringConfidence.Medium:
                return "medium";
            case P9DAnchoringConfidence.Low:
                return "low";
            case P9DAnchoringConfidence.Fallback:
                return "fallback";
            default:
                return "rejected";
        }
    }
}
