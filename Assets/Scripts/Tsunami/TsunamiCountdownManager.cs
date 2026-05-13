using UnityEngine;

public class TsunamiCountdownManager : MonoBehaviour
{
    [SerializeField] private float countdownSeconds = 180f;
    [SerializeField] private EvacuationGameManager gameManager;
    [SerializeField] private GameUIManager gameUIManager;

    private float remainingSeconds;
    private bool isRunning;

    public float RemainingSeconds => remainingSeconds;

    private void Awake()
    {
        PrepareWaiting();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        remainingSeconds -= Time.deltaTime;
        gameUIManager?.SetCountdown(remainingSeconds);

        if (remainingSeconds <= 0f)
        {
            remainingSeconds = 0f;
            isRunning = false;
            gameUIManager?.SetCountdown(remainingSeconds);
            gameManager?.HandleCountdownExpired();
        }
    }

    public void StartCountdown()
    {
        remainingSeconds = countdownSeconds;
        isRunning = true;
        gameUIManager?.SetCountdown(remainingSeconds);
    }

    public void StopCountdown()
    {
        isRunning = false;
    }

    public void PrepareWaiting()
    {
        remainingSeconds = countdownSeconds;
        isRunning = false;
        gameUIManager?.SetCountdownWaiting();
    }
}
