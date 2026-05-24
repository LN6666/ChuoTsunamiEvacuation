using System;
using UnityEngine;

public class P10BGreenGroundFrameRuntime : MonoBehaviour
{
    [SerializeField] private P10BGreenGroundFrameConfig config = new P10BGreenGroundFrameConfig();
    [SerializeField] private P10BGreenGroundFrameTarget[] targets = Array.Empty<P10BGreenGroundFrameTarget>();
    [SerializeField] private bool tsunamiStarted;
    [SerializeField] private bool generateOnStart;

    private readonly P10BGreenGroundFramePool pool = new P10BGreenGroundFramePool();
    private P10BGreenGroundFrameMetrics lastMetrics = new P10BGreenGroundFrameMetrics();
    private Transform frameRoot;

    public P10BGreenGroundFrameMetrics LastMetrics => lastMetrics;
    public int CreatedPoolObjectCount => pool.CreatedCount;
    public bool TsunamiStarted => tsunamiStarted;

    private void Start()
    {
        if (generateOnStart)
        {
            RefreshFrames();
        }
    }

    public void Configure(P10BGreenGroundFrameConfig newConfig, P10BGreenGroundFrameTarget[] newTargets)
    {
        config = newConfig ?? new P10BGreenGroundFrameConfig();
        targets = newTargets ?? Array.Empty<P10BGreenGroundFrameTarget>();
        RefreshFrames();
    }

    public void SetTsunamiStarted(bool started)
    {
        if (tsunamiStarted == started && lastMetrics.requestedTargetCount == (targets == null ? 0 : targets.Length))
        {
            RefreshFrames();
            return;
        }

        tsunamiStarted = started;
        RefreshFrames();
    }

    public P10BGreenGroundFrameMetrics RefreshFrames()
    {
        EnsureFrameRoot();
        bool debugPreview = config != null && config.debugPreviewBeforeTsunamiStart;
        lastMetrics = P10BGreenGroundFrameGenerator.ConfigureFrames(
            targets,
            config,
            frameRoot,
            pool,
            tsunamiStarted,
            debugPreview);
        return lastMetrics;
    }

    public P10BGreenGroundFrameMetrics BuildTargetsFromP9DAndRefresh(P9DAnchoringReport report, P10BGreenGroundFrameConfig newConfig)
    {
        config = newConfig ?? new P10BGreenGroundFrameConfig();
        targets = P10BGreenGroundFrameGenerator.BuildTargetsFromAnchoringReport(report, config);
        return RefreshFrames();
    }

    private void EnsureFrameRoot()
    {
        if (frameRoot != null)
        {
            return;
        }

        var rootObject = new GameObject("P10B_GreenGroundFrameRoot");
        rootObject.transform.SetParent(transform, false);
        frameRoot = rootObject.transform;
    }
}
