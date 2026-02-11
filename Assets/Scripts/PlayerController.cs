using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Camera mainCamera;
    private Vector3 currentVelocity;

    [Header("Core References")]
    public GameManager gameManager;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Movement Tuning")]
    public float maxSpeed = 7f;
    public float acceleration = 12f;
    public float turnSpeed = 100f;
    public float hoverHeight = 1.0f;
    public float projectileForce = 30f;

    [Header("Hardware Links")]
    public VRLever throttleScript; // Link the object with VRLever.cs here
    public InputActionReference moveAction; // VR Controller Fallback
    public InputActionReference interactAction; // VR Trigger Fallback

    [Header("Cockpit Visuals")]
    public Transform cockpitJoystickHandle;
    public float maxVisualTilt = 20f;

    [Header("Diagnostics")]
    public bool showDebugLogs = true;

    // --- TANGIBLE BRIDGE (WEBSOCKET DATA) ---
    private float extX = 0;
    private float extY = 0;

    // This method is called by your WebSocketClientExample script
    public void UpdateExternalInput(float x, float y)
    {
        extX = x * -1f; // Inverts X to match tank steering
        extY = y;
    }

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
        if (characterController == null || !characterController.enabled) return;

        // Keep the VR capsule aligned with the player's head
        SyncCapsuleToHead();

        if (gameManager != null && gameManager.IsGameStarted())
        {
            HandleTankMovement();
            UpdateJoystickVisuals();
        }

        // Shooting via VR Controller Trigger
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleTankMovement()
    {
        Vector2 stickInput = new Vector2(extX, extY);

        // Fallback logic
        if (stickInput.magnitude < 0.05f && moveAction != null)
        {
            stickInput = moveAction.action.ReadValue<Vector2>();
        }

        // --- THE STABILITY FIX ---
        // If the lever script is missing or hasn't loaded, default to 0 (Safe) 
        // or 1 (Always move) depending on your preference.
        float throttle = 0f;
        if (throttleScript != null)
        {
            throttle = throttleScript.speedPercentage;
        }

        if (stickInput.magnitude > 0.1f)
        {
            // Rotation
            transform.Rotate(0, stickInput.x * turnSpeed * Time.deltaTime, 0);

            // Movement Calculation
            Vector3 targetMove = transform.forward * stickInput.y * (maxSpeed * throttle);

            // Use Move() directly for more consistent response than Lerp if it's lagging
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetMove, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        // --- THE "STUCK" FIX ---
        // CharacterController.Move expects World Space, and we need to ensure 
        // vertical force is always applied so the tank doesn't float.
        Vector3 finalMove = (currentVelocity + (Vector3.up * -9.81f)) * Time.deltaTime;
        characterController.Move(finalMove);
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
        if (projectileSpawnPoint == null || projectilePrefab == null) return;

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