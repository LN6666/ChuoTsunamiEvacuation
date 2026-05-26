using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class NewMapPlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float mouseSensitivity = 2.5f;
    [SerializeField] private float keyboardTurnSpeed = 90f;
    [SerializeField] private float cameraDistance = 8f;
    [SerializeField] private float cameraHeight = 3.2f;
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float fallRecoveryDistance = 20f;

    private CharacterController characterController;
    private NewMapGameMode currentMode = NewMapGameMode.None;
    private NewMapWeatherPreset currentWeather = NewMapWeatherPreset.ClearDay;
    private float verticalVelocity;
    private float yaw;
    private float pitch = 25f;
    private bool controlEnabled;
    private Vector3 lastValidGroundPosition;
    private float stamina = 100f;

    public float WalkSpeedMetersPerSecond { get; private set; }
    public float SprintSpeedMetersPerSecond { get; private set; }
    public bool StaminaEnabled => currentMode == NewMapGameMode.Evacuation;
    public float Stamina => stamina;
    public float Stamina01 => Mathf.Clamp01(stamina / 100f);
    public NewMapGameMode CurrentMode => currentMode;
    public NewMapWeatherPreset CurrentWeather => currentWeather;
    public bool ControlEnabled => controlEnabled;
    public bool HasActiveCamera => cameraTransform != null && cameraTransform.GetComponent<Camera>() != null;
    public Vector3 LastValidGroundPosition => lastValidGroundPosition;

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
        if (!enabled)
        {
            verticalVelocity = 0f;
        }
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
            camera.tag = "MainCamera";
        }

        cameraTransform = camera.transform;
        cameraTransform.SetParent(cameraPivot, false);
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 5000f;
    }

    private void UpdateCameraOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, minPitch, maxPitch);
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
