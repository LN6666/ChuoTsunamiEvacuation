using System.Text;

public static class P9CResultPanelFeedbackFormatter
{
    public static string Format(P9COutcomeResult result)
    {
        if (result == null)
        {
            return "P9-C evacuation proxy feedback unavailable.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("P9-C evacuation proxy");
        builder.AppendLine("- Target type: " + EmptyFallback(result.selectedTargetType, "unknown"));
        builder.AppendLine("- Target ID: " + EmptyFallback(result.selectedTargetId, "none"));
        builder.AppendLine("- Official shelter: " + (result.isOfficialShelter ? "yes" : "no"));
        if (result.nonOfficialWarningRequired)
        {
            builder.AppendLine("- Warning: " + EmptyFallback(result.nonOfficialWarningText, P9CVerticalEvacuationTargetDecision.NonOfficialWarningText));
        }

        builder.AppendLine("- Entrance status: " + EmptyFallback(result.entranceStatus, "unknown"));
        builder.AppendLine("- Safe-floor status: " + EmptyFallback(result.safeFloorStatus, "unknown"));
        builder.AppendLine("- Queue delay: " + result.queueDelaySeconds.ToString("0.0") + "s");
        builder.AppendLine("- Crowd delay: " + result.congestionDelaySeconds.ToString("0.0") + "s");
        if (result.collapseDebrisExposureTriggered)
        {
            builder.AppendLine("- Collapse/debris exposure: " + (result.collapseDebrisFatality ? "fatality proxy" : "survived proxy"));
            builder.AppendLine("- Collapse/debris note: Gameplay-level collapse/debris proxy, not real structural simulation.");
        }

        builder.AppendLine("- Final reason code: " + EmptyFallback(result.finalReasonCode, "unknown"));
        return builder.ToString().TrimEnd();
    }

    private static string EmptyFallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
