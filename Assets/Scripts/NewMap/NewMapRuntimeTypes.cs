using UnityEngine;

public enum NewMapGameMode
{
    None,
    Tourism,
    Evacuation
}

public enum NewMapTsunamiStage
{
    Inactive,
    Warning,
    FrontApproaching
}

public enum NewMapWeatherPreset
{
    ClearDay,
    RainyDay,
    NightClear,
    NightRain
}

public sealed class NewMapRuntimeTarget
{
    public string Id;
    public string DisplayName;
    public string Category;
    public bool IsOfficialShelter;
    public bool NonOfficialWarningRequired;
    public bool SafeApprovedByDefault;
    public bool EntranceBlocked;
    public bool SafeFloorAvailable;
    public float InteractionDistance = 3f;
    public float ClimbSeconds = 5f;
    public string DisabledReason = string.Empty;
    public string FinalBehavior = string.Empty;
    public Transform Anchor;
    public GameObject Marker;
    public GameObject GreenFrame;
    public GameObject RouteGuide;
    public NewMapBuildingEntryTrigger EntryTrigger;
    public bool HasBuildingEntryBounds;
    public Bounds BuildingEntryBounds;
    public System.Func<GameObject> GreenFrameFactory;
    public System.Func<GameObject> RouteGuideFactory;

    public bool ActiveInGame => Anchor != null && string.IsNullOrWhiteSpace(DisabledReason);

    public void EnsureGuidanceVisuals()
    {
        if (GreenFrame == null && GreenFrameFactory != null)
        {
            GreenFrame = GreenFrameFactory();
            GreenFrameFactory = null;
        }

        if (RouteGuide == null && RouteGuideFactory != null)
        {
            RouteGuide = RouteGuideFactory();
            RouteGuideFactory = null;
        }
    }
}

public static class NewMapRuntimeConstants
{
    public const string ScenePath = "Assets/Scenes/Chuo_BaseMap.unity";
    public const string SceneName = "Chuo_BaseMap";

    public const float EvacuationWalkSpeed = 1.0f;
    public const float EvacuationSprintSpeed = 5.0f;
    public const float TourismWalkSpeed = 2.0f;
    public const float TourismSprintSpeed = 10.0f;

    public const float ClearDayModifier = 1.0f;
    public const float RainyDayModifier = 0.75f;
    public const float NightClearModifier = 0.85f;
    public const float NightRainModifier = 0.65f;

    public static float GetWeatherModifier(NewMapWeatherPreset weather)
    {
        switch (weather)
        {
            case NewMapWeatherPreset.RainyDay:
                return RainyDayModifier;
            case NewMapWeatherPreset.NightClear:
                return NightClearModifier;
            case NewMapWeatherPreset.NightRain:
                return NightRainModifier;
            default:
                return ClearDayModifier;
        }
    }

    public static string GetWeatherLabel(NewMapWeatherPreset weather)
    {
        switch (weather)
        {
            case NewMapWeatherPreset.RainyDay:
                return "rainy_day";
            case NewMapWeatherPreset.NightClear:
                return "night_clear";
            case NewMapWeatherPreset.NightRain:
                return "night_rain";
            default:
                return "clear_day";
        }
    }
}
