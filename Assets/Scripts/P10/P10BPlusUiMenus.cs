using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class P10BPlusUiConfig
{
    public string schemaVersion = "p10b_plus.ui_config.v1";
    public string defaultLanguage = "en";
    public bool startMenuEnabled = true;
    public bool pauseMenuEnabled = true;
    public bool rulesPanelScrollable = true;
    public bool wrapDynamicText = true;
    public bool resultPanelOverflowRiskReduced = true;
    public float backgroundOpacity = 0.35f;
    public string backgroundAssetMode = "placeholder_until_licensed_asset_confirmed";
    public string backgroundImagePath = string.Empty;
    public string backgroundSourceUrl = string.Empty;
    public string backgroundLicense = string.Empty;
    public string backgroundAuthorOrProvider = string.Empty;
    public bool unlicensedInternetImageCommitted;
    public string notes = string.Empty;

    public bool IsBackgroundPolicySafe()
    {
        if (unlicensedInternetImageCommitted)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(backgroundSourceUrl) &&
            !string.IsNullOrWhiteSpace(backgroundLicense) &&
            !string.IsNullOrWhiteSpace(backgroundAuthorOrProvider);
    }
}

public class P10BPlusMenuRuntime : MonoBehaviour
{
    [SerializeField] private P10BPlusUiConfig config = new P10BPlusUiConfig();
    [SerializeField] private bool buildOnStart;

    private P10BPlusLocalizationService localizationService;
    private GameObject canvasObject;
    private GameObject startPanel;
    private GameObject pausePanel;
    private GameObject rulesPanel;

    public bool IsPaused { get; private set; }
    public bool StartMenuBuilt => startPanel != null;
    public bool PauseMenuBuilt => pausePanel != null;
    public bool RulesPanelBuilt => rulesPanel != null;

    private void Start()
    {
        if (buildOnStart)
        {
            Build(config, P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English));
        }
    }

    private void Update()
    {
        if (config != null && config.pauseMenuEnabled && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void Build(P10BPlusUiConfig newConfig, P10BPlusLocalizationService service)
    {
        config = newConfig ?? new P10BPlusUiConfig();
        localizationService = service ?? P10BPlusDataLoader.CreateLocalizationService(P10BPlusLanguage.English);
        EnsureCanvas();
        BuildStartPanel();
        BuildPausePanel();
        BuildRulesPanel();
        ShowStartMenu();
    }

    public void SetLanguage(P10BPlusLanguage language)
    {
        localizationService?.SetLanguage(language);
        RefreshLocalizedTexts();
    }

    public void ShowStartMenu()
    {
        SetPanel(startPanel, true);
        SetPanel(pausePanel, false);
        SetPanel(rulesPanel, false);
        IsPaused = false;
    }

    public void ShowRules()
    {
        SetPanel(rulesPanel, true);
    }

    public void HideRules()
    {
        SetPanel(rulesPanel, false);
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        SetPanel(pausePanel, true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        SetPanel(pausePanel, false);
        Time.timeScale = 1f;
    }

    public string GetForceQuitExplanation()
    {
        return localizationService == null
            ? "Force quit immediately exits without saving the current run."
            : localizationService.Translate("quit.force.explanation");
    }

    private void EnsureCanvas()
    {
        if (canvasObject != null)
        {
            return;
        }

        canvasObject = new GameObject("P10BPlus_MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
    }

    private void BuildStartPanel()
    {
        startPanel = CreatePanel("P10BPlus_StartPanel");
        AddText(startPanel.transform, "title", "game.title", new Vector2(0f, 230f), new Vector2(900f, 56f), 30);
        AddButtonLabel(startPanel.transform, "start", "menu.start", new Vector2(0f, 130f));
        AddButtonLabel(startPanel.transform, "rules", "menu.rules", new Vector2(0f, 70f));
        AddButtonLabel(startPanel.transform, "options", "menu.options", new Vector2(0f, 10f));
        AddButtonLabel(startPanel.transform, "language", "menu.language", new Vector2(0f, -50f));
        AddButtonLabel(startPanel.transform, "quit", "menu.quit", new Vector2(0f, -110f));
    }

    private void BuildPausePanel()
    {
        pausePanel = CreatePanel("P10BPlus_PausePanel");
        AddText(pausePanel.transform, "pause_title", "pause.title", new Vector2(0f, 210f), new Vector2(820f, 48f), 26);
        AddButtonLabel(pausePanel.transform, "resume", "pause.resume", new Vector2(0f, 120f));
        AddButtonLabel(pausePanel.transform, "rules", "menu.rules", new Vector2(0f, 60f));
        AddButtonLabel(pausePanel.transform, "language", "menu.language", new Vector2(0f, 0f));
        AddButtonLabel(pausePanel.transform, "return_title", "pause.return_to_title", new Vector2(0f, -60f));
        AddButtonLabel(pausePanel.transform, "force_quit", "pause.force_quit", new Vector2(0f, -120f));
        AddText(pausePanel.transform, "force_explain", "quit.force.explanation", new Vector2(0f, -190f), new Vector2(860f, 72f), 14);
        SetPanel(pausePanel, false);
    }

    private void BuildRulesPanel()
    {
        rulesPanel = CreatePanel("P10BPlus_RulesPanel");
        AddText(rulesPanel.transform, "rules_title", "rules.title", new Vector2(0f, 250f), new Vector2(900f, 42f), 24);
        AddScrollableRulesBody(rulesPanel.transform);
        AddButtonLabel(rulesPanel.transform, "close", "rules.close", new Vector2(0f, -285f));
        SetPanel(rulesPanel, false);
    }

    private Text AddScrollableRulesBody(Transform parent)
    {
        var scrollObject = new GameObject("RulesScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchoredPosition = new Vector2(0f, -12f);
        scrollRectTransform.sizeDelta = new Vector2(960f, 460f);
        Image scrollBackground = scrollObject.GetComponent<Image>();
        scrollBackground.color = new Color(0f, 0f, 0f, 0.12f);

        var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(14f, 14f);
        viewportRect.offsetMax = new Vector2(-14f, -14f);
        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        var contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 900f);

        Text body = AddText(contentObject.transform, "rules_body", "rules.body", new Vector2(0f, -430f), new Vector2(900f, 840f), 15);
        P10BPlusUiLayoutSafety.ApplyLongTextSafety(body, true);

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 26f;
        return body;
    }

    private GameObject CreatePanel(string objectName)
    {
        var panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, Mathf.Clamp01(config == null ? 0.35f : config.backgroundOpacity));
        return panel;
    }

    private Text AddButtonLabel(Transform parent, string name, string key, Vector2 position)
    {
        Text text = AddText(parent, name, key, position, new Vector2(360f, 44f), 18);
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private Text AddText(Transform parent, string name, string key, Vector2 position, Vector2 size, int fontSize)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        P10BPlusUiLayoutSafety.ApplyDynamicTextSafety(text);
        var localized = textObject.AddComponent<P10BPlusLocalizedText>();
        localized.Configure(text, key, localizationService);
        return text;
    }

    private void RefreshLocalizedTexts()
    {
        if (canvasObject == null)
        {
            return;
        }

        P10BPlusLocalizedText[] texts = canvasObject.GetComponentsInChildren<P10BPlusLocalizedText>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].Refresh(localizationService);
        }
    }

    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}

public static class P10BPlusUiLayoutSafety
{
    public static void ApplyDynamicTextSafety(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = Mathf.Max(10, text.fontSize);
    }

    public static void ApplyLongTextSafety(Text text, bool scrollableContent)
    {
        ApplyDynamicTextSafety(text);
        if (text == null)
        {
            return;
        }

        text.alignment = TextAnchor.UpperLeft;
        text.verticalOverflow = scrollableContent ? VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
    }
}

[Serializable]
public class P10BPlusManualPlaytestChecklist
{
    public string schemaVersion = "p10b_plus.manual_playtest_checklist.v1";
    public bool finalWindowsExeBuildDeferredToP10C = true;
    public string[] checklist = Array.Empty<string>();
}
