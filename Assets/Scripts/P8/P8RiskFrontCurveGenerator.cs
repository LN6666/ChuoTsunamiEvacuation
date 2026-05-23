using System;
using UnityEngine;

public static class P8RiskFrontCurveGenerator
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool GeneratesVisualBoundaryOnly = true;

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
        if (feature == null || feature.inundationBoundary == null || feature.inundationBoundary.Length == 0)
        {
            result.failSafe = true;
            result.summary = "Selected hazard feature has no data boundary.";
            return result;
        }

        Vector3[] dataBoundary = ConvertBoundaryToLocal(feature.inundationBoundary, visualConfig.coordinateScaleMeters);
        if (dataBoundary.Length == 0)
        {
            result.failSafe = true;
            result.summary = "Data boundary conversion produced no points.";
            return result;
        }

        result.dataBoundary = dataBoundary;
        result.visualBoundary = GenerateVisualBoundary(dataBoundary, visualConfig, hazardLayer.timeOriginSeconds, feature.arrivalTimeSeconds, simulationTimeSeconds);
        result.selectedFeatureId = feature.featureId;
        result.arrivalProgress = CalculateArrivalProgress(hazardLayer.timeOriginSeconds, feature.arrivalTimeSeconds, simulationTimeSeconds);
        result.success = result.visualBoundary.Length >= 2;
        result.failSafe = !result.success;
        result.summary = result.success
            ? "Generated P8-B cinematic visual boundary from data boundary. " + P8RiskFrontVisualConfig.CinematicDisclaimer
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

    private static Vector3[] GenerateVisualBoundary(
        Vector3[] dataBoundary,
        P8RiskFrontVisualConfig visualConfig,
        float timeOriginSeconds,
        float arrivalTimeSeconds,
        float simulationTimeSeconds)
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

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0f : i / (float)(count - 1);
            float x = Mathf.Lerp(minX, maxX, t);
            float phase = (t * Mathf.PI * 2f * visualConfig.waveFrequency) + (progress * Mathf.PI * 2f);
            float wave = Mathf.Sin(phase) * visualConfig.waveAmplitudeMeters;
            float noise = (Mathf.PerlinNoise(t * 3.17f, progress * 5.11f) - 0.5f) * 2f * visualConfig.noiseStrengthMeters;
            points[i] = new Vector3(x, 0f, baseZ + wave + noise);
        }

        return points;
    }

    private static P8HazardFeature SelectFeature(P8HazardLayerData hazardLayer, float simulationTimeSeconds)
    {
        P8HazardFeature selected = null;
        float selectedArrival = float.MaxValue;

        for (int i = 0; i < hazardLayer.features.Length; i++)
        {
            P8HazardFeature feature = hazardLayer.features[i];
            if (feature == null)
            {
                continue;
            }

            if (simulationTimeSeconds <= feature.arrivalTimeSeconds && feature.arrivalTimeSeconds < selectedArrival)
            {
                selected = feature;
                selectedArrival = feature.arrivalTimeSeconds;
            }
        }

        return selected ?? hazardLayer.features[0];
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
}
public class P8RiskFrontCurveResult
{
    public bool success;
    public bool failSafe;
    public string selectedFeatureId = string.Empty;
    public float arrivalProgress;
    public Vector3[] dataBoundary = new Vector3[0];
    public Vector3[] visualBoundary = new Vector3[0];
    public string summary = string.Empty;
}
