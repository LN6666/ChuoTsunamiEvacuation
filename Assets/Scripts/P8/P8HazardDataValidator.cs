using System;
using System.Collections.Generic;

public static class P8HazardDataValidator
{
    public const float CinematicHeightThresholdMeters = 10f;

    private static readonly string[] RequiredScienceFields =
    {
        "arrivalTimeSeconds",
        "inundationDepthMeters",
        "waterLevelMeters",
        "tsunamiHeightMeters",
        "inundationBoundary",
        "hazardIntensity",
        "confidence",
        "evidenceSourceId"
    };

    private static readonly string[] RequiredVisualFields =
    {
        "visualHeightMeters",
        "visualHeightIsCinematicOnly"
    };

    public static P8HazardValidationResult ValidateHazardLayer(P8HazardLayerData data)
    {
        var result = new P8HazardValidationResult();

        if (data == null)
        {
            result.AddError("Hazard layer is null.");
            result.MarkFailSafe();
            result.Finish();
            return result;
        }

        ValidateScenarioFields(data.scenarioId, data.sourceMode, data.hazardLayerVersion, result);
        ValidateLayerFieldSeparation(data.scienceLayerFields, data.visualLayerFields, "hazard layer", result);
        HashSet<string> evidenceSourceIds = ValidateEvidenceSources(data.evidenceSources, data.sourceMode, result);
        ValidateLayerSpatialGateMetadata(data, result);

        if (data.timeOriginSeconds < 0f)
        {
            result.AddError("timeOriginSeconds must be zero or positive.");
        }

        if (data.features == null || data.features.Length == 0)
        {
            result.AddError("Hazard layer must contain at least one feature.");
            result.MarkFailSafe();
        }
        else
        {
            for (int i = 0; i < data.features.Length; i++)
            {
                ValidateFeature(data.features[i], i, data.sourceMode, evidenceSourceIds, result);
            }
        }

        result.checkedFeatureCount = data.features == null ? 0 : data.features.Length;
        result.Finish();
        return result;
    }

    public static P8HazardValidationResult ValidateP8CSpatialGate(P8HazardLayerData data, bool userAcceptedProxyManualLayer)
    {
        P8HazardValidationResult result = ValidateHazardLayer(data);
        if (data == null)
        {
            return result;
        }

        string gateDecision = data.p8cGateDecision ?? string.Empty;
        bool extracted = string.Equals(data.extractionStatus, "extracted", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(data.spatialExtractionStatus, "extracted", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(gateDecision, "PASS", StringComparison.OrdinalIgnoreCase))
        {
            if (!extracted || !data.completeOfficialSpatialLayerExtracted)
            {
                result.AddError("P8-C gate PASS requires extractionStatus/spatialExtractionStatus=extracted and completeOfficialSpatialLayerExtracted=true.");
                result.MarkFailSafe();
            }
        }
        else if (string.Equals(gateDecision, "CONDITIONAL PASS", StringComparison.OrdinalIgnoreCase))
        {
            if (!userAcceptedProxyManualLayer)
            {
                result.AddError("P8-C gate CONDITIONAL PASS requires explicit user override for proxy/manual geometry.");
                result.MarkFailSafe();
            }
        }
        else if (string.Equals(gateDecision, "BLOCKED", StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("P8-C gate is BLOCKED pending Chuo tsunami spatial extraction.");
            result.MarkFailSafe();
        }
        else
        {
            result.AddError("P8-C gate decision must be PASS, CONDITIONAL PASS, or BLOCKED.");
            result.MarkFailSafe();
        }

        result.Finish();
        return result;
    }

    public static P8HazardValidationResult ValidateRiskFrontConfig(P8RiskFrontConfig config)
    {
        var result = new P8HazardValidationResult();

        if (config == null)
        {
            result.AddError("Risk front config is null.");
            result.MarkFailSafe();
            result.Finish();
            return result;
        }

        ValidateScenarioFields(config.scenarioId, config.sourceMode, config.configVersion, result);
        ValidateEvidenceSourceIdRequired(config.sourceMode, config.evidenceSourceId, "risk front config", result);
        ValidateLayerFieldSeparation(config.scienceLayerFields, config.visualLayerFields, "risk front config", result);

        if (!IsAllowedGeometryType(config.geometryType))
        {
            result.AddError("risk front config.geometryType must be grid, polygon, polyline, point, or synthetic.");
            result.MarkFailSafe();
        }

        if (config.visualHeightMeters > CinematicHeightThresholdMeters && !config.visualHeightIsCinematicOnly)
        {
            result.AddError("Large visualHeightMeters requires visualHeightIsCinematicOnly=true.");
            result.MarkFailSafe();
        }

        if (config.riskFrontEnabledInP8A)
        {
            result.AddError("P8-A must not enable risk-front rendering.");
            result.MarkFailSafe();
        }

        if (config.p8bVisualSceneObjectsImplemented)
        {
            result.AddError("This P8-B guard package must not implement visual scene objects.");
            result.MarkFailSafe();
        }

        if (config.p8cInfrastructureInteractionImplemented || config.p8dCollapseProxyGameplayImplemented)
        {
            result.AddError("P8-B guard config must not enable P8-C infrastructure interaction or P8-D collapse gameplay.");
            result.MarkFailSafe();
        }

        if (!IsAllowedBoundaryKind(config.boundaryIsEvidenceBasedOrPrototype))
        {
            result.AddError("boundaryIsEvidenceBasedOrPrototype must be evidence_based or prototype.");
        }

        if (string.IsNullOrWhiteSpace(config.visualLayerPurpose) ||
            config.visualLayerPurpose.IndexOf("cinematic", StringComparison.OrdinalIgnoreCase) < 0)
        {
            result.AddError("visualLayerPurpose must explicitly describe the P8-B visual layer as cinematic.");
        }

        if (IsEvidenceRequiredSourceMode(config.sourceMode) && config.manualSampleIsOfficial)
        {
            result.AddError("Manual/sample risk-front config must not be marked official.");
            result.MarkFailSafe();
        }

        result.Finish();
        return result;
    }

    public static P8HazardValidationResult ValidateInfrastructureConfig(P8InfrastructureHazardInteractionConfig config)
    {
        var result = new P8HazardValidationResult();

        if (config == null)
        {
            result.AddError("Infrastructure interaction config is null.");
            result.MarkFailSafe();
            result.Finish();
            return result;
        }

        ValidateScenarioFields(config.scenarioId, config.sourceMode, config.configVersion, result);
        ValidateEvidenceSourceIdRequired(config.sourceMode, config.evidenceSourceId, "infrastructure config", result);

        if (config.interactionEnabledInP8A)
        {
            result.AddError("P8-A must not enable infrastructure hazard interaction.");
            result.MarkFailSafe();
        }

        if (config.collapseProxyEnabledInP8A || config.collapseGameplayEnabledInP8A || config.hazardDrivenCollapse)
        {
            result.AddError("P8-A must keep collapse proxy behavior disabled.");
            result.MarkFailSafe();
        }

        if (config.collapseProbability < 0f || config.collapseProbability > 1f)
        {
            result.AddError("collapseProbability must be between 0 and 1.");
        }

        if (IsEvidenceRequiredSourceMode(config.sourceMode) && config.manualSampleIsOfficial)
        {
            result.AddError("Manual/sample infrastructure config must not be marked official.");
            result.MarkFailSafe();
        }

        result.Finish();
        return result;
    }

    public static bool IsAllowedSourceMode(string sourceMode)
    {
        return string.Equals(sourceMode, "test", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "manual_sample", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "evidence_planned", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "official_tsunami_metropolitan", StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateScenarioFields(
        string scenarioId,
        string sourceMode,
        string version,
        P8HazardValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            result.AddError("scenarioId is required.");
            result.MarkFailSafe();
        }

        if (string.IsNullOrWhiteSpace(sourceMode))
        {
            result.AddError("sourceMode is required.");
            result.MarkFailSafe();
        }
        else if (!IsAllowedSourceMode(sourceMode))
        {
            result.AddError("sourceMode must be test, manual_sample, evidence_planned, or official_tsunami_metropolitan.");
            result.MarkFailSafe();
        }
        else if (string.Equals(sourceMode, "evidence_planned", StringComparison.OrdinalIgnoreCase))
        {
            result.AddWarning("sourceMode=evidence_planned still requires source review before official hazard claims.");
        }
        else if (string.Equals(sourceMode, "official_tsunami_metropolitan", StringComparison.OrdinalIgnoreCase))
        {
            result.AddWarning("sourceMode=official_tsunami_metropolitan requires reviewed source metadata and extracted spatial data before final official surface claims.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            result.AddError("version field is required.");
        }
    }

    private static HashSet<string> ValidateEvidenceSources(
        P8EvidenceSource[] evidenceSources,
        string layerSourceMode,
        P8HazardValidationResult result)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if ((evidenceSources == null || evidenceSources.Length == 0) && IsEvidenceRequiredSourceMode(layerSourceMode))
        {
            result.AddError("manual_sample, evidence_planned, and official_tsunami_metropolitan hazard layers require at least one evidence source entry.");
            result.MarkFailSafe();
            return ids;
        }

        if (evidenceSources == null)
        {
            return ids;
        }

        for (int i = 0; i < evidenceSources.Length; i++)
        {
            P8EvidenceSource source = evidenceSources[i];
            string prefix = "evidenceSources[" + i + "]";
            if (source == null)
            {
                result.AddError(prefix + " is null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(source.evidenceSourceId))
            {
                result.AddError(prefix + ".evidenceSourceId is required.");
            }
            else if (!ids.Add(source.evidenceSourceId))
            {
                result.AddError(prefix + ".evidenceSourceId is duplicated: " + source.evidenceSourceId);
            }

            if (!IsAllowedSourceMode(source.sourceMode))
            {
                result.AddError(prefix + ".sourceMode must be test, manual_sample, evidence_planned, or official_tsunami_metropolitan.");
                result.MarkFailSafe();
            }

            if (!IsAllowedSourceCategory(source.sourceCategory))
            {
                result.AddError(prefix + ".sourceCategory is not a recognized P8-A evidence category.");
            }

            if (!IsAllowedReviewedStatus(source.reviewedStatus))
            {
                result.AddError(prefix + ".reviewedStatus is not recognized.");
            }

            if (string.IsNullOrWhiteSpace(source.title))
            {
                result.AddError(prefix + ".title is required.");
            }
        }

        return ids;
    }

    private static void ValidateLayerSpatialGateMetadata(P8HazardLayerData data, P8HazardValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(data.p8cGateDecision) && !IsAllowedP8CGateDecision(data.p8cGateDecision))
        {
            result.AddError("p8cGateDecision must be PASS, CONDITIONAL PASS, or BLOCKED.");
            result.MarkFailSafe();
        }

        if (!string.IsNullOrWhiteSpace(data.extractionStatus) && !IsAllowedExtractionStatus(data.extractionStatus))
        {
            result.AddError("extractionStatus is not recognized: " + data.extractionStatus);
            result.MarkFailSafe();
        }

        if (!string.IsNullOrWhiteSpace(data.spatialExtractionStatus) && !IsAllowedExtractionStatus(data.spatialExtractionStatus))
        {
            result.AddError("spatialExtractionStatus is not recognized: " + data.spatialExtractionStatus);
            result.MarkFailSafe();
        }

        if (string.Equals(data.p8cGateDecision, "PASS", StringComparison.OrdinalIgnoreCase) &&
            !data.completeOfficialSpatialLayerExtracted)
        {
            result.AddError("p8cGateDecision=PASS requires completeOfficialSpatialLayerExtracted=true.");
            result.MarkFailSafe();
        }
    }

    private static void ValidateFeature(
        P8HazardFeature feature,
        int index,
        string layerSourceMode,
        HashSet<string> evidenceSourceIds,
        P8HazardValidationResult result)
    {
        string prefix = "features[" + index + "]";

        if (feature == null)
        {
            result.AddError(prefix + " is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(feature.featureId))
        {
            result.AddError(prefix + ".featureId is required.");
        }

        if (!IsAllowedSourceMode(feature.sourceMode))
        {
            result.AddError(prefix + ".sourceMode must be test, manual_sample, evidence_planned, or official_tsunami_metropolitan.");
            result.MarkFailSafe();
        }
        else if (!string.IsNullOrWhiteSpace(layerSourceMode) &&
                 !string.Equals(feature.sourceMode, layerSourceMode, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError(prefix + ".sourceMode must match the layer sourceMode in P8-A.");
            result.MarkFailSafe();
        }

        if (!IsAllowedGeometryType(feature.geometryType))
        {
            result.AddError(prefix + ".geometryType must be grid, polygon, polyline, point, or synthetic.");
        }

        if (!string.IsNullOrWhiteSpace(feature.sourceCategory) && !IsAllowedSourceCategory(feature.sourceCategory))
        {
            result.AddError(prefix + ".sourceCategory is not a recognized P8 evidence category.");
        }

        if (!string.IsNullOrWhiteSpace(feature.extractionStatus) && !IsAllowedExtractionStatus(feature.extractionStatus))
        {
            result.AddError(prefix + ".extractionStatus is not recognized: " + feature.extractionStatus);
            result.MarkFailSafe();
        }

        if (!string.IsNullOrWhiteSpace(feature.spatialExtractionStatus) && !IsAllowedExtractionStatus(feature.spatialExtractionStatus))
        {
            result.AddError(prefix + ".spatialExtractionStatus is not recognized: " + feature.spatialExtractionStatus);
            result.MarkFailSafe();
        }

        if (feature.arrivalTimeSeconds < 0f)
        {
            result.AddError(prefix + ".arrivalTimeSeconds must be zero or positive.");
        }

        if (feature.inundationDepthMeters < 0f)
        {
            result.AddError(prefix + ".inundationDepthMeters must be zero or positive.");
        }

        if (feature.tsunamiHeightMeters < 0f)
        {
            result.AddError(prefix + ".tsunamiHeightMeters must be zero or positive.");
        }

        if (feature.inundationBoundary == null || feature.inundationBoundary.Length == 0)
        {
            result.AddError(prefix + ".inundationBoundary must contain at least one point.");
        }

        if (feature.hazardIntensity < 0f || feature.hazardIntensity > 1f)
        {
            result.AddError(prefix + ".hazardIntensity must be between 0 and 1.");
        }

        if (feature.confidence < 0f || feature.confidence > 1f)
        {
            result.AddError(prefix + ".confidence must be between 0 and 1.");
        }

        ValidateEvidenceSourceIdRequired(feature.sourceMode, feature.evidenceSourceId, prefix, result);
        if (!string.IsNullOrWhiteSpace(feature.evidenceSourceId) &&
            evidenceSourceIds != null &&
            evidenceSourceIds.Count > 0 &&
            !evidenceSourceIds.Contains(feature.evidenceSourceId))
        {
            result.AddError(prefix + ".evidenceSourceId is not registered in evidenceSources: " + feature.evidenceSourceId);
        }

        if (feature.visualHeightMeters > Math.Max(CinematicHeightThresholdMeters, feature.tsunamiHeightMeters) &&
            !feature.visualHeightIsCinematicOnly)
        {
            result.AddError(prefix + ".visualHeightMeters exceeds science values and requires visualHeightIsCinematicOnly=true.");
            result.MarkFailSafe();
        }

        if (!IsAllowedBoundaryKind(feature.boundaryIsEvidenceBasedOrPrototype))
        {
            result.AddError(prefix + ".boundaryIsEvidenceBasedOrPrototype must be evidence_based or prototype.");
        }
        else if (string.Equals(feature.boundaryIsEvidenceBasedOrPrototype, "evidence_based", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(feature.spatialExtractionStatus, "extracted", StringComparison.OrdinalIgnoreCase))
        {
            result.AddError(prefix + ".boundaryIsEvidenceBasedOrPrototype=evidence_based requires spatialExtractionStatus=extracted.");
            result.MarkFailSafe();
        }

        if (feature.spatialSampleCount > 0 &&
            feature.spatialSamples != null &&
            feature.spatialSamples.Length > 0 &&
            feature.spatialSampleCount != feature.spatialSamples.Length)
        {
            result.AddError(prefix + ".spatialSampleCount must match spatialSamples length.");
        }

        if (feature.collapseProbability < 0f || feature.collapseProbability > 1f)
        {
            result.AddError(prefix + ".collapseProbability must be between 0 and 1.");
        }

        if (feature.hazardDrivenCollapse)
        {
            result.AddError(prefix + ".hazardDrivenCollapse must remain false in P8-A.");
            result.MarkFailSafe();
        }
    }

    private static void ValidateEvidenceSourceIdRequired(
        string sourceMode,
        string evidenceSourceId,
        string label,
        P8HazardValidationResult result)
    {
        if (IsEvidenceRequiredSourceMode(sourceMode) && string.IsNullOrWhiteSpace(evidenceSourceId))
        {
            result.AddError(label + ".evidenceSourceId is required for evidence-aware records.");
            result.MarkFailSafe();
        }
    }

    private static void ValidateLayerFieldSeparation(
        string[] scienceLayerFields,
        string[] visualLayerFields,
        string label,
        P8HazardValidationResult result)
    {
        if (scienceLayerFields == null || scienceLayerFields.Length == 0)
        {
            result.AddError(label + " scienceLayerFields are required.");
        }

        if (visualLayerFields == null || visualLayerFields.Length == 0)
        {
            result.AddError(label + " visualLayerFields are required.");
        }

        foreach (string requiredScienceField in RequiredScienceFields)
        {
            if (!ContainsField(scienceLayerFields, requiredScienceField))
            {
                result.AddError(label + " scienceLayerFields missing " + requiredScienceField + ".");
            }
        }

        foreach (string requiredVisualField in RequiredVisualFields)
        {
            if (!ContainsField(visualLayerFields, requiredVisualField))
            {
                result.AddError(label + " visualLayerFields missing " + requiredVisualField + ".");
            }
        }

        foreach (string visualField in RequiredVisualFields)
        {
            if (ContainsField(scienceLayerFields, visualField))
            {
                result.AddError(label + " scienceLayerFields must not contain visual field " + visualField + ".");
                result.MarkFailSafe();
            }
        }

        foreach (string scienceField in RequiredScienceFields)
        {
            if (ContainsField(visualLayerFields, scienceField))
            {
                result.AddError(label + " visualLayerFields must not contain science field " + scienceField + ".");
                result.MarkFailSafe();
            }
        }
    }

    private static bool IsEvidenceRequiredSourceMode(string sourceMode)
    {
        return string.Equals(sourceMode, "manual_sample", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "evidence_planned", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "official_tsunami_metropolitan", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedGeometryType(string geometryType)
    {
        return string.Equals(geometryType, "grid", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "polygon", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "polyline", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "point", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "synthetic", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedBoundaryKind(string boundaryKind)
    {
        return string.Equals(boundaryKind, "evidence_based", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(boundaryKind, "prototype", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedSourceCategory(string sourceCategory)
    {
        return string.Equals(sourceCategory, "official_tsunami_inundation_map", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "official_tsunami_metropolitan", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "official_tsunami_report_reference", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "official_flood_proxy", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "academic_model_candidate", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "manual_extraction_required", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "evidence_planned", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "official_admin_boundary", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "tokyo_chuo_hazard_map", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "cabinet_office_mlit_local_government", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "academic_tsunami_simulation_paper", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "plateau_citygml_category", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "osm_route_context", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceCategory, "manual_sample", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedReviewedStatus(string reviewedStatus)
    {
        return string.Equals(reviewedStatus, "not_attached", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(reviewedStatus, "attached_unreviewed", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(reviewedStatus, "reviewed_for_planning", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(reviewedStatus, "reviewed_for_values", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedExtractionStatus(string extractionStatus)
    {
        return string.Equals(extractionStatus, "extracted", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extractionStatus, "manual_extraction_required", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extractionStatus, "blocked", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extractionStatus, "reference_only", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extractionStatus, "not_used_for_tsunami_front", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedP8CGateDecision(string gateDecision)
    {
        return string.Equals(gateDecision, "PASS", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(gateDecision, "CONDITIONAL PASS", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(gateDecision, "BLOCKED", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsField(string[] fields, string expected)
    {
        if (fields == null)
        {
            return false;
        }

        for (int i = 0; i < fields.Length; i++)
        {
            if (string.Equals(fields[i], expected, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

public class P8HazardValidationResult
{
    private readonly List<string> errors = new List<string>();
    private readonly List<string> warnings = new List<string>();

    public bool isValid;
    public bool isFailSafe;
    public int checkedFeatureCount;
    public string summary = string.Empty;
    public string[] errorsArray = new string[0];
    public string[] warningsArray = new string[0];

    public void AddError(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            errors.Add(message);
        }
    }

    public void AddWarning(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            warnings.Add(message);
        }
    }

    public void MarkFailSafe()
    {
        isFailSafe = true;
    }

    public void Finish()
    {
        errorsArray = errors.ToArray();
        warningsArray = warnings.ToArray();
        isValid = errorsArray.Length == 0;
        if (!isValid)
        {
            isFailSafe = true;
        }

        summary = isValid
            ? "P8 hazard data validation passed."
            : "P8 hazard data validation failed safe with " + errorsArray.Length + " error(s).";
    }
}
