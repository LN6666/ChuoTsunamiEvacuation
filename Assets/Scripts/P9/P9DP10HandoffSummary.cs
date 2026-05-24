using System;

[Serializable]
public class P9DP10HandoffStatus
{
    public string schemaVersion = string.Empty;
    public bool p9Complete;
    public bool p9EfgCreated;
    public bool p10ReleasePackagingStarted;
    public bool windowsExeProfilingRequiredInP10 = true;
    public bool highDetailSceneArchiveRequiredInP10 = true;
    public bool officialInundationContourRefinementDeferredToP10 = true;
    public bool fullPlateauObjectSemanticCoverageDeferred = true;
    public bool officialRouteValidationDeferred = true;
    public string[] completedItems = Array.Empty<string>();
    public string[] p10KnownLimitations = Array.Empty<string>();
    public string[] futureResearchItems = Array.Empty<string>();
}

public static class P9DP10HandoffSummary
{
    public static P9DP10HandoffSummaryResult Create(
        P9DAnchoringReport anchoringReport,
        P9DFinalGameplayFlowSummary flowSummary,
        P9DP10HandoffStatus handoffStatus)
    {
        handoffStatus = handoffStatus ?? new P9DP10HandoffStatus();
        return new P9DP10HandoffSummaryResult
        {
            readyForP10 = handoffStatus.p9Complete &&
                !handoffStatus.p9EfgCreated &&
                !handoffStatus.p10ReleasePackagingStarted &&
                anchoringReport != null &&
                flowSummary != null &&
                flowSummary.success,
            p9Complete = handoffStatus.p9Complete,
            p9EfgCreated = handoffStatus.p9EfgCreated,
            p10ReleasePackagingStarted = handoffStatus.p10ReleasePackagingStarted,
            windowsExeProfilingRequiredInP10 = handoffStatus.windowsExeProfilingRequiredInP10,
            highDetailSceneArchiveRequiredInP10 = handoffStatus.highDetailSceneArchiveRequiredInP10,
            officialRouteValidationDeferred = handoffStatus.officialRouteValidationDeferred,
            fullPlateauObjectSemanticCoverageDeferred = handoffStatus.fullPlateauObjectSemanticCoverageDeferred,
            summary = "P9-D P10 handoff: P9 complete=" + handoffStatus.p9Complete +
                      ", readyForP10=" + (flowSummary != null && flowSummary.success) +
                      ", P10 packaging started=" + handoffStatus.p10ReleasePackagingStarted + "."
        };
    }
}

[Serializable]
public class P9DP10HandoffSummaryResult
{
    public bool readyForP10;
    public bool p9Complete;
    public bool p9EfgCreated;
    public bool p10ReleasePackagingStarted;
    public bool windowsExeProfilingRequiredInP10;
    public bool highDetailSceneArchiveRequiredInP10;
    public bool officialRouteValidationDeferred;
    public bool fullPlateauObjectSemanticCoverageDeferred;
    public string summary = string.Empty;
}
