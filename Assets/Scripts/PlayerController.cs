using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    public GameManager gameManager;
    public GameObject pickupEffectPrefab;

    [Header("Movement (Left Thumbstick)")]
    public InputActionReference moveAction;
    public float maxSpeed = 4f;

    [Header("Combat (Right Hand)")]
    public InputActionReference interactAction;
    public Transform projectileSpawnPoint; // Drag the 'MuzzleTip' empty object here

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public float projectileForce = 25f;

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (interactAction != null) interactAction.action.Enable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Bypass the start panel by forcing the game state to 'Started'
        if (gameManager != null) gameManager.StartGame();
    }

    void Update()
    {
        // 1. MOVEMENT: Handled every frame via CharacterController.Move
        HandleMovement();

        // 2. SHOOTING: Check for trigger press
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleMovement()
    {
        // Safety check for GameManager
        if (gameManager != null && !gameManager.IsGameStarted()) return;

        // Read the Vector2 input from the thumbstick
        Vector2 stickInput = moveAction.action.ReadValue<Vector2>();

        // Calculate direction relative to where the XR Origin is facing
        Vector3 moveDir = transform.forward * stickInput.y + transform.right * stickInput.x;

        // Character Controllers use .Move() and require Time.deltaTime to be smooth
        characterController.Move(moveDir * maxSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (projectileSpawnPoint == null) return;

        // Create the projectile at the tip's position and rotation
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // Tell the Character Controller (your body) to ignore the projectile's collider
        // This prevents the "sticky" effect where the orb hits your own body
        Collider projCollider = proj.GetComponent<Collider>();
        if (characterController != null && projCollider != null)
        {
            Physics.IgnoreCollision(characterController, projCollider);
        }

        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = Vector3.zero; // Clear any inherited velocity

            // LAUNCH: Follows the Blue Arrow (Z-axis) of the MuzzleTip in 3D space
            // This allows for up, down, and diagonal shooting
            projRb.AddForce(projectileSpawnPoint.forward * projectileForce, ForceMode.Impulse);
        }

        // Cleanup: Remove orb after 3 seconds
        Destroy(proj, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            if (pickupEffectPrefab) Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);

            // Grow mechanic: Scale up slightly when hitting a pickup
            transform.localScale += Vector3.one * 0.1f;
            Destroy(other.gameObject);
        }
    }
}