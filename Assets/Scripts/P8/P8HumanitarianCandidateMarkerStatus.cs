public static class P8HumanitarianCandidateMarkerStatus
{
    public const bool RequiresChuoBaseMap = false;
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool ImplementsP9SelectableGameplay = false;

    public static bool IsAllowedMarkerMode(string markerMode)
    {
        return markerMode == "named_marker" ||
               markerMode == "id_only_marker" ||
               markerMode == "cluster_marker" ||
               markerMode == "data_only_pending";
    }

    public static bool IsNonOfficialAndWarningRequired(P8HumanitarianCandidateMarkerRecord record)
    {
        return record != null &&
               !record.isOfficialShelter &&
               record.nonOfficialWarningRequired;
    }

    public static bool CanRenderAsPersistentMarker(P8HumanitarianCandidateMarkerRecord record)
    {
        return record != null &&
               IsAllowedMarkerMode(record.markerMode) &&
               record.hasCoordinates &&
               record.p8ePersistentVisibilityReady &&
               !record.selectableGameplayEnabled &&
               !record.affectsGameplaySuccessFailure &&
               IsNonOfficialAndWarningRequired(record);
    }
}
