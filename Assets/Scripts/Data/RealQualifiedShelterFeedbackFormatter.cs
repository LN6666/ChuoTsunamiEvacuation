using System;
using System.Text;
using UnityEngine;

public static class RealQualifiedShelterFeedbackFormatter
{
    private const int MaxResultWarnings = 2;

    public static string BuildForShelterId(string gameplayShelterId)
    {
        if (string.IsNullOrWhiteSpace(gameplayShelterId))
        {
            return RealQualifiedShelterDataLoader.UnavailableMessage;
        }

        try
        {
            RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult loadResult =
                RealQualifiedShelterDataLoader.LoadFromAssetsData();
            return BuildFromLoadResult(loadResult, gameplayShelterId, true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"P5-D real-qualified shelter feedback unavailable for '{gameplayShelterId}': {exception.Message}");
            return RealQualifiedShelterDataLoader.UnavailableMessage;
        }
    }

    public static string BuildFromLoadResult(
        RealQualifiedShelterDataLoader.RealQualifiedShelterLoadResult loadResult,
        string gameplayShelterId,
        bool includeUnavailableFallback)
    {
        if (string.IsNullOrWhiteSpace(gameplayShelterId))
        {
            return includeUnavailableFallback ? RealQualifiedShelterDataLoader.UnavailableMessage : string.Empty;
        }

        try
        {
            if (!RealQualifiedShelterDataLoader.TryFindRecord(
                    loadResult,
                    gameplayShelterId,
                    out RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record))
            {
                return includeUnavailableFallback ? RealQualifiedShelterDataLoader.UnavailableMessage : string.Empty;
            }

            return BuildFromRecord(record);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"P5-D real-qualified shelter feedback unavailable for '{gameplayShelterId}': {exception.Message}");
            return includeUnavailableFallback ? RealQualifiedShelterDataLoader.UnavailableMessage : string.Empty;
        }
    }

    public static string BuildFromRecord(RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record)
    {
        if (record == null)
        {
            return RealQualifiedShelterDataLoader.UnavailableMessage;
        }

        try
        {
            return BuildFromRecordUnchecked(record);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"P5-D real-qualified shelter feedback unavailable for '{record.gameplayShelterId}': {exception.Message}");
            return RealQualifiedShelterDataLoader.UnavailableMessage;
        }
    }

    private static string BuildFromRecordUnchecked(RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Real qualified shelter:");
        builder.AppendLine(FirstNonEmpty(record.displayName, record.officialShelterId));
        builder.AppendLine($"Qualification: {FirstNonEmpty(record.qualificationStatus, "unknown")} / {FirstNonEmpty(record.confidence, "unknown")}");
        builder.AppendLine($"Manual review: {FormatBool(record.manualReviewNeeded)}");
        AppendWarningSummary(builder, record.warnings);
        builder.AppendLine();
        builder.AppendLine("Estimated prototype route:");
        builder.AppendLine($"{FormatDistance(record)} / {FormatEstimatedTime(record)}");
        builder.AppendLine(P5CStaticDataLoader.EstimatedPrototypeRouteLabel);
        builder.AppendLine($"OSM/ODbL attribution: {FirstNonEmpty(record.attribution, "OpenStreetMap/ODbL attribution applies.")}");
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

    private static string FormatDistance(RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record)
    {
        return record != null && IsUsableNonNegativeFloat(record.routeDistanceMeters)
            ? $"{record.routeDistanceMeters:0.#} m"
            : "distance unavailable";
    }

    private static string FormatEstimatedTime(RealQualifiedShelterDataLoader.RealQualifiedShelterRecord record)
    {
        return record != null && IsUsableNonNegativeFloat(record.estimatedRouteTimeSeconds)
            ? $"{record.estimatedRouteTimeSeconds:0.#} s"
            : "estimated time unavailable";
    }

    private static bool IsUsableNonNegativeFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
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
