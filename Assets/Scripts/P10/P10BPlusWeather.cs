using System;
using UnityEngine;
using UnityEngine.UI;

public enum P10BPlusWeatherMode
{
    ClearDay,
    RainyDay,
    NightClear,
    NightRain
}

[Serializable]
public class P10BPlusWeatherConfig
{
    public string schemaVersion = "p10b_plus.weather_config.v1";
    public P10BPlusWeatherRule[] modes = Array.Empty<P10BPlusWeatherRule>();

    public P10BPlusWeatherRule GetRule(P10BPlusWeatherMode mode)
    {
        string id = P10BPlusWeatherMovementModifier.ToModeId(mode);
        if (modes != null)
        {
            for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i] != null && string.Equals(modes[i].modeId, id, StringComparison.Ordinal))
                {
                    return modes[i];
                }
            }
        }

        return P10BPlusWeatherRule.CreateDefault(mode);
    }
}

[Serializable]
public class P10BPlusWeatherRule
{
    public string modeId = "clear_day";
    public float movementSpeedMultiplier = 1f;
    public bool nightOverlayEnabled;
    public float nightOverlayAlpha;
    public bool rainVisualPlaceholderOnly;
    public string notes = string.Empty;

    public static P10BPlusWeatherRule CreateDefault(P10BPlusWeatherMode mode)
    {
        switch (mode)
        {
            case P10BPlusWeatherMode.RainyDay:
                return new P10BPlusWeatherRule { modeId = "rainy_day", movementSpeedMultiplier = 0.75f, rainVisualPlaceholderOnly = true };
            case P10BPlusWeatherMode.NightClear:
                return new P10BPlusWeatherRule { modeId = "night_clear", movementSpeedMultiplier = 0.85f, nightOverlayEnabled = true, nightOverlayAlpha = 0.35f };
            case P10BPlusWeatherMode.NightRain:
                return new P10BPlusWeatherRule { modeId = "night_rain", movementSpeedMultiplier = 0.65f, nightOverlayEnabled = true, nightOverlayAlpha = 0.45f, rainVisualPlaceholderOnly = true };
            default:
                return new P10BPlusWeatherRule { modeId = "clear_day", movementSpeedMultiplier = 1f };
        }
    }
}

public static class P10BPlusWeatherMovementModifier
{
    public static float GetMultiplier(P10BPlusWeatherConfig config, P10BPlusWeatherMode mode)
    {
        P10BPlusWeatherRule rule = (config ?? new P10BPlusWeatherConfig()).GetRule(mode);
        return Mathf.Clamp(rule.movementSpeedMultiplier, 0.1f, 2f);
    }

    public static bool IsNightMode(P10BPlusWeatherConfig config, P10BPlusWeatherMode mode)
    {
        return (config ?? new P10BPlusWeatherConfig()).GetRule(mode).nightOverlayEnabled;
    }

    public static string ToModeId(P10BPlusWeatherMode mode)
    {
        switch (mode)
        {
            case P10BPlusWeatherMode.RainyDay:
                return "rainy_day";
            case P10BPlusWeatherMode.NightClear:
                return "night_clear";
            case P10BPlusWeatherMode.NightRain:
                return "night_rain";
            default:
                return "clear_day";
        }
    }
}

public class P10BPlusNightOverlay : MonoBehaviour
{
    [SerializeField] private Image overlayImage;

    public bool OverlayEnabled => overlayImage != null && overlayImage.gameObject.activeSelf;

    public void Apply(P10BPlusWeatherConfig config, P10BPlusWeatherMode mode)
    {
        P10BPlusWeatherRule rule = (config ?? new P10BPlusWeatherConfig()).GetRule(mode);
        EnsureImage();
        overlayImage.gameObject.SetActive(rule.nightOverlayEnabled);
        overlayImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(rule.nightOverlayAlpha));
    }

    private void EnsureImage()
    {
        if (overlayImage != null)
        {
            return;
        }

        var overlay = new GameObject("P10BPlus_NightOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlayImage = overlay.GetComponent<Image>();
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
