using System;
using System.Collections.Generic;

public static class P8HazardDataValidator
{
    public const float CinematicHeightThresholdMeters = 10f;

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
                ValidateFeature(data.features[i], i, result);
            }
        }

        result.checkedFeatureCount = data.features == null ? 0 : data.features.Length;
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

        if (string.IsNullOrWhiteSpace(config.boundaryIsEvidenceBasedOrPrototype))
        {
            result.AddError("boundaryIsEvidenceBasedOrPrototype is required.");
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

        if (config.interactionEnabledInP8A)
        {
            result.AddError("P8-A must not enable infrastructure hazard interaction.");
            result.MarkFailSafe();
        }

        if (config.collapseProxyEnabledInP8A || config.hazardDrivenCollapse)
        {
            result.AddError("P8-A must keep collapse proxy behavior disabled.");
            result.MarkFailSafe();
        }

        if (config.collapseProbability < 0f || config.collapseProbability > 1f)
        {
            result.AddError("collapseProbability must be between 0 and 1.");
        }

        result.Finish();
        return result;
    }

    public static bool IsAllowedSourceMode(string sourceMode)
    {
        return string.Equals(sourceMode, "test", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "manual_sample", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sourceMode, "evidence_planned", StringComparison.OrdinalIgnoreCase);
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
            result.AddError("sourceMode must be test, manual_sample, or evidence_planned.");
            result.MarkFailSafe();
        }
        else if (string.Equals(sourceMode, "evidence_planned", StringComparison.OrdinalIgnoreCase))
        {
            result.AddWarning("sourceMode=evidence_planned still requires source review before official hazard claims.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            result.AddError("version field is required.");
        }
    }

    private static void ValidateFeature(P8HazardFeature feature, int index, P8HazardValidationResult result)
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

        if (!IsAllowedGeometryType(feature.geometryType))
        {
            result.AddError(prefix + ".geometryType must be grid, polygon, polyline, point, or synthetic.");
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

        if (feature.hazardIntensity < 0f || feature.hazardIntensity > 1f)
        {
            result.AddError(prefix + ".hazardIntensity must be between 0 and 1.");
        }

        if (feature.confidence < 0f || feature.confidence > 1f)
        {
            result.AddError(prefix + ".confidence must be between 0 and 1.");
        }

        if (string.IsNullOrWhiteSpace(feature.evidenceSourceId))
        {
            result.AddError(prefix + ".evidenceSourceId is required.");
        }

        if (feature.visualHeightMeters > Math.Max(CinematicHeightThresholdMeters, feature.tsunamiHeightMeters) &&
            !feature.visualHeightIsCinematicOnly)
        {
            result.AddError(prefix + ".visualHeightMeters exceeds science values and requires visualHeightIsCinematicOnly=true.");
            result.MarkFailSafe();
        }

        if (string.IsNullOrWhiteSpace(feature.boundaryIsEvidenceBasedOrPrototype))
        {
            result.AddError(prefix + ".boundaryIsEvidenceBasedOrPrototype is required.");
        }

        if (feature.collapseProbability < 0f || feature.collapseProbability > 1f)
        {
            result.AddError(prefix + ".collapseProbability must be between 0 and 1.");
        }

        if (feature.hazardDrivenCollapse)
        {
            result.AddWarning(prefix + ".hazardDrivenCollapse is data only in P8-A and must not drive gameplay.");
        }
    }

    private static bool IsAllowedGeometryType(string geometryType)
    {
        return string.Equals(geometryType, "grid", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "polygon", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "polyline", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "point", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(geometryType, "synthetic", StringComparison.OrdinalIgnoreCase);
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
