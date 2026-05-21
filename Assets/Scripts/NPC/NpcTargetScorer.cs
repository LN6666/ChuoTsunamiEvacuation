using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcTargetScoringSettings
{
    public bool skipUnavailableTargets = true;
    public float distanceWeight = 1f;
    public float unavailablePenalty = 100000f;
    public float nonOfficialPenalty = 8f;
    public float humanitarianCandidatePenalty = 25f;
    public float manualReviewPenalty = 18f;
    public float warningPenalty = 6f;
    public float crowdingDelayWeight = 1.5f;
    public float entryDelayWeight = 0.5f;
    public float climbTimeWeight = 0.15f;
}

public struct NpcTargetScore
{
    public NpcEvacuationTargetInfo Target;
    public float Score;
}

public static class NpcTargetScorer
{
    public static bool TrySelectBestTarget(
        Vector3 npcPosition,
        IList<NpcEvacuationTargetInfo> targets,
        out NpcEvacuationTargetInfo selectedTarget,
        out float selectedScore)
    {
        return TrySelectBestTarget(npcPosition, targets, null, out selectedTarget, out selectedScore);
    }

    public static bool TrySelectBestTarget(
        Vector3 npcPosition,
        IList<NpcEvacuationTargetInfo> targets,
        NpcTargetScoringSettings settings,
        out NpcEvacuationTargetInfo selectedTarget,
        out float selectedScore)
    {
        selectedTarget = null;
        selectedScore = float.PositiveInfinity;

        if (targets == null || targets.Count == 0)
        {
            return false;
        }

        bool found = false;
        for (int i = 0; i < targets.Count; i++)
        {
            if (!TryScoreTarget(npcPosition, targets[i], settings, out float score))
            {
                continue;
            }

            if (!found || score < selectedScore)
            {
                selectedTarget = targets[i];
                selectedScore = score;
                found = true;
            }
        }

        return found;
    }

    public static bool TryScoreTarget(
        Vector3 npcPosition,
        NpcEvacuationTargetInfo target,
        NpcTargetScoringSettings settings,
        out float score)
    {
        score = float.PositiveInfinity;

        if (target == null)
        {
            return false;
        }

        NpcTargetScoringSettings safeSettings = settings ?? new NpcTargetScoringSettings();
        bool available = target.IsAvailable;
        if (safeSettings.skipUnavailableTargets && !available)
        {
            return false;
        }

        float distance = Vector3.Distance(npcPosition, target.GetCurrentPosition());
        score = distance * NonNegative(safeSettings.distanceWeight);

        if (!available)
        {
            score += NonNegative(safeSettings.unavailablePenalty);
        }

        if (!target.isOfficialShelter)
        {
            score += NonNegative(safeSettings.nonOfficialPenalty);
        }

        if (target.isHumanitarianCandidate)
        {
            score += NonNegative(safeSettings.humanitarianCandidatePenalty);
        }

        if (target.manualReviewNeeded)
        {
            score += NonNegative(safeSettings.manualReviewPenalty);
        }

        score += Mathf.Max(0, target.warningCount) * NonNegative(safeSettings.warningPenalty);
        score += NonNegative(target.crowdingDelaySeconds) * NonNegative(safeSettings.crowdingDelayWeight);
        score += NonNegative(target.entryDelaySeconds) * NonNegative(safeSettings.entryDelayWeight);
        score += NonNegative(target.climbTimeSeconds) * NonNegative(safeSettings.climbTimeWeight);
        return !float.IsNaN(score) && !float.IsInfinity(score);
    }

    private static float NonNegative(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
        {
            return 0f;
        }

        return value;
    }
}
