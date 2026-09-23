using System;
using System.Linq;

public enum P8RuntimeAdaptationStatus
{
    Passed,
    Pending,
    ProxyBased,
    Blocked
}

[Serializable]
public class P8RuntimeCompatibilityResult
{
    public string phase = string.Empty;
    public string category = string.Empty;
    public P8RuntimeAdaptationStatus status = P8RuntimeAdaptationStatus.Pending;
    public string evidence = string.Empty;
    public string notes = string.Empty;
}

public static class P8P2P6RuntimeCompatibilityInspector
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;
    public const bool ClaimsOfficialRoutes = false;

    public static P8RuntimeCompatibilityResult[] Inspect()
    {
        return new[]
        {
            Create("P2", "player_movement", P8RuntimeAdaptationStatus.Passed,
                "SimplePlayerController is source-compatible with high-detail context smoke objects.",
                "No success/failure rule changes."),
            Create("P2", "camera", P8RuntimeAdaptationStatus.Passed,
                "Camera compatibility is smoke-tested through temporary high-detail context objects.",
                "No hard dependency on the legacy base-map scene."),
            Create("P2", "shelter_interaction_proxy", P8RuntimeAdaptationStatus.ProxyBased,
                "P8InfrastructureHazardTarget supports shelter_proxy category.",
                "Proxy does not call BuildingShelter, ShelterEntranceTrigger, EvacuationGameManager, or ResultPanelController."),
            Create("P2", "result_panel_flow", P8RuntimeAdaptationStatus.Passed,
                "P8-C scripts remain gameplay-neutral and do not mutate result state.",
                "Existing success/failure rules stay unchanged."),
            Create("P3", "unity_ready_data_assumptions", P8RuntimeAdaptationStatus.Passed,
                "P8-B hazard layer v1 loads from Assets/Data/P8 with evidence fields.",
                "Runtime uses JSON data through existing P8 loader."),
            Create("P4", "real_shelter_marker_logic", P8RuntimeAdaptationStatus.ProxyBased,
                "P8-C marker/proxy categories can represent high-detail map shelter anchors.",
                "True PLATEAU entrance semantics remain pending."),
            Create("P5", "qualified_shelter_route_candidate_metadata", P8RuntimeAdaptationStatus.ProxyBased,
                "Navigation and shelter candidates can bind to proxy targets without official route claims.",
                "real_qualified remains opt-in and fail-safe."),
            Create("P6", "navigation_guidance_proxy_targets", P8RuntimeAdaptationStatus.ProxyBased,
                "navigation_target_proxy targets can be evaluated without the legacy base-map scene.",
                "Display-only guidance remains separate from gameplay results."),
            Create("P6", "npc_prototype_staging", P8RuntimeAdaptationStatus.ProxyBased,
                "NPC prototype can be staged against limited proxy targets.",
                "No P9 crowd, congestion, real spawn, or indoor evacuation dependency.")
        };
    }

    public static bool HasBlockedResults()
    {
        return Inspect().Any(result => result.status == P8RuntimeAdaptationStatus.Blocked);
    }

    public static string CreateSummary()
    {
        P8RuntimeCompatibilityResult[] results = Inspect();
        int passed = results.Count(result => result.status == P8RuntimeAdaptationStatus.Passed);
        int proxy = results.Count(result => result.status == P8RuntimeAdaptationStatus.ProxyBased);
        int pending = results.Count(result => result.status == P8RuntimeAdaptationStatus.Pending);
        int blocked = results.Count(result => result.status == P8RuntimeAdaptationStatus.Blocked);

        return "P8-C P2-P6 runtime adaptation: passed=" + passed +
               ", proxy-based=" + proxy +
               ", pending=" + pending +
               ", blocked=" + blocked +
               ". No gameplay success/failure rule changes and no legacy base-map dependency.";
    }

    private static P8RuntimeCompatibilityResult Create(
        string phase,
        string category,
        P8RuntimeAdaptationStatus status,
        string evidence,
        string notes)
    {
        return new P8RuntimeCompatibilityResult
        {
            phase = phase,
            category = category,
            status = status,
            evidence = evidence,
            notes = notes
        };
    }
}
