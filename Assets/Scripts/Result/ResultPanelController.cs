using UnityEngine;
using UnityEngine.UI;

public class ResultPanelController : MonoBehaviour
{
    private static readonly Vector2 MetricsPanelSize = new Vector2(760f, 420f);

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

    public void Show(ResultMetrics metrics)
    {
        if (metrics == null)
        {
            ShowFailure("No shelter selected", 0f, "Result metrics were missing.");
            return;
        }

        string title = metrics.success ? "Evacuation Success" : "Evacuation Failed";
        Show(title, metrics);
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

    private void Show(string title, ResultMetrics metrics)
    {
        ConfigureMetricsLayout();

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
            shelterText.text = $"Shelter: {metrics.GetShelterLabel()}";
        }

        if (elapsedTimeText != null)
        {
            elapsedTimeText.text = $"Elapsed Time: {metrics.GetElapsedTimeLabel()}";
        }

        if (reasonText != null)
        {
            reasonText.text = metrics.GetDetailText();
        }
    }

    private void ConfigureMetricsLayout()
    {
        if (panelRoot != null && panelRoot.TryGetComponent(out RectTransform panelRect))
        {
            panelRect.sizeDelta = MetricsPanelSize;
        }

        ConfigurePanelText(titleText, new Vector2(0f, 165f), new Vector2(700f, 38f), TextAnchor.MiddleCenter, 26);
        ConfigurePanelText(shelterText, new Vector2(0f, 110f), new Vector2(700f, 54f), TextAnchor.MiddleLeft, 16);
        ConfigurePanelText(elapsedTimeText, new Vector2(0f, 68f), new Vector2(700f, 30f), TextAnchor.MiddleLeft, 16);
        ConfigurePanelText(reasonText, new Vector2(0f, -45f), new Vector2(700f, 190f), TextAnchor.UpperLeft, 15);
    }

    private static void ConfigurePanelText(Text text, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize)
    {
        if (text == null)
        {
            return;
        }

        if (text.TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
        }

        text.alignment = alignment;
        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 11;
        text.resizeTextMaxSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private static string FormatElapsedTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
