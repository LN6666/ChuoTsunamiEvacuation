using UnityEngine;

public sealed class NewMapFullscreenModeController : MonoBehaviour
{
    public const string FullscreenPreferenceKey = "ChuoTsunamiEvacuation.FullscreenEnabled";
    private const string WindowedWidthPreferenceKey = "ChuoTsunamiEvacuation.WindowedWidth";
    private const string WindowedHeightPreferenceKey = "ChuoTsunamiEvacuation.WindowedHeight";
    private const int DefaultFullscreenPreference = 1;
    private const int MinimumWindowedWidth = 1024;
    private const int MinimumWindowedHeight = 576;
    private const float DefaultWindowedScale = 0.82f;

    private static NewMapFullscreenModeController instance;

    private float nextWindowedSizeSaveTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            return;
        }
#endif

        if (instance != null || FindObjectOfType<NewMapFullscreenModeController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("NewMap_FullscreenModeController");
        DontDestroyOnLoad(controllerObject);
        controllerObject.AddComponent<NewMapFullscreenModeController>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyStartupMode();
    }

    private void Update()
    {
        if (IsToggleShortcut(
            Input.GetKeyDown(KeyCode.F11),
            Input.GetKeyDown(KeyCode.Return),
            Input.GetKeyDown(KeyCode.KeypadEnter),
            Input.GetKey(KeyCode.LeftAlt),
            Input.GetKey(KeyCode.RightAlt)))
        {
            ToggleFullscreen();
        }

        SaveWindowedSizeIfNeeded();
    }

    public void ToggleFullscreen()
    {
        ApplyFullscreenMode(!IsFullscreenActive(), true);
    }

    public static bool IsToggleShortcut(
        bool f11Down,
        bool returnDown,
        bool keypadEnterDown,
        bool leftAltDown,
        bool rightAltDown)
    {
        if (f11Down)
        {
            return true;
        }

        return (returnDown || keypadEnterDown) && (leftAltDown || rightAltDown);
    }

    public static bool TryGetCommandLineFullscreenOverride(string[] args, out bool fullscreen)
    {
        fullscreen = false;
        if (args == null)
        {
            return false;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (string.IsNullOrWhiteSpace(arg))
            {
                continue;
            }

            string normalized = arg.Trim().ToLowerInvariant();
            if (normalized == "-fullscreen" || normalized == "--fullscreen")
            {
                fullscreen = true;
                return true;
            }

            if (normalized == "-windowed" || normalized == "--windowed")
            {
                fullscreen = false;
                return true;
            }

            if (normalized == "-screen-fullscreen" && i + 1 < args.Length)
            {
                string value = args[i + 1].Trim();
                if (value == "1")
                {
                    fullscreen = true;
                    return true;
                }

                if (value == "0")
                {
                    fullscreen = false;
                    return true;
                }
            }
        }

        return false;
    }

    public static Vector2Int ResolveWindowedSize(
        int savedWidth,
        int savedHeight,
        int displayWidth,
        int displayHeight,
        int currentWidth,
        int currentHeight)
    {
        int maxWidth = Mathf.Max(MinimumWindowedWidth, displayWidth > 0 ? displayWidth : currentWidth);
        int maxHeight = Mathf.Max(MinimumWindowedHeight, displayHeight > 0 ? displayHeight : currentHeight);

        if (savedWidth >= MinimumWindowedWidth && savedHeight >= MinimumWindowedHeight)
        {
            return new Vector2Int(
                Mathf.Clamp(savedWidth, MinimumWindowedWidth, maxWidth),
                Mathf.Clamp(savedHeight, MinimumWindowedHeight, maxHeight));
        }

        float aspect = currentWidth > 0 && currentHeight > 0
            ? (float)currentWidth / currentHeight
            : 16f / 9f;
        int targetWidth = Mathf.Clamp(Mathf.RoundToInt(maxWidth * DefaultWindowedScale), MinimumWindowedWidth, maxWidth);
        int targetHeight = Mathf.RoundToInt(targetWidth / Mathf.Max(0.1f, aspect));
        if (targetHeight > maxHeight)
        {
            targetHeight = Mathf.Clamp(Mathf.RoundToInt(maxHeight * DefaultWindowedScale), MinimumWindowedHeight, maxHeight);
            targetWidth = Mathf.Clamp(Mathf.RoundToInt(targetHeight * aspect), MinimumWindowedWidth, maxWidth);
        }

        return new Vector2Int(targetWidth, targetHeight);
    }

    private void ApplyStartupMode()
    {
        bool fullscreen;
        bool hasCommandLineOverride = TryGetCommandLineFullscreenOverride(System.Environment.GetCommandLineArgs(), out fullscreen);
        if (!hasCommandLineOverride)
        {
            fullscreen = PlayerPrefs.GetInt(FullscreenPreferenceKey, DefaultFullscreenPreference) != 0;
        }

        ApplyFullscreenMode(fullscreen, hasCommandLineOverride);
    }

    private static bool IsFullscreenActive()
    {
        return Screen.fullScreen && Screen.fullScreenMode != FullScreenMode.Windowed;
    }

    private static void ApplyFullscreenMode(bool fullscreen, bool savePreference)
    {
        if (fullscreen)
        {
            Resolution resolution = Screen.currentResolution;
            int width = resolution.width > 0 ? resolution.width : Mathf.Max(Screen.width, MinimumWindowedWidth);
            int height = resolution.height > 0 ? resolution.height : Mathf.Max(Screen.height, MinimumWindowedHeight);
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Resolution resolution = Screen.currentResolution;
            Vector2Int size = ResolveWindowedSize(
                PlayerPrefs.GetInt(WindowedWidthPreferenceKey, 0),
                PlayerPrefs.GetInt(WindowedHeightPreferenceKey, 0),
                resolution.width,
                resolution.height,
                Screen.width,
                Screen.height);
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
        }

        if (savePreference)
        {
            PlayerPrefs.SetInt(FullscreenPreferenceKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

    }

    private static void SaveWindowedSize(int width, int height)
    {
        if (width < MinimumWindowedWidth || height < MinimumWindowedHeight)
        {
            return;
        }

        PlayerPrefs.SetInt(WindowedWidthPreferenceKey, width);
        PlayerPrefs.SetInt(WindowedHeightPreferenceKey, height);
        PlayerPrefs.Save();
    }

    private void SaveWindowedSizeIfNeeded()
    {
        if (IsFullscreenActive() || Time.unscaledTime < nextWindowedSizeSaveTime)
        {
            return;
        }

        nextWindowedSizeSaveTime = Time.unscaledTime + 2f;
        SaveWindowedSize(Screen.width, Screen.height);
    }
}
