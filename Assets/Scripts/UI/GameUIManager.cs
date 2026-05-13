using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private Text countdownText;
    [SerializeField] private Text warningText;
    [SerializeField] private Text shelterText;
    [SerializeField] private Text interactionPromptText;
    [SerializeField] private Text climbProgressText;

    [Header("Progress")]
    [SerializeField] private Slider climbProgressSlider;

    public void SetCountdown(float remainingSeconds)
    {
        if (countdownText != null)
        {
            countdownText.text = FormatTime(remainingSeconds);
        }
    }

    public void SetCountdownWaiting()
    {
        if (countdownText != null)
        {
            countdownText.text = "Waiting";
        }
    }

    public void ShowWarning(string message)
    {
        if (warningText == null)
        {
            return;
        }

        warningText.gameObject.SetActive(true);
        warningText.text = message;
    }

    public void ShowInteractionPrompt(BuildingShelter shelter)
    {
        if (shelterText != null)
        {
            shelterText.gameObject.SetActive(true);
            shelterText.text = shelter != null ? shelter.GetDisplayText() : string.Empty;
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(true);
            interactionPromptText.text = "Press E to enter";
        }
    }

    public void HideInteractionPrompt()
    {
        if (shelterText != null)
        {
            shelterText.gameObject.SetActive(false);
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    public void SetClimbProgress(float normalizedProgress)
    {
        float progress = Mathf.Clamp01(normalizedProgress);

        if (climbProgressSlider != null)
        {
            climbProgressSlider.gameObject.SetActive(true);
            climbProgressSlider.value = progress;
        }

        if (climbProgressText != null)
        {
            climbProgressText.gameObject.SetActive(true);
            climbProgressText.text = $"Climbing: {Mathf.RoundToInt(progress * 100f)}%";
        }
    }

    public void HideClimbProgress()
    {
        if (climbProgressSlider != null)
        {
            climbProgressSlider.gameObject.SetActive(false);
        }

        if (climbProgressText != null)
        {
            climbProgressText.gameObject.SetActive(false);
        }
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
