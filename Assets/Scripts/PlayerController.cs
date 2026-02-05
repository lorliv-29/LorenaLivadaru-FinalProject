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
    public Transform projectileSpawnPoint; // Drag the 'MuzzleTip' here

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

        // Force-start the game logic immediately
        if (gameManager != null) gameManager.StartGame();
    }

    void Update()
    {
        // 1. MOVEMENT: Standard FPS-style thumbstick move
        HandleMovement();

        // 2. SHOOTING: Fire from the MuzzleTip
        if (interactAction.action.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    void HandleMovement()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        Vector2 stickInput = moveAction.action.ReadValue<Vector2>();

        // Move relative to the player's facing direction (Thumbstick only)
        Vector3 moveDir = transform.forward * stickInput.y + transform.right * stickInput.x;

        characterController.Move(moveDir * maxSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (projectileSpawnPoint == null) return;

        // 1. Spawn at the exact tip of your hand-held object
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = Vector3.zero;
            // 2. Launch forward along the MuzzleTip's Blue Axis
            projRb.AddForce(projectileSpawnPoint.forward * projectileForce, ForceMode.Impulse);
        }

        // Mechanic: Shrink slightly with every shot
        //transform.localScale *= 0.98f;

        Destroy(proj, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            if (pickupEffectPrefab) Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);

            // Mechanic: Grow when picking up items
            transform.localScale += Vector3.one * 0.1f;
            Destroy(other.gameObject);
        }
    }
}