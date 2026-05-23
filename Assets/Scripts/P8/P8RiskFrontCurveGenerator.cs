using System;
using UnityEngine;

public static class P8RiskFrontCurveGenerator
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool GeneratesVisualBoundaryOnly = true;
    public const string FrontDriverSource = "hazard_layer_arrival_depth_boundary_v1";

    public static P8RiskFrontCurveResult Generate(
        P8HazardLayerData hazardLayer,
        P8RiskFrontVisualConfig visualConfig,
        float simulationTimeSeconds)
    {
        var result = new P8RiskFrontCurveResult();

        if (hazardLayer == null || hazardLayer.features == null || hazardLayer.features.Length == 0)
        {
            result.failSafe = true;
            result.summary = "Missing hazard data. Risk-front visual hidden.";
            return result;
        }

        if (visualConfig == null || !visualConfig.Validate())
        {
            result.failSafe = true;
            result.summary = visualConfig == null ? "Missing visual config." : visualConfig.validationMessage;
            return result;
        }

        P8HazardFeature feature = SelectFeature(hazardLayer, simulationTimeSeconds);
        if (feature == null)
        {
            result.failSafe = true;
            result.summary = "No usable hazard feature. Risk-front visual hidden.";
            return result;
        }

        Vector3[] dataBoundary = ConvertBoundaryToLocal(feature.inundationBoundary, visualConfig.coordinateScaleMeters);
        if (dataBoundary.Length == 0)
        {
            dataBoundary = GenerateFallbackBoundary(feature, visualConfig, hazardLayer.timeOriginSeconds, simulationTimeSeconds);
            result.usedFallbackBoundary = true;
        }

        result.dataBoundary = dataBoundary;
        result.visualIntensity01 = CalculateVisualIntensity(feature);
        result.warningLevel = CalculateWarningLevel(result.visualIntensity01);
        result.visualBoundary = GenerateVisualBoundary(
            dataBoundary,
            visualConfig,
            hazardLayer.timeOriginSeconds,
            feature.arrivalTimeSeconds,
            simulationTimeSeconds,
            result.visualIntensity01);
        result.selectedFeatureId = feature.featureId;
        result.arrivalProgress = CalculateArrivalProgress(hazardLayer.timeOriginSeconds, feature.arrivalTimeSeconds, simulationTimeSeconds);
        result.arrivalTimeSeconds = feature.arrivalTimeSeconds;
        result.inundationDepthMeters = feature.inundationDepthMeters;
        result.hazardIntensity = feature.hazardIntensity;
        result.confidence = feature.confidence;
        result.evidenceSourceId = feature.evidenceSourceId ?? string.Empty;
        result.sourceMode = feature.sourceMode ?? string.Empty;
        result.geometryType = feature.geometryType ?? string.Empty;
        result.frontDriverSource = FrontDriverSource;
        result.success = result.visualBoundary.Length >= 2;
        result.failSafe = !result.success;
        result.summary = result.success
            ? CreateSummary(result)
            : "P8-B visual boundary generation failed safe.";
        return result;
    }

    public static Vector3[] ConvertBoundaryToLocal(P8BoundaryPoint[] boundary, float coordinateScaleMeters)
    {
        if (boundary == null || boundary.Length == 0)
        {
            return new Vector3[0];
        }

        float scale = Mathf.Max(1f, coordinateScaleMeters);
        float originX = boundary[0].x;
        float originY = boundary[0].y;
        var points = new Vector3[boundary.Length];

        for (int i = 0; i < boundary.Length; i++)
        {
            points[i] = new Vector3(
                (boundary[i].x - originX) * scale,
                0f,
                (boundary[i].y - originY) * scale);
        }

        return points;
    }

    public static P8HazardFeature SelectFeatureForTime(P8HazardLayerData hazardLayer, float simulationTimeSeconds)
    {
        return SelectFeature(hazardLayer, simulationTimeSeconds);
    }

    private static Vector3[] GenerateVisualBoundary(
        Vector3[] dataBoundary,
        P8RiskFrontVisualConfig visualConfig,
        float timeOriginSeconds,
        float arrivalTimeSeconds,
        float simulationTimeSeconds,
        float visualIntensity01)
    {
        Bounds bounds = CalculateBounds(dataBoundary);
        float width = Mathf.Max(bounds.size.x, 20f);
        float minX = bounds.center.x - width * 0.5f;
        float maxX = bounds.center.x + width * 0.5f;
        float minZ = bounds.min.z - visualConfig.frontTravelMeters * 0.25f;
        float maxZ = bounds.max.z + visualConfig.frontTravelMeters;
        float progress = CalculateArrivalProgress(timeOriginSeconds, arrivalTimeSeconds, simulationTimeSeconds);
        float baseZ = Mathf.Lerp(minZ, maxZ, progress);
        int count = Mathf.Clamp(visualConfig.segmentCount, 2, 256);
        var points = new Vector3[count];
        float intensityMultiplier = Mathf.Lerp(0.75f, 1.35f, Mathf.Clamp01(visualIntensity01));
        float waveAmplitude = visualConfig.waveAmplitudeMeters * intensityMultiplier;
        float noiseStrength = visualConfig.noiseStrengthMeters * intensityMultiplier;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0f : i / (float)(count - 1);
            float x = Mathf.Lerp(minX, maxX, t);
            float phase = (t * Mathf.PI * 2f * visualConfig.waveFrequency) + (progress * Mathf.PI * 2f);
            float wave = Mathf.Sin(phase) * waveAmplitude;
            float noise = (Mathf.PerlinNoise(t * 3.17f, progress * 5.11f) - 0.5f) * 2f * noiseStrength;
            points[i] = new Vector3(x, 0f, baseZ + wave + noise);
        }

        return points;
    }

    private static P8HazardFeature SelectFeature(P8HazardLayerData hazardLayer, float simulationTimeSeconds)
    {
        if (hazardLayer == null || hazardLayer.features == null || hazardLayer.features.Length == 0)
        {
            return null;
        }

        P8HazardFeature next = null;
        P8HazardFeature latest = null;
        float nextArrival = float.MaxValue;
        float latestArrival = float.MinValue;

        for (int i = 0; i < hazardLayer.features.Length; i++)
        {
            P8HazardFeature feature = hazardLayer.features[i];
            if (feature == null)
            {
                continue;
            }

            if (simulationTimeSeconds <= feature.arrivalTimeSeconds && feature.arrivalTimeSeconds < nextArrival)
            {
                next = feature;
                nextArrival = feature.arrivalTimeSeconds;
            }

            if (feature.arrivalTimeSeconds <= simulationTimeSeconds && feature.arrivalTimeSeconds >= latestArrival)
            {
                latest = feature;
                latestArrival = feature.arrivalTimeSeconds;
            }
        }

        return next ?? latest;
    }

    private static Vector3[] GenerateFallbackBoundary(
        P8HazardFeature feature,
        P8RiskFrontVisualConfig visualConfig,
        float timeOriginSeconds,
        float simulationTimeSeconds)
    {
        float progress = CalculateArrivalProgress(timeOriginSeconds, feature.arrivalTimeSeconds, simulationTimeSeconds);
        float halfWidth = Mathf.Max(10f, visualConfig.frontTravelMeters * 0.25f);
        float z = Mathf.Lerp(0f, visualConfig.frontTravelMeters, progress);

        return new[]
        {
            new Vector3(-halfWidth, 0f, z),
            new Vector3(halfWidth, 0f, z)
        };
    }

    private static float CalculateVisualIntensity(P8HazardFeature feature)
    {
        if (feature == null)
        {
            return 0f;
        }

        float depthSignal = Mathf.Clamp01(feature.inundationDepthMeters / 2.5f);
        float intensitySignal = Mathf.Clamp01(feature.hazardIntensity);
        return Mathf.Clamp01(Mathf.Max(depthSignal, intensitySignal));
    }

    private static string CalculateWarningLevel(float visualIntensity01)
    {
        if (visualIntensity01 >= 0.66f)
        {
            return "high";
        }

        if (visualIntensity01 >= 0.33f)
        {
            return "medium";
        }

        return "low";
    }

    private static float CalculateArrivalProgress(float timeOriginSeconds, float arrivalTimeSeconds, float simulationTimeSeconds)
    {
        float duration = Mathf.Max(1f, arrivalTimeSeconds - timeOriginSeconds);
        return Mathf.Clamp01((simulationTimeSeconds - timeOriginSeconds) / duration);
    }

    private static Bounds CalculateBounds(Vector3[] points)
    {
        var bounds = new Bounds(points[0], Vector3.zero);
        for (int i = 1; i < points.Length; i++)
        {
            bounds.Encapsulate(points[i]);
        }

        return bounds;
    }

    private static string CreateSummary(P8RiskFrontCurveResult result)
    {
        string boundaryMode = result.usedFallbackBoundary ? "fallback_procedural_boundary" : "hazard_boundary";
        return "Generated P8-B hazard-layer-driven v1 visual boundary. driver=" + result.frontDriverSource +
               " boundary=" + boundaryMode +
               " selectedFeature=" + result.selectedFeatureId +
               " arrivalTimeSeconds=" + result.arrivalTimeSeconds.ToString("0.##") +
               " inundationDepthMeters=" + result.inundationDepthMeters.ToString("0.##") +
               " hazardIntensity=" + result.hazardIntensity.ToString("0.##") +
               " confidence=" + result.confidence.ToString("0.##") +
               " warningLevel=" + result.warningLevel +
               " sourceMode=" + result.sourceMode +
               " evidenceSourceId=" + result.evidenceSourceId +
               ". " + P8RiskFrontVisualConfig.CinematicDisclaimer;
    }
}
public class P8RiskFrontCurveResult
{
    public bool success;
    public bool failSafe;
    public string selectedFeatureId = string.Empty;
    public string frontDriverSource = string.Empty;
    public string warningLevel = string.Empty;
    public string evidenceSourceId = string.Empty;
    public string sourceMode = string.Empty;
    public string geometryType = string.Empty;
    public float arrivalProgress;
    public float arrivalTimeSeconds;
    public float inundationDepthMeters;
    public float hazardIntensity;
    public float confidence;
    public float visualIntensity01;
    public bool usedFallbackBoundary;
    public Vector3[] dataBoundary = new Vector3[0];
    public Vector3[] visualBoundary = new Vector3[0];
    public string summary = string.Empty;
}
