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
    public float turnSpeed = 100f;    // Increased for manual rotation
    public float hoverHeight = 1.0f;

    [Header("Combat Settings")]
    public InputActionReference interactAction;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 30f;

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
        // FIX: Safety check to stop "Inactive Controller" error
        if (characterController == null || !characterController.enabled || !gameObject.activeInHierarchy)
            return;

        // SCALE SAFETY: Prevents the AABB/Finite crash
        if (transform.localScale.x < 0.1f) transform.localScale = Vector3.one;

        SyncCapsuleToHead();

        if (gameManager != null && gameManager.IsGameStarted())
        {
            // The function name must match exactly here!
            HandleTankMovement();
        }

        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleTankMovement()
    {
        Vector2 stickInput = moveAction.action.ReadValue<Vector2>();

        // DECOUPLED MOVEMENT: Head direction is ignored for steering
        if (stickInput.magnitude > 0.1f)
        {
            // 1. ROTATION: Left/Right on stick rotates the TANK, not the view
            float rotationAmount = stickInput.x * turnSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);

            // 2. FORWARD: Pushing Forward moves the tank along its own forward axis
            Vector3 moveDir = transform.forward * stickInput.y * maxSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, moveDir, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        // HOVER & GRAVITY: Keeps the legs from snagging on the floor
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

    void SyncCapsuleToHead()
    {
        if (mainCamera == null) return;
        float headHeight = Mathf.Clamp(mainCamera.transform.localPosition.y, 1f, 2.5f);
        characterController.height = headHeight;
        Vector3 newCenter = mainCamera.transform.localPosition;
        newCenter.y = headHeight / 2f;
        characterController.center = newCenter;
    }

    void Shoot()
    {
        if (projectileSpawnPoint == null || projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            // Impulse follows the Muzzle's forward (for diagonal shots)
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