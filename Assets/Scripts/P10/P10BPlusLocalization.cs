using System;
using UnityEngine;
using UnityEngine.UI;

public enum P10BPlusLanguage
{
    English,
    Japanese
}

[Serializable]
public class P10BPlusLocalizationTable
{
    public string schemaVersion = "p10b_plus.localization.v1";
    public string language = "en";
    public P10BPlusLocalizationEntry[] entries = Array.Empty<P10BPlusLocalizationEntry>();

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || entries == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] != null && string.Equals(entries[i].key, key, StringComparison.Ordinal))
            {
                return entries[i].value ?? string.Empty;
            }
        }

        return string.Empty;
    }
}

[Serializable]
public class P10BPlusLocalizationEntry
{
    public string key = string.Empty;
    public string value = string.Empty;
}

public class P10BPlusLocalizationService
{
    private readonly P10BPlusLocalizationTable englishTable;
    private readonly P10BPlusLocalizationTable japaneseTable;

    public P10BPlusLanguage CurrentLanguage { get; private set; }

    public P10BPlusLocalizationService(
        P10BPlusLocalizationTable english,
        P10BPlusLocalizationTable japanese,
        P10BPlusLanguage initialLanguage = P10BPlusLanguage.English)
    {
        englishTable = english ?? new P10BPlusLocalizationTable();
        japaneseTable = japanese ?? new P10BPlusLocalizationTable();
        CurrentLanguage = initialLanguage;
    }

    public void SetLanguage(P10BPlusLanguage language)
    {
        CurrentLanguage = language;
    }

    public string Translate(string key)
    {
        string value = GetTable(CurrentLanguage).Get(key);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        value = englishTable.Get(key);
        return string.IsNullOrEmpty(value) ? "[" + (key ?? string.Empty) + "]" : value;
    }

    private P10BPlusLocalizationTable GetTable(P10BPlusLanguage language)
    {
        return language == P10BPlusLanguage.Japanese ? japaneseTable : englishTable;
    }
}

public class P10BPlusLocalizedText : MonoBehaviour
{
    [SerializeField] private Text targetText;
    [SerializeField] private string localizationKey = string.Empty;

    public string LocalizationKey => localizationKey;

    public void Configure(Text text, string key, P10BPlusLocalizationService service)
    {
        targetText = text == null ? GetComponent<Text>() : text;
        localizationKey = key ?? string.Empty;
        Refresh(service);
    }

    public void Refresh(P10BPlusLocalizationService service)
    {
        if (targetText == null)
        {
            targetText = GetComponent<Text>();
        }

        if (targetText != null && service != null)
        {
            targetText.text = service.Translate(localizationKey);
        }
    }
}

public class P10BPlusLanguageSelector
{
    private readonly P10BPlusLocalizationService localizationService;

    public P10BPlusLanguageSelector(P10BPlusLocalizationService service)
    {
        localizationService = service;
    }

    public P10BPlusLanguage CurrentLanguage => localizationService == null
        ? P10BPlusLanguage.English
        : localizationService.CurrentLanguage;

    public void SelectEnglish()
    {
        localizationService?.SetLanguage(P10BPlusLanguage.English);
    }

    public void SelectJapanese()
    {
        localizationService?.SetLanguage(P10BPlusLanguage.Japanese);
    }
}
