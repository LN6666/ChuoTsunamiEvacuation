using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintSpeed = 14f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private bool logMovementDebug = true;
    [SerializeField] private bool fallbackTranslateIfControllerStuck = true;
    [SerializeField] private float movementLogInterval = 1f;

    [Header("Third-Person Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float cameraDistance = 9f;
    [SerializeField] private float cameraHeight = 4.5f;
    [SerializeField] private float mouseSensitivity = 2.5f;
    [SerializeField] private bool orbitCameraWithMouseDrag = true;
    [SerializeField] private bool enableKeyboardCameraDebugFallback = true;
    [SerializeField] private float keyboardCameraTurnSpeed = 90f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 65f;
    [SerializeField] private float initialPitch = 25f;
    [SerializeField] private bool lockCursorOnPlay;

    [Header("Debug")]
    public bool allowMovementDebugAlways = true;

    private CharacterController characterController;
    private float verticalVelocity;
    private float yaw;
    private float pitch;
    private bool controlEnabled = true;
    private string disabledByState = string.Empty;
    private float nextMovementLogTime;
    private float nextDisabledLogTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        EnsureCameraRig();

        yaw = transform.eulerAngles.y;
        pitch = Mathf.Clamp(initialPitch, minPitch, maxPitch);
    }

    private void Start()
    {
        if (lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UpdateCameraOrbit();

        if (controlEnabled)
        {
            UpdateMovement();
            return;
        }

        if (ReadMovementInput().sqrMagnitude > 0.01f && Time.time >= nextDisabledLogTime)
        {
            nextDisabledLogTime = Time.time + movementLogInterval;
            Debug.Log($"Movement input ignored because game state disabled control: {disabledByState}");
        }
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    public void SetControlEnabled(bool isEnabled)
    {
        SetControlEnabled(isEnabled, isEnabled ? string.Empty : "Unknown");
    }

    public void SetControlEnabled(bool isEnabled, string gameStateReason)
    {
        controlEnabled = isEnabled;
        disabledByState = isEnabled ? string.Empty : gameStateReason;

        if (!isEnabled)
        {
            verticalVelocity = 0f;
        }
    }

    private void EnsureCameraRig()
    {
        if (cameraPivot == null)
        {
            GameObject pivotObject = new GameObject("CameraPivot_Runtime");
            pivotObject.transform.SetParent(transform);
            pivotObject.transform.localPosition = Vector3.up * cameraHeight;
            cameraPivot = pivotObject.transform;
        }

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        if (cameraTransform != null && cameraTransform.parent != cameraPivot)
        {
            cameraTransform.SetParent(cameraPivot, false);
        }
    }

    private void UpdateCameraOrbit()
    {
        float mouseX = 0f;
        float mouseY = 0f;

        bool mouseDragOrbitActive = orbitCameraWithMouseDrag && (Input.GetMouseButton(0) || Input.GetMouseButton(1));
        if (lockCursorOnPlay || mouseDragOrbitActive)
        {
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");
        }

        if (enableKeyboardCameraDebugFallback && Input.GetKey(KeyCode.Q))
        {
            mouseX -= keyboardCameraTurnSpeed * Time.deltaTime / Mathf.Max(0.01f, mouseSensitivity);
        }

        if (enableKeyboardCameraDebugFallback && Input.GetKey(KeyCode.E))
        {
            mouseX += keyboardCameraTurnSpeed * Time.deltaTime / Mathf.Max(0.01f, mouseSensitivity);
        }

        yaw += mouseX * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - mouseY * mouseSensitivity, minPitch, maxPitch);
    }

    private void UpdateCameraPosition()
    {
        if (cameraPivot == null || cameraTransform == null)
        {
            return;
        }

        cameraPivot.position = transform.position + Vector3.up * cameraHeight;
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cameraTransform.localPosition = new Vector3(0f, 0f, -Mathf.Max(0.5f, cameraDistance));
        cameraTransform.localRotation = Quaternion.identity;
    }

    private void UpdateMovement()
    {
        Vector2 rawInput = ReadMovementInput();
        Vector2 input = Vector2.ClampMagnitude(rawInput, 1f);
        Vector3 horizontalMove = GetCameraRelativeMovement(input);
        bool sprintActive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = sprintActive ? sprintSpeed : moveSpeed;
        Vector3 velocity = horizontalMove * speed;
        Vector3 beforePosition = transform.position;

        if (characterController == null)
        {
            if (Time.time >= nextMovementLogTime)
            {
                nextMovementLogTime = Time.time + movementLogInterval;
                Debug.LogWarning($"{nameof(SimplePlayerController)} cannot move because CharacterController is missing.", this);
            }

            return;
        }

        if (!characterController.enabled)
        {
            if (Time.time >= nextMovementLogTime)
            {
                nextMovementLogTime = Time.time + movementLogInterval;
                Debug.LogWarning($"{nameof(SimplePlayerController)} cannot move because CharacterController is disabled.", this);
            }

            return;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
        Vector3 afterPosition = transform.position;

        if (fallbackTranslateIfControllerStuck &&
            horizontalMove.sqrMagnitude > 0.01f &&
            Vector3.Distance(beforePosition, afterPosition) < 0.0001f)
        {
            transform.position += horizontalMove * speed * Time.deltaTime;
            afterPosition = transform.position;

            if (logMovementDebug && Time.time >= nextMovementLogTime)
            {
                Debug.LogWarning("CharacterController.Move did not change Player position, so debug fallback translation was applied.", this);
            }
        }

        if (logMovementDebug && horizontalMove.sqrMagnitude > 0.01f && Time.time >= nextMovementLogTime)
        {
            nextMovementLogTime = Time.time + movementLogInterval;
            Debug.Log($"Player movement input is detected. input={input}, move={horizontalMove}, speed={speed}, sprint={sprintActive}, before={beforePosition}, after={afterPosition}");
        }

        Vector3 lookDirection = new Vector3(horizontalMove.x, 0f, horizontalMove.z);
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
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

        return new Vector2(x, y);
    }

    private Vector3 GetCameraRelativeMovement(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            return Vector3.zero;
        }

        Transform referenceTransform = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = referenceTransform.forward;
        Vector3 right = referenceTransform.right;
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
