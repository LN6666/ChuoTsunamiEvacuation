using System;
using System.Text;

public static class P5CDecisionFeedbackFormatter
{
    private const int MaxResultWarnings = 2;

    public static bool ShouldShowForSourceType(string sourceType)
    {
        return string.Equals(sourceType, ShelterSourceConfigLoader.RealSampleSourceMode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceType, P5CStaticDataLoader.P5CSourceType, StringComparison.OrdinalIgnoreCase);
    }

    public static string BuildForShelterId(string shelterId)
    {
        return BuildForShelterId(shelterId, true);
    }

    public static string BuildForShelterId(string shelterId, bool includeUnavailableFallback)
    {
        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
        return BuildFromBundle(bundle, shelterId, includeUnavailableFallback);
    }

    public static string BuildFromBundle(
        P5CStaticDataLoader.P5CDataBundle bundle,
        string shelterId,
        bool includeUnavailableFallback)
    {
        if (bundle == null || !bundle.TryGetEvidenceForShelter(shelterId, out P5CStaticDataLoader.ShelterEvidence evidence))
        {
            return includeUnavailableFallback ? P5CStaticDataLoader.UnavailableMessage : string.Empty;
        }

        return BuildFromEvidence(evidence);
    }

    public static string BuildFromEvidence(P5CStaticDataLoader.ShelterEvidence evidence)
    {
        if (evidence == null || evidence.integratedQualification == null)
        {
            return P5CStaticDataLoader.UnavailableMessage;
        }

        P5CStaticDataLoader.IntegratedRouteQualificationRecord qualification =
            evidence.integratedQualification;

        string qualificationStatus = FirstNonEmpty(qualification.qualificationStatus, "unknown");
        string confidence = FirstNonEmpty(qualification.confidence, "unknown");
        string classification = P5CStaticDataLoader.ClassifyQualificationStatus(qualificationStatus);
        string distance = FormatDistance(qualification, evidence.routeSample);
        string estimatedTime = FormatEstimatedTime(qualification, evidence.routeSample);

        var builder = new StringBuilder();
        builder.AppendLine("Evidence qualification:");
        builder.AppendLine($"{qualificationStatus} ({classification}) / {confidence}");
        builder.AppendLine($"Manual review: {FormatBool(qualification.manualReviewNeeded)}");
        AppendWarningSummary(builder, qualification.warnings);
        builder.AppendLine();
        builder.AppendLine("Estimated prototype route:");
        builder.AppendLine($"{distance} / {estimatedTime}");
        builder.AppendLine(P5CStaticDataLoader.EstimatedPrototypeRouteLabel);
        builder.AppendLine("Not an official evacuation route.");
        builder.AppendLine("OSM/ODbL attribution applies.");

        return builder.ToString().TrimEnd();
    }

    private static void AppendWarningSummary(StringBuilder builder, string[] warnings)
    {
        if (warnings == null || warnings.Length == 0)
        {
            return;
        }

        int shown = 0;
        for (int i = 0; i < warnings.Length && shown < MaxResultWarnings; i++)
        {
            if (string.IsNullOrWhiteSpace(warnings[i]))
            {
                continue;
            }

            builder.AppendLine($"Warning: {warnings[i].Trim()}");
            shown++;
        }

        int remaining = CountWarnings(warnings) - shown;
        if (remaining > 0)
        {
            builder.AppendLine($"+{remaining} more warning{(remaining == 1 ? string.Empty : "s")}");
        }
    }

    private static int CountWarnings(string[] warnings)
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

    private static string FormatDistance(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord qualification,
        P5CStaticDataLoader.RouteSampleRecord route)
    {
        if (qualification != null && qualification.HasNearestRouteDistanceMeters)
        {
            return $"{qualification.nearestRouteDistanceMeters:0.#} m";
        }

        if (route != null && route.HasRouteDistanceMeters)
        {
            return $"{route.routeDistanceMeters:0.#} m";
        }

        return "distance unavailable";
    }

    private static string FormatEstimatedTime(
        P5CStaticDataLoader.IntegratedRouteQualificationRecord qualification,
        P5CStaticDataLoader.RouteSampleRecord route)
    {
        if (qualification != null && qualification.HasEstimatedTravelTimeSeconds)
        {
            return $"{qualification.estimatedTravelTimeSeconds:0.#} s";
        }

        if (route != null && route.HasEstimatedTravelTimeSeconds)
        {
            return $"{route.estimatedTravelTimeSeconds:0.#} s";
        }

        return "estimated time unavailable";
    }

    private static string FormatBool(bool value)
    {
        return value ? "yes" : "no";
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
