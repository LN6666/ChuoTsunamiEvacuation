using UnityEngine;
using UnityEngine.UI;

public class ResultPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text titleText;
    [SerializeField] private Text shelterText;
    [SerializeField] private Text elapsedTimeText;
    [SerializeField] private Text reasonText;

    private void Awake()
    {
        Hide();
    }

    public void ShowSuccess(string shelterName, float elapsedSeconds, string reason)
    {
        Show("Evacuation Success", shelterName, elapsedSeconds, reason);
    }

    public void ShowFailure(string shelterName, float elapsedSeconds, string reason)
    {
        Show("Evacuation Failed", shelterName, elapsedSeconds, reason);
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Show(string title, string shelterName, float elapsedSeconds, string reason)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (shelterText != null)
        {
            shelterText.text = $"Shelter: {shelterName}";
        }

        if (elapsedTimeText != null)
        {
            elapsedTimeText.text = $"Elapsed Time: {FormatElapsedTime(elapsedSeconds)}";
        }

        if (reasonText != null)
        {
            reasonText.text = reason;
        }
    }

    private static string FormatElapsedTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
