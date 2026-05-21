using System.Text;
using UnityEngine;

public class HumanitarianCandidateMetadata : MonoBehaviour
{
    [SerializeField] private string candidateId;
    [SerializeField] private string candidateLayer;
    [SerializeField] private string humanitarianCandidateStatus;
    [SerializeField] private string confidence;
    [SerializeField] private bool manualReviewNeeded;
    [SerializeField] private string publicAccessStatus;
    [SerializeField] private string managementAgreementStatus;
    [SerializeField] private string seismicEvidenceLevel;
    [SerializeField] private string officialDesignationStatus;
    [SerializeField] private string buildingName;
    [SerializeField] private string plateauBuildingId;
    [SerializeField] private float heightMeters = -1f;
    [SerializeField] private int floorsAboveGround = -1;
    [SerializeField] private string[] warnings = new string[0];
    [SerializeField] private string[] reviewRisks = new string[0];
    [SerializeField] private string nonSelectableReason;
    [SerializeField] private bool selectableInLifeFirstMode;

    public string CandidateId => candidateId;
    public string CandidateLayer => candidateLayer;
    public string HumanitarianCandidateStatus => humanitarianCandidateStatus;
    public string Confidence => confidence;
    public bool ManualReviewNeeded => manualReviewNeeded;
    public string PublicAccessStatus => publicAccessStatus;
    public string ManagementAgreementStatus => managementAgreementStatus;
    public string SeismicEvidenceLevel => seismicEvidenceLevel;
    public string OfficialDesignationStatus => officialDesignationStatus;
    public string BuildingName => buildingName;
    public string PlateauBuildingId => plateauBuildingId;
    public float HeightMeters => heightMeters;
    public int FloorsAboveGround => floorsAboveGround;
    public string[] Warnings => warnings;
    public string[] ReviewRisks => reviewRisks;
    public string NonSelectableReason => nonSelectableReason;
    public bool SelectableInLifeFirstMode => selectableInLifeFirstMode;

    public void ApplyRecord(HumanitarianCandidateDataLoader.HumanitarianCandidateRecord record)
    {
        if (record == null)
        {
            return;
        }

        candidateId = record.candidateId;
        candidateLayer = record.candidateLayer;
        humanitarianCandidateStatus = record.humanitarianCandidateStatus;
        confidence = record.confidence;
        manualReviewNeeded = record.manualReviewNeeded;
        publicAccessStatus = record.publicAccessStatus;
        managementAgreementStatus = record.managementAgreementStatus;
        seismicEvidenceLevel = record.seismicEvidenceLevel;
        officialDesignationStatus = record.officialDesignationStatus;
        buildingName = record.buildingName;
        plateauBuildingId = record.plateauBuildingId;
        heightMeters = record.heightMeters;
        floorsAboveGround = record.floorsAboveGround;
        warnings = record.warnings != null ? (string[])record.warnings.Clone() : new string[0];
        reviewRisks = record.reviewRisks != null ? (string[])record.reviewRisks.Clone() : new string[0];
        nonSelectableReason = record.nonSelectableReason;
        selectableInLifeFirstMode = record.isSelectableInLifeFirstMode;
    }

    public string BuildCompactLabel()
    {
        var builder = new StringBuilder();
        AppendLine(builder, FirstNonEmpty(buildingName, candidateId));
        AppendLine(builder, HumanitarianCandidateDataLoader.HumanitarianCandidateLabel);
        AppendLine(builder, HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel);
        AppendLine(builder, $"Review: {(manualReviewNeeded ? "yes" : "no")}");
        return builder.ToString().TrimEnd();
    }

    public string BuildDetailedLabel()
    {
        var builder = new StringBuilder();
        AppendLine(builder, BuildCompactLabel());
        AppendLine(builder, $"{FirstNonEmpty(humanitarianCandidateStatus, "unknown")} / {FirstNonEmpty(confidence, "unknown")}");
        AppendLine(builder, $"PLATEAU: {FirstNonEmpty(plateauBuildingId, "unknown")}");
        AppendLine(builder, $"Height/floors: {FormatHeight()} / {FormatFloors()}");
        AppendLine(builder, FormatAccessWarning());
        AppendLine(builder, FormatManagementWarning());
        AppendLine(builder, FormatSeismicWarning());
        AppendFirstWarnings(builder);
        AppendFirstReviewRisks(builder);
        AppendLine(builder, HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel);
        AppendLine(builder, HumanitarianCandidateDataLoader.NotLegalAccessGuaranteeLabel);
        AppendLine(builder, HumanitarianCandidateDataLoader.ControlledSampleNotice);
        return builder.ToString().TrimEnd();
    }

    public string BuildPromptText()
    {
        var builder = new StringBuilder();
        AppendLine(builder, HumanitarianCandidateDataLoader.HumanitarianCandidateLabel);
        AppendLine(builder, HumanitarianCandidateDataLoader.NotOfficiallyDesignatedLabel);
        AppendLine(builder, HumanitarianCandidateDataLoader.LifeFirstAssumptionLabel);
        AppendLine(builder, $"{FirstNonEmpty(humanitarianCandidateStatus, "unknown")} / {FirstNonEmpty(confidence, "unknown")}");
        AppendLine(builder, manualReviewNeeded
            ? HumanitarianCandidateDataLoader.ManualReviewNeededLabel
            : "Manual review: no");
        AppendLine(builder, FormatAccessWarning());
        AppendLine(builder, FormatManagementWarning());
        AppendLine(builder, FormatSeismicWarning());
        AppendLine(builder, HumanitarianCandidateDataLoader.NotLegalAccessGuaranteeLabel);
        return builder.ToString().TrimEnd();
    }

    private void AppendFirstWarnings(StringBuilder builder)
    {
        AppendFirstListItems(builder, "Warning", warnings, 2);
    }

    private void AppendFirstReviewRisks(StringBuilder builder)
    {
        AppendFirstListItems(builder, "Review risk", reviewRisks, 3);
    }

    private static void AppendFirstListItems(StringBuilder builder, string label, string[] values, int limit)
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

            AppendLine(builder, $"{label}: {value.Trim()}");
            shown++;
            if (shown >= limit)
            {
                break;
            }
        }

        int remaining = total - shown;
        if (remaining > 0)
        {
            AppendLine(builder, $"+{remaining} more {label.ToLowerInvariant()}{(remaining == 1 ? string.Empty : "s")}");
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

    private string FormatAccessWarning()
    {
        return IsUnknownOrNotEvaluated(publicAccessStatus) ||
            string.Equals(publicAccessStatus, "restricted_private", System.StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.AccessUncertainLabel
            : $"Access: {publicAccessStatus}";
    }

    private string FormatManagementWarning()
    {
        return IsUnknownOrNotEvaluated(managementAgreementStatus) ||
            string.Equals(managementAgreementStatus, "no_agreement_found", System.StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.ManagementUncertainLabel
            : $"Management: {managementAgreementStatus}";
    }

    private string FormatSeismicWarning()
    {
        return IsUnknownOrNotEvaluated(seismicEvidenceLevel) ||
            string.Equals(seismicEvidenceLevel, "estimated", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(seismicEvidenceLevel, "concern", System.StringComparison.OrdinalIgnoreCase)
            ? HumanitarianCandidateDataLoader.SeismicUncertainLabel
            : $"Seismic: {seismicEvidenceLevel}";
    }

    private string FormatHeight()
    {
        return IsUsableNonNegativeFloat(heightMeters) ? $"{heightMeters:0.#}m" : "unknown";
    }

    private string FormatFloors()
    {
        return floorsAboveGround >= 0 ? $"{floorsAboveGround}" : "unknown";
    }

    private static bool IsUnknownOrNotEvaluated(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
            string.Equals(value, "unknown", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "not_evaluated", System.StringComparison.OrdinalIgnoreCase);
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
