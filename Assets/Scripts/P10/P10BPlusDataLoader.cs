using System.IO;
using UnityEngine;

public static class P10BPlusDataLoader
{
    public const string LocalizationEnglishFileName = "p10b_plus_localization_en.json";
    public const string LocalizationJapaneseFileName = "p10b_plus_localization_ja.json";
    public const string UiConfigFileName = "p10b_plus_ui_config.json";
    public const string WeatherConfigFileName = "p10b_plus_weather_config.json";
    public const string MovementStaminaConfigFileName = "p10b_plus_movement_stamina_config.json";
    public const string AvatarMobilityConfigFileName = "p10b_plus_avatar_mobility_config.json";
    public const string ManualPlaytestChecklistFileName = "p10b_plus_manual_playtest_checklist.json";

    public static P9BLoadResult<P10BPlusLocalizationTable> LoadEnglishLocalization()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusLocalizationTable>(
            P10DataPath(LocalizationEnglishFileName),
            "P10-B+ English localization");
    }

    public static P9BLoadResult<P10BPlusLocalizationTable> LoadJapaneseLocalization()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusLocalizationTable>(
            P10DataPath(LocalizationJapaneseFileName),
            "P10-B+ Japanese localization");
    }

    public static P9BLoadResult<P10BPlusUiConfig> LoadUiConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusUiConfig>(
            P10DataPath(UiConfigFileName),
            "P10-B+ UI config");
    }

    public static P9BLoadResult<P10BPlusWeatherConfig> LoadWeatherConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusWeatherConfig>(
            P10DataPath(WeatherConfigFileName),
            "P10-B+ weather config");
    }

    public static P9BLoadResult<P10BPlusMovementConfig> LoadMovementStaminaConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusMovementConfig>(
            P10DataPath(MovementStaminaConfigFileName),
            "P10-B+ movement and stamina config");
    }

    public static P9BLoadResult<P10BPlusAvatarMobilityConfig> LoadAvatarMobilityConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusAvatarMobilityConfig>(
            P10DataPath(AvatarMobilityConfigFileName),
            "P10-B+ avatar mobility config");
    }

    public static P9BLoadResult<P10BPlusManualPlaytestChecklist> LoadManualPlaytestChecklist()
    {
        return P9BDataLoader.LoadFromPath<P10BPlusManualPlaytestChecklist>(
            P10DataPath(ManualPlaytestChecklistFileName),
            "P10-B+ manual playtest checklist");
    }

    public static P10BPlusLocalizationService CreateLocalizationService(P10BPlusLanguage initialLanguage)
    {
        return new P10BPlusLocalizationService(
            LoadEnglishLocalization().data,
            LoadJapaneseLocalization().data,
            initialLanguage);
    }

    private static string P10DataPath(string fileName)
    {
        return RuntimeDataPathResolver.GetDataPath("P10", fileName);
    }
}
