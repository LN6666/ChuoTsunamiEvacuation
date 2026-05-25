using System;
using UnityEngine;

[Serializable]
public class P10CPreStagedActivationConfig
{
    public string schemaVersion = "p10c_pre.staged_activation_config.v1";
    public bool stageSceneStartActivation = true;
    public bool stageTsunamiStartActivation = true;
    public bool stageGreenFrames = true;
    public bool stageLightCurtain = true;
    public bool stageCrowdSpawn = true;
    public bool stageMarkerLabels = true;
    public int maxActivationsPerFrame = 24;
    public float maxMillisecondsPerFrame = 4f;
    public int prewarmBeforeHazardStartCount = 64;
    public bool debugLabelsOffByDefault = true;
    public bool throttleUiRefresh = true;
    public float uiRefreshMinIntervalSeconds = 0.25f;

    public bool IsSafeStagingConfig()
    {
        return maxActivationsPerFrame > 0 &&
            maxActivationsPerFrame <= 128 &&
            maxMillisecondsPerFrame > 0f &&
            maxMillisecondsPerFrame <= 16f &&
            prewarmBeforeHazardStartCount >= 0 &&
            prewarmBeforeHazardStartCount <= 256 &&
            debugLabelsOffByDefault &&
            uiRefreshMinIntervalSeconds >= 0.05f;
    }

    public int ClampBatchSize(int requestedCount)
    {
        return Mathf.Clamp(requestedCount, 0, Mathf.Max(1, maxActivationsPerFrame));
    }

    public int EstimateFrameCount(int itemCount)
    {
        itemCount = Mathf.Max(0, itemCount);
        int batchSize = Mathf.Max(1, maxActivationsPerFrame);
        return Mathf.CeilToInt(itemCount / (float)batchSize);
    }
}

public class P10CPreStagedActivationCursor
{
    private readonly int totalCount;
    private readonly P10CPreStagedActivationConfig config;
    private int nextIndex;

    public P10CPreStagedActivationCursor(int totalItemCount, P10CPreStagedActivationConfig stagedConfig)
    {
        totalCount = Mathf.Max(0, totalItemCount);
        config = stagedConfig ?? new P10CPreStagedActivationConfig();
    }

    public int NextIndex => nextIndex;
    public int RemainingCount => Mathf.Max(0, totalCount - nextIndex);
    public bool IsComplete => RemainingCount == 0;

    public int TakeNextBatchSize()
    {
        if (IsComplete)
        {
            return 0;
        }

        int batchSize = Mathf.Min(RemainingCount, Mathf.Max(1, config.maxActivationsPerFrame));
        nextIndex += batchSize;
        return batchSize;
    }
}
