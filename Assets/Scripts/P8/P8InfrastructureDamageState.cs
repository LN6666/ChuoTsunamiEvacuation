public enum P8InfrastructureDamageState
{
    NoDamage,
    Warning,
    LowFloorInundationWarning,
    EntranceBlockedProxy,
    RoadRestrictedProxy,
    BridgeRestrictedProxy,
    UndergroundAvoidProxy,
    BuildingDamagedProxy,
    CollapsedProxyVisual,
    InaccessibleProxy,
    ManualReviewRequired
}

public enum P8InfrastructureBlockageState
{
    None,
    Warning,
    EntranceBlockedProxy,
    RoadRestrictedProxy,
    BridgeRestrictedProxy,
    UndergroundAvoidProxy,
    InaccessibleProxy
}

public enum P8CollapseProxyState
{
    None,
    Eligible,
    CollapsedProxyVisual,
    SuppressedByConfig,
    SuppressedByProbability,
    SuppressedBySampleLimit
}

public static class P8InfrastructureDamageStateUtility
{
    public static string ToToken(P8InfrastructureDamageState state)
    {
        switch (state)
        {
            case P8InfrastructureDamageState.Warning:
                return "warning";
            case P8InfrastructureDamageState.LowFloorInundationWarning:
                return "low_floor_inundation_warning";
            case P8InfrastructureDamageState.EntranceBlockedProxy:
                return "entrance_blocked_proxy";
            case P8InfrastructureDamageState.RoadRestrictedProxy:
                return "road_restricted_proxy";
            case P8InfrastructureDamageState.BridgeRestrictedProxy:
                return "bridge_restricted_proxy";
            case P8InfrastructureDamageState.UndergroundAvoidProxy:
                return "underground_avoid_proxy";
            case P8InfrastructureDamageState.BuildingDamagedProxy:
                return "building_damaged_proxy";
            case P8InfrastructureDamageState.CollapsedProxyVisual:
                return "collapsed_proxy_visual";
            case P8InfrastructureDamageState.InaccessibleProxy:
                return "inaccessible_proxy";
            case P8InfrastructureDamageState.ManualReviewRequired:
                return "manual_review_required";
            default:
                return "no_damage";
        }
    }

    public static string ToToken(P8InfrastructureBlockageState state)
    {
        switch (state)
        {
            case P8InfrastructureBlockageState.Warning:
                return "warning";
            case P8InfrastructureBlockageState.EntranceBlockedProxy:
                return "entrance_blocked_proxy";
            case P8InfrastructureBlockageState.RoadRestrictedProxy:
                return "road_restricted_proxy";
            case P8InfrastructureBlockageState.BridgeRestrictedProxy:
                return "bridge_restricted_proxy";
            case P8InfrastructureBlockageState.UndergroundAvoidProxy:
                return "underground_avoid_proxy";
            case P8InfrastructureBlockageState.InaccessibleProxy:
                return "inaccessible_proxy";
            default:
                return "none";
        }
    }

    public static string ToToken(P8CollapseProxyState state)
    {
        switch (state)
        {
            case P8CollapseProxyState.Eligible:
                return "eligible";
            case P8CollapseProxyState.CollapsedProxyVisual:
                return "collapsed_proxy_visual";
            case P8CollapseProxyState.SuppressedByConfig:
                return "suppressed_by_config";
            case P8CollapseProxyState.SuppressedByProbability:
                return "suppressed_by_probability";
            case P8CollapseProxyState.SuppressedBySampleLimit:
                return "suppressed_by_sample_limit";
            default:
                return "none";
        }
    }
}
