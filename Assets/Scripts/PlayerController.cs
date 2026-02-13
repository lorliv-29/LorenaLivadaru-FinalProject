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
    // --- LEVER BYPASS ---
    // Commented out the lever reference for now
    // public SliderThrottle throttleScript; 

    // Using the Automatic Speed script as the bridge
    public AutomaticThrottle throttleScript;

    public InputActionReference moveAction;
    public InputActionReference interactAction;

    [Header("Cockpit Visuals")]
    public Transform cockpitJoystickHandle;
    public float maxVisualTilt = 20f;

    [Header("Diagnostics")]
    public bool showDebugLogs = true;

    // --- TANGIBLE BRIDGE (WEBSOCKET DATA) ---
    private float extX = 0;
    private float extY = 0;

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
    }

    void FixedUpdate()
    {
        /* --- BYPASSING DIRECT LEVER MOVEMENT ---
        // We are moving the lever logic into HandleTankMovement() 
        // to keep everything inside the CharacterController system.
        
        if (throttleScript != null)
        {
            float finalSpeed = throttleScript.speedPercentage * maxSpeed;
            transform.position += transform.forward * finalSpeed * Time.fixedDeltaTime;
        }
        */
    }

    void Update()
    {
        if (characterController == null || !characterController.enabled) return;

        SyncCapsuleToHead();

        // Ensure the game manager check doesn't block testing if null
        if (gameManager == null || gameManager.IsGameStarted())
        {
            HandleTankMovement();
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

        // Fallback to VR Controllers if WebSocket isn't sending data
        if (stickInput.magnitude < 0.05f && moveAction != null)
        {
            stickInput = moveAction.action.ReadValue<Vector2>();
        }

        // --- AUTOMATIC THROTTLE LOGIC ---
        float throttle = 0f;
        if (throttleScript != null)
        {
            throttle = throttleScript.speedPercentage;
        }
        else
        {
            // If the script is missing, we default to 0.5 so you can still move
            throttle = 0.5f;
        }

        // Apply Rotation
        if (Mathf.Abs(stickInput.x) > 0.1f)
        {
            transform.Rotate(0, stickInput.x * turnSpeed * Time.deltaTime, 0);
        }

        // Apply Forward Movement
        // Note: stickInput.y handles controller forward, throttle handles the 'cruise' speed
        float forwardInput = (Mathf.Abs(stickInput.y) > 0.1f) ? stickInput.y : 1.0f;
        Vector3 targetMove = transform.forward * forwardInput * (maxSpeed * throttle);

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetMove, acceleration * Time.deltaTime);

        // Apply gravity and Move
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