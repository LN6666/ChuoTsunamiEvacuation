using System.Collections.Generic;
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
    [SerializeField] private int[] allowedLookMouseButtons = { 0, 1 };
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
    private Vector3 safeRecoveryPosition;
    private readonly List<Bounds> buildingCollisionBounds = new List<Bounds>();
    private NewMapPlayableBounds safetyPlayableBounds;
    private NewMapCircularBoundary circularBoundary;
    private NewMapNpcCrowdPrototype playerNpcCollisionSource;
    private NewMapPlayerNpcCollisionConfig playerNpcCollisionConfig;
    private bool hasSafetyPlayableBounds;
    private bool circularBoundaryClampEnabled;
    private bool recoverOutsidePlayableBounds = true;
    private bool logRecoveryEvents;
    private bool buildingCollisionEnabled;
    private bool playerNpcCollisionEnabled;
    private float buildingCollisionMarginMeters = 0.35f;
    private float lastPlayerNpcSlowdownFactor = 1f;
    private float safeGroundY;
    private float fallRecoveryThresholdY = -8f;
    private NewMapPlayerStaminaConfig staminaConfig;
    private float maxStamina = 100f;
    private float staminaMultiplier = 1f;
    private float sprintSpeedMultiplierAdditional = 1f;
    private float baselineMaxStamina = 100f;
    private float stamina = 100f;
    private int fallRecoveryCount;
    private int buildingCollisionBlockedCount;
    private int buildingCollisionRecoveryCount;
    private int playerNpcCollisionBlockedCount;
    private int playerNpcCollisionSlowdownCount;
    private int playerNpcCollisionEscapeCount;

    public float WalkSpeedMetersPerSecond { get; private set; }
    public float SprintSpeedMetersPerSecond { get; private set; }
    public bool StaminaEnabled => currentMode == NewMapGameMode.Evacuation;
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public float BaselineMaxStamina => baselineMaxStamina;
    public float StaminaMultiplier => staminaMultiplier;
    public float SprintSpeedMultiplierAdditional => sprintSpeedMultiplierAdditional;
    public float Stamina01 => Mathf.Clamp01(stamina / Mathf.Max(1f, maxStamina));
    public NewMapGameMode CurrentMode => currentMode;
    public NewMapWeatherPreset CurrentWeather => currentWeather;
    public bool ControlEnabled => controlEnabled;
    public bool MouseLookEnabled => mouseLookEnabled;
    public bool WantsLockedCursor => wantsLockedCursor;
    public bool IsMouseLookDragging => isMouseLookDragging;
    public bool LookRequiresMouseButton => lookRequiresMouseButton;
    public string LookMouseButtonName => string.Join(",", AllowedLookMouseButtonNames);
    public string[] AllowedLookMouseButtonNames => NewMapMouseDragLookConfig.MouseButtonNamesFromIndices(allowedLookMouseButtons);
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
    public Vector3 SafeRecoveryPosition => safeRecoveryPosition;
    public float FallRecoveryThresholdY => fallRecoveryThresholdY;
    public string LastFallRecoveryReason { get; private set; } = string.Empty;
    public int FallRecoveryCount => fallRecoveryCount;
    public bool BuildingCollisionEnabled => buildingCollisionEnabled;
    public int BuildingCollisionBoundsCount => buildingCollisionBounds.Count;
    public int BuildingCollisionBlockedCount => buildingCollisionBlockedCount;
    public int BuildingCollisionRecoveryCount => buildingCollisionRecoveryCount;
    public bool PlayerNpcCollisionEnabled => playerNpcCollisionEnabled;
    public int PlayerNpcCollisionBlockedCount => playerNpcCollisionBlockedCount;
    public int PlayerNpcCollisionSlowdownCount => playerNpcCollisionSlowdownCount;
    public int PlayerNpcCollisionEscapeCount => playerNpcCollisionEscapeCount;
    public float LastPlayerNpcSlowdownFactor => lastPlayerNpcSlowdownFactor;
    public bool CircularBoundaryClampEnabled => circularBoundaryClampEnabled && circularBoundary.IsValid;
    public float CircularBoundaryRadiusMeters => circularBoundary.RadiusMeters;

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
        player.safeRecoveryPosition = spawnPosition;
        player.safeGroundY = spawnPosition.y - GroundSkinOffset;
        player.SetMode(NewMapGameMode.Tourism, NewMapWeatherPreset.ClearDay);
        player.SetControlEnabled(false);
        return player;
    }

    public void ConfigureGroundSafety(
        Vector3 safeSpawnPosition,
        NewMapPlayableBounds playableBounds,
        NewMapCircularBoundary boundary,
        bool enableCircularBoundaryClamp,
        float supportY,
        float recoverBelowY,
        bool recoverOutsideBounds,
        bool logRecoveries)
    {
        safeRecoveryPosition = safeSpawnPosition;
        lastValidGroundPosition = safeSpawnPosition;
        safetyPlayableBounds = playableBounds;
        hasSafetyPlayableBounds = playableBounds.IsValid;
        circularBoundary = boundary;
        circularBoundaryClampEnabled = enableCircularBoundaryClamp && boundary.IsValid && boundary.AffectsPlayer;
        safeGroundY = Mathf.Clamp(supportY, -20f, 30f);
        fallRecoveryThresholdY = Mathf.Clamp(recoverBelowY, -50f, 5f);
        recoverOutsidePlayableBounds = recoverOutsideBounds;
        logRecoveryEvents = logRecoveries;
    }

    public bool TryRecoverForDiagnostics()
    {
        int previousCount = fallRecoveryCount;
        RecoverIfFalling();
        return fallRecoveryCount > previousCount;
    }

    public void ConfigureBuildingCollision(IEnumerable<Bounds> buildingBounds, float marginMeters, bool enabled)
    {
        buildingCollisionBounds.Clear();
        if (buildingBounds != null)
        {
            foreach (Bounds bounds in buildingBounds)
            {
                if (IsFiniteBounds(bounds) && bounds.size.x >= 0.5f && bounds.size.z >= 0.5f && bounds.size.y >= 1f)
                {
                    buildingCollisionBounds.Add(bounds);
                }
            }
        }

        buildingCollisionMarginMeters = Mathf.Clamp(marginMeters, 0f, 5f);
        buildingCollisionEnabled = enabled && buildingCollisionBounds.Count > 0;
    }

    public Bounds[] GetBuildingCollisionBoundsForDiagnostics()
    {
        return buildingCollisionBounds.ToArray();
    }

    public bool IsInsideBuildingForDiagnostics(Vector3 position, float marginMeters = 0f)
    {
        return IsInsideBuildingBoundsXZ(position, marginMeters);
    }

    public void ConfigurePlayerNpcCollision(NewMapNpcCrowdPrototype crowd, NewMapPlayerNpcCollisionConfig config)
    {
        playerNpcCollisionSource = crowd;
        playerNpcCollisionConfig = config ?? NewMapPlayerNpcCollisionConfig.Default();
        playerNpcCollisionEnabled =
            playerNpcCollisionConfig.enabled &&
            playerNpcCollisionConfig.preventDirectOverlap &&
            playerNpcCollisionSource != null;
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
        EnsureStaminaConfig();
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
        EnsureStaminaConfig();
        currentMode = mode;
        currentWeather = weather;
        float modifier = NewMapRuntimeConstants.GetWeatherModifier(weather);

        if (mode == NewMapGameMode.Evacuation)
        {
            WalkSpeedMetersPerSecond = NewMapRuntimeConstants.EvacuationWalkSpeed * modifier;
            SprintSpeedMetersPerSecond =
                NewMapRuntimeConstants.EvacuationSprintSpeed *
                staminaConfig.SprintSpeedMultiplierAdditional *
                modifier;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            return;
        }

        WalkSpeedMetersPerSecond = NewMapRuntimeConstants.TourismWalkSpeed * modifier;
        SprintSpeedMetersPerSecond = NewMapRuntimeConstants.TourismSprintSpeed * modifier;
        stamina = maxStamina;
    }

    public void ResetStamina()
    {
        EnsureStaminaConfig();
        stamina = maxStamina;
    }

    public void TeleportToSpawn(Vector3 spawnPosition)
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        bool wasEnabled = characterController != null && characterController.enabled;
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = spawnPosition;
        verticalVelocity = 0f;
        lastValidGroundPosition = spawnPosition;
        safeRecoveryPosition = spawnPosition;
        safeGroundY = spawnPosition.y - GroundSkinOffset;
        ResetStamina();

        if (characterController != null)
        {
            characterController.enabled = wasEnabled;
        }
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
        EnsureStaminaConfig();
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
                stamina = Mathf.Min(maxStamina, stamina + 16f * stepDelta);
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
        Vector3 previousPosition = transform.position;
        ApplyPlayerNpcCollisionToVelocity(previousPosition, stepDelta, ref velocity);
        if (characterController != null)
        {
            characterController.Move(velocity * stepDelta);
        }
        else
        {
            transform.position += velocity * stepDelta;
        }

        ApplyBuildingCollisionCorrection(previousPosition);
        ApplyPlayerNpcCollisionCorrection(previousPosition);
        ApplyCircularBoundaryClamp();

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
            bool buttonHeld = !lookRequiresMouseButton || IsAnyLookMouseButtonHeld();
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
        return ApplyLookInputForDiagnosticsResolved(mouseX, mouseY, !lookRequiresMouseButton || mouseButtonHeld);
    }

    public bool ApplyLookInputForDiagnostics(float mouseX, float mouseY, string mouseButtonName)
    {
        int buttonIndex = NewMapMouseDragLookConfig.ParseMouseButtonIndex(mouseButtonName);
        return ApplyLookInputForDiagnosticsResolved(mouseX, mouseY, !lookRequiresMouseButton || IsLookMouseButtonAllowed(buttonIndex));
    }

    public bool ApplyLookInputForDiagnostics(float mouseX, float mouseY, int mouseButtonIndex)
    {
        return ApplyLookInputForDiagnosticsResolved(mouseX, mouseY, !lookRequiresMouseButton || IsLookMouseButtonAllowed(mouseButtonIndex));
    }

    private bool ApplyLookInputForDiagnosticsResolved(float mouseX, float mouseY, bool canLook)
    {
        if (!controlEnabled || !mouseLookEnabled)
        {
            SetMouseLookDragging(false);
            return false;
        }

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
        allowedLookMouseButtons = NewMapMouseDragLookConfig.ParseAllowedMouseButtonIndices(config.allowedButtons, config.lookMouseButton);
        lookMouseButton = allowedLookMouseButtons != null && allowedLookMouseButtons.Length > 0
            ? allowedLookMouseButtons[0]
            : NewMapMouseDragLookConfig.ParseMouseButtonIndex(config.lookMouseButton);
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

    private bool IsAnyLookMouseButtonHeld()
    {
        if (allowedLookMouseButtons == null || allowedLookMouseButtons.Length == 0)
        {
            return Input.GetMouseButton(lookMouseButton);
        }

        for (int i = 0; i < allowedLookMouseButtons.Length; i++)
        {
            if (Input.GetMouseButton(allowedLookMouseButtons[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsLookMouseButtonAllowed(int buttonIndex)
    {
        if (allowedLookMouseButtons == null || allowedLookMouseButtons.Length == 0)
        {
            return buttonIndex == lookMouseButton;
        }

        for (int i = 0; i < allowedLookMouseButtons.Length; i++)
        {
            if (allowedLookMouseButtons[i] == buttonIndex)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateMovement()
    {
        EnsureStaminaConfig();
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
                stamina = Mathf.Min(maxStamina, stamina + 16f * Time.deltaTime);
            }
        }
        else
        {
            stamina = maxStamina;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            lastValidGroundPosition = transform.position;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;
        Vector3 previousPosition = transform.position;
        ApplyPlayerNpcCollisionToVelocity(previousPosition, Time.deltaTime, ref velocity);
        characterController.Move(velocity * Time.deltaTime);
        ApplyBuildingCollisionCorrection(previousPosition);
        ApplyPlayerNpcCollisionCorrection(previousPosition);
        ApplyCircularBoundaryClamp();
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
        bool belowLastValid = transform.position.y < lastValidGroundPosition.y - fallRecoveryDistance;
        bool belowAbsoluteThreshold = transform.position.y < fallRecoveryThresholdY;
        bool outsidePlayableBounds = recoverOutsidePlayableBounds &&
            ((CircularBoundaryClampEnabled && !circularBoundary.ContainsXZ(transform.position, 0f)) ||
            (hasSafetyPlayableBounds && !safetyPlayableBounds.ContainsXZ(transform.position, 0f)));

        if (!belowLastValid && !belowAbsoluteThreshold && !outsidePlayableBounds)
        {
            return;
        }

        Vector3 recovery = belowAbsoluteThreshold || belowLastValid ? safeRecoveryPosition : transform.position;
        if (CircularBoundaryClampEnabled)
        {
            recovery = circularBoundary.ClampXZ(recovery, 2f);
        }
        else if (hasSafetyPlayableBounds)
        {
            recovery = safetyPlayableBounds.ClampXZ(recovery, 2f);
        }

        recovery.y = Mathf.Max(safeGroundY + GroundSkinOffset, safeRecoveryPosition.y);
        transform.position = recovery;
        verticalVelocity = 0f;
        lastValidGroundPosition = recovery;
        fallRecoveryCount++;
        LastFallRecoveryReason = (belowAbsoluteThreshold || belowLastValid) ? "below_fall_threshold" : "outside_playable_bounds";
        if (logRecoveryEvents)
        {
            Debug.Log($"NewMap player safety recovery reason={LastFallRecoveryReason} count={fallRecoveryCount}");
        }
    }

    private void ApplyCircularBoundaryClamp()
    {
        if (!CircularBoundaryClampEnabled || circularBoundary.ContainsXZ(transform.position, 0f))
        {
            return;
        }

        Vector3 clamped = circularBoundary.ClampXZ(transform.position, 0.5f);
        clamped.y = Mathf.Max(clamped.y, safeGroundY + GroundSkinOffset);
        transform.position = clamped;
        verticalVelocity = Mathf.Min(verticalVelocity, -2f);
        lastValidGroundPosition = clamped;
    }

    private void ApplyBuildingCollisionCorrection(Vector3 previousPosition)
    {
        if (!buildingCollisionEnabled || !IsInsideBuildingBoundsXZ(transform.position, buildingCollisionMarginMeters))
        {
            return;
        }

        buildingCollisionBlockedCount++;
        Vector3 corrected = ResolveNearestOutsideBuildingPosition(transform.position, buildingCollisionMarginMeters + 0.15f);
        if (IsInsideBuildingBoundsXZ(corrected, 0.02f))
        {
            corrected = previousPosition;
            buildingCollisionRecoveryCount++;
        }

        corrected.y = Mathf.Max(corrected.y, safeGroundY + GroundSkinOffset);
        transform.position = corrected;
        verticalVelocity = Mathf.Min(verticalVelocity, -2f);
        if (!IsInsideBuildingBoundsXZ(corrected, 0.02f))
        {
            lastValidGroundPosition = corrected;
        }
    }

    private void ApplyPlayerNpcCollisionCorrection(Vector3 previousPosition)
    {
        if (!TryResolvePlayerNpcCollision(previousPosition, transform.position, out Vector3 corrected))
        {
            return;
        }

        transform.position = corrected;
    }

    private void ApplyPlayerNpcCollisionToVelocity(Vector3 previousPosition, float deltaTime, ref Vector3 velocity)
    {
        if (deltaTime <= 0.0001f)
        {
            return;
        }

        Vector3 intendedPosition = previousPosition + velocity * deltaTime;
        if (!TryResolvePlayerNpcCollision(previousPosition, intendedPosition, out Vector3 corrected))
        {
            return;
        }

        velocity.x = (corrected.x - previousPosition.x) / deltaTime;
        velocity.z = (corrected.z - previousPosition.z) / deltaTime;
    }

    private bool TryResolvePlayerNpcCollision(Vector3 previousPosition, Vector3 candidatePosition, out Vector3 corrected)
    {
        corrected = candidatePosition;
        lastPlayerNpcSlowdownFactor = 1f;
        if (!playerNpcCollisionEnabled || playerNpcCollisionSource == null)
        {
            return false;
        }

        if (!playerNpcCollisionSource.ResolvePlayerPositionAgainstNpcs(
            previousPosition,
            candidatePosition,
            playerNpcCollisionConfig,
            out corrected,
            out bool blocked,
            out bool slowed,
            out bool escapeApplied,
            out float slowdownFactor))
        {
            return false;
        }

        corrected.y = Mathf.Max(corrected.y, safeGroundY + GroundSkinOffset);
        lastPlayerNpcSlowdownFactor = slowdownFactor;
        if (blocked)
        {
            playerNpcCollisionBlockedCount++;
        }

        if (slowed)
        {
            playerNpcCollisionSlowdownCount++;
        }

        if (escapeApplied)
        {
            playerNpcCollisionEscapeCount++;
        }

        if (!IsInsideBuildingBoundsXZ(corrected, 0.02f))
        {
            lastValidGroundPosition = corrected;
        }

        return true;
    }

    private bool IsInsideBuildingBoundsXZ(Vector3 position, float marginMeters)
    {
        float margin = Mathf.Max(0f, marginMeters);
        for (int i = 0; i < buildingCollisionBounds.Count; i++)
        {
            Bounds bounds = buildingCollisionBounds[i];
            if (position.x >= bounds.min.x - margin &&
                position.x <= bounds.max.x + margin &&
                position.z >= bounds.min.z - margin &&
                position.z <= bounds.max.z + margin)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 ResolveNearestOutsideBuildingPosition(Vector3 position, float marginMeters)
    {
        Vector3 corrected = position;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < buildingCollisionBounds.Count; i++)
        {
            Bounds bounds = buildingCollisionBounds[i];
            float minX = bounds.min.x - marginMeters;
            float maxX = bounds.max.x + marginMeters;
            float minZ = bounds.min.z - marginMeters;
            float maxZ = bounds.max.z + marginMeters;
            if (position.x < minX || position.x > maxX || position.z < minZ || position.z > maxZ)
            {
                continue;
            }

            float left = Mathf.Abs(position.x - minX);
            float right = Mathf.Abs(maxX - position.x);
            float back = Mathf.Abs(position.z - minZ);
            float front = Mathf.Abs(maxZ - position.z);
            float nearest = Mathf.Min(Mathf.Min(left, right), Mathf.Min(back, front));
            if (nearest >= bestDistance)
            {
                continue;
            }

            bestDistance = nearest;
            if (nearest == left)
            {
                corrected = new Vector3(minX, position.y, position.z);
            }
            else if (nearest == right)
            {
                corrected = new Vector3(maxX, position.y, position.z);
            }
            else if (nearest == back)
            {
                corrected = new Vector3(position.x, position.y, minZ);
            }
            else
            {
                corrected = new Vector3(position.x, position.y, maxZ);
            }
        }

        return corrected;
    }

    private static bool IsFiniteBounds(Bounds bounds)
    {
        return IsFinite(bounds.min.x) &&
            IsFinite(bounds.min.y) &&
            IsFinite(bounds.min.z) &&
            IsFinite(bounds.max.x) &&
            IsFinite(bounds.max.y) &&
            IsFinite(bounds.max.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
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

    private void EnsureStaminaConfig()
    {
        if (staminaConfig != null)
        {
            return;
        }

        staminaConfig = NewMapPlayerStaminaConfig.Load();
        baselineMaxStamina = staminaConfig.baselineMaxStamina;
        staminaMultiplier = staminaConfig.staminaMultiplier;
        sprintSpeedMultiplierAdditional = staminaConfig.SprintSpeedMultiplierAdditional;
        maxStamina = staminaConfig.MaxStamina;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }
}

[System.Serializable]
public sealed class NewMapPlayerStaminaConfig
{
    public float baselineMaxStamina = 100f;
    public float staminaMultiplier = 35f;
    public float sprintSpeedMultiplierAdditional = 0.918f;

    public float MaxStamina => Mathf.Max(1f, baselineMaxStamina) * Mathf.Max(1f, staminaMultiplier);
    public float SprintSpeedMultiplierAdditional => Mathf.Clamp(sprintSpeedMultiplierAdditional, 0.1f, 10f);
    public float FinalEvacuationSprintSpeed => NewMapRuntimeConstants.EvacuationSprintSpeed * SprintSpeedMultiplierAdditional;

    public static NewMapPlayerStaminaConfig Default()
    {
        return new NewMapPlayerStaminaConfig();
    }

    public static NewMapPlayerStaminaConfig Load()
    {
        NewMapPlayerStaminaConfig config = Default();
        string path = Path.Combine(Application.dataPath, "Data/P10/newmap_player_stamina_config.json");
        if (File.Exists(path))
        {
            try
            {
                config = JsonUtility.FromJson<NewMapPlayerStaminaConfig>(File.ReadAllText(path)) ?? config;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"NewMap player stamina config could not be loaded; using 100x defaults. {exception.Message}");
            }
        }

        config.baselineMaxStamina = Mathf.Clamp(config.baselineMaxStamina, 1f, 10000f);
        config.staminaMultiplier = Mathf.Clamp(config.staminaMultiplier, 1f, 1000f);
        config.sprintSpeedMultiplierAdditional = config.SprintSpeedMultiplierAdditional;
        return config;
    }
}

[System.Serializable]
public sealed class NewMapMouseDragLookConfig
{
    public bool enabled = true;
    public bool lookRequiresMouseButton = true;
    public string lookMouseButton = "RightMouse";
    public string[] allowedButtons = { "LeftMouse", "RightMouse" };
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

        config.allowedButtons = MouseButtonNamesFromIndices(ParseAllowedMouseButtonIndices(config.allowedButtons, config.lookMouseButton));

        return config;
    }

    public static int[] ParseAllowedMouseButtonIndices(string[] values, string fallbackValue)
    {
        var parsed = new List<int>();
        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                AddUnique(parsed, ParseMouseButtonIndex(values[i]));
            }
        }

        if (parsed.Count == 0)
        {
            AddUnique(parsed, ParseMouseButtonIndex(fallbackValue));
        }

        return parsed.ToArray();
    }

    public static string[] MouseButtonNamesFromIndices(int[] indices)
    {
        if (indices == null || indices.Length == 0)
        {
            return new[] { "RightMouse" };
        }

        var names = new string[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            names[i] = MouseButtonNameFromIndex(indices[i]);
        }

        return names;
    }

    private static void AddUnique(List<int> values, int candidate)
    {
        if (!values.Contains(candidate))
        {
            values.Add(candidate);
        }
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
