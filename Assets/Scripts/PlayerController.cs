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
    public float projectileForce = 1500f; // MASSIVE force 

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
        //  squeeze the right grip+ debug
        float testSqueeze = turretSqueezeAction.action.ReadValue<float>();
        if (testSqueeze > 0.05f) Debug.Log($"<color=orange>TURRET SQUEEZE:</color> {testSqueeze}");

        if (!gameManager || !gameManager.IsGameStarted()) return;

        // --- TURRET MOVEMENT ---
        if (turretHinge != null)
        {
            if (testSqueeze > 0.5f)
            {
                //  Uses hand position relative to the tank
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

        // Thumbstick for Move
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
        // 1. Spawn the projectile at the muzzle position and rotation
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // 2. Clear self-collision so it doesn't "hit" the barrel and float
        Collider tankCollider = GetComponent<Collider>();
        Collider projCollider = proj.GetComponent<Collider>();
        if (tankCollider != null && projCollider != null) Physics.IgnoreCollision(tankCollider, projCollider);

        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = Vector3.zero; // Clear any physics "lag"

            // 3. APPLY FORCE (Using 'forward' ensures it follows the muzzle's blue arrow)
            // Set 'projectileForce' to at least 1500 in the Inspector for a fast shot
            projRb.AddForce(projectileSpawnPoint.forward * projectileForce);
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