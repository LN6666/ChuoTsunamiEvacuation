using UnityEngine;

public sealed class NewMapPerformanceProbe : MonoBehaviour
{
    private const float WarmupSeconds = 10f;
    private const float SampleSeconds = 180f;
    private const float StutterThresholdSeconds = 0.066f;

    private float warmupElapsedSeconds;
    private int warmupFrameCount;
    private int warmupStutterFrameCount;
    private float warmupMaxFrameSeconds;
    private float elapsedSeconds;
    private int frameCount;
    private int stutterFrameCount;
    private float maxFrameSeconds;
    private bool samplingActive;
    private bool reported;

    public static NewMapPerformanceProbe Create(Transform parent)
    {
        GameObject probeObject = new GameObject("NewMap_PerformanceProbe");
        probeObject.transform.SetParent(parent, false);
        return probeObject.AddComponent<NewMapPerformanceProbe>();
    }

    private void Update()
    {
        if (reported)
        {
            return;
        }

        float deltaSeconds = Time.unscaledDeltaTime;
        if (!samplingActive)
        {
            warmupElapsedSeconds += deltaSeconds;
            warmupFrameCount++;
            warmupMaxFrameSeconds = Mathf.Max(warmupMaxFrameSeconds, deltaSeconds);
            if (deltaSeconds >= StutterThresholdSeconds)
            {
                warmupStutterFrameCount++;
            }

            if (warmupElapsedSeconds < WarmupSeconds)
            {
                return;
            }

            samplingActive = true;
            return;
        }

        elapsedSeconds += deltaSeconds;
        frameCount++;
        maxFrameSeconds = Mathf.Max(maxFrameSeconds, deltaSeconds);

        if (deltaSeconds >= StutterThresholdSeconds)
        {
            stutterFrameCount++;
        }

        if (elapsedSeconds >= SampleSeconds)
        {
            float averageFps = frameCount / elapsedSeconds;
            float warmupMaxFrameMs = warmupMaxFrameSeconds * 1000f;
            float maxFrameMs = maxFrameSeconds * 1000f;
            NewMapNpcCrowdPrototype crowd = FindObjectOfType<NewMapNpcCrowdPrototype>();
            int requestedNpcCount = crowd != null ? crowd.RequestedNpcCount : 0;
            int spawnedNpcCount = crowd != null ? crowd.SpawnedNpcCount : 0;
            int cappedNpcCount = crowd != null ? crowd.CappedNpcCount : 0;
            int activeNpcCount = crowd != null ? crowd.ActiveNpcCount : 0;
            int movingNpcCount = crowd != null ? crowd.MovingCount : 0;
            int arrivedNpcCount = crowd != null ? crowd.ArrivedCount : 0;
            int queuedNpcCount = crowd != null ? crowd.QueuedCount : 0;
            int stuckNpcCount = crowd != null ? crowd.StuckCount : 0;
            int recoveredNpcCount = crowd != null ? crowd.RecoveredCount : 0;
            int staticProxyNpcCount = crowd != null ? crowd.StaticProxyCount : 0;
            int stoppedWithoutReasonCount = crowd != null ? crowd.StoppedWithoutReasonCount : 0;
            float averageNpcSpeed = crowd != null ? crowd.AverageSpeedMetersPerSecond : 0f;
            Debug.Log(
                $"NewMap performance sample: warmupSeconds={warmupElapsedSeconds:F2} warmupFrameCount={warmupFrameCount} " +
                $"warmupMaxFrameMs={warmupMaxFrameMs:F2} warmupStutterFramesOver66ms={warmupStutterFrameCount} " +
                $"elapsedSeconds={elapsedSeconds:F2} frameCount={frameCount} avgFps={averageFps:F2} " +
                $"maxFrameMs={maxFrameMs:F2} stutterFramesOver66ms={stutterFrameCount} requestedNpcCount={requestedNpcCount} " +
                $"spawnedNpcCount={spawnedNpcCount} cappedNpcCount={cappedNpcCount} activeNpcCount={activeNpcCount} " +
                $"movingNpcCount={movingNpcCount} arrivedNpcCount={arrivedNpcCount} queuedNpcCount={queuedNpcCount} " +
                $"stuckNpcCount={stuckNpcCount} recoveredNpcCount={recoveredNpcCount} staticProxyNpcCount={staticProxyNpcCount} " +
                $"stoppedWithoutReasonCount={stoppedWithoutReasonCount} averageNpcSpeed={averageNpcSpeed:F3}");
            reported = true;
        }
    }
}
