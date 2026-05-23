using UnityEngine;

public class P8RiskFrontTimeDriver : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;

    [SerializeField] private bool autoAdvanceInPlayMode;
    [SerializeField] private float playbackSpeed = 1f;
    [SerializeField] private bool loop;
    [SerializeField] private float loopDurationSeconds = 3000f;
    [SerializeField] private float currentTimeSeconds;

    private P8RiskFrontController owningController;

    public float CurrentTimeSeconds => currentTimeSeconds;

    private void Awake()
    {
        owningController = GetComponent<P8RiskFrontController>();
    }

    private void Update()
    {
        if (autoAdvanceInPlayMode && owningController == null)
        {
            Advance(Time.deltaTime);
        }
    }

    public void Configure(P8RiskFrontVisualConfig config)
    {
        if (config == null)
        {
            return;
        }

        loop = config.loopPlayback;
        loopDurationSeconds = Mathf.Max(1f, config.playbackDurationSeconds);
    }

    public void SetTime(float timeSeconds)
    {
        currentTimeSeconds = Mathf.Max(0f, timeSeconds);
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Max(0f, speed);
    }

    public void SetAutoAdvance(bool enabled)
    {
        autoAdvanceInPlayMode = enabled;
    }

    public void Advance(float deltaSeconds)
    {
        currentTimeSeconds += Mathf.Max(0f, deltaSeconds) * playbackSpeed;
        if (loop && currentTimeSeconds > loopDurationSeconds)
        {
            currentTimeSeconds %= loopDurationSeconds;
        }
    }
}
