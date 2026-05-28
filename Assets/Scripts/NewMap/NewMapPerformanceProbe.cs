using UnityEngine;

public sealed class NewMapPerformanceProbe : MonoBehaviour
{
    private const float SampleSeconds = 180f;
    private const float StutterThresholdSeconds = 0.066f;

    private float elapsedSeconds;
    private int frameCount;
    private int stutterFrameCount;
    private float maxFrameSeconds;
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
            float maxFrameMs = maxFrameSeconds * 1000f;
            NewMapNpcCrowdPrototype crowd = FindObjectOfType<NewMapNpcCrowdPrototype>();
            int requestedNpcCount = crowd != null ? crowd.RequestedNpcCount : 0;
            int spawnedNpcCount = crowd != null ? crowd.SpawnedNpcCount : 0;
            int cappedNpcCount = crowd != null ? crowd.CappedNpcCount : 0;
            int activeNpcCount = crowd != null ? crowd.ActiveNpcCount : 0;
            Debug.Log(
                $"NewMap performance sample: elapsedSeconds={elapsedSeconds:F2} frameCount={frameCount} avgFps={averageFps:F2} " +
                $"maxFrameMs={maxFrameMs:F2} stutterFramesOver66ms={stutterFrameCount} requestedNpcCount={requestedNpcCount} " +
                $"spawnedNpcCount={spawnedNpcCount} cappedNpcCount={cappedNpcCount} activeNpcCount={activeNpcCount}");
            reported = true;
        }
    }
}
