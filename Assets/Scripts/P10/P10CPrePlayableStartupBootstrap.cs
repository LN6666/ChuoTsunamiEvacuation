using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class P10CPreStartupDiagnosticsSnapshot
{
    public string buildMode = "playable";
    public string activeSceneName = string.Empty;
    public string activeScenePath = string.Empty;
    public string targetHighDetailScenePath = string.Empty;
    public bool targetHighDetailSceneActive;
    public bool targetHighDetailSceneCanLoad;
    public int preBootstrapSceneRendererCount;
    public bool p9TargetSceneLooksLikePlaceholder;
    public bool dataRootExists;
    public bool p8DataExists;
    public bool p9DataExists;
    public bool p10DataExists;
    public bool localizationLoaded;
    public bool startupUiBuilt;
    public int cameraCount;
    public int playerCount;
    public int eventSystemCount;
    public int canvasCount;
    public int gameManagerCount;
    public int resultPanelCount;
    public int shelterCount;
    public int shelterEntranceCount;
    public int p4RealShelterMarkerCount;
    public int p5QualificationMarkerCount;
    public int p5RealQualifiedShelterCount;
    public int p5HumanitarianCandidateCount;
    public int p6NpcCount;
    public int p6NavigationControllerCount;
    public bool p8RiskFrontLoaded;
    public bool p8RiskFrontVisible;
    public bool p8HandoffConsumed;
    public bool p9FinalOutcomeValidated;
    public int p9CrowdAgentCount;
    public int p9SpawnMarkerCount;
    public int p9EntranceSafeFloorMarkerCount;
    public int p9CollapseDebrisZoneCount;
    public int p10GreenFrameRuntimeCount;
    public int p10GreenFrameCount;
    public bool p10PauseRulesUiAvailable;
    public bool p10WeatherStaminaAttached;
    public bool profilingExporterEnabledByDefault;
    public bool profilingAutoQuitEnabled;
    public string[] limitations = Array.Empty<string>();
    public string summary = string.Empty;
}

public class P10CPrePlayableStartupBootstrap : MonoBehaviour
{
    public const string DisablePlayableStartupArg = "-p10cPreDisablePlayableStartup";
    public const string ForcePlayableStartupArg = "-p10cPrePlayableStartup";
    public const string AutoStartGameArg = "-p10cPreAutoStartGame";
    private const string StartupCanvasName = "P10CPre_PlayableStartupCanvas";
    private const string GameplayRootName = "GameplayTestRoot";
    private const string TestGroundName = "TestGround";
    private const string PlayerObjectName = "Player";

    private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly Vector3 FallbackOrigin = new Vector3(0f, 80f, -250f);
    private static bool installedInPlayer;

    [SerializeField] private P10CPrePlayableStartupConfig config = new P10CPrePlayableStartupConfig();

    private P10BPlusLocalizationService localizationService;
    private readonly List<LocalizedTextBinding> localizedTexts = new List<LocalizedTextBinding>();
    private GameObject canvasObject;
    private GameObject startPanel;
    private GameObject rulesPanel;
    private GameObject pausePanel;
    private Text diagnosticText;
    private int preBootstrapSceneRendererCount;
    private bool skipTargetSceneLoadForNextStart;

    public P10CPreStartupDiagnosticsSnapshot LastDiagnostics { get; private set; } = new P10CPreStartupDiagnosticsSnapshot();
    public string LastStartGameRequestedScenePath { get; private set; } = string.Empty;
    public bool StartMenuVisible => startPanel != null && startPanel.activeSelf;
    public bool RulesPanelVisible => rulesPanel != null && rulesPanel.activeSelf;
    public bool PauseMenuVisible => pausePanel != null && pausePanel.activeSelf;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallInPlayer()
    {
        if (installedInPlayer || HasCommandLineArgument(DisablePlayableStartupArg))
        {
            return;
        }

        P10CPrePlayableStartupConfig loadedConfig = LoadConfig();
        if (!loadedConfig.playableStartupMode && !HasCommandLineArgument(ForcePlayableStartupArg))
        {
            return;
        }

        installedInPlayer = true;
        GameObject bootstrapObject = new GameObject("P10CPre_PlayableStartupBootstrap");
        DontDestroyOnLoad(bootstrapObject);
        P10CPrePlayableStartupBootstrap bootstrap = bootstrapObject.AddComponent<P10CPrePlayableStartupBootstrap>();
        bootstrap.Build(loadedConfig);
    }
#endif

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pausePanel != null && startPanel != null && !startPanel.activeSelf)
        {
            TogglePause();
        }
    }

    public static P10CPrePlayableStartupBootstrap InstallForTest(P10CPrePlayableStartupConfig startupConfig = null)
    {
        GameObject bootstrapObject = new GameObject("P10CPre_PlayableStartupBootstrap_Test");
        P10CPrePlayableStartupBootstrap bootstrap = bootstrapObject.AddComponent<P10CPrePlayableStartupBootstrap>();
        bootstrap.Build(startupConfig ?? new P10CPrePlayableStartupConfig());
        return bootstrap;
    }

    public void Build(P10CPrePlayableStartupConfig startupConfig)
    {
        config = startupConfig ?? new P10CPrePlayableStartupConfig();
        preBootstrapSceneRendererCount = CountExistingSceneRenderers();
        localizationService = P10BPlusDataLoader.CreateLocalizationService(GetDefaultLanguage());
        EnsureEventSystem();
        EnsureStartupCanvas();
        BuildStartPanel();
        BuildRulesPanel();
        BuildPausePanel();
        ShowStartMenu();
        RefreshLocalizedTexts();
        CaptureAndLogDiagnostics("startup_menu_built");

        if (HasCommandLineArgument(AutoStartGameArg))
        {
            StartCoroutine(AutoStartGameForVerification());
        }
    }

    public void ShowStartMenu()
    {
        SetActive(startPanel, true);
        SetActive(rulesPanel, false);
        SetActive(pausePanel, false);
        Time.timeScale = 0f;
    }

    public void ShowRules()
    {
        SetActive(rulesPanel, true);
    }

    public void HideRules()
    {
        SetActive(rulesPanel, false);
    }

    public void SetLanguageEnglish()
    {
        localizationService?.SetLanguage(P10BPlusLanguage.English);
        RefreshLocalizedTexts();
    }

    public void SetLanguageJapanese()
    {
        localizationService?.SetLanguage(P10BPlusLanguage.Japanese);
        RefreshLocalizedTexts();
    }

    public void StartGame()
    {
        LastStartGameRequestedScenePath = config.targetHighDetailScenePath ?? string.Empty;
        SetActive(startPanel, false);
        SetActive(rulesPanel, false);
        SetActive(pausePanel, false);
        Time.timeScale = 1f;
        StartCoroutine(StartGameRoutine());
    }

    public void StartGameForTest(bool loadTargetScene)
    {
        skipTargetSceneLoadForNextStart = !loadTargetScene;
        StartGame();
    }

    private IEnumerator AutoStartGameForVerification()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(1f);
        if (StartMenuVisible)
        {
            Debug.Log("P10-C-Pre auto-start verification argument detected. Starting gameplay flow.");
            StartGame();
        }
    }

    private IEnumerator StartGameRoutine()
    {
        bool shouldLoadTargetScene = config.startGameLoadsHighDetailScene && !skipTargetSceneLoadForNextStart;
        skipTargetSceneLoadForNextStart = false;

        if (shouldLoadTargetScene && !IsTargetSceneActive())
        {
            if (CanLoadTargetScene())
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(config.targetHighDetailScenePath, LoadSceneMode.Single);
                while (load != null && !load.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                ShowDiagnostic(
                    "P10-C-Pre startup could not load " + config.targetHighDetailScenePath +
                    ". A fallback runtime bootstrap will run in the active scene.",
                    true);
            }
        }

        yield return null;

        RuntimeGameplayReferences gameplay = EnsurePlayableRuntime();
        RunActualP7IntegrationDiagnostics(gameplay);
        if (gameplay.GameManager != null && gameplay.GameManager.CurrentState == EvacuationGameManager.GameState.WaitingToStart)
        {
            gameplay.GameManager.StartGame();
        }

        if (HasCommandLineArgument(AutoStartGameArg))
        {
            TriggerVerificationOnlyGameplaySystems(gameplay);
            yield return null;
        }

        CaptureAndLogDiagnostics("start_game_completed");
        ShowDiagnostic(LastDiagnostics.summary, LastDiagnostics.limitations.Length > 0);
    }

    private void TriggerVerificationOnlyGameplaySystems(RuntimeGameplayReferences gameplay)
    {
        if (gameplay == null || gameplay.GameManager == null)
        {
            return;
        }

        if (gameplay.GameManager.CurrentState == EvacuationGameManager.GameState.PreEvent)
        {
            gameplay.GameManager.StartEvacuationEvent();
        }

        P10BGreenGroundFrameRuntime[] frameRuntimes = FindObjectsOfType<P10BGreenGroundFrameRuntime>();
        for (int i = 0; i < frameRuntimes.Length; i++)
        {
            frameRuntimes[i].SetTsunamiStarted(true);
        }
    }

    private RuntimeGameplayReferences EnsurePlayableRuntime()
    {
        var refs = new RuntimeGameplayReferences();
        refs.Root = EnsureGameplayRoot();
        refs.Origin = ResolvePlayableOrigin();
        refs.Camera = EnsureCamera(refs.Root.transform, refs.Origin);
        refs.Player = EnsurePlayer(refs.Root.transform, refs.Origin, refs.Camera);
        refs.GameplayCanvas = EnsureGameplayCanvas(refs.Root.transform);
        refs.GameUiManager = EnsureGameUi(refs.GameplayCanvas.transform);
        refs.ResultPanelController = EnsureResultPanel(refs.GameplayCanvas.transform);
        refs.CountdownManager = null;
        refs.TsunamiWall = EnsureTsunamiWall(refs.Root.transform, refs.Origin);
        refs.ClimbSimulation = null;
        refs.GameManager = EnsureGameManager(refs.Root.transform, refs.Player, refs.TsunamiWall, refs.GameUiManager, refs.ResultPanelController, out refs.CountdownManager, out refs.ClimbSimulation);
        EnsureFallbackGroundIfNeeded(refs.Origin);
        AttachWeatherAndStamina(refs.Player);
        return refs;
    }

    private void RunActualP7IntegrationDiagnostics(RuntimeGameplayReferences refs)
    {
        if (refs == null || !config.runActualP7SceneIntegrationDiagnostics)
        {
            return;
        }

        GenerateP4RealShelterMarkers(refs);
        GenerateP5QualificationAndCandidateLayers(refs);
        GenerateP8RiskFront();
        GenerateP9RuntimeLayers(refs);
        GenerateP10RuntimeLayers(refs);
        EnsureFallbackSheltersIfNeeded(refs);
        GenerateP6NpcAndNavigation(refs);
    }

    private void GenerateP4RealShelterMarkers(RuntimeGameplayReferences refs)
    {
        try
        {
            var sourceConfig = ShelterSourceConfigLoader.CreateDefaultConfig();
            sourceConfig.sourceMode = ShelterSourceConfigLoader.RealSampleSourceMode;
            sourceConfig.enableRealSampleLoading = true;
            ShelterDataSourceResolver.ShelterDataSourceResult source = ShelterDataSourceResolver.LoadFromConfig(sourceConfig);
            if (source.success && source.realShelters.Length > 0)
            {
                GameObject generatorObject = new GameObject("P10CPre_P4_RealShelterMarkerVerification");
                generatorObject.transform.SetParent(refs.Root.transform, false);
                generatorObject.AddComponent<RealShelterMarkerRuntimeGenerator>().Generate(source, refs.GameManager, refs.GameUiManager);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P4 real shelter marker verification skipped: " + exception.Message);
        }
    }

    private void GenerateP5QualificationAndCandidateLayers(RuntimeGameplayReferences refs)
    {
        try
        {
            P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
            if (bundle != null && bundle.HasCoreQualificationData)
            {
                GameObject overlayObject = new GameObject("P10CPre_P5C_QualificationOverlayVerification");
                overlayObject.transform.SetParent(refs.Root.transform, false);
                overlayObject.AddComponent<P5CQualificationOverlayRuntimeGenerator>().Generate(bundle);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P5-C qualification overlay verification skipped: " + exception.Message);
        }

        try
        {
            var sourceConfig = ShelterSourceConfigLoader.CreateDefaultConfig();
            sourceConfig.sourceMode = ShelterSourceConfigLoader.RealQualifiedSourceMode;
            ShelterDataSourceResolver.ShelterDataSourceResult source = ShelterDataSourceResolver.LoadFromConfig(sourceConfig);
            if (source.success && source.realQualifiedShelters.Length > 0)
            {
                GameObject generatorObject = new GameObject("P10CPre_P5D_RealQualifiedVerification");
                generatorObject.transform.SetParent(refs.Root.transform, false);
                generatorObject.AddComponent<P5DRealQualifiedShelterRuntimeGenerator>().Generate(source, refs.GameManager, refs.GameUiManager);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P5-D real-qualified verification skipped: " + exception.Message);
        }

        try
        {
            HumanitarianCandidateDataLoader.HumanitarianCandidateLoadResult candidates =
                HumanitarianCandidateDataLoader.LoadFromAssetsData();
            if (candidates != null && candidates.success && candidates.records.Length > 0)
            {
                GameObject generatorObject = new GameObject("P10CPre_P5GH_HumanitarianCandidateVerification");
                generatorObject.transform.SetParent(refs.Root.transform, false);
                generatorObject.AddComponent<P5GHHumanitarianCandidateRuntimeGenerator>().Generate(
                    candidates,
                    true,
                    refs.GameManager,
                    refs.GameUiManager);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P5-GH humanitarian candidate verification skipped: " + exception.Message);
        }
    }

    private void GenerateP8RiskFront()
    {
        try
        {
            GameObject riskFrontObject = GameObject.Find("P10CPre_P8_RiskFrontVerification") ??
                new GameObject("P10CPre_P8_RiskFrontVerification");
            P8RiskFrontController riskFront = riskFrontObject.GetComponent<P8RiskFrontController>() ??
                riskFrontObject.AddComponent<P8RiskFrontController>();
            riskFront.LoadFromP8Data();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P8 risk front verification skipped: " + exception.Message);
        }
    }

    private void GenerateP9RuntimeLayers(RuntimeGameplayReferences refs)
    {
        try
        {
            P9SceneRuntimeBootstrap bootstrap = refs.Root.GetComponent<P9SceneRuntimeBootstrap>() ??
                refs.Root.AddComponent<P9SceneRuntimeBootstrap>();
            P9BScenarioRuntimeSummaryResult summary = bootstrap.GenerateRuntimePrototype();
            Debug.Log("P10-C-Pre P9 runtime layer verification: " + (summary != null ? summary.summary : "summary unavailable"));

            Transform crowdParent = bootstrap.RuntimeRoot != null ? bootstrap.RuntimeRoot : refs.Root.transform;
            P9CrowdRuntimeSpawner crowdSpawner = refs.Root.GetComponent<P9CrowdRuntimeSpawner>() ??
                refs.Root.AddComponent<P9CrowdRuntimeSpawner>();
            P9CrowdRuntimeSpawnResult crowdResult = crowdSpawner.SpawnFromSampleData(crowdParent);
            Debug.Log("P10-C-Pre P9 crowd verification: " + (crowdResult != null ? crowdResult.summary : "summary unavailable"));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P9 runtime layer verification skipped: " + exception.Message);
        }

        try
        {
            P9DFinalGameplayFlowSummary summary = P9DFinalGameplayFlowValidator.Validate(
                P9DDataLoader.LoadFinalGameplayScenarioSample().data,
                P9DDataLoader.LoadAnchoringReportSample().data);
            Debug.Log("P10-C-Pre P9 outcome verification: " + (summary != null ? summary.summary : "summary unavailable"));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P9 outcome verification skipped: " + exception.Message);
        }
    }

    private void GenerateP10RuntimeLayers(RuntimeGameplayReferences refs)
    {
        try
        {
            P10BGreenGroundFrameConfig frameConfig = P10BDataLoader.LoadGreenGroundFrameConfig().data ?? new P10BGreenGroundFrameConfig();
            P10BGreenGroundFrameTargetCollection targets = P10BDataLoader.LoadGreenGroundFrameTargetsSample().data;
            GameObject frameObject = new GameObject("P10CPre_P10B_GreenGroundFrameVerification");
            frameObject.transform.SetParent(refs.Root.transform, false);
            P10BGreenGroundFrameRuntime frameRuntime = frameObject.AddComponent<P10BGreenGroundFrameRuntime>();
            frameRuntime.Configure(frameConfig, targets != null ? targets.targets : Array.Empty<P10BGreenGroundFrameTarget>());
            P10CPreGreenFrameTsunamiBridge bridge = frameObject.AddComponent<P10CPreGreenFrameTsunamiBridge>();
            bridge.Configure(refs.GameManager, frameRuntime);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P10 green frame verification skipped: " + exception.Message);
        }

        try
        {
            P10BPlusNightOverlay overlay = gameObject.GetComponent<P10BPlusNightOverlay>() ??
                gameObject.AddComponent<P10BPlusNightOverlay>();
            overlay.Apply(P10BPlusDataLoader.LoadWeatherConfig().data, P10BPlusWeatherMode.ClearDay);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P10 weather overlay verification skipped: " + exception.Message);
        }
    }

    private void GenerateP6NpcAndNavigation(RuntimeGameplayReferences refs)
    {
        BuildingShelter targetShelter = FindObjectOfType<BuildingShelter>();
        if (targetShelter == null || refs.Player == null)
        {
            return;
        }

        try
        {
            GameObject navigationObject = new GameObject("P10CPre_P6_NavigationGuidanceVerification");
            navigationObject.transform.SetParent(refs.Root.transform, false);
            NavigationGuidanceController navigation = navigationObject.AddComponent<NavigationGuidanceController>();
            navigation.SetPlayerTransform(refs.Player.transform);
            navigation.SetTargetShelter(targetShelter);
            navigation.RefreshGuidance();

            GameObject npcObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObject.name = "P10CPre_P6_NpcEvacuationVerification";
            npcObject.transform.SetParent(refs.Root.transform, true);
            npcObject.transform.position = refs.Origin + new Vector3(-6f, 1f, -4f);
            NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
            agent.ConfigureTargets(new[] { NpcEvacuationTargetInfo.FromBuildingShelter(targetShelter) }, true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("P10-C-Pre P6 NPC/navigation verification skipped: " + exception.Message);
        }
    }

    private void EnsureFallbackSheltersIfNeeded(RuntimeGameplayReferences refs)
    {
        if (FindObjectsOfType<ShelterEntranceTrigger>().Length > 0)
        {
            return;
        }

        ShelterDataLoader.Reload();
        ShelterDataLoader.ShelterData[] shelters = ShelterDataLoader.GetAllShelters();
        if (shelters == null || shelters.Length == 0)
        {
            shelters = new[] { CreateFallbackShelterData() };
        }

        int count = Mathf.Min(3, shelters.Length);
        for (int i = 0; i < count; i++)
        {
            CreateShelter(refs, shelters[i], i);
        }
    }

    private GameObject EnsureGameplayRoot()
    {
        GameObject root = GameObject.Find(GameplayRootName);
        return root != null ? root : new GameObject(GameplayRootName);
    }

    private Vector3 ResolvePlayableOrigin()
    {
        Camera sceneCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (sceneCamera != null)
        {
            Vector3 forward = sceneCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }

            return sceneCamera.transform.position + forward.normalized * 8f + Vector3.down * 2f;
        }

        Renderer renderer = FindObjectOfType<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.center + Vector3.up * 2f;
        }

        return FallbackOrigin + Vector3.up;
    }

    private Camera EnsureCamera(Transform parent, Vector3 origin)
    {
        Camera camera = Camera.main ?? FindObjectOfType<Camera>();
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            TrySetTag(cameraObject, "MainCamera");
            cameraObject.transform.SetParent(parent, true);
            cameraObject.transform.position = origin + new Vector3(0f, 4.5f, -9f);
            cameraObject.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
            camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            cameraObject.AddComponent<AudioListener>();
        }

        if (camera.GetComponent<AudioListener>() == null && FindObjectsOfType<AudioListener>().Length == 0)
        {
            camera.gameObject.AddComponent<AudioListener>();
        }

        return camera;
    }

    private SimplePlayerController EnsurePlayer(Transform parent, Vector3 origin, Camera camera)
    {
        SimplePlayerController existing = FindObjectOfType<SimplePlayerController>();
        if (existing != null)
        {
            return existing;
        }

        GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerObject.name = PlayerObjectName;
        playerObject.transform.SetParent(parent, true);
        playerObject.transform.position = origin;
        TrySetTag(playerObject, "Player");

        Collider collider = playerObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;

        P10BPlusMovementRuntimeAdapter adapter = playerObject.AddComponent<P10BPlusMovementRuntimeAdapter>();
        ConfigureMovementAdapter(adapter);

        SimplePlayerController player = playerObject.AddComponent<SimplePlayerController>();
        Transform pivot = new GameObject("CameraPivot_Runtime").transform;
        pivot.SetParent(playerObject.transform, false);
        pivot.localPosition = Vector3.up * 4.5f;

        if (camera != null)
        {
            camera.transform.SetParent(pivot, false);
        }

        SetPrivateField(player, "cameraPivot", pivot);
        SetPrivateField(player, "cameraTransform", camera != null ? camera.transform : null);
        SetPrivateField(player, "lockCursorOnPlay", false);
        SetPrivateField(player, "fallbackTranslateIfControllerStuck", true);
        SetPrivateField(player, "allowMovementDebugAlways", true);
        return player;
    }

    private Canvas EnsureGameplayCanvas(Transform parent)
    {
        GameObject canvasObject = GameObject.Find("P10CPre_GameplayCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("P10CPre_GameplayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        return canvas;
    }

    private GameUIManager EnsureGameUi(Transform canvasTransform)
    {
        GameUIManager existing = FindObjectOfType<GameUIManager>();
        if (existing != null)
        {
            return existing;
        }

        GameObject uiRoot = CreateFullScreenRect("P10CPre_GameplayUIRoot", canvasTransform);
        GameUIManager manager = uiRoot.AddComponent<GameUIManager>();
        Text countdown = CreateText(uiRoot.transform, "CountdownText", "Waiting", new Vector2(20f, -20f), new Vector2(260f, 40f), TextAnchor.MiddleLeft, 26);
        Text warning = CreateText(uiRoot.transform, "WarningText", string.Empty, new Vector2(20f, -70f), new Vector2(860f, 130f), TextAnchor.UpperLeft, 18);
        Text shelter = CreateText(uiRoot.transform, "ShelterText", string.Empty, new Vector2(20f, -215f), new Vector2(460f, 170f), TextAnchor.UpperLeft, 16);
        Text prompt = CreateText(uiRoot.transform, "InteractionPromptText", string.Empty, new Vector2(20f, -395f), new Vector2(420f, 36f), TextAnchor.MiddleLeft, 22);
        Text climb = CreateText(uiRoot.transform, "ClimbProgressText", string.Empty, new Vector2(20f, -445f), new Vector2(420f, 36f), TextAnchor.MiddleLeft, 20);
        Slider slider = CreateSlider(uiRoot.transform, "ClimbProgressSlider", new Vector2(20f, -490f), new Vector2(420f, 18f));

        SetPrivateField(manager, "countdownText", countdown);
        SetPrivateField(manager, "warningText", warning);
        SetPrivateField(manager, "shelterText", shelter);
        SetPrivateField(manager, "interactionPromptText", prompt);
        SetPrivateField(manager, "climbProgressText", climb);
        SetPrivateField(manager, "climbProgressSlider", slider);
        manager.HideInteractionPrompt();
        manager.HideClimbProgress();
        return manager;
    }

    private ResultPanelController EnsureResultPanel(Transform canvasTransform)
    {
        ResultPanelController existing = FindObjectOfType<ResultPanelController>();
        if (existing != null)
        {
            return existing;
        }

        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasTransform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(840f, 540f);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        ResultPanelController controller = panel.AddComponent<ResultPanelController>();
        Text title = CreateCenteredPanelText(panel.transform, "ResultTitleText", "Result", new Vector2(0f, 220f), new Vector2(780f, 38f), 26);
        Text shelter = CreateCenteredPanelText(panel.transform, "ResultShelterText", "Shelter:", new Vector2(0f, 168f), new Vector2(780f, 54f), 16);
        Text elapsed = CreateCenteredPanelText(panel.transform, "ResultElapsedTimeText", "Elapsed Time:", new Vector2(0f, 126f), new Vector2(780f, 30f), 16);
        Text reason = CreateCenteredPanelText(panel.transform, "ResultReasonText", "Reason:", new Vector2(0f, -70f), new Vector2(780f, 350f), 14);
        reason.alignment = TextAnchor.UpperLeft;

        SetPrivateField(controller, "panelRoot", panel);
        SetPrivateField(controller, "titleText", title);
        SetPrivateField(controller, "shelterText", shelter);
        SetPrivateField(controller, "elapsedTimeText", elapsed);
        SetPrivateField(controller, "reasonText", reason);
        controller.Hide();
        return controller;
    }

    private MovingTsunamiWall EnsureTsunamiWall(Transform parent, Vector3 origin)
    {
        MovingTsunamiWall existing = FindObjectOfType<MovingTsunamiWall>();
        if (existing != null)
        {
            return existing;
        }

        Transform start = new GameObject("TsunamiStart").transform;
        start.SetParent(parent, true);
        start.position = origin + new Vector3(-40f, 2f, 0f);

        Transform end = new GameObject("TsunamiEnd").transform;
        end.SetParent(parent, true);
        end.position = origin + new Vector3(40f, 2f, 0f);

        GameObject wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallObject.name = "TsunamiWall";
        wallObject.transform.SetParent(parent, true);
        wallObject.transform.position = start.position;
        wallObject.transform.localScale = new Vector3(0.3f, 4f, 30f);
        Rigidbody body = wallObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        MovingTsunamiWall wall = wallObject.AddComponent<MovingTsunamiWall>();
        SetPrivateField(wall, "startPoint", start);
        SetPrivateField(wall, "endPoint", end);
        SetPrivateField(wall, "playerTag", "Player");
        SetPrivateField(wall, "durationSeconds", 60f);
        SetPrivateField(wall, "resetToStartOnPlay", true);
        return wall;
    }

    private EvacuationGameManager EnsureGameManager(
        Transform parent,
        SimplePlayerController player,
        MovingTsunamiWall wall,
        GameUIManager ui,
        ResultPanelController resultPanel,
        out TsunamiCountdownManager countdown,
        out ClimbSimulation climb)
    {
        EvacuationGameManager existing = FindObjectOfType<EvacuationGameManager>();
        if (existing != null)
        {
            countdown = existing.GetComponent<TsunamiCountdownManager>() ?? existing.gameObject.AddComponent<TsunamiCountdownManager>();
            climb = existing.GetComponent<ClimbSimulation>() ?? existing.gameObject.AddComponent<ClimbSimulation>();
            return existing;
        }

        GameObject managerObject = new GameObject("GameManager");
        managerObject.transform.SetParent(parent, false);
        EvacuationGameManager manager = managerObject.AddComponent<EvacuationGameManager>();
        countdown = managerObject.AddComponent<TsunamiCountdownManager>();
        climb = managerObject.AddComponent<ClimbSimulation>();

        SetPrivateField(manager, "playerController", player);
        SetPrivateField(manager, "countdownManager", countdown);
        SetPrivateField(manager, "tsunamiWall", wall);
        SetPrivateField(manager, "climbSimulation", climb);
        SetPrivateField(manager, "gameUIManager", ui);
        SetPrivateField(manager, "resultPanelController", resultPanel);
        SetPrivateField(manager, "startOnAwake", false);
        SetPrivateField(manager, "eventStartKey", KeyCode.T);
        SetPrivateField(manager, "preEventMessage", "Move with WASD or arrow keys. Press T to start the tsunami warning. Press E near a shelter entrance.");
        SetPrivateField(manager, "startMessage", "Tsunami warning issued. Find a usable evacuation building.");

        SetPrivateField(countdown, "gameManager", manager);
        SetPrivateField(countdown, "gameUIManager", ui);
        SetPrivateField(wall, "gameManager", manager);
        return manager;
    }

    private void EnsureFallbackGroundIfNeeded(Vector3 origin)
    {
        if (!config.createFallbackGroundWhenNoSceneCollider || GameObject.Find(TestGroundName) != null)
        {
            return;
        }

        if (Physics.Raycast(origin + Vector3.up * 2f, Vector3.down, 8f))
        {
            return;
        }

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = TestGroundName;
        ground.transform.position = new Vector3(origin.x, origin.y - 1f, origin.z);
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        Debug.LogWarning("P10-C-Pre startup created fallback ground because no scene collider was detected under the player start.");
    }

    private void AttachWeatherAndStamina(SimplePlayerController player)
    {
        if (player == null)
        {
            return;
        }

        P10BPlusMovementRuntimeAdapter adapter = player.GetComponent<P10BPlusMovementRuntimeAdapter>() ??
            player.gameObject.AddComponent<P10BPlusMovementRuntimeAdapter>();
        ConfigureMovementAdapter(adapter);
    }

    private void ConfigureMovementAdapter(P10BPlusMovementRuntimeAdapter adapter)
    {
        if (adapter == null)
        {
            return;
        }

        adapter.Configure(
            P10BPlusDataLoader.LoadMovementStaminaConfig().data,
            P10BPlusDataLoader.LoadWeatherConfig().data,
            P10BPlusDataLoader.LoadAvatarMobilityConfig().data,
            P10BPlusWeatherMode.ClearDay,
            P10BPlusAvatarPresentation.Male,
            "standard");
    }

    private void CreateShelter(RuntimeGameplayReferences refs, ShelterDataLoader.ShelterData shelterData, int index)
    {
        shelterData = shelterData ?? CreateFallbackShelterData();
        Vector3 offset = new Vector3(8f + index * 7f, 0f, 8f);
        Vector3 worldPosition = refs.Origin + offset;
        string safeId = MakeSafeObjectName(shelterData.shelterId);

        GameObject shelterObject = new GameObject("P10CPre_FallbackShelter_" + safeId);
        shelterObject.transform.SetParent(refs.Root.transform, true);
        shelterObject.transform.position = worldPosition;
        BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(shelterData);

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "P10CPre_FallbackShelterMarker_" + safeId;
        marker.transform.SetParent(shelterObject.transform, true);
        marker.transform.position = worldPosition + new Vector3(0f, 0.25f, 0f);
        marker.transform.localScale = new Vector3(3f, 0.5f, 3f);
        Destroy(marker.GetComponent<Collider>());

        GameObject entranceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entranceObject.name = "P10CPre_FallbackShelterEntrance_" + safeId;
        entranceObject.transform.SetParent(shelterObject.transform, true);
        entranceObject.transform.position = worldPosition + new Vector3(0f, 1f, 0f);
        entranceObject.transform.localScale = new Vector3(2f, 2f, 2f);
        Collider entranceCollider = entranceObject.GetComponent<Collider>();
        if (entranceCollider != null)
        {
            entranceCollider.isTrigger = true;
        }

        ShelterEntranceTrigger entrance = entranceObject.AddComponent<ShelterEntranceTrigger>();
        SetPrivateField(entrance, "shelter", shelter);
        SetPrivateField(entrance, "gameManager", refs.GameManager);
        SetPrivateField(entrance, "gameUIManager", refs.GameUiManager);
        SetPrivateField(entrance, "playerTag", "Player");
        SetPrivateField(entrance, "interactKey", KeyCode.E);
    }

    private void EnsureStartupCanvas()
    {
        if (canvasObject != null)
        {
            return;
        }

        canvasObject = GameObject.Find(StartupCanvasName);
        if (canvasObject == null)
        {
            canvasObject = new GameObject(StartupCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        DontDestroyOnLoad(canvasObject);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
    }

    private void BuildStartPanel()
    {
        if (startPanel != null)
        {
            Destroy(startPanel);
        }

        startPanel = CreatePanel("P10CPre_StartPanel", new Color(0.02f, 0.025f, 0.03f, 0.94f));
        AddLocalizedText(startPanel.transform, "Title", "game.title", new Vector2(0f, 230f), new Vector2(900f, 56f), 30);
        CreateButton(startPanel.transform, "StartButton", "menu.start", new Vector2(0f, 132f), StartGame);
        CreateButton(startPanel.transform, "RulesButton", "menu.rules", new Vector2(0f, 72f), ShowRules);
        CreateButton(startPanel.transform, "EnglishButton", "English", new Vector2(-105f, 10f), SetLanguageEnglish, false);
        CreateButton(startPanel.transform, "JapaneseButton", "Japanese", new Vector2(105f, 10f), SetLanguageJapanese, false);
        AddText(startPanel.transform, "LanguageLabel", "Language", new Vector2(0f, -45f), new Vector2(360f, 32f), 15, TextAnchor.MiddleCenter);
        diagnosticText = AddText(startPanel.transform, "Diagnostics", string.Empty, new Vector2(0f, -235f), new Vector2(980f, 120f), 13, TextAnchor.UpperLeft);
    }

    private void BuildRulesPanel()
    {
        if (rulesPanel != null)
        {
            Destroy(rulesPanel);
        }

        rulesPanel = CreatePanel("P10CPre_RulesPanel", new Color(0.02f, 0.025f, 0.03f, 0.96f));
        AddLocalizedText(rulesPanel.transform, "RulesTitle", "rules.title", new Vector2(0f, 250f), new Vector2(900f, 42f), 24);

        GameObject scrollObject = new GameObject("RulesScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(rulesPanel.transform, false);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchoredPosition = new Vector2(0f, -12f);
        scrollRectTransform.sizeDelta = new Vector2(960f, 460f);
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(14f, 14f);
        viewport.offsetMax = new Vector2(-14f, -14f);
        viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = new Vector2(0f, 900f);

        Text body = AddLocalizedText(contentObject.transform, "RulesBody", "rules.body", new Vector2(0f, -430f), new Vector2(900f, 840f), 15);
        body.alignment = TextAnchor.UpperLeft;
        body.verticalOverflow = VerticalWrapMode.Overflow;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        CreateButton(rulesPanel.transform, "CloseRulesButton", "rules.close", new Vector2(0f, -285f), HideRules);
        rulesPanel.SetActive(false);
    }

    private void BuildPausePanel()
    {
        if (pausePanel != null)
        {
            Destroy(pausePanel);
        }

        pausePanel = CreatePanel("P10CPre_PausePanel", new Color(0.02f, 0.025f, 0.03f, 0.9f));
        AddLocalizedText(pausePanel.transform, "PauseTitle", "pause.title", new Vector2(0f, 190f), new Vector2(820f, 48f), 26);
        CreateButton(pausePanel.transform, "ResumeButton", "pause.resume", new Vector2(0f, 110f), TogglePause);
        CreateButton(pausePanel.transform, "PauseRulesButton", "menu.rules", new Vector2(0f, 50f), ShowRules);
        CreateButton(pausePanel.transform, "PauseEnglishButton", "English", new Vector2(-105f, -12f), SetLanguageEnglish, false);
        CreateButton(pausePanel.transform, "PauseJapaneseButton", "Japanese", new Vector2(105f, -12f), SetLanguageJapanese, false);
        pausePanel.SetActive(false);
    }

    private GameObject CreatePanel(string name, Color color)
    {
        GameObject panel = CreateFullScreenRect(name, canvasObject.transform);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Button CreateButton(Transform parent, string name, string labelOrKey, Vector2 position, UnityEngine.Events.UnityAction action, bool localized = true)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(260f, 44f);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.24f, 0.94f);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        Text label = AddText(buttonObject.transform, "Label", localized ? string.Empty : labelOrKey, Vector2.zero, rect.sizeDelta, 17, TextAnchor.MiddleCenter);
        if (localized)
        {
            localizedTexts.Add(new LocalizedTextBinding(label, labelOrKey));
        }

        return button;
    }

    private Text AddLocalizedText(Transform parent, string name, string key, Vector2 position, Vector2 size, int fontSize)
    {
        Text text = AddText(parent, name, string.Empty, position, size, fontSize, TextAnchor.MiddleCenter);
        localizedTexts.Add(new LocalizedTextBinding(text, key));
        return text;
    }

    private Text AddText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = GetBuiltInFont();
        text.text = value ?? string.Empty;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize;
        return text;
    }

    private Text CreateCenteredPanelText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
    {
        Text text = AddText(parent, name, value, position, size, fontSize, TextAnchor.MiddleCenter);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        return text;
    }

    private Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, TextAnchor anchor, int fontSize)
    {
        return AddText(parent, name, value, position, size, fontSize, anchor);
    }

    private Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        sliderObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(sliderObject.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        fill.GetComponent<Image>().color = new Color(0.15f, 0.75f, 1f, 0.9f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        slider.fillRect = fillRect;
        return slider;
    }

    private GameObject CreateFullScreenRect(string name, Transform parent)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        root.transform.SetParent(parent, false);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return root;
    }

    private void TogglePause()
    {
        bool show = pausePanel != null && !pausePanel.activeSelf;
        SetActive(pausePanel, show);
        Time.timeScale = show ? 0f : 1f;
    }

    private void RefreshLocalizedTexts()
    {
        for (int i = 0; i < localizedTexts.Count; i++)
        {
            localizedTexts[i].Refresh(localizationService);
        }
    }

    private void ShowDiagnostic(string message, bool warning)
    {
        if (diagnosticText == null)
        {
            return;
        }

        diagnosticText.text = message ?? string.Empty;
        diagnosticText.color = warning ? new Color(1f, 0.86f, 0.35f, 1f) : new Color(0.8f, 1f, 0.86f, 1f);
    }

    private P10CPreStartupDiagnosticsSnapshot CaptureAndLogDiagnostics(string phase)
    {
        LastDiagnostics = CaptureDiagnostics();
        Debug.Log("[P10CPreStartupDiagnostics] phase=" + phase + " " + LastDiagnostics.summary);
        Debug.Log("[P10CPreStartupDiagnostics] scene=" + LastDiagnostics.activeScenePath +
                  ", target=" + LastDiagnostics.targetHighDetailScenePath +
                  ", targetActive=" + LastDiagnostics.targetHighDetailSceneActive +
                  ", canLoad=" + LastDiagnostics.targetHighDetailSceneCanLoad +
                  ", preBootstrapRenderers=" + LastDiagnostics.preBootstrapSceneRendererCount +
                  ", placeholderLikely=" + LastDiagnostics.p9TargetSceneLooksLikePlaceholder);
        Debug.Log("[P10CPreStartupDiagnostics] dataRoot=" + RuntimeDataPathResolver.GetDataRoot() +
                  ", P8=" + LastDiagnostics.p8DataExists +
                  ", P9=" + LastDiagnostics.p9DataExists +
                  ", P10=" + LastDiagnostics.p10DataExists +
                  ", localizationLoaded=" + LastDiagnostics.localizationLoaded);
        Debug.Log("[P10CPreStartupDiagnostics] counts cameras=" + LastDiagnostics.cameraCount +
                  ", players=" + LastDiagnostics.playerCount +
                  ", canvases=" + LastDiagnostics.canvasCount +
                  ", eventSystems=" + LastDiagnostics.eventSystemCount +
                  ", shelters=" + LastDiagnostics.shelterCount +
                  ", entrances=" + LastDiagnostics.shelterEntranceCount +
                  ", resultPanels=" + LastDiagnostics.resultPanelCount);
        Debug.Log("[P10CPreStartupDiagnostics] integration p4RealShelters=" + LastDiagnostics.p4RealShelterMarkerCount +
                  ", p5Qualified=" + LastDiagnostics.p5RealQualifiedShelterCount +
                  ", p5Candidates=" + LastDiagnostics.p5HumanitarianCandidateCount +
                  ", p6Npc=" + LastDiagnostics.p6NpcCount +
                  ", p8RiskFrontLoaded=" + LastDiagnostics.p8RiskFrontLoaded +
                  ", p9Crowd=" + LastDiagnostics.p9CrowdAgentCount +
                  ", p9SpawnMarkers=" + LastDiagnostics.p9SpawnMarkerCount +
                  ", p9EntranceSafeFloorMarkers=" + LastDiagnostics.p9EntranceSafeFloorMarkerCount +
                  ", p9CollapseDebrisZones=" + LastDiagnostics.p9CollapseDebrisZoneCount +
                  ", p10GreenFrames=" + LastDiagnostics.p10GreenFrameCount);
        return LastDiagnostics;
    }

    public P10CPreStartupDiagnosticsSnapshot CaptureDiagnostics()
    {
        Scene scene = SceneManager.GetActiveScene();
        var limitations = new List<string>();
        var snapshot = new P10CPreStartupDiagnosticsSnapshot
        {
            buildMode = config.playableStartupMode ? "playable_startup" : "non_playable_config",
            activeSceneName = scene.name ?? string.Empty,
            activeScenePath = scene.path ?? string.Empty,
            targetHighDetailScenePath = config.targetHighDetailScenePath ?? string.Empty,
            targetHighDetailSceneActive = IsTargetSceneActive(),
            targetHighDetailSceneCanLoad = CanLoadTargetScene(),
            preBootstrapSceneRendererCount = preBootstrapSceneRendererCount,
            dataRootExists = System.IO.Directory.Exists(RuntimeDataPathResolver.GetDataRoot()),
            p8DataExists = RuntimeDataPathResolver.DataFolderExists("P8"),
            p9DataExists = RuntimeDataPathResolver.DataFolderExists("P9"),
            p10DataExists = RuntimeDataPathResolver.DataFolderExists("P10"),
            localizationLoaded = P10BPlusDataLoader.LoadEnglishLocalization().success && P10BPlusDataLoader.LoadJapaneseLocalization().success,
            startupUiBuilt = canvasObject != null && startPanel != null && rulesPanel != null,
            cameraCount = FindObjectsOfType<Camera>().Length,
            playerCount = FindObjectsOfType<SimplePlayerController>().Length,
            eventSystemCount = FindObjectsOfType<EventSystem>().Length,
            canvasCount = FindObjectsOfType<Canvas>().Length,
            gameManagerCount = FindObjectsOfType<EvacuationGameManager>().Length,
            resultPanelCount = CountLoadedSceneComponents<ResultPanelController>(),
            shelterCount = FindObjectsOfType<BuildingShelter>().Length,
            shelterEntranceCount = FindObjectsOfType<ShelterEntranceTrigger>().Length,
            p4RealShelterMarkerCount = CountActiveObjectsByName("RealShelter_"),
            p5QualificationMarkerCount = CountActiveObjectsByName("P5C_QualificationMarker_"),
            p5RealQualifiedShelterCount = CountActiveObjectsByName("P5D_RealQualifiedShelter_"),
            p5HumanitarianCandidateCount = CountActiveObjectsByName("P5GH_HumanitarianCandidate_"),
            p6NpcCount = FindObjectsOfType<NpcEvacuationAgent>().Length,
            p6NavigationControllerCount = FindObjectsOfType<NavigationGuidanceController>().Length,
            p9CrowdAgentCount = FindObjectsOfType<P9CrowdRuntimeAgent>().Length,
            p9SpawnMarkerCount = CountActiveObjectsByName("P9B_SpawnMarker_"),
            p9EntranceSafeFloorMarkerCount = FindObjectsOfType<P9EntranceSafeFloorMarkerRuntime>().Length,
            p9CollapseDebrisZoneCount = FindObjectsOfType<P9CollapseDebrisRiskZone>().Length,
            p10GreenFrameRuntimeCount = FindObjectsOfType<P10BGreenGroundFrameRuntime>().Length,
            p10PauseRulesUiAvailable = pausePanel != null && rulesPanel != null,
            p10WeatherStaminaAttached = FindObjectOfType<P10BPlusMovementRuntimeAdapter>() != null,
            profilingExporterEnabledByDefault = config.enableProfilingExporterByDefault,
            profilingAutoQuitEnabled = config.enableProfilingAutoQuit
        };

        P8RiskFrontController riskFront = FindObjectOfType<P8RiskFrontController>();
        snapshot.p8RiskFrontLoaded = riskFront != null && !string.IsNullOrWhiteSpace(riskFront.LastStatus);
        snapshot.p8RiskFrontVisible = riskFront != null && riskFront.IsVisualVisible;
        snapshot.p8HandoffConsumed = P9BDataLoader.VerifyP8HandoffAvailability().allExpectedFilesPresent;

        try
        {
            P9DFinalGameplayFlowSummary flowSummary = P9DFinalGameplayFlowValidator.Validate(
                P9DDataLoader.LoadFinalGameplayScenarioSample().data,
                P9DDataLoader.LoadAnchoringReportSample().data);
            snapshot.p9FinalOutcomeValidated = flowSummary != null && flowSummary.success;
        }
        catch
        {
            snapshot.p9FinalOutcomeValidated = false;
        }

        P10BGreenGroundFrameRuntime frameRuntime = FindObjectOfType<P10BGreenGroundFrameRuntime>();
        snapshot.p10GreenFrameCount = frameRuntime != null ? frameRuntime.LastMetrics.activeFrameCount : 0;

        snapshot.p9TargetSceneLooksLikePlaceholder =
            snapshot.targetHighDetailSceneActive &&
            snapshot.preBootstrapSceneRendererCount < Math.Max(1, config.minimumRenderableSceneRendererCount);

        if (!snapshot.targetHighDetailSceneActive)
        {
            limitations.Add("Target P7_HighDetail_Chuo scene is not the active scene.");
        }
        if (snapshot.p9TargetSceneLooksLikePlaceholder)
        {
            limitations.Add("P9 target scene looks like the tracked placeholder/status shell, not the 22GB P7 source high-detail scene.");
        }
        if (snapshot.cameraCount == 0)
        {
            limitations.Add("No active camera was found after startup.");
        }
        if (snapshot.playerCount == 0)
        {
            limitations.Add("No SimplePlayerController was found after startup.");
        }
        if (snapshot.eventSystemCount == 0 || snapshot.canvasCount == 0)
        {
            limitations.Add("UI Canvas/EventSystem bootstrap is missing.");
        }
        if (snapshot.shelterEntranceCount == 0)
        {
            limitations.Add("No shelter entrance is reachable for E interaction.");
        }
        if (snapshot.resultPanelCount == 0)
        {
            limitations.Add("P2 ResultPanel controller is missing.");
        }
        if (!snapshot.p8RiskFrontLoaded)
        {
            limitations.Add("P8 risk-front handoff did not load.");
        }
        if (snapshot.p9SpawnMarkerCount == 0)
        {
            limitations.Add("P9 spawn markers were not generated.");
        }
        if (snapshot.p9CrowdAgentCount == 0)
        {
            limitations.Add("P9 lightweight crowd prototype agents were not spawned.");
        }
        if (snapshot.p9EntranceSafeFloorMarkerCount == 0)
        {
            limitations.Add("P9 entrance/safe-floor markers were not generated.");
        }
        if (snapshot.p9CollapseDebrisZoneCount == 0)
        {
            limitations.Add("P9 collapse/debris risk zones were not generated.");
        }
        if (!snapshot.p9FinalOutcomeValidated)
        {
            limitations.Add("P9 final outcome validation did not pass in startup diagnostics.");
        }
        if (snapshot.p10GreenFrameRuntimeCount == 0)
        {
            limitations.Add("P10 green ground frame runtime is missing.");
        }
        if (HasCommandLineArgument(AutoStartGameArg) && snapshot.p10GreenFrameCount == 0)
        {
            limitations.Add("P10 green ground frames did not activate after verification tsunami start.");
        }
        if (snapshot.profilingAutoQuitEnabled)
        {
            limitations.Add("Profiling auto-quit is enabled; default playable builds must keep it disabled.");
        }

        snapshot.limitations = limitations.ToArray();
        snapshot.summary = "P10-C-Pre playable startup diagnostics: scene=" + snapshot.activeSceneName +
                           ", UI=" + snapshot.startupUiBuilt +
                           ", player=" + snapshot.playerCount +
                           ", camera=" + snapshot.cameraCount +
                           ", P2-P10 limitations=" + snapshot.limitations.Length + ".";
        return snapshot;
    }

    private bool IsTargetSceneActive()
    {
        Scene scene = SceneManager.GetActiveScene();
        return string.Equals(scene.path, config.targetHighDetailScenePath, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scene.name, config.targetHighDetailSceneName, StringComparison.OrdinalIgnoreCase);
    }

    private bool CanLoadTargetScene()
    {
        if (string.IsNullOrWhiteSpace(config.targetHighDetailScenePath))
        {
            return false;
        }

        return Application.CanStreamedLevelBeLoaded(config.targetHighDetailScenePath) ||
            Application.CanStreamedLevelBeLoaded(config.targetHighDetailSceneName);
    }

    private static P10CPrePlayableStartupConfig LoadConfig()
    {
        P9BLoadResult<P10CPrePlayableStartupConfig> result = P10CPreDataLoader.LoadPlayableStartupConfig();
        return result != null && result.data != null ? result.data : new P10CPrePlayableStartupConfig();
    }

    private P10BPlusLanguage GetDefaultLanguage()
    {
        P10BPlusUiConfig uiConfig = P10BPlusDataLoader.LoadUiConfig().data;
        return uiConfig != null && string.Equals(uiConfig.defaultLanguage, "ja", StringComparison.OrdinalIgnoreCase)
            ? P10BPlusLanguage.Japanese
            : P10BPlusLanguage.English;
    }

    private static int CountExistingSceneRenderers()
    {
        int count = 0;
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].gameObject.scene.IsValid())
            {
                count++;
            }
        }

        return count;
    }

    private static int CountActiveObjectsByName(string nameFragment)
    {
        int count = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject candidate = objects[i];
            if (candidate != null &&
                candidate.scene.IsValid() &&
                candidate.activeInHierarchy &&
                candidate.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountLoadedSceneComponents<T>() where T : Component
    {
        int count = 0;
        T[] components = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];
            if (component != null && component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded)
            {
                count++;
            }
        }

        return count;
    }

    private static ShelterDataLoader.ShelterData CreateFallbackShelterData()
    {
        return new ShelterDataLoader.ShelterData
        {
            shelterId = "p10c_pre_fallback_shelter",
            shelterName = "P10-C-Pre Fallback Shelter",
            shelterRank = "B",
            isOfficialShelter = true,
            canEnter = true,
            climbTimeSeconds = 10f,
            sourceType = "test",
            facilityType = "runtime_fallback_shelter",
            coordinateSystem = "runtime_fallback_layout",
            layoutPosition = new ShelterDataLoader.LayoutPosition { x = 8f, y = 0f, z = 8f },
            notes = "Generated only if no P7 startup shelter entrances are reachable."
        };
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static Font GetBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, PrivateInstanceFlags);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static void TrySetTag(GameObject target, string tag)
    {
        if (target == null || string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        try
        {
            target.tag = tag;
        }
        catch (UnityException)
        {
            Debug.LogWarning("P10-C-Pre startup could not assign tag '" + tag + "' because it is not defined.");
        }
    }

    private static string MakeSafeObjectName(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "unnamed"
            : value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
    }

    private static bool HasCommandLineArgument(string argumentName)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], argumentName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class RuntimeGameplayReferences
    {
        public GameObject Root;
        public Vector3 Origin;
        public Camera Camera;
        public SimplePlayerController Player;
        public Canvas GameplayCanvas;
        public GameUIManager GameUiManager;
        public ResultPanelController ResultPanelController;
        public EvacuationGameManager GameManager;
        public TsunamiCountdownManager CountdownManager;
        public MovingTsunamiWall TsunamiWall;
        public ClimbSimulation ClimbSimulation;
    }

    private sealed class LocalizedTextBinding
    {
        private readonly Text text;
        private readonly string key;

        public LocalizedTextBinding(Text targetText, string localizationKey)
        {
            text = targetText;
            key = localizationKey ?? string.Empty;
        }

        public void Refresh(P10BPlusLocalizationService service)
        {
            if (text != null && service != null)
            {
                text.text = service.Translate(key);
            }
        }
    }
}

public class P10CPreGreenFrameTsunamiBridge : MonoBehaviour
{
    private EvacuationGameManager gameManager;
    private P10BGreenGroundFrameRuntime frameRuntime;
    private bool triggered;

    public void Configure(EvacuationGameManager manager, P10BGreenGroundFrameRuntime runtime)
    {
        gameManager = manager;
        frameRuntime = runtime;
    }

    private void Update()
    {
        if (triggered || gameManager == null || frameRuntime == null)
        {
            return;
        }

        if (gameManager.CurrentState == EvacuationGameManager.GameState.Playing ||
            gameManager.CurrentState == EvacuationGameManager.GameState.Climbing)
        {
            triggered = true;
            frameRuntime.SetTsunamiStarted(true);
        }
    }
}
