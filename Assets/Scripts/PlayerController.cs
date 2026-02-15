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
    public float maxSpeed = 15f;       // Increased for manual control feel
    public float offTrackSpeed = 4f;
    public float acceleration = 15f;    // How fast you reach top speed
    public float deceleration = 10f;    // How fast you stop when letting go
    public float turnSpeed = 100f;
   

    [Header("Track Detection")]
    public LayerMask trackLayer;
    public float raycastDistance = 2.0f;

    [Header("Hardware Links")]
    // We keep this reference just in case, but we won't use it for movement
    public AutomaticThrottle throttleScript;
    public InputActionReference moveAction;
    public InputActionReference interactAction;

    [Header("Cockpit Visuals")]
    public Transform cockpitJoystickHandle;
    public float maxVisualTilt = 20f;

    [Header("Ballistics")]
    public float projectileForce = 30f;
    public float muzzleOffset = 0.5f; // NEW: Positive moves the spawn forward, negative moves it back

    [Header("Pickup Feedback")]
    public GameObject pickupVFX;
    public AudioSource pickupAudioSource;

    private Vector3 externalForce;
    private float extX = 0;
    private float extY = 0;
    private bool isOnTrack = true;

    public void ApplyExternalForce(Vector3 force)
    {
        externalForce = force;
    }

    public void UpdateExternalInput(float x, float y)
    {
        extX = x * -1f; // Steering
        extY = y;       // THIS is now your gas pedal
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

    void Update()
    {
        if (characterController == null || !characterController.enabled) return;

        SyncCapsuleToHead();
        CheckSurface();

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

    void CheckSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, raycastDistance, trackLayer))
        {
            isOnTrack = true;
        }
        else
        {
            isOnTrack = false;
        }
    }

    void HandleTankMovement()
    {
        // 1. Get Input (Priority to External WebSocket input, fallback to VR controller)
        Vector2 stickInput = new Vector2(extX, extY);

        if (stickInput.magnitude < 0.05f && moveAction != null)
        {
            stickInput = moveAction.action.ReadValue<Vector2>();
        }

        // 2. Manual Rotation
        if (Mathf.Abs(stickInput.x) > 0.1f)
        {
            transform.Rotate(0, stickInput.x * turnSpeed * Time.deltaTime, 0);
        }

        // 3. Manual Acceleration Logic
        // We use stickInput.y directly. Push up = Forward, Pull back = Reverse.
        float currentMax = isOnTrack ? maxSpeed : offTrackSpeed;

        // Calculate the direction we WANT to go based on the stick
        Vector3 targetMove = transform.forward * stickInput.y * currentMax;

        // Determine if we are accelerating or braking to apply the right feel
        float lerpSpeed = (Mathf.Abs(stickInput.y) > 0.05f) ? acceleration : deceleration;

        // Smoothly move our current velocity toward the target stick input
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetMove, lerpSpeed * Time.deltaTime);

        // 4. Final Execution
        Vector3 finalMove = (currentVelocity + externalForce + (Vector3.up * -9.81f)) * Time.deltaTime;
        characterController.Move(finalMove);

        externalForce = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            if (pickupVFX != null) Instantiate(pickupVFX, other.transform.position, Quaternion.identity);
            if (pickupAudioSource != null) pickupAudioSource.Play();
            Destroy(other.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Wall collision logic can go here
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

        Vector3 spawnPos = projectileSpawnPoint.position + (projectileSpawnPoint.forward * muzzleOffset);
        GameObject proj = Instantiate(projectilePrefab, spawnPos, projectileSpawnPoint.rotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset velocity
            rb.linearVelocity = Vector3.zero;

            // NEW: Add the tank's current speed to the projectile force
            // This ensures the bullet always moves away from you at the same relative speed
            float speedBoost = currentVelocity.magnitude;
            rb.AddForce(projectileSpawnPoint.forward * (projectileForce + speedBoost), ForceMode.Impulse);
        }

        Destroy(proj, 3f);
    }
}