using System;
using System.Text;
using UnityEngine;

public static class HumanitarianCandidateFeedbackFormatter
{
    private const int MaxWarnings = 2;
    private const int MaxReviewRisks = 3;

    public static string BuildForCandidateId(string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return "P5-GH humanitarian candidate data unavailable for this candidate";
        }

        try
        {
            HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult loadResult =
                HumanitarianCandidateDataLoader.LoadFromAssetsData();
            return BuildFromLoadResult(loadResult, candidateId, true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"P5-GH humanitarian candidate feedback unavailable for '{candidateId}': {exception.Message}");
            return "P5-GH humanitarian candidate data unavailable for this candidate";
        }
    }

    public static string BuildFromLoadResult(
        HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult loadResult,
        string candidateId,
        bool includeUnavailableFallback)
    {
        if (loadResult == null || loadResult.records == null || string.IsNullOrWhiteSpace(candidateId))
        {
            return includeUnavailableFallback ? "P5-GH humanitarian candidate data unavailable for this candidate" : string.Empty;
        }

        foreach (HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record in loadResult.records)
        {
            if (record != null &&
                string.Equals(record.candidateId, candidateId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BuildFromRecord(record);
            }
        }

        return includeUnavailableFallback ? "P5-GH humanitarian candidate data unavailable for this candidate" : string.Empty;
    }

    public static string BuildFromRecord(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        if (record == null)
        {
            return "P5-GH humanitarian candidate data unavailable for this candidate";
        }

        var builder = new StringBuilder();
        builder.AppendLine(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedWarning);
        builder.AppendLine($"{HumanitarianCandidateDataLoader.HumanitarianCandidateLabel}:");
        builder.AppendLine(FirstNonEmpty(record.buildingName, record.candidateId));
        builder.AppendLine(HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel);
        builder.AppendLine(HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel);
        builder.AppendLine(HumanitarianCandidateDataLoader.NotLegalAccessGuaranteeLabel);
        builder.AppendLine($"{FirstNonEmpty(record.humanitarianCandidateStatus, "unknown")} / {FirstNonEmpty(record.confidence, "unknown")}");
        builder.AppendLine($"Manual review: {(record.manualReviewNeeded ? "yes" : "no")}");
        builder.AppendLine(FormatAccess(record));
        builder.AppendLine(FormatManagement(record));
        builder.AppendLine(FormatSeismic(record));
        AppendWarnings(builder, record.warnings);
        AppendReviewRisks(builder, record.reviewRisks);
        builder.AppendLine();
        builder.AppendLine("Route/candidate evidence:");
        builder.AppendLine($"{FormatDistance(record)} / {FormatTime(record)}");
        builder.AppendLine("Prototype/supporting evidence only; not an official evacuation route.");
        builder.AppendLine(HumanitarianCandidateDataLoader.RouteAttributionNotice);
        builder.AppendLine(HumanitarianCandidateDataLoader.ControlledSampleNotice);
        return builder.ToString().TrimEnd();
    }

    private static void AppendWarnings(StringBuilder builder, string[] warnings)
    {
        AppendList(builder, "Warning", warnings, MaxWarnings);
    }

    private static void AppendReviewRisks(StringBuilder builder, string[] reviewRisks)
    {
        AppendList(builder, "Review risk", reviewRisks, MaxReviewRisks);
    }

    private static void AppendList(StringBuilder builder, string label, string[] values, int limit)
    {
        if (values == null || values.Length == 0)
        {
            return;
        }

        int shown = 0;
        int total = CountNonEmpty(values);
        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.AppendLine($"{label}: {value.Trim()}");
            shown++;
            if (shown >= limit)
            {
                break;
            }
        }

        int remaining = total - shown;
        if (remaining > 0)
        {
            builder.AppendLine($"+{remaining} more {label.ToLowerInvariant()}{(remaining == 1 ? string.Empty : "s")}");
        }
    }

    private static int CountNonEmpty(string[] values)
    {
        int count = 0;
        if (values == null)
        {
            return count;
        }

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                count++;
            }
        }

        return count;
    }

    private static string FormatAccess(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        return IsUnknownOrNotEvaluated(record.publicAccessStatus) ||
            string.Equals(record.publicAccessStatus, "restricted_private", StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.AccessUncertainLabel
            : $"Access: {record.publicAccessStatus}";
    }

    private static string FormatManagement(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        return IsUnknownOrNotEvaluated(record.managementAgreementStatus) ||
            string.Equals(record.managementAgreementStatus, "no_agreement_found", StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.ManagementUncertainLabel
            : $"Management: {record.managementAgreementStatus}";
    }

    private static string FormatSeismic(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        return IsUnknownOrNotEvaluated(record.seismicEvidenceLevel) ||
            string.Equals(record.seismicEvidenceLevel, "estimated", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(record.seismicEvidenceLevel, "concern", StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.SeismicUncertainLabel
            : $"Seismic: {record.seismicEvidenceLevel}";
    }

    private static string FormatDistance(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        return record != null && record.HasRouteDistanceMeters
            ? $"{record.routeDistanceMeters:0.#} m"
            : "distance unavailable";
    }

    private static string FormatTime(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        return record != null && record.HasRouteTimeSeconds
            ? $"{record.routeTimeSeconds:0.#} s"
            : "estimated time unavailable";
    }

    private static bool IsUnknownOrNotEvaluated(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
            string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "not_evaluated", StringComparison.OrdinalIgnoreCase);
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
