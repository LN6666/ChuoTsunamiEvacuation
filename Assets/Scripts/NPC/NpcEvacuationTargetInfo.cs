using System;
using UnityEngine;

[Serializable]
public class NpcEvacuationTargetInfo
{
    public string targetId;
    public string displayName;
    public Vector3 position;
    public Transform targetTransform;
    public bool canEnter = true;
    public bool isSelectable = true;
    public string postEarthquakeStatus = "usable";
    public bool isOfficialShelter;
    public bool isHumanitarianCandidate;
    public bool manualReviewNeeded;
    public int warningCount;
    public float entryDelaySeconds;
    public float climbTimeSeconds;
    public float crowdingDelaySeconds;
    public string sourceType;
    public string note;

    public bool IsAvailable => isSelectable && canEnter && IsUsableStatus(postEarthquakeStatus);

    public Vector3 GetCurrentPosition()
    {
        return targetTransform != null ? targetTransform.position : position;
    }

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        return string.IsNullOrWhiteSpace(targetId) ? "Unnamed target" : targetId.Trim();
    }

    public static NpcEvacuationTargetInfo FromBuildingShelter(BuildingShelter shelter)
    {
        if (shelter == null)
        {
            return null;
        }

        var target = new NpcEvacuationTargetInfo
        {
            targetId = shelter.ShelterId,
            displayName = shelter.ShelterName,
            position = shelter.transform.position,
            targetTransform = shelter.transform,
            canEnter = shelter.CanEnter,
            isSelectable = true,
            postEarthquakeStatus = shelter.PostEarthquakeStatus,
            isOfficialShelter = shelter.IsOfficialShelter,
            entryDelaySeconds = shelter.EntryDelaySeconds,
            climbTimeSeconds = shelter.ClimbTimeSeconds,
            crowdingDelaySeconds = shelter.CrowdingDelaySeconds,
            sourceType = shelter.SourceType
        };

        ApplyRealQualifiedMetadata(shelter, target);
        ApplyHumanitarianCandidateMetadata(shelter, target);
        return target;
    }

    public static bool IsUsableStatus(string status)
    {
        return string.IsNullOrWhiteSpace(status) ||
            string.Equals(status, "usable", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyRealQualifiedMetadata(BuildingShelter shelter, NpcEvacuationTargetInfo target)
    {
        RealQualifiedShelterMetadata metadata = shelter.GetComponent<RealQualifiedShelterMetadata>();
        if (metadata == null)
        {
            return;
        }

        target.manualReviewNeeded = target.manualReviewNeeded || metadata.ManualReviewNeeded;
        target.warningCount += CountNonEmpty(metadata.Warnings);

        if (string.Equals(target.sourceType, RealQualifiedShelterDataLoader.SourceType, StringComparison.OrdinalIgnoreCase))
        {
            target.isSelectable = metadata.IsSelectable;
        }
    }

    private static void ApplyHumanitarianCandidateMetadata(BuildingShelter shelter, NpcEvacuationTargetInfo target)
    {
        HumanitarianCandidateMetadata metadata = shelter.GetComponent<HumanitarianCandidateMetadata>();
        if (metadata == null)
        {
            target.isHumanitarianCandidate =
                string.Equals(target.sourceType, HumanitarianCandidateDataLoader.SourceType, StringComparison.OrdinalIgnoreCase);
            return;
        }

        target.isHumanitarianCandidate = true;
        target.manualReviewNeeded = target.manualReviewNeeded || metadata.ManualReviewNeeded;
        target.warningCount += CountNonEmpty(metadata.Warnings) + CountNonEmpty(metadata.ReviewRisks);

        if (string.Equals(target.sourceType, HumanitarianCandidateDataLoader.SourceType, StringComparison.OrdinalIgnoreCase))
        {
            target.isSelectable = metadata.SelectableInLifeFirstMode;
        }
    }

    private static int CountNonEmpty(string[] values)
    {
        if (values == null)
        {
            return 0;
        }

        int count = 0;
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                count++;
            }
        }

        return count;
    }
}
