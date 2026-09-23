using System;
using System.Collections.Generic;
using UnityEngine;

public static class P10BGreenGroundFrameGenerator
{
    public static P10BGreenGroundFrameTarget[] BuildTargetsFromAnchoringReport(
        P9DAnchoringReport report,
        P10BGreenGroundFrameConfig config)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        if (report == null || report.results == null)
        {
            return Array.Empty<P10BGreenGroundFrameTarget>();
        }

        var targets = new List<P10BGreenGroundFrameTarget>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < report.results.Length; i++)
        {
            P9DCoordinateAnchoringResult result = report.results[i];
            if (result == null)
            {
                continue;
            }

            bool officialShelter = result.isOfficialShelter && string.Equals(result.anchorType, "official_shelter_marker", StringComparison.Ordinal);
            bool humanitarianCandidate = result.isHumanitarianCandidate && string.Equals(result.anchorType, "humanitarian_candidate_marker", StringComparison.Ordinal);
            if (!officialShelter && !humanitarianCandidate)
            {
                continue;
            }

            string key = string.IsNullOrWhiteSpace(result.sourceId) ? result.anchorId : result.sourceId;
            if (string.IsNullOrWhiteSpace(key) || seenIds.Contains(key))
            {
                continue;
            }

            seenIds.Add(key);
            targets.Add(P10BGreenGroundFrameTarget.FromAnchoringResult(result, config));
        }

        return targets.ToArray();
    }

    public static P10BGreenGroundFrameMetrics ConfigureFrames(
        P10BGreenGroundFrameTarget[] targets,
        P10BGreenGroundFrameConfig config,
        Transform parent,
        P10BGreenGroundFramePool pool,
        bool tsunamiStarted,
        bool debugPreview)
    {
        config = config ?? new P10BGreenGroundFrameConfig();
        targets = targets ?? Array.Empty<P10BGreenGroundFrameTarget>();
        pool = pool ?? new P10BGreenGroundFramePool();
        pool.ReleaseAll();

        var metrics = CreateBaseMetrics(targets.Length, config, tsunamiStarted, debugPreview);
        if (!config.featureEnabled || (!tsunamiStarted && !debugPreview))
        {
            metrics.hiddenBeforeTsunamiStart = !tsunamiStarted && !debugPreview;
            metrics.createdPoolObjectCount = pool.CreatedCount;
            metrics.summary = "P10-B green ground frames hidden until tsunami start.";
            return metrics;
        }

        int cap = Mathf.Max(0, config.maxFrameCount);
        for (int i = 0; i < targets.Length && metrics.generatedFrameCount < cap; i++)
        {
            P10BGreenGroundFrameTarget target = targets[i];
            if (target == null || !target.IsValidForFrame(config))
            {
                metrics.rejectedTargetCount++;
                continue;
            }

            GameObject frame = pool.Acquire(parent);
            ApplyFrame(frame, target, config);
            metrics.validTargetCount++;
            metrics.generatedFrameCount++;
            metrics.activeFrameCount++;
            if (target.isOfficialShelter)
            {
                metrics.officialShelterFrameCount++;
            }
            if (target.isHumanitarianCandidate)
            {
                metrics.humanitarianCandidateFrameCount++;
                metrics.allHumanitarianCandidatesRemainNonOfficial &= !target.isOfficialShelter;
                metrics.allHumanitarianWarningsPreserved &= target.nonOfficialWarningRequired && !target.safeApprovedByDefault;
            }
        }

        metrics.createdPoolObjectCount = pool.CreatedCount;
        metrics.summary = "P10-B generated " + metrics.generatedFrameCount +
                          " green ground frame rectangle proxies after tsunami start.";
        return metrics;
    }

    private static P10BGreenGroundFrameMetrics CreateBaseMetrics(
        int requestedTargetCount,
        P10BGreenGroundFrameConfig config,
        bool tsunamiStarted,
        bool debugPreview)
    {
        return new P10BGreenGroundFrameMetrics
        {
            featureEnabled = config.featureEnabled,
            tsunamiStarted = tsunamiStarted,
            debugPreviewEnabled = debugPreview,
            requestedTargetCount = requestedTargetCount,
            activationBudgetPerFrame = Mathf.Max(1, config.activationBudgetPerFrame),
            allHumanitarianCandidatesRemainNonOfficial = true,
            allHumanitarianWarningsPreserved = true
        };
    }

    private static void ApplyFrame(
        GameObject frame,
        P10BGreenGroundFrameTarget target,
        P10BGreenGroundFrameConfig config)
    {
        if (frame == null || target == null)
        {
            return;
        }

        Vector3 center = target.GetFrameCenter();
        frame.name = "P10B_GreenGroundFrame_" + SafeObjectName(target.targetId);
        frame.transform.position = center + Vector3.up * config.groundYOffsetMeters;
        frame.transform.rotation = Quaternion.identity;

        float halfWidth = target.GetWidth(config) * 0.5f;
        float halfDepth = target.GetDepth(config) * 0.5f;
        LineRenderer line = frame.GetComponent<LineRenderer>();
        if (line != null)
        {
            line.positionCount = 5;
            line.SetPosition(0, new Vector3(-halfWidth, 0f, -halfDepth));
            line.SetPosition(1, new Vector3(halfWidth, 0f, -halfDepth));
            line.SetPosition(2, new Vector3(halfWidth, 0f, halfDepth));
            line.SetPosition(3, new Vector3(-halfWidth, 0f, halfDepth));
            line.SetPosition(4, new Vector3(-halfWidth, 0f, -halfDepth));
            line.startColor = config.frameColor;
            line.endColor = config.frameColor;
            line.widthMultiplier = Mathf.Max(0.01f, config.lineWidthMeters);
        }

        P10BGreenGroundFrameMarker marker = frame.GetComponent<P10BGreenGroundFrameMarker>();
        if (marker == null)
        {
            marker = frame.AddComponent<P10BGreenGroundFrameMarker>();
        }
        marker.Apply(target);
    }

    private static string SafeObjectName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace("/", "_").Replace("\\", "_");
    }
}

public class P10BGreenGroundFrameMarker : MonoBehaviour
{
    [SerializeField] private string targetId = string.Empty;
    [SerializeField] private string targetType = string.Empty;
    [SerializeField] private bool isOfficialShelter;
    [SerializeField] private bool isHumanitarianCandidate;
    [SerializeField] private bool nonOfficialWarningRequired;
    [SerializeField] private bool safeApprovedByDefault;
    [SerializeField] private bool coordinateDerivedProxy = true;
    [SerializeField] private bool exactFootprintProven;
    [SerializeField] private string warningText = string.Empty;
    [SerializeField] private string fallbackReason = string.Empty;

    public string TargetId => targetId;
    public string TargetType => targetType;
    public bool IsOfficialShelter => isOfficialShelter;
    public bool IsHumanitarianCandidate => isHumanitarianCandidate;
    public bool NonOfficialWarningRequired => nonOfficialWarningRequired;
    public bool SafeApprovedByDefault => safeApprovedByDefault;
    public bool CoordinateDerivedProxy => coordinateDerivedProxy;
    public bool ExactFootprintProven => exactFootprintProven;
    public string WarningText => warningText;
    public string FallbackReason => fallbackReason;
    public bool PreservesHumanitarianSemantics => !isHumanitarianCandidate || (!isOfficialShelter && nonOfficialWarningRequired && !safeApprovedByDefault);

    public void Apply(P10BGreenGroundFrameTarget target)
    {
        if (target == null)
        {
            return;
        }

        targetId = target.targetId ?? string.Empty;
        targetType = target.targetType ?? string.Empty;
        isOfficialShelter = target.isOfficialShelter;
        isHumanitarianCandidate = target.isHumanitarianCandidate;
        nonOfficialWarningRequired = target.nonOfficialWarningRequired;
        safeApprovedByDefault = target.safeApprovedByDefault;
        coordinateDerivedProxy = target.coordinateDerivedProxy;
        exactFootprintProven = target.exactFootprintProven;
        warningText = target.warningText ?? string.Empty;
        fallbackReason = target.fallbackReason ?? string.Empty;
    }
}
