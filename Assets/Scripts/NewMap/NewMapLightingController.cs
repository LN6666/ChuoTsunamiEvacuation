using UnityEngine;
using UnityEngine.Rendering;

public sealed class NewMapLightingController : MonoBehaviour
{
    private Light sun;
    private NewMapWeatherPreset currentWeather = NewMapWeatherPreset.ClearDay;

    public NewMapWeatherPreset CurrentWeather => currentWeather;
    public bool ClearDayConfigured { get; private set; }
    public float DirectionalLightIntensity => sun != null ? sun.intensity : 0f;
    public float AmbientSkyBrightness => (RenderSettings.ambientSkyColor.r + RenderSettings.ambientSkyColor.g + RenderSettings.ambientSkyColor.b) / 3f;

    public static NewMapLightingController Create(Transform parent)
    {
        GameObject controllerObject = new GameObject("NewMap_LightingController");
        controllerObject.transform.SetParent(parent, false);
        NewMapLightingController controller = controllerObject.AddComponent<NewMapLightingController>();
        controller.EnsureSun();
        controller.ApplyWeather(NewMapWeatherPreset.ClearDay);
        return controller;
    }

    public void ApplyWeather(NewMapWeatherPreset weather)
    {
        currentWeather = weather;
        EnsureSun();

        switch (weather)
        {
            case NewMapWeatherPreset.RainyDay:
                ApplyLighting(
                    0.95f,
                    new Color(0.56f, 0.61f, 0.66f),
                    new Color(0.42f, 0.46f, 0.50f),
                    new Color(0.30f, 0.33f, 0.36f),
                    0.9f,
                    fog: true);
                break;
            case NewMapWeatherPreset.NightClear:
                ApplyLighting(
                    0.34f,
                    new Color(0.20f, 0.24f, 0.32f),
                    new Color(0.15f, 0.18f, 0.24f),
                    new Color(0.11f, 0.13f, 0.18f),
                    0.85f,
                    fog: false);
                break;
            case NewMapWeatherPreset.NightRain:
                ApplyLighting(
                    0.24f,
                    new Color(0.14f, 0.17f, 0.23f),
                    new Color(0.11f, 0.13f, 0.17f),
                    new Color(0.08f, 0.10f, 0.13f),
                    0.8f,
                    fog: true);
                break;
            default:
                ApplyLighting(
                    1.25f,
                    new Color(0.72f, 0.78f, 0.86f),
                    new Color(0.52f, 0.57f, 0.62f),
                    new Color(0.38f, 0.40f, 0.42f),
                    1.12f,
                    fog: false);
                ClearDayConfigured = true;
                break;
        }
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

    private void ApplyLighting(
        float sunIntensity,
        Color sky,
        Color equator,
        Color ground,
        float ambientIntensity,
        bool fog)
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = sky;
        RenderSettings.ambientEquatorColor = equator;
        RenderSettings.ambientGroundColor = ground;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.fog = fog;
        RenderSettings.fogColor = fog ? equator : Color.gray;
        RenderSettings.fogDensity = fog ? 0.004f : 0.01f;

        if (sun != null)
        {
            sun.enabled = true;
            sun.intensity = sunIntensity;
            sun.color = Color.white;
        }
    }
}
