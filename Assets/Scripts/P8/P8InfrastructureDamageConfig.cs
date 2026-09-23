using System;
using System.IO;
using UnityEngine;

[Serializable]
public class P8InfrastructureDamageConfig
{
    public string configId = "p8_infrastructure_damage_proxy_config_v1";
    public float warningDepthMeters = 0.05f;
    public float lowFloorInundationDepthMeters = 0.3f;
    public float entranceBlockedDepthMeters = 0.3f;
    public float entranceBlockedIntensity = 0.45f;
    public float roadRestrictedDepthMeters = 0.3f;
    public float bridgeRestrictedDepthMeters = 0.5f;
    public float undergroundAvoidDepthMeters = 0.05f;
    public float buildingDamageDepthMeters = 1.0f;
    public float buildingDamageIntensity = 0.65f;
    public float inaccessibleDepthMeters = 1.5f;
    public float inaccessibleIntensity = 0.85f;
    public float collapseProxyDepthMeters = 1.5f;
    public float collapseProxyIntensity = 0.85f;
    public float collapseProxyProbability = 0.03f;
    public int collapseProxyRandomSeed = 8302;
    public int maxCollapseProxySampleCount = 8;
    public bool enableCollapseProxyVisual = true;
    public bool requireNonOfficialWarningForHumanitarianCandidates = true;
    public bool manualReviewRequiredDefault = true;
    public bool noPhysicsCollapse = true;
    public bool noDebrisSimulation = true;
    public bool noGameplaySuccessFailureChange = true;
    public string notes = "P8-D lightweight status/visual proxy only. Not a structural engineering assessment.";

    public static P8InfrastructureDamageConfig Default()
    {
        return new P8InfrastructureDamageConfig();
    }

    public static P8InfrastructureDamageConfig FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Default();
        }

        P8InfrastructureDamageConfig config = JsonUtility.FromJson<P8InfrastructureDamageConfig>(json);
        return config ?? Default();
    }

    public static P8InfrastructureDamageConfig LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Default();
        }

        return FromJson(File.ReadAllText(path));
    }

    public float ClampedCollapseProbability()
    {
        return Mathf.Clamp01(collapseProxyProbability);
    }
}
