using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditorInternal;

public static class FirstPlayableSceneBuilder
{
    private const string MenuPath = "Tools/Chuo Evacuation/Build First Playable Test Setup";
    private const string RootName = "GameplayTestRoot";
    private static readonly Vector3 TestPlatformOrigin = new Vector3(0f, 80f, -250f);

    [MenuItem(MenuPath)]
    public static void BuildFirstPlayableTestSetup()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogWarning("First playable setup cannot run during Play Mode. Exit Play Mode, then run Tools > Chuo Evacuation > Build First Playable Test Setup.");
            return;
        }
#endif

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            Debug.LogError("First playable setup failed: no valid active scene is open.");
            return;
        }

        DeleteAllowedExistingSetup(activeScene);

        GameObject root = new GameObject(RootName);
        Material groundMaterial = CreateMaterial("Test Ground Material", new Color(0.45f, 0.45f, 0.42f, 1f), false);
        Material playerMaterial = CreateMaterial("Player Test Material", new Color(1f, 0.9f, 0.05f, 1f), false);
        Material tsunamiMaterial = CreateMaterial("Tsunami Wall Test Material", new Color(0f, 0.75f, 1f, 0.38f), true);
        Material riskMaterial = CreateMaterial("Risk Zone Test Material", new Color(1f, 0.05f, 0.02f, 0.28f), true);
        Material entranceMaterial = CreateMaterial("Shelter Entrance Test Material", new Color(0.1f, 1f, 0.25f, 0.85f), false);
        Material officialShelterMaterial = CreateMaterial("Official Shelter Marker Material", new Color(0.05f, 0.65f, 1f, 0.9f), false);
        Material candidateShelterMaterial = CreateMaterial("Candidate Shelter Marker Material", new Color(1f, 0.78f, 0.15f, 0.9f), false);
        Material blockedShelterMaterial = CreateMaterial("Blocked Shelter Marker Material", new Color(1f, 0.18f, 0.12f, 0.9f), false);

        GameObject ground = CreatePrimitive("TestGround", PrimitiveType.Plane, root.transform, TestPosition(new Vector3(0f, 0f, 0f)), new Vector3(60f, 1f, 60f));
        SetMaterial(ground, groundMaterial);
        GameObject player = CreatePlayer(root.transform, playerMaterial);

        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.transform.SetParent(root.transform);
        EvacuationGameManager gameManager = gameManagerObject.AddComponent<EvacuationGameManager>();
        TsunamiCountdownManager countdownManager = gameManagerObject.AddComponent<TsunamiCountdownManager>();
        ClimbSimulation climbSimulation = gameManagerObject.AddComponent<ClimbSimulation>();

        GameObject tsunamiStart = CreateEmpty("TsunamiStart", root.transform, TestPosition(new Vector3(-40f, 2f, 0f)));
        GameObject tsunamiEnd = CreateEmpty("TsunamiEnd", root.transform, TestPosition(new Vector3(40f, 2f, 0f)));
        GameObject tsunamiWallObject = CreatePrimitive("TsunamiWall", PrimitiveType.Cube, root.transform, TestPosition(new Vector3(-40f, 2f, 0f)), new Vector3(0.3f, 4f, 30f));
        SetMaterial(tsunamiWallObject, tsunamiMaterial);
        SetColliderTrigger(tsunamiWallObject, true);
        Rigidbody tsunamiWallRigidbody = tsunamiWallObject.AddComponent<Rigidbody>();
        tsunamiWallRigidbody.isKinematic = true;
        tsunamiWallRigidbody.useGravity = false;
        MovingTsunamiWall tsunamiWall = tsunamiWallObject.AddComponent<MovingTsunamiWall>();

        GameObject riskZoneObject = CreatePrimitive("RiskZone", PrimitiveType.Cube, root.transform, TestPosition(new Vector3(0f, 1f, 15f)), new Vector3(10f, 2f, 5f));
        SetMaterial(riskZoneObject, riskMaterial);
        SetColliderTrigger(riskZoneObject, true);
        RiskZone riskZone = riskZoneObject.AddComponent<RiskZone>();

        List<GeneratedShelter> generatedShelters = CreateShelterField(
            root.transform,
            officialShelterMaterial,
            candidateShelterMaterial,
            blockedShelterMaterial,
            entranceMaterial);

        Canvas canvas = CreateCanvas();
        CreateEventSystem();
        GameObject uiRoot = CreateUIRoot(canvas.transform);
        GameUIManager gameUIManager = uiRoot.AddComponent<GameUIManager>();
        ResultPanelController resultPanelController = uiRoot.AddComponent<ResultPanelController>();

        Text countdownText = CreateText("CountdownText", uiRoot.transform, "Waiting", new Vector2(20f, -20f), new Vector2(260f, 40f), TextAnchor.MiddleLeft, 26);
        Text warningText = CreateText("WarningText", uiRoot.transform, GetInstructionText(), new Vector2(20f, -70f), new Vector2(860f, 130f), TextAnchor.UpperLeft, 19);
        Text shelterText = CreateText("ShelterText", uiRoot.transform, string.Empty, new Vector2(20f, -215f), new Vector2(460f, 170f), TextAnchor.UpperLeft, 16);
        Text interactionPromptText = CreateText("InteractionPromptText", uiRoot.transform, string.Empty, new Vector2(20f, -395f), new Vector2(420f, 36f), TextAnchor.MiddleLeft, 22);
        Text climbProgressText = CreateText("ClimbProgressText", uiRoot.transform, string.Empty, new Vector2(20f, -445f), new Vector2(420f, 36f), TextAnchor.MiddleLeft, 20);
        Slider climbProgressSlider = CreateSlider("ClimbProgressSlider", uiRoot.transform, new Vector2(20f, -490f), new Vector2(420f, 18f));

        GameObject resultPanel = CreateResultPanel(canvas.transform);
        Text resultTitleText = CreatePanelText("ResultTitleText", resultPanel.transform, "Result", new Vector2(0f, 220f), new Vector2(780f, 38f), TextAnchor.MiddleCenter, 26);
        Text resultShelterText = CreatePanelText("ResultShelterText", resultPanel.transform, "Shelter:", new Vector2(0f, 168f), new Vector2(780f, 54f), TextAnchor.MiddleLeft, 16);
        Text resultElapsedTimeText = CreatePanelText("ResultElapsedTimeText", resultPanel.transform, "Elapsed Time:", new Vector2(0f, 126f), new Vector2(780f, 30f), TextAnchor.MiddleLeft, 16);
        Text resultReasonText = CreatePanelText("ResultReasonText", resultPanel.transform, "Reason:", new Vector2(0f, -70f), new Vector2(780f, 350f), TextAnchor.UpperLeft, 14);
        resultPanel.SetActive(false);

        AssignPlayer(player, player.transform.Find("CameraPivot"), player.GetComponentInChildren<Camera>().transform);
        AssignGameManager(gameManager, player.GetComponent<SimplePlayerController>(), countdownManager, tsunamiWall, climbSimulation, gameUIManager, resultPanelController);
        AssignCountdown(countdownManager, gameManager, gameUIManager);
        AssignTsunamiWall(tsunamiWall, tsunamiStart.transform, tsunamiEnd.transform, gameManager);
        AssignRiskZone(riskZone, gameManager);
        foreach (GeneratedShelter generatedShelter in generatedShelters)
        {
            AssignShelterEntrance(generatedShelter.Entrance, generatedShelter.Shelter, gameManager, gameUIManager);
        }

        AssignGameUI(gameUIManager, countdownText, warningText, shelterText, interactionPromptText, climbProgressText, climbProgressSlider);
        AssignResultPanel(resultPanelController, resultPanel, resultTitleText, resultShelterText, resultElapsedTimeText, resultReasonText);

        Selection.activeGameObject = root;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
#endif

        Debug.Log(
            "Built first playable test setup. Created GameplayTestRoot, TestGround, third-person Player with CameraPivot and Main Camera, GameManager, TsunamiWall, TsunamiStart, TsunamiEnd, RiskZone, multi-shelter field, Canvas, UIRoot, ResultPanel, and EventSystem. " +
            $"Gameplay test objects are on an isolated elevated platform at {TestPlatformOrigin}; PLATEAU geometry is background only. Player local start is (0, 1, 0), generated shelter count is {generatedShelters.Count}, risk zone is off to the side, and tsunami wall waits at local x=-40 until T is pressed. " +
            BuildShelterSummary(generatedShelters) + " " +
            "Existing scene objects named GameplayTestRoot, Canvas, and EventSystem were replaced; PLATEAU objects and assets were not modified.");
    }

    private static void DeleteAllowedExistingSetup(Scene scene)
    {
        var objectsToDelete = new List<GameObject>();
        var seen = new HashSet<GameObject>();
        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject sceneObject in sceneObjects)
        {
            if (sceneObject == null || sceneObject.scene != scene)
            {
                continue;
            }

            bool isAllowedRoot =
                sceneObject.name == RootName ||
                (sceneObject.transform.parent == null && (sceneObject.name == "Canvas" || sceneObject.name == "EventSystem"));

            if (isAllowedRoot && seen.Add(sceneObject))
            {
                objectsToDelete.Add(sceneObject);
            }
        }

        foreach (GameObject sceneObject in objectsToDelete)
        {
            UnityEngine.Object.DestroyImmediate(sceneObject);
        }
    }

    private static GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        return gameObject;
    }

    private static GameObject CreateEmpty(string name, Transform parent, Vector3 position)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        return gameObject;
    }

    private static List<GeneratedShelter> CreateShelterField(
        Transform parent,
        Material officialShelterMaterial,
        Material candidateShelterMaterial,
        Material blockedShelterMaterial,
        Material entranceMaterial)
    {
        ShelterDataLoader.ClearRuntimeOverrides();
        ShelterDataLoader.Reload();
        ShelterDataLoader.ShelterData[] shelterDataList = ShelterDataLoader.GetAllShelters();

        if (shelterDataList.Length == 0)
        {
            Debug.LogWarning("No test shelter data was loaded. Creating one fallback debug shelter.");
            shelterDataList = new[] { CreateFallbackShelterData() };
        }

        var generatedShelters = new List<GeneratedShelter>();

        foreach (ShelterDataLoader.ShelterData shelterData in shelterDataList)
        {
            if (shelterData == null)
            {
                continue;
            }

            Vector3 localPosition = shelterData.layoutPosition != null
                ? shelterData.layoutPosition.ToVector3()
                : Vector3.zero;

            string safeId = MakeSafeObjectName(shelterData.shelterId);
            GameObject shelterObject = CreateEmpty($"TestShelter_{safeId}", parent, TestPosition(localPosition));
            BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
            AssignShelter(shelter, shelterData);

            GameObject markerObject = CreatePrimitive(
                $"ShelterMarker_{safeId}",
                PrimitiveType.Cube,
                shelterObject.transform,
                TestPosition(localPosition + new Vector3(0f, 0.25f, 0f)),
                new Vector3(3f, 0.5f, 3f));
            SetMaterial(markerObject, GetShelterMarkerMaterial(shelterData, officialShelterMaterial, candidateShelterMaterial, blockedShelterMaterial));
            RemoveCollider(markerObject);

            GameObject entranceObject = CreatePrimitive(
                $"ShelterEntrance_{safeId}",
                PrimitiveType.Cube,
                shelterObject.transform,
                TestPosition(localPosition + new Vector3(0f, 1f, 0f)),
                new Vector3(2f, 2f, 2f));
            SetMaterial(entranceObject, entranceMaterial);
            SetColliderTrigger(entranceObject, true);
            ShelterEntranceTrigger shelterEntrance = entranceObject.AddComponent<ShelterEntranceTrigger>();

            generatedShelters.Add(new GeneratedShelter
            {
                Shelter = shelter,
                Entrance = shelterEntrance,
                Data = shelterData.Clone(),
                LocalPosition = localPosition
            });
        }

        return generatedShelters;
    }

    private static GameObject CreatePlayer(Transform parent, Material playerMaterial)
    {
        GameObject player = CreatePrimitive("Player", PrimitiveType.Capsule, parent, TestPosition(new Vector3(0f, 1f, 0f)), Vector3.one);
        player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        AssignTagIfExists(player, "Player");
        SetMaterial(player, playerMaterial);

        CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            UnityEngine.Object.DestroyImmediate(capsuleCollider);
        }

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.center = new Vector3(0f, 0f, 0f);
        characterController.height = 2f;
        characterController.radius = 0.5f;

        player.AddComponent<SimplePlayerController>();

        GameObject pivotObject = new GameObject("CameraPivot");
        pivotObject.transform.SetParent(player.transform);
        pivotObject.transform.localPosition = new Vector3(0f, 4.5f, 0f);
        pivotObject.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(pivotObject.transform);
        cameraObject.transform.localPosition = new Vector3(0f, 0f, -9f);
        cameraObject.transform.localRotation = Quaternion.identity;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        cameraObject.AddComponent<AudioListener>();

        return player;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static EventSystem CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
        return eventSystem;
    }

    private static GameObject CreateUIRoot(Transform parent)
    {
        GameObject uiRoot = new GameObject("UIRoot", typeof(RectTransform));
        uiRoot.transform.SetParent(parent, false);
        RectTransform rectTransform = uiRoot.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        return uiRoot;
    }

    private static Text CreateText(string name, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Text textComponent = textObject.AddComponent<Text>();
        textComponent.font = GetBuiltInFont();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        textComponent.resizeTextForBestFit = true;
        textComponent.resizeTextMinSize = 11;
        textComponent.resizeTextMaxSize = fontSize;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Truncate;
        return textComponent;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform));
        sliderObject.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);
        sliderRect.anchoredPosition = anchoredPosition;
        sliderRect.sizeDelta = size;

        Image backgroundImage = sliderObject.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform));
        fillObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.15f, 0.75f, 1f, 0.9f);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        slider.targetGraphic = backgroundImage;
        slider.fillRect = fillRect;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static GameObject CreateResultPanel(Transform parent)
    {
        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(840f, 540f);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.78f);
        return panel;
    }

    private static Text CreatePanelText(string name, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Text textComponent = textObject.AddComponent<Text>();
        textComponent.font = GetBuiltInFont();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        return textComponent;
    }

    private static Font GetBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private static void SetColliderTrigger(GameObject gameObject, bool isTrigger)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = isTrigger;
        }
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static Vector3 TestPosition(Vector3 localPosition)
    {
        return TestPlatformOrigin + localPosition;
    }

    private static void AssignTagIfExists(GameObject gameObject, string tagName)
    {
        foreach (string existingTag in InternalEditorUtility.tags)
        {
            if (existingTag == tagName)
            {
                gameObject.tag = tagName;
                return;
            }
        }

        Debug.LogWarning(
            $"First playable setup could not assign tag '{tagName}' to {gameObject.name} because the tag does not exist. " +
            "Add it via Edit > Project Settings > Tags and Layers, then rerun Tools > Chuo Evacuation > Build First Playable Test Setup.",
            gameObject);
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static Material GetShelterMarkerMaterial(
        ShelterDataLoader.ShelterData shelterData,
        Material officialShelterMaterial,
        Material candidateShelterMaterial,
        Material blockedShelterMaterial)
    {
        if (shelterData != null && !shelterData.canEnter)
        {
            return blockedShelterMaterial;
        }

        if (shelterData != null && shelterData.isOfficialShelter)
        {
            return officialShelterMaterial;
        }

        return candidateShelterMaterial;
    }

    private static string BuildShelterSummary(List<GeneratedShelter> generatedShelters)
    {
        if (generatedShelters == null || generatedShelters.Count == 0)
        {
            return "Generated shelters: none.";
        }

        var builder = new StringBuilder("Generated shelters:");

        foreach (GeneratedShelter generatedShelter in generatedShelters)
        {
            if (generatedShelter == null || generatedShelter.Data == null)
            {
                continue;
            }

            Vector3 position = generatedShelter.LocalPosition;
            builder.Append(
                $" {generatedShelter.Data.shelterId}@({position.x:0.#},{position.y:0.#},{position.z:0.#});");
        }

        return builder.ToString();
    }

    private static string MakeSafeObjectName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed";
        }

        return value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
    }

    private static ShelterDataLoader.ShelterData CreateFallbackShelterData()
    {
        return new ShelterDataLoader.ShelterData
        {
            shelterId = "test_shelter_001",
            shelterName = "Fallback Test Shelter",
            shelterRank = "B",
            isOfficialShelter = true,
            canEnter = true,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 10f,
            crowdingDelaySeconds = 0f,
            failureReason = "This shelter is not available.",
            sourceType = "test",
            facilityType = "debug_shelter",
            layoutPosition = new ShelterDataLoader.LayoutPosition { x = 12f, y = 0f, z = 0f },
            coordinateSystem = "debug_platform",
            dataSource = "FirstPlayableSceneBuilder fallback"
        };
    }

    private static Material CreateMaterial(string name, Color color, bool transparent)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return material;
    }

    private static string GetInstructionText()
    {
        return "Move: WASD / Arrow Keys\nSprint: Left or Right Shift\nCamera: Mouse drag (Q/E debug fallback)\nStart tsunami test: T\nReach green shelter and press E";
    }

    private static void AssignPlayer(GameObject player, Transform cameraPivot, Transform cameraTransform)
    {
        SimplePlayerController playerController = player.GetComponent<SimplePlayerController>();
        SetObjectReference(playerController, "cameraPivot", cameraPivot);
        SetObjectReference(playerController, "cameraTransform", cameraTransform);
        SetFloat(playerController, "moveSpeed", 6f);
        SetFloat(playerController, "sprintSpeed", 14f);
        SetFloat(playerController, "gravity", -20f);
        SetFloat(playerController, "rotationSpeed", 12f);
        SetBool(playerController, "logMovementDebug", true);
        SetBool(playerController, "fallbackTranslateIfControllerStuck", true);
        SetBool(playerController, "allowMovementDebugAlways", true);
        SetFloat(playerController, "movementLogInterval", 1f);
        SetFloat(playerController, "mouseSensitivity", 2.5f);
        SetBool(playerController, "orbitCameraWithMouseDrag", true);
        SetBool(playerController, "enableKeyboardCameraDebugFallback", true);
        SetFloat(playerController, "keyboardCameraTurnSpeed", 90f);
        SetFloat(playerController, "cameraDistance", 9f);
        SetFloat(playerController, "cameraHeight", 4.5f);
        SetFloat(playerController, "minPitch", -20f);
        SetFloat(playerController, "maxPitch", 65f);
        SetFloat(playerController, "initialPitch", 25f);
        SetBool(playerController, "lockCursorOnPlay", false);
    }

    private static void AssignGameManager(
        EvacuationGameManager gameManager,
        SimplePlayerController playerController,
        TsunamiCountdownManager countdownManager,
        MovingTsunamiWall tsunamiWall,
        ClimbSimulation climbSimulation,
        GameUIManager gameUIManager,
        ResultPanelController resultPanelController)
    {
        SetObjectReference(gameManager, "playerController", playerController);
        SetObjectReference(gameManager, "countdownManager", countdownManager);
        SetObjectReference(gameManager, "tsunamiWall", tsunamiWall);
        SetObjectReference(gameManager, "climbSimulation", climbSimulation);
        SetObjectReference(gameManager, "gameUIManager", gameUIManager);
        SetObjectReference(gameManager, "resultPanelController", resultPanelController);
        SetBool(gameManager, "startOnAwake", true);
        SetKeyCode(gameManager, "eventStartKey", KeyCode.T);
        SetString(gameManager, "preEventMessage", GetInstructionText());
        SetString(gameManager, "startMessage", "Tsunami warning issued. Countdown started. Reach green shelter and press E.");
    }

    private static void AssignCountdown(TsunamiCountdownManager countdownManager, EvacuationGameManager gameManager, GameUIManager gameUIManager)
    {
        SetFloat(countdownManager, "countdownSeconds", 60f);
        SetObjectReference(countdownManager, "gameManager", gameManager);
        SetObjectReference(countdownManager, "gameUIManager", gameUIManager);
    }

    private static void AssignTsunamiWall(MovingTsunamiWall tsunamiWall, Transform startPoint, Transform endPoint, EvacuationGameManager gameManager)
    {
        SetObjectReference(tsunamiWall, "startPoint", startPoint);
        SetObjectReference(tsunamiWall, "endPoint", endPoint);
        SetObjectReference(tsunamiWall, "gameManager", gameManager);
        SetString(tsunamiWall, "playerTag", "Player");
        SetFloat(tsunamiWall, "durationSeconds", 60f);
        SetBool(tsunamiWall, "resetToStartOnPlay", true);
        SetBool(tsunamiWall, "requireManualStartKey", false);
        SetKeyCode(tsunamiWall, "manualStartKey", KeyCode.T);
    }

    private static void AssignRiskZone(RiskZone riskZone, EvacuationGameManager gameManager)
    {
        SetObjectReference(riskZone, "gameManager", gameManager);
        SetString(riskZone, "playerTag", "Player");
        SetBool(riskZone, "failureOnEnter", true);
    }

    private static void AssignShelter(BuildingShelter shelter, ShelterDataLoader.ShelterData shelterData)
    {
        shelter.ApplyShelterData(shelterData);
        SetString(shelter, "postEarthquakeStatus", "usable");
    }

    private static void AssignShelterEntrance(
        ShelterEntranceTrigger shelterEntrance,
        BuildingShelter shelter,
        EvacuationGameManager gameManager,
        GameUIManager gameUIManager)
    {
        SetObjectReference(shelterEntrance, "shelter", shelter);
        SetObjectReference(shelterEntrance, "gameManager", gameManager);
        SetObjectReference(shelterEntrance, "gameUIManager", gameUIManager);
        SetString(shelterEntrance, "playerTag", "Player");
        SetKeyCode(shelterEntrance, "interactKey", KeyCode.E);
    }

    private static void AssignGameUI(
        GameUIManager gameUIManager,
        Text countdownText,
        Text warningText,
        Text shelterText,
        Text interactionPromptText,
        Text climbProgressText,
        Slider climbProgressSlider)
    {
        SetObjectReference(gameUIManager, "countdownText", countdownText);
        SetObjectReference(gameUIManager, "warningText", warningText);
        SetObjectReference(gameUIManager, "shelterText", shelterText);
        SetObjectReference(gameUIManager, "interactionPromptText", interactionPromptText);
        SetObjectReference(gameUIManager, "climbProgressText", climbProgressText);
        SetObjectReference(gameUIManager, "climbProgressSlider", climbProgressSlider);
    }

    private static void AssignResultPanel(
        ResultPanelController resultPanelController,
        GameObject resultPanel,
        Text titleText,
        Text shelterText,
        Text elapsedTimeText,
        Text reasonText)
    {
        SetObjectReference(resultPanelController, "panelRoot", resultPanel);
        SetObjectReference(resultPanelController, "titleText", titleText);
        SetObjectReference(resultPanelController, "shelterText", shelterText);
        SetObjectReference(resultPanelController, "elapsedTimeText", elapsedTimeText);
        SetObjectReference(resultPanelController, "reasonText", reasonText);
    }

    private static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetString(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetKeyCode(Object target, string propertyName, KeyCode value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = (int)value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private class GeneratedShelter
    {
        public BuildingShelter Shelter;
        public ShelterEntranceTrigger Entrance;
        public ShelterDataLoader.ShelterData Data;
        public Vector3 LocalPosition;
    }

}
