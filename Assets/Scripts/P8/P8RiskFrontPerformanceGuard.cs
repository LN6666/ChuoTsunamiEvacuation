using System;
using System.Collections.Generic;

[Serializable]
public class P8RiskFrontPerformanceSettings
{
    public int segmentCount = 128;
    public bool rebuildsMeshEveryFrame;
    public int transparentLayerCount = 1;
    public int lightCurtainObjectCount = 1;
    public int maxParticleCount;
    public bool particlesAreBounded = true;
    public string materialMode = "transparent_unlit";
    public string shaderRisk = "low";
    public bool hasCullingStrategy = true;
    public bool hasEnableDisableStrategy = true;
    public bool usesSharedMeshOrInstancePool = true;
}

public static class P8RiskFrontPerformanceGuard
{
    public const int RecommendedMaxSegmentCount = 256;
    public const int HardMaxSegmentCount = 1024;
    public const int RecommendedMaxTransparentLayerCount = 2;
    public const int RecommendedMaxLightCurtainObjectCount = 16;
    public const int RecommendedMaxParticleCount = 1000;

    public static P8RiskFrontPerformanceReport Inspect(P8RiskFrontPerformanceSettings settings)
    {
        var report = new P8RiskFrontPerformanceReport();

        if (settings == null)
        {
            report.AddWarning("Performance settings are missing; P8-B visual work must use bounded defaults before implementation.");
            report.MarkBlockingRisk();
            report.Finish();
            return report;
        }

        if (settings.segmentCount > RecommendedMaxSegmentCount)
        {
            report.AddWarning("segmentCount exceeds the P8-B recommended risk-front budget.");
        }

        if (settings.segmentCount > HardMaxSegmentCount)
        {
            report.AddWarning("segmentCount exceeds the P8-B hard guard and risks expensive mesh or line generation.");
            report.MarkBlockingRisk();
        }

        if (settings.rebuildsMeshEveryFrame)
        {
            report.AddWarning("rebuildsMeshEveryFrame should be false; animate shader/material parameters or pooled vertices instead.");
            report.MarkBlockingRisk();
        }

        if (settings.transparentLayerCount > RecommendedMaxTransparentLayerCount)
        {
            report.AddWarning("transparentLayerCount is high and may cause overdraw when the curtain covers dense city geometry.");
        }

        if (settings.lightCurtainObjectCount > RecommendedMaxLightCurtainObjectCount)
        {
            report.AddWarning("lightCurtainObjectCount is high; group or pool visual sections before P8-C integration.");
        }

        if (!settings.particlesAreBounded)
        {
            report.AddWarning("particlesAreBounded must be true; unbounded particle usage is not acceptable for P8-B.");
            report.MarkBlockingRisk();
        }

        if (settings.maxParticleCount > RecommendedMaxParticleCount)
        {
            report.AddWarning("maxParticleCount exceeds the P8-B recommendation.");
        }

        if (IsHighRiskMaterialMode(settings.materialMode) || IsHighRiskMaterialMode(settings.shaderRisk))
        {
            report.AddWarning("materialMode or shaderRisk indicates expensive transparency/shader behavior.");
        }

        if (!settings.hasCullingStrategy)
        {
            report.AddWarning("hasCullingStrategy must be true before visual implementation is accepted.");
            report.MarkBlockingRisk();
        }

        if (!settings.hasEnableDisableStrategy)
        {
            report.AddWarning("hasEnableDisableStrategy must be true so the visual layer can be disabled outside active use.");
            report.MarkBlockingRisk();
        }

        if (!settings.usesSharedMeshOrInstancePool)
        {
            report.AddWarning("usesSharedMeshOrInstancePool should be true to avoid many unique objects or mesh allocations.");
        }

        report.Finish();
        return report;
    }

    private static bool IsHighRiskMaterialMode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.IndexOf("expensive", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("complex", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("multi_pass", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("unbounded", StringComparison.OrdinalIgnoreCase) >= 0 ||
               value.IndexOf("high", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

public class P8RiskFrontPerformanceReport
{
    private readonly List<string> warnings = new List<string>();

    public bool hasWarnings;
    public bool hasBlockingRisk;
    public string summary = string.Empty;
    public string[] warningsArray = new string[0];

    public void AddWarning(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            warnings.Add(message);
        }
    }

    public void MarkBlockingRisk()
    {
        hasBlockingRisk = true;
    }

    public void Finish()
    {
        warningsArray = warnings.ToArray();
        hasWarnings = warningsArray.Length > 0;
        summary = hasWarnings
            ? "P8-B risk-front performance guard found " + warningsArray.Length + " warning(s)."
            : "P8-B risk-front performance guard passed.";
    }
}
