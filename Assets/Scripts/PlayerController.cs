using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Camera mainCamera;
    private Vector3 currentVelocity;

    public GameManager gameManager;
    public GameObject pickupEffectPrefab;

    [Header("Movement Settings")]
    public InputActionReference moveAction;
    public float maxSpeed = 7f;
    public float acceleration = 12f;
    public float turnSpeed = 100f;
    public float hoverHeight = 1.0f;

    [Header("VR Throttle")]
    public Transform throttleLever; // The 3D Lever handle
    public float minLeverAngle = -45f;
    public float maxLeverAngle = 45f;
    private float throttleMultiplier = 0f; // Starts at 0 (Stopped)

    // --- TANGIBLE BRIDGE VARIABLES ---
    private float extX = 0;
    private float extY = 0;

    public void UpdateExternalInput(float x, float y)
    {
        extX = x * -1f;
        extY = y;
    }

    [Header("Combat Settings")]
    public InputActionReference interactAction;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 30f;

    [Header("Cockpit Visuals")]
    public Transform cockpitJoystickHandle;
    public float maxVisualTilt = 20f;

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (interactAction != null) interactAction.action.Enable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        if (gameManager != null) gameManager.StartGame();
    }

    void Update()
    {
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
            return;

        SyncCapsuleToHead();

        if (gameManager != null && gameManager.IsGameStarted())
        {
            // Calculate the speed multiplier from the VR lever first
            CalculateThrottleFromLever();

            // Move the tank
            HandleTankMovement();

            // Tilt the cockpit joystick based on ESP32 data
            UpdateJoystickVisuals();
        }

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleTankMovement()
    {
        Vector2 stickInput = new Vector2(extX, extY);

        // Fallback to VR Controllers
        if (stickInput.magnitude < 0.05f && moveAction != null)
        {
            stickInput = moveAction.action.ReadValue<Vector2>();
        }

        if (stickInput.magnitude > 0.1f)
        {
            // 1. ROTATION (Works regardless of lever)
            float rotationAmount = stickInput.x * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);

            // 2. SCALE MULTIPLIER (For the Shrink Mechanic)
            // If your tank is at 0.1 scale, you move 10x slower.
            float scaleMultiplier = transform.localScale.x;

            // 3. FORWARD MOVEMENT
            // Ensure throttleMultiplier isn't 0 during testing! 
            // For a quick fix, you can change 'throttleMultiplier' to '1f' in Start()
            Vector3 moveDir = transform.forward * stickInput.y * (maxSpeed * throttleMultiplier * scaleMultiplier);

            currentVelocity = Vector3.Lerp(currentVelocity, moveDir, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        // Apply movement via CharacterController
        float vertical = -9.81f; // Simple gravity
        Vector3 finalMove = currentVelocity + (Vector3.up * vertical);
        characterController.Move(finalMove * Time.deltaTime);
    }

    void UpdateJoystickVisuals()
    {
        if (cockpitJoystickHandle != null)
        {
            float tiltX = extY * maxVisualTilt;
            float tiltZ = -extX * maxVisualTilt;
            cockpitJoystickHandle.localRotation = Quaternion.Euler(tiltX, 0, tiltZ);
        }
    }

    void CalculateThrottleFromLever()
    {
        if (throttleLever == null) return;

        float currentAngle = throttleLever.localEulerAngles.x;
        if (currentAngle > 180) currentAngle -= 360;

        float rawThrottle = Mathf.InverseLerp(minLeverAngle, maxLeverAngle, currentAngle);

        // Digital Snap: 0% / 50% / 100%
        if (rawThrottle < 0.2f) throttleMultiplier = 0f;
        else if (rawThrottle < 0.7f) throttleMultiplier = 0.5f;
        else throttleMultiplier = 1f;
    }

    void SyncCapsuleToHead()
    {
        if (mainCamera == null) return;
        float headHeight = Mathf.Clamp(mainCamera.transform.localPosition.y, 1f, 2.5f);
        characterController.height = headHeight;
        Vector3 newCenter = mainCamera.transform.localPosition;
        newCenter.y = headHeight / 2f;
        characterController.center = newCenter;
    }

    public void Shoot()
    {
        if (projectileSpawnPoint == null) return;
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(projectileSpawnPoint.forward * projectileForce, ForceMode.Impulse);
        }
        Destroy(proj, 3f);
    }
}