using System.Text;

public class P6DBehaviorValidationResult
{
    public int selectedTargetCount;
    public int npcTotalCount;
    public int npcArrivedCount;
    public int npcMovingCount;
    public int npcFailedNoTargetCount;
    public float initialGuidanceDistanceMeters = -1f;
    public float finalGuidanceDistanceMeters = -1f;
    public bool guidanceHasTarget;
    public bool guidanceDistanceChanged;
    public bool guidanceDistanceDecreased;
    public bool warningTextIncludesNotOfficialNavigation;
    public bool warningTextIncludesNotOfficialEvacuationGuidance;
    public bool warningTextAvoidsUnsafeOfficialNavigationClaim;
    public bool navigationDisplayOnly;
    public bool npcDoesNotAffectPlayerSuccessFailure;
    public bool npcNonBlocking;
    public bool successFailureStateUntouched;
    public string warningText = string.Empty;

    public bool Passed
    {
        get
        {
            return guidanceHasTarget &&
                guidanceDistanceChanged &&
                guidanceDistanceDecreased &&
                warningTextIncludesNotOfficialNavigation &&
                warningTextIncludesNotOfficialEvacuationGuidance &&
                warningTextAvoidsUnsafeOfficialNavigationClaim &&
                navigationDisplayOnly &&
                npcDoesNotAffectPlayerSuccessFailure &&
                npcNonBlocking &&
                successFailureStateUntouched &&
                npcTotalCount > 0 &&
                selectedTargetCount > 0 &&
                npcArrivedCount > 0;
        }
    }

    public string ToSummaryText()
    {
        var builder = new StringBuilder();
        builder.AppendLine("P6-D behavior validation summary");
        builder.AppendLine($"- Selected target count: {selectedTargetCount}");
        builder.AppendLine($"- NPC total count: {npcTotalCount}");
        builder.AppendLine($"- NPC arrived count: {npcArrivedCount}");
        builder.AppendLine($"- NPC moving count: {npcMovingCount}");
        builder.AppendLine($"- NPC failed-no-target count: {npcFailedNoTargetCount}");
        builder.AppendLine($"- Guidance initial distance: {FormatDistance(initialGuidanceDistanceMeters)}");
        builder.AppendLine($"- Guidance final distance: {FormatDistance(finalGuidanceDistanceMeters)}");
        builder.AppendLine($"- Guidance distance decreased: {guidanceDistanceDecreased}");
        builder.AppendLine($"- Required warnings present: {warningTextIncludesNotOfficialNavigation && warningTextIncludesNotOfficialEvacuationGuidance}");
        builder.AppendLine($"- Navigation display-only: {navigationDisplayOnly}");
        builder.AppendLine($"- NPC non-blocking: {npcNonBlocking}");
        builder.AppendLine($"- Success/failure state untouched: {successFailureStateUntouched}");
        builder.AppendLine($"- Passed: {Passed}");
        return builder.ToString().TrimEnd();
    }

    private static string FormatDistance(float value)
    {
        return value >= 0f ? $"{value:0.###} m" : "unavailable";
    }
}
