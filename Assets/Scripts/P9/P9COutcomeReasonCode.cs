using System;

public static class P9COutcomeReasonCode
{
    public const string SelectedOfficialShelter = "selected_official_shelter";
    public const string SelectedLifeFirstVerticalCandidate = "selected_life_first_vertical_candidate";
    public const string RejectedNonOfficialCandidateBlocked = "rejected_non_official_candidate_blocked";
    public const string RejectedNonOfficialCandidateLowFloorWarning = "rejected_non_official_candidate_low_floor_warning";
    public const string RejectedNonOfficialCandidateUnsafeHazardState = "rejected_non_official_candidate_unsafe_hazard_state";
    public const string RejectedCandidateMissingSafeFloorProxy = "rejected_candidate_missing_safe_floor_proxy";

    public const string EntranceOpen = "entrance_open";
    public const string EntranceCrowdedDelay = "entrance_crowded_delay";
    public const string EntranceBlockedFailure = "entrance_blocked_failure";
    public const string CrowdCongestionDelay = "crowd_congestion_delay";
    public const string QueueDelayApplied = "queue_delay_applied";
    public const string EntranceHazardAffected = "entrance_hazard_affected";

    public const string VerticalEvacuationCompleteProxy = "vertical_evacuation_complete_proxy";
    public const string SafeFloorAvailableSuccess = "safe_floor_available_success";
    public const string SafeFloorUnavailableFailure = "safe_floor_unavailable_failure";
    public const string SafeFloorUnknownWarning = "safe_floor_unknown_warning";
    public const string SafeFloorBelowRequiredHeightFailure = "safe_floor_below_required_height_failure";
    public const string VerticalEvacuationDelayedByQueue = "vertical_evacuation_delayed_by_queue";
    public const string VerticalEvacuationDelayedByCrowd = "vertical_evacuation_delayed_by_crowd";

    public const string DelayedByCrowdCongestion = "delayed_by_crowd_congestion";
    public const string FailedDueToCrowdDelay = "failed_due_to_crowd_delay";
    public const string FailedDueToNoAvailableEntrance = "failed_due_to_no_available_entrance";
    public const string FailedDueToNoSafeVerticalCandidate = "failed_due_to_no_safe_vertical_candidate";

    public const string CollapseDebrisExposureEvent = "collapse_debris_exposure_event";
    public const string CollapseDebrisFatalityProxy = "collapse_debris_fatality_proxy";
    public const string KilledByBuildingCollapseProxy = "killed_by_building_collapse_proxy";
    public const string SurvivedCollapseDebrisExposure = "survived_collapse_debris_exposure";
    public const string CollapseDebrisProxyDisabled = "collapse_debris_proxy_disabled";

    public const string FailedDueToTsunamiArrival = "failed_due_to_tsunami_arrival";
    public const string FailedDueToHazardReachingEntrance = "failed_due_to_hazard_reaching_entrance";
    public const string DelayedSuccessBeforeHazardArrival = "delayed_success_before_hazard_arrival";
    public const string VerticalEvacuationCompletedBeforeArrival = "vertical_evacuation_completed_before_arrival";
    public const string VerticalEvacuationFailedAfterArrival = "vertical_evacuation_failed_after_arrival";

    public static readonly string[] All =
    {
        SelectedOfficialShelter,
        SelectedLifeFirstVerticalCandidate,
        RejectedNonOfficialCandidateBlocked,
        RejectedNonOfficialCandidateLowFloorWarning,
        RejectedNonOfficialCandidateUnsafeHazardState,
        RejectedCandidateMissingSafeFloorProxy,
        EntranceOpen,
        EntranceCrowdedDelay,
        EntranceBlockedFailure,
        CrowdCongestionDelay,
        QueueDelayApplied,
        EntranceHazardAffected,
        VerticalEvacuationCompleteProxy,
        SafeFloorAvailableSuccess,
        SafeFloorUnavailableFailure,
        SafeFloorUnknownWarning,
        SafeFloorBelowRequiredHeightFailure,
        VerticalEvacuationDelayedByQueue,
        VerticalEvacuationDelayedByCrowd,
        DelayedByCrowdCongestion,
        FailedDueToCrowdDelay,
        FailedDueToNoAvailableEntrance,
        FailedDueToNoSafeVerticalCandidate,
        CollapseDebrisExposureEvent,
        CollapseDebrisFatalityProxy,
        KilledByBuildingCollapseProxy,
        SurvivedCollapseDebrisExposure,
        CollapseDebrisProxyDisabled,
        FailedDueToTsunamiArrival,
        FailedDueToHazardReachingEntrance,
        DelayedSuccessBeforeHazardArrival,
        VerticalEvacuationCompletedBeforeArrival,
        VerticalEvacuationFailedAfterArrival
    };

    public static bool IsKnown(string reasonCode)
    {
        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return false;
        }

        for (int i = 0; i < All.Length; i++)
        {
            if (string.Equals(All[i], reasonCode, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
