using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------------ Core References ------------------
    private Rigidbody rb;
    private Camera mainCamera;
    public GameManager gameManager;
    public GameObject pickupEffectPrefab;

    [Header("Audio")]
    public AudioSource pickupAudio;
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("VR Input References")]
    public InputActionReference throttleAction; // Left Grip (The "Gas Pedal")
    public InputActionReference shootAction;    // Right Trigger
    public InputActionReference moveAction;     // Left Joystick (Manual Steering)

    [Header("VR Movement Settings")]
    public float maxSpeed = 10f;
    public float drag = 1f;

    [Header("Aiming & Projectiles")]
    [SerializeField] private Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 4f;

    // --- MANUAL ACTIVATION ---
    private void OnEnable()
    {
        // Explicitly enable all actions to ensure they work in the Simulator
        if (throttleAction != null) throttleAction.action.Enable();
        if (shootAction != null) shootAction.action.Enable();
        if (moveAction != null) moveAction.action.Enable();

        Debug.Log("<color=green>SUCCESS:</color> VR Inputs Enabled!");
    }

    private void OnDisable()
    {
        if (throttleAction != null) throttleAction.action.Disable();
        if (shootAction != null) shootAction.action.Disable();
        if (moveAction != null) moveAction.action.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (mainCamera == null) Debug.LogError("No Main Camera found!");

        rb.linearDamping = drag;

        // Keep vision stable for VR: Stops the camera from spinning with the ball
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        float throttle = throttleAction.action.ReadValue<float>();
        Vector2 joystickInput = moveAction.action.ReadValue<Vector2>();

        // 1. Get Camera directions (The Compass)
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 2. Create the Move Direction with a Deadzone for Precision
        Vector3 moveDir;
        // Magnitude check: only use joystick if pushed more than 20%
        if (joystickInput.magnitude > 0.2f)
        {
            moveDir = (camForward * joystickInput.y) + (camRight * joystickInput.x);
        }
        else
        {
            moveDir = camForward; // Default to head-gaze direction
        }
        moveDir.Normalize();

        // 3. Apply Velocity
        rb.linearVelocity = moveDir * (throttle * maxSpeed);

        // 4. PRECISION ROTATION (Slerp)
        if (throttle > 0.1f && moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            // --- PRECISION TWEAK HERE ---
            // Changed from 10f to 3f for a much smoother, car-like turn.
            // If it's too slow now, try 4f or 5f.
            float turnSmoothness = 3f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSmoothness);
        }
    }

    void Update()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        // VR Shooting (Head Gaze Aiming)
        if (shootAction.action.WasPressedThisFrame())
        {
            Shoot(mainCamera.transform.forward);
        }
    }

    void Shoot(Vector3 dir)
    {
        dir.y = 0;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(dir));

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.useGravity = false;
            projRb.AddForce(dir * projectileForce, ForceMode.Impulse);
            Destroy(projectile, 2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);
            transform.localScale += Vector3.one * 0.1f;
            if (pickupAudio != null) pickupAudio.Play();
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("FireSpin"))
        {
            transform.localScale *= 0.7f;
        }
        else if (other.gameObject.CompareTag("SpeedPad"))
        {
            rb.AddForce(other.transform.forward * 30f, ForceMode.Impulse);
        }
    }
}