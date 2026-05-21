using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class HumanitarianCandidateDataLoader
{
    public const string FileName = "p5g_highrise_humanitarian_candidates_sample.json";
    public const string SourceType = "p5g_humanitarian_candidate";
    public const string CandidateLayer = "humanitarian_candidate";
    public const string OfficialLayer = "official";
    public const string HumanitarianCandidateLabel = "Humanitarian emergency candidate";
    public const string NotOfficiallyDesignatedLabel = "Not officially designated";
    public const string NotOfficiallyDesignatedWarning =
        "This building is NOT officially designated as an evacuation shelter.";
    public const string ManualReviewNeededLabel = "Manual review needed";
    public const string AccessUncertainLabel = "Access uncertain";
    public const string ManagementUncertainLabel = "Management agreement uncertain";
    public const string SeismicUncertainLabel = "Seismic evidence uncertain";
    public const string LifeFirstAssumptionLabel = "Life-first scenario assumption";
    public const string NotLegalAccessGuaranteeLabel = "Not a legal/public access guarantee";
    public const string ControlledSampleNotice =
        "Controlled P5-F sample data; not full real Chuo high-rise screening.";
    public const string RouteAttributionNotice =
        "OSM/ODbL attribution applies where route fields use OSM-derived prototype routing.";

    private const float UnknownFloat = -1f;
    private const int UnknownInt = -1;

    public class HumanitarianCandidateLoadResult
    {
        public bool success;
        public bool usesAssetsDataOnly;
        public string sourcePath = string.Empty;
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = string.Empty;
        public string rulebookVersion = string.Empty;
        public string notes = string.Empty;
        public string diagnostics = string.Empty;
        public int rawRecordCount;
        public int recordCount;
        public int skippedNonHumanitarianLayerCount;
        public int skippedUnsafeConflictCount;
        public int skippedMalformedCount;
        public int selectableCount;
        public HumanitarianCandidateRecord[] records = new HumanitarianCandidateRecord[0];
    }

    public class HumanitarianCandidateRecord
    {
        public string candidateId = string.Empty;
        public string buildingId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string buildingName = string.Empty;
        public string address = string.Empty;
        public float latitude = UnknownFloat;
        public float longitude = UnknownFloat;
        public float heightMeters = UnknownFloat;
        public int floorsAboveGround = UnknownInt;
        public string usageType = string.Empty;
        public string buildingUse = string.Empty;
        public int estimatedCapacityProxy = UnknownInt;
        public float distanceToOfficialShelter = UnknownFloat;
        public float routeDistanceMeters = UnknownFloat;
        public float routeTimeSeconds = UnknownFloat;
        public string seismicEvidenceLevel = string.Empty;
        public string seismicEvidenceSource = string.Empty;
        public string publicAccessStatus = string.Empty;
        public string managementAgreementStatus = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string candidateLayer = string.Empty;
        public string humanitarianCandidateStatus = string.Empty;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] reviewTriggers = new string[0];
        public string[] reviewRisks = new string[0];
        public SourceRefRecord[] sourceRefs = new SourceRefRecord[0];
        public int physicalSuitabilityScore = UnknownInt;
        public int operationalUncertaintyScore = UnknownInt;
        public string classificationReason = string.Empty;
        public bool isDisplayable;
        public bool isSelectableInLifeFirstMode;
        public string nonSelectableReason = string.Empty;

        public bool HasRouteDistanceMeters => IsUsableNonNegativeFloat(routeDistanceMeters);
        public bool HasRouteTimeSeconds => IsUsableNonNegativeFloat(routeTimeSeconds);
    }

    [Serializable]
    public class SourceRefRecord
    {
        public string sourceId = string.Empty;
        public string sourceFamily = string.Empty;
        public string sourceName = string.Empty;
        public string officialStatus = string.Empty;
        public string sourceUrl = string.Empty;
        public string sourcePath = string.Empty;
        public string sourceUpdatedAt = string.Empty;
        public string evidenceNote = string.Empty;
    }

    [Serializable]
    private class CandidateDataset
    {
        public string datasetId = string.Empty;
        public string generatedAt = string.Empty;
        public string coordinateReferenceSystem = string.Empty;
        public string rulebookVersion = string.Empty;
        public string notes = string.Empty;
        public CandidateRecordRaw[] records = new CandidateRecordRaw[0];
    }

    [Serializable]
    private class CandidateRecordRaw
    {
        public string candidateId = string.Empty;
        public string buildingId = string.Empty;
        public string plateauBuildingId = string.Empty;
        public string buildingName = string.Empty;
        public string address = string.Empty;
        public float latitude = UnknownFloat;
        public float longitude = UnknownFloat;
        public float heightMeters = UnknownFloat;
        public int floorsAboveGround = UnknownInt;
        public string usageType = string.Empty;
        public string buildingUse = string.Empty;
        public int estimatedCapacityProxy = UnknownInt;
        public float distanceToOfficialShelter = UnknownFloat;
        public float routeDistanceMeters = UnknownFloat;
        public float routeTimeSeconds = UnknownFloat;
        public string seismicEvidenceLevel = string.Empty;
        public string seismicEvidenceSource = string.Empty;
        public string publicAccessStatus = string.Empty;
        public string managementAgreementStatus = string.Empty;
        public string officialDesignationStatus = string.Empty;
        public string candidateLayer = string.Empty;
        public string humanitarianCandidateStatus = string.Empty;
        public string confidence = string.Empty;
        public bool manualReviewNeeded;
        public string[] warnings = new string[0];
        public string[] reviewTriggers = new string[0];
        public string[] reviewRisks = new string[0];
        public SourceRefRecord[] sourceRefs = new SourceRefRecord[0];
        public int physicalSuitabilityScore = UnknownInt;
        public int operationalUncertaintyScore = UnknownInt;
        public string classificationReason = string.Empty;
    }

    public static HumanitarianCandidateLoadResult LoadFromAssetsData()
    {
        return LoadFromPath(Path.Combine(Application.dataPath, "Data", FileName));
    }

    public static HumanitarianCandidateLoadResult LoadFromPath(string path)
    {
        var result = new HumanitarianCandidateLoadResult
        {
            sourcePath = path ?? string.Empty,
            usesAssetsDataOnly = IsAssetsDataPath(path) && !IsForbiddenUnityRuntimePath(path)
        };

        if (!result.usesAssetsDataOnly)
        {
            result.diagnostics =
                "P5-GH humanitarian candidate runtime loading must use copied static JSON under Assets/Data only.";
            Debug.LogWarning(result.diagnostics);
            return result;
        }

        if (!File.Exists(path))
        {
            result.diagnostics = $"Missing P5-GH humanitarian candidate data at {path}.";
            Debug.LogWarning(result.diagnostics);
            return result;
        }

        try
        {
            string json = File.ReadAllText(path);
            HumanitarianCandidateLoadResult parsed = LoadFromJson(json, path);
            parsed.usesAssetsDataOnly = true;
            parsed.sourcePath = path;
            return parsed;
        }
        catch (Exception exception)
        {
            result.diagnostics = $"Could not load P5-GH humanitarian candidate data from {path}. {exception.Message}";
            Debug.LogWarning(result.diagnostics);
            return result;
        }
    }

    public static HumanitarianCandidateLoadResult LoadFromJson(string json, string sourceLabel)
    {
        var result = new HumanitarianCandidateLoadResult
        {
            sourcePath = sourceLabel ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            result.diagnostics = $"Empty P5-GH humanitarian candidate data at {result.sourcePath}.";
            Debug.LogWarning(result.diagnostics);
            return result;
        }

        try
        {
            var dataset = new CandidateDataset();
            JsonUtility.FromJsonOverwrite(NormalizeNullableNumbers(json), dataset);

            result.datasetId = Trim(dataset.datasetId);
            result.generatedAt = Trim(dataset.generatedAt);
            result.coordinateReferenceSystem = Trim(dataset.coordinateReferenceSystem);
            result.rulebookVersion = Trim(dataset.rulebookVersion);
            result.notes = Trim(dataset.notes);

            if (dataset.records == null || dataset.records.Length == 0)
            {
                result.diagnostics = $"{result.sourcePath} did not contain P5-GH candidate records.";
                Debug.LogWarning(result.diagnostics);
                return result;
            }

            result.rawRecordCount = dataset.records.Length;
            var records = new List<HumanitarianCandidateRecord>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (CandidateRecordRaw raw in dataset.records)
            {
                HumanitarianCandidateRecord mapped = MapRecord(raw, result);
                if (mapped == null)
                {
                    continue;
                }

                if (!seenIds.Add(mapped.candidateId))
                {
                    result.skippedMalformedCount++;
                    Debug.LogWarning($"{result.sourcePath} contained duplicate candidateId '{mapped.candidateId}'. The first record was used.");
                    continue;
                }

                records.Add(mapped);
                if (mapped.isSelectableInLifeFirstMode)
                {
                    result.selectableCount++;
                }
            }

            result.records = records.ToArray();
            result.recordCount = result.records.Length;
            result.success = result.recordCount > 0;
            if (!result.success)
            {
                result.diagnostics = "No displayable P5-GH humanitarian candidate records were available.";
            }

            return result;
        }
        catch (Exception exception)
        {
            result.diagnostics = $"Could not parse P5-GH humanitarian candidate data from {result.sourcePath}. {exception.Message}";
            Debug.LogWarning(result.diagnostics);
            return result;
        }
    }

    public static bool IsDefaultLifeFirstSelectableStatus(string status)
    {
        return string.Equals(status, "humanitarian_strong_candidate", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "humanitarian_candidate_with_review", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsForbiddenUnityRuntimePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        string normalized = path.Replace('\\', '/').ToLowerInvariant();
        return normalized.Contains("data_pipeline/") ||
            normalized.Contains("/raw/") ||
            normalized.Contains("/download/") ||
            normalized.Contains("/downloads/") ||
            normalized.Contains("/cache/") ||
            normalized.Contains("/tmp/") ||
            normalized.Contains("/.venv/") ||
            normalized.Contains("/osm_cache/");
    }

    private static HumanitarianCandidateRecord MapRecord(
        CandidateRecordRaw raw,
        HumanitarianCandidateLoadResult result)
    {
        if (raw == null)
        {
            result.skippedMalformedCount++;
            return null;
        }

        string candidateLayer = Trim(raw.candidateLayer);
        if (string.IsNullOrWhiteSpace(candidateLayer))
        {
            result.skippedMalformedCount++;
            WarnAndAppendDiagnostic(
                result,
                $"{result.sourcePath} skipped candidateId '{FirstNonEmpty(raw.candidateId, "unknown")}' because candidateLayer is missing; expected '{CandidateLayer}'.");
            return null;
        }

        if (!string.Equals(candidateLayer, CandidateLayer, StringComparison.OrdinalIgnoreCase))
        {
            result.skippedNonHumanitarianLayerCount++;
            WarnAndAppendDiagnostic(
                result,
                $"{result.sourcePath} skipped candidateId '{FirstNonEmpty(raw.candidateId, "unknown")}' because candidateLayer '{candidateLayer}' is not '{CandidateLayer}'.");
            return null;
        }

        string officialDesignation = Trim(raw.officialDesignationStatus);
        string candidateStatus = Trim(raw.humanitarianCandidateStatus);
        if (IsOfficialDesignationStatus(officialDesignation) || IsOfficialCandidateStatus(candidateStatus))
        {
            result.skippedUnsafeConflictCount++;
            return null;
        }

        string candidateId = Trim(raw.candidateId);
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            result.skippedMalformedCount++;
            return null;
        }

        var warnings = new List<string>();
        AddUnique(warnings, raw.warnings);
        AddUnique(warnings, "not an official shelter; humanitarian emergency candidate only");
        AddUnique(warnings, LifeFirstAssumptionLabel);
        AddUnique(warnings, NotLegalAccessGuaranteeLabel);
        AddUncertaintyWarnings(warnings, raw);

        var reviewRisks = new List<string>();
        AddUnique(reviewRisks, raw.reviewRisks);
        AddUncertaintyRiskCodes(reviewRisks, raw);
        AddUnique(reviewRisks, "non_official_status");
        AddUnique(reviewRisks, "life_first_access_not_legal_guarantee");

        var reviewTriggers = new List<string>();
        AddUnique(reviewTriggers, raw.reviewTriggers);
        AddUncertaintyReviewTriggers(reviewTriggers, raw);

        bool manualReview = raw.manualReviewNeeded || RequiresManualReview(raw);
        if (manualReview)
        {
            AddUnique(warnings, ManualReviewNeededLabel);
        }

        string nonSelectableReason = BuildNonSelectableReason(raw, candidateStatus);
        bool selectable = string.IsNullOrWhiteSpace(nonSelectableReason);

        return new HumanitarianCandidateRecord
        {
            candidateId = candidateId,
            buildingId = Trim(raw.buildingId),
            plateauBuildingId = Trim(raw.plateauBuildingId),
            buildingName = FirstNonEmpty(raw.buildingName, candidateId),
            address = Trim(raw.address),
            latitude = SanitizeOptionalFloat(raw.latitude),
            longitude = SanitizeOptionalFloat(raw.longitude),
            heightMeters = SanitizeOptionalFloat(raw.heightMeters),
            floorsAboveGround = SanitizeOptionalInt(raw.floorsAboveGround),
            usageType = Trim(raw.usageType),
            buildingUse = Trim(raw.buildingUse),
            estimatedCapacityProxy = SanitizeOptionalInt(raw.estimatedCapacityProxy),
            distanceToOfficialShelter = SanitizeOptionalFloat(raw.distanceToOfficialShelter),
            routeDistanceMeters = SanitizeOptionalFloat(raw.routeDistanceMeters),
            routeTimeSeconds = SanitizeOptionalFloat(raw.routeTimeSeconds),
            seismicEvidenceLevel = Trim(raw.seismicEvidenceLevel),
            seismicEvidenceSource = Trim(raw.seismicEvidenceSource),
            publicAccessStatus = Trim(raw.publicAccessStatus),
            managementAgreementStatus = Trim(raw.managementAgreementStatus),
            officialDesignationStatus = officialDesignation,
            candidateLayer = CandidateLayer,
            humanitarianCandidateStatus = candidateStatus,
            confidence = FirstNonEmpty(raw.confidence, "unknown"),
            manualReviewNeeded = manualReview,
            warnings = warnings.ToArray(),
            reviewTriggers = reviewTriggers.ToArray(),
            reviewRisks = reviewRisks.ToArray(),
            sourceRefs = raw.sourceRefs != null ? (SourceRefRecord[])raw.sourceRefs.Clone() : new SourceRefRecord[0],
            physicalSuitabilityScore = SanitizeOptionalInt(raw.physicalSuitabilityScore),
            operationalUncertaintyScore = SanitizeOptionalInt(raw.operationalUncertaintyScore),
            classificationReason = Trim(raw.classificationReason),
            isDisplayable = true,
            isSelectableInLifeFirstMode = selectable,
            nonSelectableReason = nonSelectableReason
        };
    }

    private static string BuildNonSelectableReason(CandidateRecordRaw raw, string candidateStatus)
    {
        if (!IsDefaultLifeFirstSelectableStatus(candidateStatus))
        {
            return $"humanitarianCandidateStatus '{candidateStatus}' is display-only by default";
        }

        if (string.IsNullOrWhiteSpace(raw.plateauBuildingId))
        {
            return "missing plateauBuildingId; unsafe candidate/building mapping";
        }

        if (string.IsNullOrWhiteSpace(raw.buildingName))
        {
            return "missing buildingName; unsafe candidate display";
        }

        if (raw.heightMeters <= 0f && raw.floorsAboveGround <= 0)
        {
            return "missing height/floor evidence; display-only until reviewed";
        }

        return string.Empty;
    }

    private static bool RequiresManualReview(CandidateRecordRaw raw)
    {
        return IsUnknownOrNotEvaluated(raw.publicAccessStatus) ||
            IsUnknownOrNotEvaluated(raw.managementAgreementStatus) ||
            string.Equals(Trim(raw.managementAgreementStatus), "no_agreement_found", StringComparison.OrdinalIgnoreCase) ||
            IsUnknownOrNotEvaluated(raw.seismicEvidenceLevel) ||
            string.Equals(Trim(raw.seismicEvidenceLevel), "estimated", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Trim(raw.seismicEvidenceLevel), "concern", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(raw.officialDesignationStatus) ||
            string.Equals(Trim(raw.officialDesignationStatus), "not_official", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Trim(raw.officialDesignationStatus), "unknown", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddUncertaintyWarnings(List<string> warnings, CandidateRecordRaw raw)
    {
        string publicAccess = Trim(raw.publicAccessStatus);
        if (IsUnknownOrNotEvaluated(publicAccess))
        {
            AddUnique(warnings, AccessUncertainLabel);
        }
        else if (string.Equals(publicAccess, "restricted_private", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(warnings, "Access restricted/private");
        }

        string management = Trim(raw.managementAgreementStatus);
        if (IsUnknownOrNotEvaluated(management) ||
            string.Equals(management, "no_agreement_found", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(warnings, ManagementUncertainLabel);
        }

        string seismic = Trim(raw.seismicEvidenceLevel);
        if (IsUnknownOrNotEvaluated(seismic) ||
            string.Equals(seismic, "estimated", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(seismic, "concern", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(warnings, SeismicUncertainLabel);
        }
    }

    private static void AddUncertaintyRiskCodes(List<string> reviewRisks, CandidateRecordRaw raw)
    {
        string publicAccess = Trim(raw.publicAccessStatus);
        if (IsUnknownOrNotEvaluated(publicAccess))
        {
            AddUnique(reviewRisks, "public_access_unknown");
        }
        else if (string.Equals(publicAccess, "restricted_private", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewRisks, "public_access_restricted");
        }

        string management = Trim(raw.managementAgreementStatus);
        if (IsUnknownOrNotEvaluated(management))
        {
            AddUnique(reviewRisks, "management_agreement_unknown");
        }
        else if (string.Equals(management, "no_agreement_found", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewRisks, "management_agreement_not_found");
        }

        string seismic = Trim(raw.seismicEvidenceLevel);
        if (IsUnknownOrNotEvaluated(seismic))
        {
            AddUnique(reviewRisks, "seismic_evidence_unknown");
        }
        else if (string.Equals(seismic, "estimated", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewRisks, "seismic_evidence_estimated");
        }
        else if (string.Equals(seismic, "concern", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewRisks, "seismic_concern");
        }
    }

    private static void AddUncertaintyReviewTriggers(List<string> reviewTriggers, CandidateRecordRaw raw)
    {
        if (IsUnknownOrNotEvaluated(raw.publicAccessStatus))
        {
            AddUnique(reviewTriggers, "public access unknown");
        }

        string management = Trim(raw.managementAgreementStatus);
        if (IsUnknownOrNotEvaluated(management))
        {
            AddUnique(reviewTriggers, "management agreement unknown");
        }
        else if (string.Equals(management, "no_agreement_found", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewTriggers, "management agreement not found");
        }

        string seismic = Trim(raw.seismicEvidenceLevel);
        if (IsUnknownOrNotEvaluated(seismic))
        {
            AddUnique(reviewTriggers, "seismic evidence unknown");
        }
        else if (string.Equals(seismic, "estimated", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewTriggers, "seismic evidence estimated");
        }
        else if (string.Equals(seismic, "concern", StringComparison.OrdinalIgnoreCase))
        {
            AddUnique(reviewTriggers, "seismic evidence concern");
        }
    }

    private static bool IsUnknownOrNotEvaluated(string value)
    {
        string clean = Trim(value);
        return string.IsNullOrWhiteSpace(clean) ||
            string.Equals(clean, "unknown", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(clean, "not_evaluated", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOfficialDesignationStatus(string status)
    {
        return string.Equals(status, "official_designated", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "official_designated_with_review", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOfficialCandidateStatus(string status)
    {
        return string.Equals(status, "official_confirmed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "official_confirmed_with_review", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAssetsDataPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            string fullPath = Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');
            string assetsDataPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Data"))
                .Replace('\\', '/')
                .TrimEnd('/');
            return fullPath.StartsWith(assetsDataPath + "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fullPath, assetsDataPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void AddUnique(List<string> destination, string[] values)
    {
        if (destination == null || values == null)
        {
            return;
        }

        foreach (string value in values)
        {
            AddUnique(destination, value);
        }
    }

    private static void AddUnique(List<string> destination, string value)
    {
        if (destination == null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string clean = value.Trim();
        foreach (string existing in destination)
        {
            if (string.Equals(existing, clean, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        destination.Add(clean);
    }

    private static void AppendDiagnostic(HumanitarianCandidateLoadResult result, string message)
    {
        if (result == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        result.diagnostics = string.IsNullOrWhiteSpace(result.diagnostics)
            ? message.Trim()
            : $"{result.diagnostics}\n{message.Trim()}";
    }

    private static void WarnAndAppendDiagnostic(HumanitarianCandidateLoadResult result, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        AppendDiagnostic(result, message);
        Debug.LogWarning(message);
    }

    private static string NormalizeNullableNumbers(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        return Regex.Replace(
            json,
            "\"(latitude|longitude|heightMeters|floorsAboveGround|estimatedCapacityProxy|distanceToOfficialShelter|routeDistanceMeters|routeTimeSeconds|physicalSuitabilityScore|operationalUncertaintyScore)\"\\s*:\\s*null",
            "\"$1\": -1");
    }

    private static float SanitizeOptionalFloat(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? UnknownFloat : value;
    }

    private static int SanitizeOptionalInt(int value)
    {
        return value < 0 ? UnknownInt : value;
    }

    private static bool IsUsableNonNegativeFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string Trim(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
