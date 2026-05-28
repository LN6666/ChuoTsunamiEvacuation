using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NewMapLightingController : MonoBehaviour
{
    private Light sun;
    private Light fillLight;
    private NewMapWeatherPreset currentWeather = NewMapWeatherPreset.ClearDay;
    private NewMapLightingProfile currentProfile;
    private NewMapLightingProfilesConfig profiles;

    public NewMapWeatherPreset CurrentWeather => currentWeather;
    public bool ClearDayConfigured { get; private set; }
    public float DirectionalLightIntensity => sun != null ? sun.intensity : 0f;
    public float FillLightIntensity => fillLight != null && fillLight.enabled ? fillLight.intensity : 0f;
    public float AmbientSkyBrightness => (RenderSettings.ambientSkyColor.r + RenderSettings.ambientSkyColor.g + RenderSettings.ambientSkyColor.b) / 3f;
    public float SkyBrightness => currentProfile != null ? currentProfile.SkyBrightness : AmbientSkyBrightness;
    public float NightBuildingReadabilityScore => AmbientSkyBrightness * RenderSettings.ambientIntensity + FillLightIntensity * 0.5f;

    public static NewMapLightingController Create(Transform parent)
    {
        GameObject controllerObject = new GameObject("NewMap_LightingController");
        controllerObject.transform.SetParent(parent, false);
        NewMapLightingController controller = controllerObject.AddComponent<NewMapLightingController>();
        controller.profiles = NewMapLightingProfilesConfig.Load();
        controller.EnsureSun();
        controller.EnsureFillLight();
        controller.ApplyWeather(NewMapWeatherPreset.ClearDay);
        return controller;
    }

    public void ApplyWeather(NewMapWeatherPreset weather)
    {
        currentWeather = weather;
        EnsureSun();
        EnsureFillLight();
        if (profiles == null)
        {
            profiles = NewMapLightingProfilesConfig.Load();
        }

        ApplyLightingProfile(profiles.GetProfile(weather));
        ClearDayConfigured = weather == NewMapWeatherPreset.ClearDay || ClearDayConfigured;
    }

    private void EnsureSun()
    {
        if (sun != null)
        {
            return;
        }

        GameObject sunObject = GameObject.Find("NewMap_ClearDay_DirectionalLight");
        if (sunObject == null)
        {
            sunObject = new GameObject("NewMap_ClearDay_DirectionalLight");
        }

        sunObject.transform.SetParent(transform, true);
        sunObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        sun = sunObject.GetComponent<Light>();
        if (sun == null)
        {
            sun = sunObject.AddComponent<Light>();
        }

        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        RenderSettings.sun = sun;
    }

    private void EnsureFillLight()
    {
        if (fillLight != null)
        {
            return;
        }

        GameObject fillObject = GameObject.Find("NewMap_NightReadable_FillLight");
        if (fillObject == null)
        {
            fillObject = new GameObject("NewMap_NightReadable_FillLight");
        }

        fillObject.transform.SetParent(transform, true);
        fillObject.transform.rotation = Quaternion.Euler(35f, 140f, 0f);
        fillLight = fillObject.GetComponent<Light>();
        if (fillLight == null)
        {
            fillLight = fillObject.AddComponent<Light>();
        }

        fillLight.type = LightType.Directional;
        fillLight.shadows = LightShadows.None;
    }

    private void ApplyLightingProfile(NewMapLightingProfile profile)
    {
        currentProfile = profile ?? NewMapLightingProfilesConfig.Default().clear_day;
        Color sky = currentProfile.SkyColor;
        Color ambientSky = currentProfile.AmbientSkyColor;
        Color ambientEquator = currentProfile.AmbientEquatorColor;
        Color ambientGround = currentProfile.AmbientGroundColor;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSky;
        RenderSettings.ambientEquatorColor = ambientEquator;
        RenderSettings.ambientGroundColor = ambientGround;
        RenderSettings.ambientIntensity = currentProfile.ambientIntensity;
        RenderSettings.fog = currentProfile.fog;
        RenderSettings.fogColor = currentProfile.FogColor;
        RenderSettings.fogDensity = currentProfile.fogDensity;
        RenderSettings.skybox = null;
        ApplyCameraSkyColor(sky);

        if (sun != null)
        {
            sun.enabled = true;
            sun.intensity = currentProfile.directionalLightIntensity;
            sun.color = currentProfile.DirectionalLightColor;
        }

        if (fillLight != null)
        {
            fillLight.enabled = currentProfile.fillLightIntensity > 0.001f;
            fillLight.intensity = currentProfile.fillLightIntensity;
            fillLight.color = currentProfile.FillLightColor;
        }
    }

    private static void ApplyCameraSkyColor(Color color)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = color;
        }
    }
}

[System.Serializable]
public sealed class NewMapLightingProfilesConfig
{
    public NewMapLightingProfile clear_day;
    public NewMapLightingProfile rainy_day;
    public NewMapLightingProfile night_clear;
    public NewMapLightingProfile night_rain;

    public static NewMapLightingProfilesConfig Default()
    {
        return new NewMapLightingProfilesConfig
        {
            clear_day = NewMapLightingProfile.ClearDay(),
            rainy_day = NewMapLightingProfile.RainyDay(),
            night_clear = NewMapLightingProfile.NightClear(),
            night_rain = NewMapLightingProfile.NightRain()
        };
    }

    public static NewMapLightingProfilesConfig Load()
    {
        NewMapLightingProfilesConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_lighting_profiles.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapLightingProfilesConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap lighting profiles could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.EnsureDefaults();
        return config;
    }

    public NewMapLightingProfile GetProfile(NewMapWeatherPreset weather)
    {
        EnsureDefaults();
        switch (weather)
        {
            case NewMapWeatherPreset.RainyDay:
                return rainy_day;
            case NewMapWeatherPreset.NightClear:
                return night_clear;
            case NewMapWeatherPreset.NightRain:
                return night_rain;
            default:
                return clear_day;
        }
    }

    private void EnsureDefaults()
    {
        clear_day = clear_day ?? NewMapLightingProfile.ClearDay();
        rainy_day = rainy_day ?? NewMapLightingProfile.RainyDay();
        night_clear = night_clear ?? NewMapLightingProfile.NightClear();
        night_rain = night_rain ?? NewMapLightingProfile.NightRain();
        clear_day.ClampValues();
        rainy_day.ClampValues();
        night_clear.ClampValues();
        night_rain.ClampValues();
    }
}

[System.Serializable]
public sealed class NewMapLightingProfile
{
    public float skyR;
    public float skyG;
    public float skyB;
    public float directionalLightIntensity;
    public float fillLightIntensity;
    public float ambientIntensity;
    public float ambientSkyR;
    public float ambientSkyG;
    public float ambientSkyB;
    public float ambientEquatorR;
    public float ambientEquatorG;
    public float ambientEquatorB;
    public float ambientGroundR;
    public float ambientGroundG;
    public float ambientGroundB;
    public bool fog;
    public float fogDensity;
    public float fogR;
    public float fogG;
    public float fogB;

    public Color SkyColor => new Color(skyR, skyG, skyB, 1f);
    public Color AmbientSkyColor => new Color(ambientSkyR, ambientSkyG, ambientSkyB, 1f);
    public Color AmbientEquatorColor => new Color(ambientEquatorR, ambientEquatorG, ambientEquatorB, 1f);
    public Color AmbientGroundColor => new Color(ambientGroundR, ambientGroundG, ambientGroundB, 1f);
    public Color FogColor => new Color(fogR, fogG, fogB, 1f);
    public Color DirectionalLightColor => Color.white;
    public Color FillLightColor => new Color(0.70f, 0.78f, 1.0f, 1f);
    public float SkyBrightness => (skyR + skyG + skyB) / 3f;

    public static NewMapLightingProfile ClearDay()
    {
        return new NewMapLightingProfile
        {
            skyR = 0.62f,
            skyG = 0.74f,
            skyB = 0.92f,
            directionalLightIntensity = 1.25f,
            fillLightIntensity = 0f,
            ambientIntensity = 1.12f,
            ambientSkyR = 0.72f,
            ambientSkyG = 0.78f,
            ambientSkyB = 0.86f,
            ambientEquatorR = 0.52f,
            ambientEquatorG = 0.57f,
            ambientEquatorB = 0.62f,
            ambientGroundR = 0.38f,
            ambientGroundG = 0.40f,
            ambientGroundB = 0.42f,
            fog = false,
            fogDensity = 0.01f,
            fogR = 0.72f,
            fogG = 0.78f,
            fogB = 0.86f
        };
    }

    public static NewMapLightingProfile RainyDay()
    {
        return new NewMapLightingProfile
        {
            skyR = 0.44f,
            skyG = 0.49f,
            skyB = 0.56f,
            directionalLightIntensity = 0.95f,
            fillLightIntensity = 0.05f,
            ambientIntensity = 0.92f,
            ambientSkyR = 0.56f,
            ambientSkyG = 0.61f,
            ambientSkyB = 0.66f,
            ambientEquatorR = 0.42f,
            ambientEquatorG = 0.46f,
            ambientEquatorB = 0.50f,
            ambientGroundR = 0.30f,
            ambientGroundG = 0.33f,
            ambientGroundB = 0.36f,
            fog = true,
            fogDensity = 0.004f,
            fogR = 0.42f,
            fogG = 0.46f,
            fogB = 0.50f
        };
    }

    public static NewMapLightingProfile NightClear()
    {
        return new NewMapLightingProfile
        {
            skyR = 0.025f,
            skyG = 0.035f,
            skyB = 0.065f,
            directionalLightIntensity = 0.24f,
            fillLightIntensity = 0.42f,
            ambientIntensity = 1.0f,
            ambientSkyR = 0.28f,
            ambientSkyG = 0.30f,
            ambientSkyB = 0.34f,
            ambientEquatorR = 0.20f,
            ambientEquatorG = 0.22f,
            ambientEquatorB = 0.26f,
            ambientGroundR = 0.16f,
            ambientGroundG = 0.17f,
            ambientGroundB = 0.20f,
            fog = false,
            fogDensity = 0.01f,
            fogR = 0.04f,
            fogG = 0.05f,
            fogB = 0.08f
        };
    }

    public static NewMapLightingProfile NightRain()
    {
        return new NewMapLightingProfile
        {
            skyR = 0.018f,
            skyG = 0.024f,
            skyB = 0.040f,
            directionalLightIntensity = 0.20f,
            fillLightIntensity = 0.36f,
            ambientIntensity = 0.9f,
            ambientSkyR = 0.23f,
            ambientSkyG = 0.25f,
            ambientSkyB = 0.30f,
            ambientEquatorR = 0.17f,
            ambientEquatorG = 0.19f,
            ambientEquatorB = 0.23f,
            ambientGroundR = 0.13f,
            ambientGroundG = 0.15f,
            ambientGroundB = 0.18f,
            fog = true,
            fogDensity = 0.006f,
            fogR = 0.08f,
            fogG = 0.09f,
            fogB = 0.12f
        };
    }

    public void ClampValues()
    {
        skyR = Mathf.Clamp01(skyR);
        skyG = Mathf.Clamp01(skyG);
        skyB = Mathf.Clamp01(skyB);
        directionalLightIntensity = Mathf.Clamp(directionalLightIntensity, 0f, 3f);
        fillLightIntensity = Mathf.Clamp(fillLightIntensity, 0f, 2f);
        ambientIntensity = Mathf.Clamp(ambientIntensity, 0f, 2f);
        ambientSkyR = Mathf.Clamp01(ambientSkyR);
        ambientSkyG = Mathf.Clamp01(ambientSkyG);
        ambientSkyB = Mathf.Clamp01(ambientSkyB);
        ambientEquatorR = Mathf.Clamp01(ambientEquatorR);
        ambientEquatorG = Mathf.Clamp01(ambientEquatorG);
        ambientEquatorB = Mathf.Clamp01(ambientEquatorB);
        ambientGroundR = Mathf.Clamp01(ambientGroundR);
        ambientGroundG = Mathf.Clamp01(ambientGroundG);
        ambientGroundB = Mathf.Clamp01(ambientGroundB);
        fogDensity = Mathf.Clamp(fogDensity, 0f, 0.05f);
        fogR = Mathf.Clamp01(fogR);
        fogG = Mathf.Clamp01(fogG);
        fogB = Mathf.Clamp01(fogB);
    }
}
