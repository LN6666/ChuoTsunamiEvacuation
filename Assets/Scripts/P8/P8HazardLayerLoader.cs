using System;
using System.IO;
using UnityEngine;

public static class P8HazardLayerLoader
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool AppliesHazardInteractionsInP8A = false;
    public const string HazardLayerV1FileName = "tsunami_hazard_layer_v1_chuo.json";
    public const string SampleHazardFileName = HazardLayerV1FileName;
    public const string LegacySampleHazardFileName = "tsunami_hazard_sample_chuo.json";
    public const string EvidenceRegistryFileName = "tsunami_hazard_evidence_registry.json";
    public const string RiskFrontConfigFileName = "risk_front_visualization_config.json";
    public const string InfrastructureConfigFileName = "infrastructure_hazard_interaction_config.json";

    public static P8HazardLayerLoadResult LoadSampleHazardLayer()
    {
        return LoadHazardLayerFromPath(GetAssetsP8DataPath(SampleHazardFileName));
    }

    public static P8HazardLayerLoadResult LoadHazardLayerFromPath(string path)
    {
        var result = new P8HazardLayerLoadResult
        {
            sourcePath = path ?? string.Empty,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Debug.LogWarning("Missing P8 tsunami hazard layer at " + result.sourcePath + ". Hazard systems remain disabled.");
            result.summary = "Missing P8 hazard layer. Fail-safe disabled state.";
            return result;
        }

        try
        {
            return LoadHazardLayerFromJson(File.ReadAllText(path), path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not load P8 tsunami hazard layer from " + path + ". " + exception.Message);
            result.summary = "Unreadable P8 hazard layer. Fail-safe disabled state.";
            return result;
        }
    }

    public static P8HazardLayerLoadResult LoadHazardLayerFromJson(string json, string sourceLabel)
    {
        var result = new P8HazardLayerLoadResult
        {
            sourcePath = string.IsNullOrWhiteSpace(sourceLabel) ? "P8 hazard JSON" : sourceLabel,
            failSafe = true
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Empty P8 tsunami hazard layer at " + result.sourcePath + ". Hazard systems remain disabled.");
            result.summary = "Empty P8 hazard layer. Fail-safe disabled state.";
            return result;
        }

        try
        {
            result.data = JsonUtility.FromJson<P8HazardLayerData>(json);
            result.validation = P8HazardDataValidator.ValidateHazardLayer(result.data);
            result.success = result.validation.isValid;
            result.failSafe = result.validation.isFailSafe;
            result.summary = CreateDebugSummary(result.data, result.validation);
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not parse P8 tsunami hazard layer from " + result.sourcePath + ". " + exception.Message);
            result.validation = new P8HazardValidationResult();
            result.validation.AddError("Parse failed: " + exception.Message);
            result.validation.MarkFailSafe();
            result.validation.Finish();
            result.summary = "Invalid P8 hazard layer. Fail-safe disabled state.";
            return result;
        }
    }

    public static P8RiskFrontConfigLoadResult LoadRiskFrontConfig()
    {
        string path = GetAssetsP8DataPath(RiskFrontConfigFileName);
        var result = new P8RiskFrontConfigLoadResult
        {
            sourcePath = path,
            failSafe = true
        };

        if (!File.Exists(path))
        {
            result.summary = "Missing P8 risk-front config. Fail-safe disabled state.";
            return result;
        }

        try
        {
            result.config = JsonUtility.FromJson<P8RiskFrontConfig>(File.ReadAllText(path));
            result.validation = P8HazardDataValidator.ValidateRiskFrontConfig(result.config);
            result.success = result.validation.isValid;
            result.failSafe = result.validation.isFailSafe;
            result.summary = result.validation.summary;
            return result;
        }
        catch (Exception exception)
        {
            result.validation = new P8HazardValidationResult();
            result.validation.AddError("Risk-front config parse failed: " + exception.Message);
            result.validation.MarkFailSafe();
            result.validation.Finish();
            result.summary = result.validation.summary;
            return result;
        }
    }

    public static P8InfrastructureConfigLoadResult LoadInfrastructureConfig()
    {
        string path = GetAssetsP8DataPath(InfrastructureConfigFileName);
        var result = new P8InfrastructureConfigLoadResult
        {
            sourcePath = path,
            failSafe = true
        };

        if (!File.Exists(path))
        {
            result.summary = "Missing P8 infrastructure config. Fail-safe disabled state.";
            return result;
        }

        try
        {
            result.config = JsonUtility.FromJson<P8InfrastructureHazardInteractionConfig>(File.ReadAllText(path));
            result.validation = P8HazardDataValidator.ValidateInfrastructureConfig(result.config);
            result.success = result.validation.isValid;
            result.failSafe = result.validation.isFailSafe;
            result.summary = result.validation.summary;
            return result;
        }
        catch (Exception exception)
        {
            result.validation = new P8HazardValidationResult();
            result.validation.AddError("Infrastructure config parse failed: " + exception.Message);
            result.validation.MarkFailSafe();
            result.validation.Finish();
            result.summary = result.validation.summary;
            return result;
        }
    }

    public static string CreateDebugSummary(P8HazardLayerData data, P8HazardValidationResult validation)
    {
        if (data == null)
        {
            return "P8 hazard layer unavailable. Fail-safe disabled state.";
        }

        int featureCount = data.features == null ? 0 : data.features.Length;
        string validationText = validation != null ? validation.summary : "Validation not run.";
        return "P8 hazard layer " + data.scenarioId + " sourceMode=" + data.sourceMode +
               " features=" + featureCount + ". " + validationText;
    }

    private static string GetAssetsP8DataPath(string fileName)
    {
        return RuntimeDataPathResolver.GetDataPath("P8", fileName);
    }
}

public class P8HazardLayerLoadResult
{
    public bool success;
    public bool failSafe;
    public string sourcePath = string.Empty;
    public P8HazardLayerData data;
    public P8HazardValidationResult validation;
    public string summary = string.Empty;
}

public class P8RiskFrontConfigLoadResult
{
    public bool success;
    public bool failSafe;
    public string sourcePath = string.Empty;
    public P8RiskFrontConfig config;
    public P8HazardValidationResult validation;
    public string summary = string.Empty;
}

public class P8InfrastructureConfigLoadResult
{
    public bool success;
    public bool failSafe;
    public string sourcePath = string.Empty;
    public P8InfrastructureHazardInteractionConfig config;
    public P8HazardValidationResult validation;
    public string summary = string.Empty;
}
