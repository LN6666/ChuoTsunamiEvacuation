using System;
using UnityEngine;

[Serializable]
public class P8RiskFrontVisualConfig
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool ImplementsP8CInfrastructureInteraction = false;
    public const bool ImplementsP8DCollapseProxy = false;
    public const bool RequiresChuoBaseMap = false;
    public const string CinematicDisclaimer = "Cinematic risk-front visualization, not physical tsunami height.";

    public bool visualEnabled;
    public float visualHeightMeters = 1000f;
    public bool visualHeightIsCinematicOnly = true;
    public int segmentCount = 32;
    public float waveAmplitudeMeters = 35f;
    public float waveFrequency = 2f;
    public float noiseStrengthMeters = 4f;
    public float frontTravelMeters = 650f;
    public float coordinateScaleMeters = 100000f;
    public float meshRebuildIntervalSeconds = 0.15f;
    public float materialAlpha = 0.42f;
    public bool loopPlayback;
    public float playbackDurationSeconds = 3000f;
    public string boundaryIsEvidenceBasedOrPrototype = "prototype";
    public string visualLayerPurpose = "cinematic risk-front readability only";
    public string disclaimer = CinematicDisclaimer;
    public bool isValid;
    public string validationMessage = string.Empty;

    public static P8RiskFrontVisualConfig FromRiskFrontConfig(P8RiskFrontConfig config)
    {
        var visualConfig = CreateDefault();
        if (config == null)
        {
            visualConfig.visualEnabled = false;
            visualConfig.isValid = false;
            visualConfig.validationMessage = "Missing risk-front config.";
            return visualConfig;
        }

        visualConfig.visualEnabled = config.riskFrontEnabledInP8B;
        visualConfig.visualHeightMeters = config.visualHeightMeters;
        visualConfig.visualHeightIsCinematicOnly = config.visualHeightIsCinematicOnly;
        visualConfig.segmentCount = Mathf.Clamp(config.segmentCount <= 0 ? visualConfig.segmentCount : config.segmentCount, 2, 256);
        visualConfig.waveAmplitudeMeters = Mathf.Max(0f, config.waveAmplitudeMeters);
        visualConfig.waveFrequency = Mathf.Max(0f, config.waveFrequency);
        visualConfig.noiseStrengthMeters = Mathf.Max(0f, config.noiseStrengthMeters);
        visualConfig.frontTravelMeters = Mathf.Max(1f, config.frontTravelMeters);
        visualConfig.coordinateScaleMeters = Mathf.Max(1f, config.coordinateScaleMeters);
        visualConfig.meshRebuildIntervalSeconds = Mathf.Clamp(config.meshRebuildIntervalSeconds, 0.02f, 2f);
        visualConfig.materialAlpha = Mathf.Clamp01(config.materialAlpha <= 0f ? visualConfig.materialAlpha : config.materialAlpha);
        visualConfig.loopPlayback = config.loopPlayback;
        visualConfig.playbackDurationSeconds = Mathf.Max(1f, config.playbackDurationSeconds);
        visualConfig.boundaryIsEvidenceBasedOrPrototype = string.IsNullOrWhiteSpace(config.boundaryIsEvidenceBasedOrPrototype)
            ? "prototype"
            : config.boundaryIsEvidenceBasedOrPrototype;
        visualConfig.visualLayerPurpose = string.IsNullOrWhiteSpace(config.visualLayerPurpose)
            ? visualConfig.visualLayerPurpose
            : config.visualLayerPurpose;
        visualConfig.disclaimer = string.IsNullOrWhiteSpace(config.p8bDisclaimer)
            ? CinematicDisclaimer
            : config.p8bDisclaimer;

        visualConfig.Validate();
        return visualConfig;
    }

    public static P8RiskFrontVisualConfig CreateDefault()
    {
        var config = new P8RiskFrontVisualConfig
        {
            visualEnabled = false,
            isValid = true,
            validationMessage = "Default P8-B visual config."
        };
        return config;
    }

    public bool Validate()
    {
        if (visualHeightMeters > P8HazardDataValidator.CinematicHeightThresholdMeters && !visualHeightIsCinematicOnly)
        {
            isValid = false;
            validationMessage = "Large visualHeightMeters requires visualHeightIsCinematicOnly=true.";
            return false;
        }

        if (segmentCount < 2)
        {
            isValid = false;
            validationMessage = "segmentCount must be at least 2.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(disclaimer) ||
            disclaimer.IndexOf("not physical tsunami height", StringComparison.OrdinalIgnoreCase) < 0)
        {
            isValid = false;
            validationMessage = "P8-B cinematic disclaimer is required.";
            return false;
        }

        isValid = true;
        validationMessage = "P8-B visual config valid.";
        return true;
    }
}
