using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Camera mainCamera;
    public GameManager gameManager;
    public GameObject pickupEffectPrefab;

    [Header("Driving (Left Hand & Gaze)")]
    public InputActionReference throttleAction; // Left Grip (Squeeze for Speed)
    public InputActionReference moveAction;     // Left Stick (The Steering)
    public float maxSpeed = 10f;
    public float drag = 1f;

    [Header("Combat (Right Hand)")]
    public InputActionReference turretSqueezeAction; // Right Grip (Unlock Aim)
    public InputActionReference rightHandPosAction;  // Right Hand Aiming
    public InputActionReference interactAction;      // Right Interact (Fire)
    public Transform turretHinge;                    // The 'hinge' object
    public float turretTurnSpeed = 8f;

    [Header("Projectiles")]
    [SerializeField] private Transform projectileSpawnPoint; // Barrel tip
    public GameObject projectilePrefab;
    public float projectileForce = 1500f; // MASSIVE force for high speed

    private void OnEnable()
    {
        if (throttleAction != null) throttleAction.action.Enable();
        if (moveAction != null) moveAction.action.Enable();
        if (turretSqueezeAction != null) turretSqueezeAction.action.Enable();
        if (rightHandPosAction != null) rightHandPosAction.action.Enable();
        if (interactAction != null) interactAction.action.Enable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        rb.linearDamping = drag;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Stops the tank from tipping
    }

    void Update()
    {
        // EMERGENCY DEBUG: If you squeeze the right grip, this WILL show in the console
        float testSqueeze = turretSqueezeAction.action.ReadValue<float>();
        if (testSqueeze > 0.05f) Debug.Log($"<color=orange>TURRET SQUEEZE:</color> {testSqueeze}");

        if (!gameManager || !gameManager.IsGameStarted()) return;

        // --- TURRET MOVEMENT ---
        if (turretHinge != null)
        {
            if (testSqueeze > 0.5f)
            {
                // RESTORED: Uses hand position relative to the tank
                Vector3 handPos = rightHandPosAction.action.ReadValue<Vector3>();
                Vector3 aimDir = new Vector3(handPos.x, 0, handPos.z).normalized;

                if (aimDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(aimDir);
                    turretHinge.localRotation = Quaternion.Slerp(turretHinge.localRotation, targetRot, Time.deltaTime * turretTurnSpeed);
                }
            }
            else
            {
                // Returns turret to face forward when grip is released
                turretHinge.localRotation = Quaternion.Slerp(turretHinge.localRotation, Quaternion.identity, Time.deltaTime * turretTurnSpeed);
            }
        }

        if (interactAction.action.WasPressedThisFrame()) Shoot();
    }

    void FixedUpdate()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        // RESTORED: Thumbstick for Move
        float throttle = throttleAction.action.ReadValue<float>();
        Vector2 leftStick = moveAction.action.ReadValue<Vector2>();

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        // Direction based on Gaze + Thumbstick
        Vector3 moveDir = (camForward * leftStick.y) + (camRight * leftStick.x);
        if (leftStick.magnitude < 0.1f) moveDir = camForward;
        moveDir.Normalize();

        rb.linearVelocity = moveDir * (throttle * maxSpeed);

        if (throttle > 0.1f && moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 3f);
        }
    }

    void Shoot()
    {
        // 1. Spawn the projectile
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // 2. Clear self-collision so it doesn't "hit" the barrel and float
        Collider tankCollider = GetComponent<Collider>();
        Collider projCollider = proj.GetComponent<Collider>();
        if (tankCollider != null && projCollider != null) Physics.IgnoreCollision(tankCollider, projCollider);

        // 3. APPLY FORCE (If it floats, the force isn't high enough or Gravity is off)
        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.useGravity = false; // Keep it straight like a laser
            projRb.AddForce(projectileSpawnPoint.forward * projectileForce); // Speed applied
        }
        Destroy(proj, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            if (pickupEffectPrefab) Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);
            transform.localScale += Vector3.one * 0.1f;
            Destroy(other.gameObject);
        }
    }
}