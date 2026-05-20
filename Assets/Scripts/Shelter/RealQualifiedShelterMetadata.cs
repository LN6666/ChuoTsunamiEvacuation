using System.Text;
using UnityEngine;

public class RealQualifiedShelterMetadata : MonoBehaviour
{
    [SerializeField] private string gameplayShelterId;
    [SerializeField] private string officialShelterId;
    [SerializeField] private string displayName;
    [SerializeField] private string plateauBuildingId;
    [SerializeField] private string qualificationStatus;
    [SerializeField] private string confidence;
    [SerializeField] private bool manualReviewNeeded;
    [SerializeField] private string[] warnings = new string[0];
    [SerializeField] private string routeTargetId;
    [SerializeField] private float routeDistanceMeters = -1f;
    [SerializeField] private float estimatedRouteTimeSeconds = -1f;
    [SerializeField] private bool isSelectable;
    [SerializeField] private string routeDisclaimer;
    [SerializeField] private string attribution;
    [SerializeField] private string placementNote;

    public string GameplayShelterId => gameplayShelterId;
    public string OfficialShelterId => officialShelterId;
    public string DisplayName => displayName;
    public string PlateauBuildingId => plateauBuildingId;
    public string QualificationStatus => qualificationStatus;
    public string Confidence => confidence;
    public bool ManualReviewNeeded => manualReviewNeeded;
    public string[] Warnings => warnings;
    public string RouteTargetId => routeTargetId;
    public float RouteDistanceMeters => routeDistanceMeters;
    public float EstimatedRouteTimeSeconds => estimatedRouteTimeSeconds;
    public bool IsSelectable => isSelectable;
    public string RouteDisclaimer => routeDisclaimer;
    public string Attribution => attribution;
    public string PlacementNote => placementNote;

    public void ApplyRecord(RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record)
    {
        if (record == null)
        {
            return;
        }

        gameplayShelterId = record.gameplayShelterId;
        officialShelterId = record.officialShelterId;
        displayName = record.displayName;
        plateauBuildingId = record.plateauBuildingId;
        qualificationStatus = record.qualificationStatus;
        confidence = record.confidence;
        manualReviewNeeded = record.manualReviewNeeded;
        warnings = record.warnings != null ? (string[])record.warnings.Clone() : new string[0];
        routeTargetId = record.routeTargetId;
        routeDistanceMeters = record.routeDistanceMeters;
        estimatedRouteTimeSeconds = record.estimatedRouteTimeSeconds;
        isSelectable = record.isSelectable;
        routeDisclaimer = record.routeDisclaimer;
        attribution = record.attribution;
        placementNote = RealQualifiedShelterDataLoader.NoVerifiedGeospatialPlacementNote;
    }

    public string BuildCompactLabel()
    {
        var builder = new StringBuilder();
        AppendLine(builder, FirstNonEmpty(displayName, gameplayShelterId));
        AppendLine(builder, $"{FirstNonEmpty(qualificationStatus, "unknown")} / {FirstNonEmpty(confidence, "unknown")}");
        AppendLine(builder, $"Review: {(manualReviewNeeded ? "yes" : "no")}");
        AppendLine(builder, FormatRouteSummary());
        return builder.ToString().TrimEnd();
    }

    public string BuildDetailedLabel()
    {
        var builder = new StringBuilder();
        AppendLine(builder, BuildCompactLabel());
        AppendLine(builder, $"Official ID: {officialShelterId}");
        AppendLine(builder, $"PLATEAU: {plateauBuildingId}");
        AppendFirstWarnings(builder);
        AppendLine(builder, routeDisclaimer);
        AppendLine(builder, "OSM/ODbL attribution applies.");
        AppendLine(builder, placementNote);
        return builder.ToString().TrimEnd();
    }

    public string BuildPromptText()
    {
        var builder = new StringBuilder();
        AppendLine(builder, $"Qualification: {FirstNonEmpty(qualificationStatus, "unknown")} / {FirstNonEmpty(confidence, "unknown")}");
        AppendLine(builder, $"Manual review: {(manualReviewNeeded ? "yes" : "no")}");
        AppendLine(builder, FormatRouteSummary());
        AppendLine(builder, routeDisclaimer);
        return builder.ToString().TrimEnd();
    }

    private void AppendFirstWarnings(StringBuilder builder)
    {
        if (warnings == null || warnings.Length == 0)
        {
            return;
        }

        int shown = 0;
        foreach (string warning in warnings)
        {
            if (string.IsNullOrWhiteSpace(warning))
            {
                continue;
            }

            AppendLine(builder, $"Warning: {warning.Trim()}");
            shown++;
            if (shown >= 2)
            {
                break;
            }
        }

        int remaining = CountWarnings() - shown;
        if (remaining > 0)
        {
            AppendLine(builder, $"+{remaining} more warning{(remaining == 1 ? string.Empty : "s")}");
        }
    }

    private int CountWarnings()
    {
        if (warnings == null)
        {
            return 0;
        }

        int count = 0;
        foreach (string warning in warnings)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                count++;
            }
        }

        return count;
    }

    private string FormatRouteSummary()
    {
        string distance = IsUsableNonNegativeFloat(routeDistanceMeters) ? $"{routeDistanceMeters:0.#}m" : "distance unavailable";
        string time = IsUsableNonNegativeFloat(estimatedRouteTimeSeconds) ? $"{estimatedRouteTimeSeconds:0.#}s" : "time unavailable";
        return $"Route: {distance} / {time}";
    }

    private static bool IsUsableNonNegativeFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static void AppendLine(StringBuilder builder, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine(value.Trim());
        }
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
