using System;

public static class P9P8HandoffAdapter
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresP8DEFinalHandoff = P9RuntimePolicy.RequiresP8DEFinalHandoff;
    public const bool UsesNoEffectFallbackWhenMissing = true;

    public static P9P8HandoffState CreateMissingHandoffDefault()
    {
        return new P9P8HandoffState
        {
            success = true,
            failSafe = false,
            isMissingOrFallback = true,
            infrastructureHazardState = "unknown_no_effect",
            roadBridgeUndergroundState = "unknown_no_effect",
            buildingWarningState = "unknown_no_effect",
            entranceBlockedState = false,
            lowFloorInundationWarning = false,
            collapseDamageProxyState = "unknown_no_effect",
            hazardSourceMode = string.Empty,
            evidenceSourceId = string.Empty,
            depthStatus = "unknown",
            intensityStatus = "unknown",
            arrivalTimingStatus = "unknown",
            summary = "P8-D/E handoff missing. P9-A uses no-effect fallback state."
        };
    }

    public static P9P8HandoffState Adapt(P9P8HandoffInput input)
    {
        if (input == null)
        {
            return CreateMissingHandoffDefault();
        }

        return new P9P8HandoffState
        {
            success = true,
            failSafe = false,
            isMissingOrFallback = input.isMissingOrFallback,
            infrastructureHazardState = EmptyToFallback(input.infrastructureHazardState),
            roadBridgeUndergroundState = EmptyToFallback(input.roadBridgeUndergroundState),
            buildingWarningState = EmptyToFallback(input.buildingWarningState),
            entranceBlockedState = input.entranceBlockedState,
            lowFloorInundationWarning = input.lowFloorInundationWarning,
            collapseDamageProxyState = EmptyToFallback(input.collapseDamageProxyState),
            hazardSourceMode = input.hazardSourceMode ?? string.Empty,
            evidenceSourceId = input.evidenceSourceId ?? string.Empty,
            depthStatus = EmptyToUnknown(input.depthStatus),
            intensityStatus = EmptyToUnknown(input.intensityStatus),
            arrivalTimingStatus = EmptyToUnknown(input.arrivalTimingStatus),
            summary = "P8 handoff adapted for P9 proxy state. P9-A records it without gameplay failure effect."
        };
    }

    private static string EmptyToFallback(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown_no_effect" : value;
    }

    private static string EmptyToUnknown(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value;
    }
}

[Serializable]
public class P9P8HandoffInput
{
    public bool isMissingOrFallback;
    public string infrastructureHazardState = string.Empty;
    public string roadBridgeUndergroundState = string.Empty;
    public string buildingWarningState = string.Empty;
    public bool entranceBlockedState;
    public bool lowFloorInundationWarning;
    public string collapseDamageProxyState = string.Empty;
    public string hazardSourceMode = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string depthStatus = string.Empty;
    public string intensityStatus = string.Empty;
    public string arrivalTimingStatus = string.Empty;
}

[Serializable]
public class P9P8HandoffState
{
    public bool success;
    public bool failSafe = true;
    public bool isMissingOrFallback = true;
    public bool affectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public bool requiresP8DEFinalHandoff = P9RuntimePolicy.RequiresP8DEFinalHandoff;
    public string infrastructureHazardState = "unknown_no_effect";
    public string roadBridgeUndergroundState = "unknown_no_effect";
    public string buildingWarningState = "unknown_no_effect";
    public bool entranceBlockedState;
    public bool lowFloorInundationWarning;
    public string collapseDamageProxyState = "unknown_no_effect";
    public string hazardSourceMode = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string depthStatus = "unknown";
    public string intensityStatus = "unknown";
    public string arrivalTimingStatus = "unknown";
    public string summary = string.Empty;
}
