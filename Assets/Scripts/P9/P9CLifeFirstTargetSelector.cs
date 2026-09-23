using System;
using System.Collections.Generic;

public static class P9CLifeFirstTargetSelector
{
    public static P9CVerticalEvacuationTargetDecision Select(P9CVerticalEvacuationTargetRules rules)
    {
        var rejected = new List<P9CRejectedTargetReason>();
        if (rules == null || rules.targets == null || rules.targets.Length == 0)
        {
            return CreateNoSelection(P9COutcomeReasonCode.FailedDueToNoSafeVerticalCandidate, rejected);
        }

        var eligible = new List<P9CScoredTarget>();
        for (int i = 0; i < rules.targets.Length; i++)
        {
            P9CVerticalEvacuationTargetRecord target = rules.targets[i];
            string rejectionCode = GetRejectionCode(target, rules);
            if (!string.IsNullOrWhiteSpace(rejectionCode))
            {
                rejected.Add(new P9CRejectedTargetReason
                {
                    targetId = target == null ? string.Empty : target.targetId,
                    reasonCode = rejectionCode,
                    message = "Target rejected by P9-C deterministic vertical evacuation rules."
                });
                continue;
            }

            eligible.Add(new P9CScoredTarget
            {
                target = target,
                score = ScoreTarget(target, rules.officialShelterAccessUnsafeOrUnavailable)
            });
        }

        if (eligible.Count == 0)
        {
            return CreateNoSelection(P9COutcomeReasonCode.FailedDueToNoSafeVerticalCandidate, rejected);
        }

        eligible.Sort(CompareScoredTargets);
        P9CVerticalEvacuationTargetRecord selected = SelectPreferredTarget(eligible, rules.officialShelterAccessUnsafeOrUnavailable);
        bool nonOfficial = selected.isHumanitarianCandidate && !selected.isOfficialShelter;

        return new P9CVerticalEvacuationTargetDecision
        {
            selected = true,
            selectedTargetId = selected.targetId ?? string.Empty,
            selectedProxyId = selected.proxyId ?? string.Empty,
            selectedDisplayName = selected.displayName ?? string.Empty,
            selectedTargetType = nonOfficial ? "life_first_humanitarian_candidate" : "official_shelter",
            isOfficialShelter = selected.isOfficialShelter,
            isHumanitarianCandidate = selected.isHumanitarianCandidate,
            nonOfficialWarningRequired = nonOfficial && selected.nonOfficialWarningRequired,
            remainsNonOfficialAndWarningRequired = !nonOfficial || (!selected.isOfficialShelter && selected.nonOfficialWarningRequired),
            safeApprovedByDefault = selected.safeApprovedByDefault,
            safeFloorStatus = selected.safeFloorStatus ?? string.Empty,
            entranceStatus = selected.entranceStatus ?? string.Empty,
            estimatedTravelTimeSeconds = Math.Max(0f, selected.estimatedTravelTimeSeconds),
            routeGuidanceStatus = selected.routeIsEstimatedPrototypeGuidance
                ? "estimated_prototype_guidance_not_official_route"
                : "route_guidance_unavailable",
            routeIsOfficial = selected.routeIsOfficial,
            routeIsEstimatedPrototypeGuidance = selected.routeIsEstimatedPrototypeGuidance,
            reasonCode = nonOfficial
                ? P9COutcomeReasonCode.SelectedLifeFirstVerticalCandidate
                : P9COutcomeReasonCode.SelectedOfficialShelter,
            warningText = nonOfficial
                ? P9CVerticalEvacuationTargetDecision.NonOfficialWarningText + " " + P9CVerticalEvacuationTargetDecision.LifeFirstUseText
                : string.Empty,
            rejectedTargets = rejected.ToArray()
        };
    }

    private static string GetRejectionCode(P9CVerticalEvacuationTargetRecord target, P9CVerticalEvacuationTargetRules rules)
    {
        if (target == null)
        {
            return P9COutcomeReasonCode.RejectedCandidateMissingSafeFloorProxy;
        }

        bool nonOfficial = target.isHumanitarianCandidate && !target.isOfficialShelter;
        if (nonOfficial)
        {
            if (!rules.allowLifeFirstCandidates || (rules.requireNonOfficialWarning && !target.nonOfficialWarningRequired))
            {
                return P9COutcomeReasonCode.RejectedNonOfficialCandidateUnsafeHazardState;
            }

            if (target.safeApprovedByDefault)
            {
                return P9COutcomeReasonCode.RejectedNonOfficialCandidateUnsafeHazardState;
            }
        }

        if (rules.requireSafeFloorProxy && !target.hasSafeFloorProxy)
        {
            return P9COutcomeReasonCode.RejectedCandidateMissingSafeFloorProxy;
        }

        if (rules.requireRouteGuidanceProxy && !target.routeGuidanceProxyAvailable)
        {
            return P9COutcomeReasonCode.RejectedCandidateMissingSafeFloorProxy;
        }

        if (IsBlockedEntrance(target.entranceStatus))
        {
            return nonOfficial
                ? P9COutcomeReasonCode.RejectedNonOfficialCandidateBlocked
                : P9COutcomeReasonCode.FailedDueToNoAvailableEntrance;
        }

        if (rules.rejectLowFloorWarning && target.lowFloorInundationWarning)
        {
            return P9COutcomeReasonCode.RejectedNonOfficialCandidateLowFloorWarning;
        }

        if (IsUnsafeHazardState(target.hazardState) || IsUnsafeHazardState(target.buildingDamageState))
        {
            return P9COutcomeReasonCode.RejectedNonOfficialCandidateUnsafeHazardState;
        }

        if (P9CSafeFloorEvaluator.IsTerminalFailureStatus(target.safeFloorStatus))
        {
            return string.Equals(target.safeFloorStatus, "below_required_height", StringComparison.OrdinalIgnoreCase)
                ? P9COutcomeReasonCode.SafeFloorBelowRequiredHeightFailure
                : P9COutcomeReasonCode.SafeFloorUnavailableFailure;
        }

        if (target.routeIsOfficial)
        {
            return P9COutcomeReasonCode.RejectedNonOfficialCandidateUnsafeHazardState;
        }

        return string.Empty;
    }

    private static bool IsBlockedEntrance(string status)
    {
        return string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "blocked_proxy", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnsafeHazardState(string status)
    {
        return string.Equals(status, "unsafe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "damaged_blocked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "inundated_proxy", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase);
    }

    private static float ScoreTarget(P9CVerticalEvacuationTargetRecord target, bool officialShelterAccessUnsafeOrUnavailable)
    {
        float travel = target.estimatedTravelTimeSeconds > 0f ? target.estimatedTravelTimeSeconds : target.distanceMeters / 1.2f;
        float score = 1000f - travel - target.crowdQueueLength * 4f + target.scenarioPriorityBonus;

        if (target.isOfficialShelter && !officialShelterAccessUnsafeOrUnavailable)
        {
            score += 250f;
        }

        if (target.isHumanitarianCandidate && officialShelterAccessUnsafeOrUnavailable)
        {
            score += 180f;
        }

        return score;
    }

    private static P9CVerticalEvacuationTargetRecord SelectPreferredTarget(
        List<P9CScoredTarget> eligible,
        bool officialShelterAccessUnsafeOrUnavailable)
    {
        if (!officialShelterAccessUnsafeOrUnavailable)
        {
            for (int i = 0; i < eligible.Count; i++)
            {
                if (eligible[i].target.isOfficialShelter)
                {
                    return eligible[i].target;
                }
            }
        }

        return eligible[0].target;
    }

    private static int CompareScoredTargets(P9CScoredTarget left, P9CScoredTarget right)
    {
        int scoreCompare = right.score.CompareTo(left.score);
        if (scoreCompare != 0)
        {
            return scoreCompare;
        }

        return string.CompareOrdinal(left.target.targetId, right.target.targetId);
    }

    private static P9CVerticalEvacuationTargetDecision CreateNoSelection(
        string reasonCode,
        List<P9CRejectedTargetReason> rejected)
    {
        return new P9CVerticalEvacuationTargetDecision
        {
            selected = false,
            reasonCode = reasonCode,
            rejectedTargets = rejected == null ? Array.Empty<P9CRejectedTargetReason>() : rejected.ToArray()
        };
    }

    private class P9CScoredTarget
    {
        public P9CVerticalEvacuationTargetRecord target;
        public float score;
    }
}
