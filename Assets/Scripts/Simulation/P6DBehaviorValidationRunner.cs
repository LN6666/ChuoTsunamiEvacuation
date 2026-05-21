using System.Collections.Generic;
using UnityEngine;

public static class P6DBehaviorValidationRunner
{
    public static P6DBehaviorValidationResult Evaluate(
        NavigationGuidanceResult initialGuidance,
        NavigationGuidanceResult finalGuidance,
        IList<NpcEvacuationAgent> npcAgents,
        bool successFailureStateUntouched)
    {
        var result = new P6DBehaviorValidationResult
        {
            initialGuidanceDistanceMeters = GetDistance(initialGuidance),
            finalGuidanceDistanceMeters = GetDistance(finalGuidance),
            guidanceHasTarget = finalGuidance != null && finalGuidance.hasTarget,
            warningText = finalGuidance != null ? finalGuidance.statusText ?? string.Empty : string.Empty,
            navigationDisplayOnly = !NavigationGuidanceController.AffectsGameplayRules,
            npcDoesNotAffectPlayerSuccessFailure =
                !NpcEvacuationAgent.AffectsPlayerSuccessFailure &&
                !NpcEvacuationSpawner.AffectsPlayerSuccessFailure,
            successFailureStateUntouched = successFailureStateUntouched
        };

        result.guidanceDistanceChanged =
            IsUsableDistance(result.initialGuidanceDistanceMeters) &&
            IsUsableDistance(result.finalGuidanceDistanceMeters) &&
            !Mathf.Approximately(result.initialGuidanceDistanceMeters, result.finalGuidanceDistanceMeters);
        result.guidanceDistanceDecreased =
            result.guidanceDistanceChanged &&
            result.finalGuidanceDistanceMeters < result.initialGuidanceDistanceMeters;
        result.warningTextIncludesNotOfficialNavigation =
            Contains(result.warningText, NavigationGuidanceCalculator.NotOfficialNavigationWarning);
        result.warningTextIncludesNotOfficialEvacuationGuidance =
            Contains(result.warningText, NavigationGuidanceCalculator.NotOfficialEvacuationGuidanceWarning);
        result.warningTextAvoidsUnsafeOfficialNavigationClaim =
            !NavigationGuidanceCalculator.ContainsUnsafeOfficialNavigationClaim(result.warningText);

        PopulateNpcCounts(result, npcAgents);
        result.npcNonBlocking = AreNpcsNonBlocking(npcAgents);
        return result;
    }

    public static bool AreNpcsNonBlocking(IList<NpcEvacuationAgent> npcAgents)
    {
        if (npcAgents == null || npcAgents.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < npcAgents.Count; i++)
        {
            NpcEvacuationAgent agent = npcAgents[i];
            if (agent == null || !IsNpcNonBlocking(agent))
            {
                return false;
            }
        }

        return true;
    }

    private static void PopulateNpcCounts(P6DBehaviorValidationResult result, IList<NpcEvacuationAgent> npcAgents)
    {
        if (npcAgents == null)
        {
            return;
        }

        result.npcTotalCount = npcAgents.Count;
        for (int i = 0; i < npcAgents.Count; i++)
        {
            NpcEvacuationAgent agent = npcAgents[i];
            if (agent == null)
            {
                continue;
            }

            if (agent.CurrentTarget != null)
            {
                result.selectedTargetCount++;
            }

            if (agent.CurrentState == NpcEvacuationState.Arrived)
            {
                result.npcArrivedCount++;
            }
            else if (agent.CurrentState == NpcEvacuationState.MovingToTarget)
            {
                result.npcMovingCount++;
            }
            else if (agent.CurrentState == NpcEvacuationState.FailedNoTarget)
            {
                result.npcFailedNoTargetCount++;
            }
        }
    }

    private static bool IsNpcNonBlocking(NpcEvacuationAgent agent)
    {
        Collider[] colliders = agent.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled)
            {
                return false;
            }
        }

        Rigidbody[] rigidbodies = agent.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody body = rigidbodies[i];
            if (body != null && (!body.isKinematic || body.detectCollisions || body.useGravity))
            {
                return false;
            }
        }

        return true;
    }

    private static float GetDistance(NavigationGuidanceResult guidance)
    {
        return guidance != null ? guidance.distanceMeters : -1f;
    }

    private static bool IsUsableDistance(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static bool Contains(string text, string expected)
    {
        return !string.IsNullOrEmpty(text) &&
            !string.IsNullOrEmpty(expected) &&
            text.Contains(expected);
    }
}
