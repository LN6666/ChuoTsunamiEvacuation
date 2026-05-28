using System.IO;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class NewMapPlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private bool dragLookEnabled = true;
    [SerializeField] private bool lookRequiresMouseButton = true;
    [SerializeField] private int lookMouseButton = 1;
    [SerializeField] private bool cursorVisibleWhenNotDragging = true;
    [SerializeField] private float mouseSensitivityX = 2.0f;
    [SerializeField] private float mouseSensitivityY = 1.5f;
    [SerializeField] private float keyboardTurnSpeed = 90f;
    [SerializeField] private float cameraDistance = 8f;
    [SerializeField] private float cameraHeight = 3.2f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float fallRecoveryDistance = 20f;

    private const float GroundSkinOffset = 0.04f;

    private CharacterController characterController;
    private NewMapGameMode currentMode = NewMapGameMode.None;
    private NewMapWeatherPreset currentWeather = NewMapWeatherPreset.ClearDay;
    private float verticalVelocity;
    private float yaw;
    private float pitch = 25f;
    private bool controlEnabled;
    private bool mouseLookEnabled;
    private bool wantsLockedCursor;
    private bool isMouseLookDragging;
    private Vector3 lastValidGroundPosition;
    private float stamina = 100f;
    private int fallRecoveryCount;

    public float WalkSpeedMetersPerSecond { get; private set; }
    public float SprintSpeedMetersPerSecond { get; private set; }
    public bool StaminaEnabled => currentMode == NewMapGameMode.Evacuation;
    public float Stamina => stamina;
    public float Stamina01 => Mathf.Clamp01(stamina / 100f);
    public NewMapGameMode CurrentMode => currentMode;
    public NewMapWeatherPreset CurrentWeather => currentWeather;
    public bool ControlEnabled => controlEnabled;
    public bool MouseLookEnabled => mouseLookEnabled;
    public bool WantsLockedCursor => wantsLockedCursor;
    public bool IsMouseLookDragging => isMouseLookDragging;
    public bool LookRequiresMouseButton => lookRequiresMouseButton;
    public string LookMouseButtonName => NewMapMouseDragLookConfig.MouseButtonNameFromIndex(lookMouseButton);
    public bool CursorVisibleWhenNotDragging => cursorVisibleWhenNotDragging;
    public float MouseSensitivity => (MouseSensitivityX + MouseSensitivityY) * 0.5f;
    public float MouseSensitivityX => mouseSensitivityX;
    public float MouseSensitivityY => mouseSensitivityY;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;
    public float CurrentYaw => yaw;
    public float CurrentPitch => pitch;
    public bool HasActiveCamera => cameraTransform != null && cameraTransform.GetComponent<Camera>() != null;
    public Vector3 LastValidGroundPosition => lastValidGroundPosition;
    public int FallRecoveryCount => fallRecoveryCount;

    public static NewMapPlayerController Create(Transform parent, Vector3 spawnPosition)
    {
        GameObject playerObject = new GameObject("NewMap_Player");
        playerObject.transform.SetParent(parent, true);
        playerObject.transform.position = spawnPosition;
        playerObject.transform.rotation = Quaternion.identity;
        TrySetPlayerTag(playerObject);

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.35f;
        controller.slopeLimit = 50f;

        NewMapPlayerController player = playerObject.AddComponent<NewMapPlayerController>();
        player.ApplyMouseDragLookConfig(NewMapMouseDragLookConfig.Load());
        player.CreateCameraRig();
        NewMapVisualFactory.CreateHumanoid(playerObject.transform, "PlayerVisual", new Color(0.1f, 0.55f, 1f, 1f));
        player.lastValidGroundPosition = spawnPosition;
        player.SetMode(NewMapGameMode.Tourism, NewMapWeatherPreset.ClearDay);
        player.SetControlEnabled(false);
        return player;
    }

    private static void TrySetPlayerTag(GameObject playerObject)
    {
        try
        {
            playerObject.tag = "Player";
        }
        catch (UnityException)
        {
            Debug.LogWarning("Default Player tag was unavailable. NewMap player still runs, but tag-based legacy triggers may ignore it.");
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraPivot == null || cameraTransform == null)
        {
            CreateCameraRig();
        }
    }

    private void Update()
    {
        UpdateCameraOrbit();

        if (!controlEnabled)
        {
            return;
        }

        UpdateMovement();
        RecoverIfFalling();
    }

    private void LateUpdate()
    {
        if (cameraPivot == null || cameraTransform == null)
        {
            return;
        }

        cameraPivot.position = transform.position + Vector3.up * cameraHeight;
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cameraTransform.localPosition = new Vector3(0f, 0f, -cameraDistance);
        cameraTransform.localRotation = Quaternion.identity;
    }

    public void SetMode(NewMapGameMode mode, NewMapWeatherPreset weather)
    {
        currentMode = mode;
        currentWeather = weather;
        float modifier = NewMapRuntimeConstants.GetWeatherModifier(weather);

        if (mode == NewMapGameMode.Evacuation)
        {
            WalkSpeedMetersPerSecond = NewMapRuntimeConstants.EvacuationWalkSpeed * modifier;
            SprintSpeedMetersPerSecond = NewMapRuntimeConstants.EvacuationSprintSpeed * modifier;
            stamina = Mathf.Clamp(stamina, 0f, 100f);
            return;
        }

        WalkSpeedMetersPerSecond = NewMapRuntimeConstants.TourismWalkSpeed * modifier;
        SprintSpeedMetersPerSecond = NewMapRuntimeConstants.TourismSprintSpeed * modifier;
        stamina = 100f;
    }

    public void ResetStamina()
    {
        stamina = 100f;
    }

    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
        mouseLookEnabled = enabled && dragLookEnabled;
        SetMouseLookDragging(false);
        if (!enabled)
        {
            verticalVelocity = 0f;
        }

        ApplyCursorState();
    }

    public void MoveForDiagnostics(Vector3 worldDirection, float deltaTime, bool sprint)
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        Vector3 direction = worldDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        float stepDelta = Mathf.Max(0f, deltaTime);
        float speed = sprint ? SprintSpeedMetersPerSecond : WalkSpeedMetersPerSecond;
        if (StaminaEnabled)
        {
            if (sprint && direction.sqrMagnitude > 0.01f && stamina > 1f)
            {
                stamina = Mathf.Max(0f, stamina - 24f * stepDelta);
            }
            else
            {
                stamina = Mathf.Min(100f, stamina + 16f * stepDelta);
            }
        }

        if (characterController != null && characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            lastValidGroundPosition = transform.position;
        }

        verticalVelocity += gravity * stepDelta;
        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;
        if (characterController != null)
        {
            characterController.Move(velocity * stepDelta);
        }
        else
        {
            transform.position += velocity * stepDelta;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        SnapToRuntimeGroundSupport(4f);
        RecoverIfFalling();
    }

    private void SnapToRuntimeGroundSupport(float probeDistance)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Max(1f, probeDistance), ~0, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        float groundedY = hit.point.y + GroundSkinOffset;
        if (transform.position.y < groundedY)
        {
            transform.position = new Vector3(transform.position.x, groundedY, transform.position.z);
            verticalVelocity = -2f;
        }

        lastValidGroundPosition = transform.position;
    }

    private void CreateCameraRig()
    {
        if (cameraPivot == null)
        {
            GameObject pivot = new GameObject("NewMap_CameraPivot");
            pivot.transform.SetParent(transform, true);
            cameraPivot = pivot.transform;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        TrySetMainCameraTag(camera.gameObject);
        cameraTransform = camera.transform;
        cameraTransform.SetParent(cameraPivot, false);
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 5000f;
    }

    private static void TrySetMainCameraTag(GameObject cameraObject)
    {
        if (cameraObject == null)
        {
            return;
        }

        try
        {
            cameraObject.tag = "MainCamera";
        }
        catch (UnityException)
        {
            Debug.LogWarning("Default MainCamera tag was unavailable. NewMap lighting will still run, but camera sky color may need manual verification.");
        }
    }

    private void UpdateCameraOrbit()
    {
        if (!controlEnabled)
        {
            SetMouseLookDragging(false);
            return;
        }

        if (mouseLookEnabled)
        {
            bool buttonHeld = !lookRequiresMouseButton || Input.GetMouseButton(lookMouseButton);
            SetMouseLookDragging(buttonHeld);
            if (buttonHeld)
            {
                ApplyLookDelta(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            }

            return;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            yaw -= keyboardTurnSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            yaw += keyboardTurnSpeed * Time.deltaTime;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursorState();
        }
    }

    private void ApplyCursorState()
    {
        if (isMouseLookDragging)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = !controlEnabled || cursorVisibleWhenNotDragging || !mouseLookEnabled;
    }

    public void ApplyLookDeltaForDiagnostics(float mouseX, float mouseY)
    {
        ApplyLookInputForDiagnostics(mouseX, mouseY, true);
    }

    public bool ApplyLookInputForDiagnostics(float mouseX, float mouseY, bool mouseButtonHeld)
    {
        if (!controlEnabled || !mouseLookEnabled)
        {
            SetMouseLookDragging(false);
            return false;
        }

        bool canLook = !lookRequiresMouseButton || mouseButtonHeld;
        SetMouseLookDragging(canLook);
        if (!canLook)
        {
            return false;
        }

        ApplyLookDelta(mouseX, mouseY);
        return true;
    }

    public void ReleaseLookDragForDiagnostics()
    {
        SetMouseLookDragging(false);
    }

    private void ApplyLookDelta(float mouseX, float mouseY)
    {
        yaw += mouseX * mouseSensitivityX;
        pitch = Mathf.Clamp(pitch - mouseY * mouseSensitivityY, minPitch, maxPitch);
    }

    private void ApplyMouseDragLookConfig(NewMapMouseDragLookConfig config)
    {
        config = config ?? NewMapMouseDragLookConfig.Default();
        dragLookEnabled = config.enabled;
        lookRequiresMouseButton = config.lookRequiresMouseButton;
        lookMouseButton = NewMapMouseDragLookConfig.ParseMouseButtonIndex(config.lookMouseButton);
        cursorVisibleWhenNotDragging = config.cursorVisibleWhenNotDragging;
        mouseSensitivityX = Mathf.Clamp(config.sensitivityX, 0.05f, 20f);
        mouseSensitivityY = Mathf.Clamp(config.sensitivityY, 0.05f, 20f);
        minPitch = Mathf.Clamp(config.pitchMin, -89f, 0f);
        maxPitch = Mathf.Clamp(config.pitchMax, 0f, 89f);
        if (maxPitch <= minPitch)
        {
            minPitch = -60f;
            maxPitch = 70f;
        }
    }

    private void SetMouseLookDragging(bool dragging)
    {
        isMouseLookDragging = controlEnabled && mouseLookEnabled && dragging;
        wantsLockedCursor = isMouseLookDragging;
        ApplyCursorState();
    }

    private void UpdateMovement()
    {
        Vector2 input = ReadMovementInput();
        Vector3 direction = GetCameraRelativeMovement(input);
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool canSprint = wantsSprint && (!StaminaEnabled || stamina > 1f);
        float speed = canSprint ? SprintSpeedMetersPerSecond : WalkSpeedMetersPerSecond;

        if (StaminaEnabled)
        {
            if (canSprint && direction.sqrMagnitude > 0.01f)
            {
                stamina = Mathf.Max(0f, stamina - 24f * Time.deltaTime);
            }
            else
            {
                stamina = Mathf.Min(100f, stamina + 16f * Time.deltaTime);
            }
        }
        else
        {
            stamina = 100f;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            lastValidGroundPosition = transform.position;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
        SnapToRuntimeGroundSupport(4f);

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                12f * Time.deltaTime);
        }
    }

    private void RecoverIfFalling()
    {
        if (transform.position.y >= lastValidGroundPosition.y - fallRecoveryDistance)
        {
            return;
        }

        transform.position = lastValidGroundPosition + Vector3.up * 1.5f;
        verticalVelocity = 0f;
        fallRecoveryCount++;
        Debug.LogWarning("NewMap player fall recovery returned the player to the last valid ground position.");
    }

    private static Vector2 ReadMovementInput()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1f;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            y += 1f;
        }

        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    private Vector3 GetCameraRelativeMovement(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            return Vector3.zero;
        }

        Transform reference = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;
        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude <= 0.01f)
        {
            forward = transform.forward;
        }

        forward.Normalize();
        right.Normalize();
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}

[System.Serializable]
public sealed class NewMapMouseDragLookConfig
{
    public bool enabled = true;
    public bool lookRequiresMouseButton = true;
    public string lookMouseButton = "RightMouse";
    public float sensitivityX = 2.0f;
    public float sensitivityY = 1.5f;
    public float pitchMin = -60f;
    public float pitchMax = 70f;
    public bool cursorVisibleWhenNotDragging = true;

    public static NewMapMouseDragLookConfig Default()
    {
        return new NewMapMouseDragLookConfig();
    }

    public static NewMapMouseDragLookConfig Load()
    {
        NewMapMouseDragLookConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_mouse_drag_look_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapMouseDragLookConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap mouse drag-look config could not be loaded; using defaults. {exception.Message}");
            }
        }

        config.sensitivityX = Mathf.Clamp(config.sensitivityX, 0.05f, 20f);
        config.sensitivityY = Mathf.Clamp(config.sensitivityY, 0.05f, 20f);
        config.pitchMin = Mathf.Clamp(config.pitchMin, -89f, 0f);
        config.pitchMax = Mathf.Clamp(config.pitchMax, 0f, 89f);
        if (config.pitchMax <= config.pitchMin)
        {
            config.pitchMin = -60f;
            config.pitchMax = 70f;
        }

        return config;
    }

    public static int ParseMouseButtonIndex(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 1;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "left":
            case "leftmouse":
            case "mouse0":
                return 0;
            case "middle":
            case "middlemouse":
            case "mouse2":
                return 2;
            case "right":
            case "rightmouse":
            case "mouse1":
            default:
                return 1;
        }
    }

    public static string MouseButtonNameFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return "LeftMouse";
            case 2:
                return "MiddleMouse";
            default:
                return "RightMouse";
        }
    }
}
