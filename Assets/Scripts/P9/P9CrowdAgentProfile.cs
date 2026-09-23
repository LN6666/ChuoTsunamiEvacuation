using System;
using UnityEngine;

public class P9CrowdAgentProfile : MonoBehaviour
{
    public const bool AffectsPlayerSuccessFailure = P9RuntimePolicy.CanCausePlayerFailureInP9A;
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool ClaimsRealCrowdModel = false;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;

    [SerializeField] private string agentProfileId = "p9_test_agent";
    [SerializeField] private string agentType = "test_agent";
    [SerializeField] private float speedMetersPerSecond = 1.2f;
    [SerializeField] private string targetPreference = "nearest_available_proxy_entrance";
    [SerializeField] private string evacuationBehaviorMode = "direct_to_entrance_proxy_no_failure_effect";
    [SerializeField] private float crowdRadius = 0.35f;
    [SerializeField] private float panicLevelProxy;
    [SerializeField] private string sourceMode = "test";
    [SerializeField] private string notes = string.Empty;

    public string AgentProfileId => agentProfileId;
    public string AgentType => agentType;
    public float SpeedMetersPerSecond => Mathf.Max(0f, speedMetersPerSecond);
    public string TargetPreference => targetPreference;
    public string EvacuationBehaviorMode => evacuationBehaviorMode;
    public float CrowdRadius => Mathf.Max(0f, crowdRadius);
    public float PanicLevelProxy => Mathf.Clamp01(panicLevelProxy);
    public string SourceMode => sourceMode;
    public string Notes => notes;

    public void ConfigureForTests(
        string profileId,
        string type,
        float speed,
        string preference,
        string behaviorMode,
        float radius,
        float panicProxy)
    {
        agentProfileId = profileId ?? string.Empty;
        agentType = type ?? "test_agent";
        speedMetersPerSecond = Mathf.Max(0f, speed);
        targetPreference = preference ?? string.Empty;
        evacuationBehaviorMode = behaviorMode ?? string.Empty;
        crowdRadius = Mathf.Max(0f, radius);
        panicLevelProxy = Mathf.Clamp01(panicProxy);
    }

    public void ApplyRecord(P9CrowdAgentProfileRecord record)
    {
        if (record == null)
        {
            return;
        }

        agentProfileId = record.agentProfileId ?? string.Empty;
        agentType = record.agentType ?? "test_agent";
        speedMetersPerSecond = Mathf.Max(0f, record.speedMetersPerSecond);
        targetPreference = record.targetPreference ?? string.Empty;
        evacuationBehaviorMode = record.evacuationBehaviorMode ?? string.Empty;
        crowdRadius = Mathf.Max(0f, record.crowdRadius);
        panicLevelProxy = Mathf.Clamp01(record.panicLevelProxy);
        sourceMode = record.sourceMode ?? "test";
        notes = record.notes ?? string.Empty;
    }
}

[Serializable]
public class P9CrowdAgentProfileCollection
{
    public string schemaVersion = string.Empty;
    public string sourceMode = string.Empty;
    public string notes = string.Empty;
    public P9CrowdAgentProfileRecord[] agentProfiles = Array.Empty<P9CrowdAgentProfileRecord>();
}

[Serializable]
public class P9CrowdAgentProfileRecord
{
    public string agentProfileId = string.Empty;
    public string agentType = string.Empty;
    public float speedMetersPerSecond;
    public string targetPreference = string.Empty;
    public string evacuationBehaviorMode = string.Empty;
    public float crowdRadius;
    public float panicLevelProxy;
    public string sourceMode = string.Empty;
    public string notes = string.Empty;
}
