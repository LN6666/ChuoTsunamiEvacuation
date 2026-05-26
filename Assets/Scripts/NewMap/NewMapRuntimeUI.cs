using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class NewMapRuntimeUI : MonoBehaviour
{
    private static Font resolvedRuntimeFont;

    private Canvas canvas;
    private GameObject startPanel;
    private GameObject pausePanel;
    private GameObject rulesPanel;
    private GameObject resultPanel;
    private Text hudText;
    private Text interactionText;
    private Text rulesText;
    private Text resultTitleText;
    private Text resultBodyText;
    private Text modeTitleText;
    private Text forceQuitExplanationText;
    private bool japanese;
    private string lastResultReason = string.Empty;
    private string lastResultDetail = string.Empty;

    public Action TourismRequested;
    public Action EvacuationRequested;
    public Action ResumeRequested;
    public Action ResetRequested;
    public Action ForceQuitRequested;
    public Action<NewMapWeatherPreset> WeatherRequested;

    public bool IsJapanese => japanese;
    public bool IsStartMenuVisible => startPanel != null && startPanel.activeSelf;
    public bool IsPauseVisible => pausePanel != null && pausePanel.activeSelf;
    public bool IsRulesVisible => rulesPanel != null && rulesPanel.activeSelf;
    public bool IsResultVisible => resultPanel != null && resultPanel.activeSelf;
    public bool RulesPanelHasScrollRect => rulesPanel != null && rulesPanel.GetComponentInChildren<ScrollRect>(true) != null;
    public string LastResultReason => lastResultReason;
    public string LastResultDetail => lastResultDetail;

    public static NewMapRuntimeUI Create(Transform parent)
    {
        GameObject uiObject = new GameObject("NewMap_RuntimeUI");
        uiObject.transform.SetParent(parent, false);
        NewMapRuntimeUI ui = uiObject.AddComponent<NewMapRuntimeUI>();
        ui.Build();
        return ui;
    }

    public void SetLanguage(bool useJapanese)
    {
        japanese = useJapanese;
        RefreshStaticText();
    }

    public void ShowStartMenu()
    {
        SetActive(startPanel, true);
        SetActive(pausePanel, false);
        SetActive(rulesPanel, false);
        SetActive(resultPanel, false);
        HideInteraction();
        SetHud("Select mode.");
    }

    public void ShowHud()
    {
        SetActive(startPanel, false);
        SetActive(pausePanel, false);
        SetActive(rulesPanel, false);
    }

    public void SetPauseVisible(bool visible)
    {
        SetActive(pausePanel, visible);
        SetActive(rulesPanel, false);
    }

    public void ToggleRules()
    {
        SetActive(rulesPanel, rulesPanel != null && !rulesPanel.activeSelf);
    }

    public void SetHud(string text)
    {
        if (hudText != null)
        {
            hudText.text = text;
        }
    }

    public void ShowInteraction(NewMapRuntimeTarget target, NewMapGameMode mode)
    {
        if (interactionText == null)
        {
            return;
        }

        if (target == null)
        {
            HideInteraction();
            return;
        }

        interactionText.gameObject.SetActive(true);
        string warning = target.NonOfficialWarningRequired
            ? (japanese ? "非公式候補です。安全承認ではありません。" : "Non-official candidate. This is not a safety approval.")
            : string.Empty;
        string action = mode == NewMapGameMode.Tourism
            ? (japanese ? "E: 情報を見る" : "E: inspect")
            : (japanese ? "E: 入る" : "E: enter");
        interactionText.text = $"{target.DisplayName}\n{warning}\n{action}";
    }

    public void HideInteraction()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
            interactionText.text = string.Empty;
        }
    }

    public void ShowResult(bool success, string reason, string detail)
    {
        SetActive(resultPanel, true);
        lastResultReason = reason ?? string.Empty;
        lastResultDetail = detail ?? string.Empty;
        if (resultTitleText != null)
        {
            resultTitleText.text = success
                ? (japanese ? "避難成功" : "Evacuation Success")
                : (japanese ? "避難失敗" : "Evacuation Failed");
        }

        if (resultBodyText != null)
        {
            resultBodyText.text = $"{reason}\n\n{detail}";
        }
    }

    public void HideResult()
    {
        SetActive(resultPanel, false);
    }

    private void Build()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        EnsureRuntimeEventSystem();

        hudText = CreateText("NewMap_HUD", transform, new Vector2(18f, -18f), new Vector2(740f, 160f), 20, TextAnchor.UpperLeft);
        interactionText = CreateText("NewMap_Interaction", transform, new Vector2(18f, -210f), new Vector2(620f, 130f), 20, TextAnchor.UpperLeft);
        interactionText.gameObject.SetActive(false);

        startPanel = CreatePanel("NewMap_StartMenu", new Vector2(0f, 0f), new Vector2(920f, 720f), new Color(0.05f, 0.07f, 0.08f, 0.86f));
        modeTitleText = CreateText("Title", startPanel.transform, new Vector2(0f, -28f), new Vector2(840f, 70f), 30, TextAnchor.MiddleCenter);
        CreateButton("TourismButton", startPanel.transform, new Vector2(-230f, -130f), new Vector2(300f, 58f), "Tourism Mode", () => TourismRequested?.Invoke());
        CreateButton("EvacuationButton", startPanel.transform, new Vector2(230f, -130f), new Vector2(300f, 58f), "Evacuation Mode", () => EvacuationRequested?.Invoke());
        CreateButton("EnglishButton", startPanel.transform, new Vector2(-300f, -220f), new Vector2(190f, 48f), "English", () => SetLanguage(false));
        CreateButton("JapaneseButton", startPanel.transform, new Vector2(-90f, -220f), new Vector2(190f, 48f), "日本語", () => SetLanguage(true));
        CreateButton("RulesButton", startPanel.transform, new Vector2(180f, -220f), new Vector2(220f, 48f), "Rules", ToggleRules);
        CreateButton("ClearDayButton", startPanel.transform, new Vector2(-300f, -310f), new Vector2(190f, 42f), "Clear", () => WeatherRequested?.Invoke(NewMapWeatherPreset.ClearDay));
        CreateButton("RainButton", startPanel.transform, new Vector2(-90f, -310f), new Vector2(190f, 42f), "Rain", () => WeatherRequested?.Invoke(NewMapWeatherPreset.RainyDay));
        CreateButton("NightButton", startPanel.transform, new Vector2(120f, -310f), new Vector2(190f, 42f), "Night", () => WeatherRequested?.Invoke(NewMapWeatherPreset.NightClear));
        CreateButton("NightRainButton", startPanel.transform, new Vector2(330f, -310f), new Vector2(190f, 42f), "Night Rain", () => WeatherRequested?.Invoke(NewMapWeatherPreset.NightRain));

        pausePanel = CreatePanel("NewMap_PauseMenu", new Vector2(0f, 0f), new Vector2(760f, 580f), new Color(0.05f, 0.07f, 0.08f, 0.9f));
        CreateText("PauseTitle", pausePanel.transform, new Vector2(0f, -28f), new Vector2(680f, 60f), 28, TextAnchor.MiddleCenter).text = "Pause";
        CreateButton("ResumeButton", pausePanel.transform, new Vector2(0f, -105f), new Vector2(260f, 50f), "Resume", () => ResumeRequested?.Invoke());
        CreateButton("PauseLanguageButton", pausePanel.transform, new Vector2(0f, -175f), new Vector2(260f, 50f), "Language / 言語", () => SetLanguage(!japanese));
        CreateButton("PauseRulesButton", pausePanel.transform, new Vector2(0f, -245f), new Vector2(260f, 50f), "Rules", ToggleRules);
        CreateButton("QuitStartButton", pausePanel.transform, new Vector2(0f, -315f), new Vector2(260f, 50f), "Quit to Menu", () => ResetRequested?.Invoke());
        CreateButton("ForceQuitButton", pausePanel.transform, new Vector2(0f, -385f), new Vector2(260f, 50f), "Force Quit", () => ForceQuitRequested?.Invoke());
        forceQuitExplanationText = CreateText("ForceQuitText", pausePanel.transform, new Vector2(0f, -470f), new Vector2(620f, 70f), 16, TextAnchor.UpperCenter);

        rulesPanel = CreatePanel("NewMap_RulesPanel", new Vector2(0f, 0f), new Vector2(980f, 700f), new Color(0.03f, 0.04f, 0.05f, 0.94f));
        rulesText = CreateRulesScrollArea(rulesPanel.transform);
        CreateButton("CloseRulesButton", rulesPanel.transform, new Vector2(0f, -325f), new Vector2(180f, 44f), "Close", ToggleRules);

        resultPanel = CreatePanel("NewMap_ResultPanel", new Vector2(0f, 0f), new Vector2(860f, 540f), new Color(0.04f, 0.05f, 0.06f, 0.92f));
        resultTitleText = CreateText("ResultTitle", resultPanel.transform, new Vector2(0f, -45f), new Vector2(760f, 58f), 28, TextAnchor.MiddleCenter);
        resultBodyText = CreateText("ResultBody", resultPanel.transform, new Vector2(0f, -150f), new Vector2(760f, 320f), 19, TextAnchor.UpperLeft);
        CreateButton("ResultCloseButton", resultPanel.transform, new Vector2(0f, -230f), new Vector2(180f, 44f), "Close", HideResult);

        RefreshStaticText();
        ShowStartMenu();
    }

    private void RefreshStaticText()
    {
        if (modeTitleText != null)
        {
            modeTitleText.text = japanese
                ? "Chuo_BaseMap - 新マップ基準"
                : "Chuo_BaseMap - New Map Baseline";
        }

        if (rulesText != null)
        {
            rulesText.text = japanese ? GetRulesJa() : GetRulesEn();
        }

        if (forceQuitExplanationText != null)
        {
            forceQuitExplanationText.text = japanese
                ? "通常はメニューに戻ってください。Force Quit はプレイヤービルド終了用です。"
                : "Use Quit to Menu normally. Force Quit exits a player build when needed.";
        }
    }

    private Text CreateRulesScrollArea(Transform parent)
    {
        GameObject scrollObject = new GameObject("RulesScrollView");
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 1f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 1f);
        scrollRectTransform.pivot = new Vector2(0.5f, 1f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -24f);
        scrollRectTransform.sizeDelta = new Vector2(900f, 570f);

        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0.12f);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 34f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.offsetMin = new Vector2(12f, 12f);
        viewportRect.offsetMax = new Vector2(-12f, -12f);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 900f);

        Text text = CreateText("RulesText", content.transform, Vector2.zero, new Vector2(850f, 880f), 18, TextAnchor.UpperLeft);
        text.rectTransform.anchorMin = new Vector2(0f, 1f);
        text.rectTransform.anchorMax = new Vector2(1f, 1f);
        text.rectTransform.pivot = new Vector2(0.5f, 1f);
        text.rectTransform.anchoredPosition = Vector2.zero;
        text.rectTransform.sizeDelta = new Vector2(0f, 880f);
        text.verticalOverflow = VerticalWrapMode.Overflow;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        return text;
    }

    private GameObject CreatePanel(string name, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(transform, false);
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = parent == transform ? new Vector2(0f, 1f) : new Vector2(0.5f, 1f);
        rectTransform.anchorMax = parent == transform ? new Vector2(0f, 1f) : new Vector2(0.5f, 1f);
        rectTransform.pivot = parent == transform ? new Vector2(0f, 1f) : new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        Font font = ResolveRuntimeFont();
        if (font != null)
        {
            text.font = font;
        }

        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 11;
        text.resizeTextMaxSize = fontSize;
        return text;
    }

    private static Font ResolveRuntimeFont()
    {
        if (resolvedRuntimeFont != null)
        {
            return resolvedRuntimeFont;
        }

        resolvedRuntimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (resolvedRuntimeFont != null)
        {
            return resolvedRuntimeFont;
        }

        resolvedRuntimeFont = Font.CreateDynamicFontFromOSFont(
            new[] { "Yu Gothic UI", "Meiryo", "Segoe UI", "Arial" },
            16);
        if (resolvedRuntimeFont == null)
        {
            Debug.LogWarning("NewMap runtime UI could not resolve a font. Text components will keep Unity defaults.");
        }

        return resolvedRuntimeFont;
    }

    private void CreateButton(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.14f, 0.19f, 0.22f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);

        Text text = CreateText(name + "_Text", buttonObject.transform, Vector2.zero, size, 17, TextAnchor.MiddleCenter);
        text.text = label;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchoredPosition = Vector2.zero;
        text.rectTransform.sizeDelta = Vector2.zero;
    }

    public static void EnsureRuntimeEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static void SetActive(GameObject gameObject, bool active)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(active);
        }
    }

    private static string GetRulesEn()
    {
        return
            "New Chuo_BaseMap baseline\n" +
            "Tourism Mode: exploration only. Tsunami warning, light curtain, hazard failure, crowd failure, collapse/debris failure, and stamina drain are disabled.\n" +
            "Evacuation Mode: Stage 1 Warning shows the countdown while the light curtain stays hidden and risk contact is ignored. Stage 2 FrontApproaching shows the light curtain and hazard checks become active.\n" +
            "Green frames mark prototype guidance targets only. A green frame does not mean official safety approval.\n" +
            "Non-official candidates require warnings and are not safe by default.\n" +
            "Routes are estimated prototype guidance, not official evacuation routes.\n" +
            "Old map targets missing from the new map are disabled and must not appear as active gameplay targets.";
    }

    private static string GetRulesJa()
    {
        return
            "新しい Chuo_BaseMap 基準\n" +
            "観光モード: 探索のみ。津波警報、ライトカーテン、危険判定、群衆失敗、倒壊・瓦礫失敗、スタミナ消費は無効です。\n" +
            "避難モード: 第1段階は警報とカウントダウンのみで、ライトカーテンは非表示、接触判定は無視されます。第2段階でライトカーテンが表示され、危険判定が有効になります。\n" +
            "緑フレームは試作ガイダンス対象です。公式な安全承認を意味しません。\n" +
            "非公式候補には警告が必要で、標準では安全承認されません。\n" +
            "ルートは推定プロトタイプ案内であり、公式避難ルートではありません。\n" +
            "新マップに存在確認できない旧ターゲットは無効化され、ゲーム中に表示してはいけません。";
    }
}
