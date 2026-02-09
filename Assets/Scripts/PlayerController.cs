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

    // --- TANGIBLE BRIDGE VARIABLES ---
    private float extX = 0;
    private float extY = 0;

    // This allows the WebSocket script to feed in joystick data
    public void UpdateExternalInput(float x, float y)
    {
        extX = x * -1f;
        extY = y;
        if (x != 0 || y != 0) Debug.Log($"Tank received input: X={x}, Y={y}");
    }
    // ----------------------------------

    [Header("Combat Settings")]
    public InputActionReference interactAction;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 30f;

    [Header("Cockpit Visuals")]
    public Transform cockpitJoystickHandle; // Drag the 3D stick handle here
    public float maxVisualTilt = 20f; // Degrees it can tilt

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

        if (transform.localScale.x < 0.1f) transform.localScale = Vector3.one;

        SyncCapsuleToHead();

        if (gameManager != null && gameManager.IsGameStarted())
        {
            HandleTankMovement();

            // --- ADD THIS LINE HERE ---
            UpdateJoystickVisuals();
        }

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleTankMovement()
    {
        // 1. INPUT SOURCE: Use ESP32 values if they exist, otherwise fallback to VR controllers
        Vector2 stickInput = new Vector2(extX, extY);

        if (stickInput.magnitude < 0.05f && moveAction != null)
        {
            stickInput = moveAction.action.ReadValue<Vector2>();
        }

        // 2. ROTATION: Decoupled from head view
        if (stickInput.magnitude > 0.1f)
        {
            float rotationAmount = stickInput.x * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);

            Vector3 moveDir = transform.forward * stickInput.y * maxSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, moveDir, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        // 3. GRAVITY & HOVER
        float vertical = -9.81f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, hoverHeight + 0.2f))
        {
            float error = hoverHeight - hit.distance;
            vertical = error * 15f;
        }

        Vector3 finalMove = currentVelocity + (Vector3.up * vertical);
        characterController.Move(finalMove * Time.deltaTime);
    }

    void UpdateJoystickVisuals()
    {
        if (cockpitJoystickHandle != null)
        {
            // Calculate tilt: Vertical moves X axis, Horizontal moves Z axis
            // We use -extX because tilting the stick right usually means a negative Z rotation in Unity
            float tiltX = extY * maxVisualTilt;
            float tiltZ = -extX * maxVisualTilt;

            // Apply the rotation smoothly
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

    // UPDATED: Now 'public' so the WebSocket script can call it from the ESP32 button
    public void Shoot()
    {
        if (projectileSpawnPoint == null || projectileSpawnPoint == this.transform)
        {
            Debug.LogError("Projectile Spawn Point is missing or assigned to the Player itself!");
            return;
        }

        Vector3 spawnPosition = projectileSpawnPoint.position;
        Quaternion spawnRotation = projectileSpawnPoint.rotation;

        GameObject proj = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(projectileSpawnPoint.forward * projectileForce, ForceMode.Impulse);
        }

        Destroy(proj, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            if (pickupEffectPrefab) Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);
            if (transform.localScale.x < 3.0f) transform.localScale += Vector3.one * 0.1f;
            Destroy(other.gameObject);
        }
    }
}