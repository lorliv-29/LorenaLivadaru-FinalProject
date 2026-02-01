using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------------ Core References (PRESERVED) ------------------
    private Rigidbody rb;
    private Camera mainCamera;
    public GameManager gameManager;
    public GameObject pickupEffectPrefab;

    [Header("Audio (PRESERVED)")]
    public AudioSource pickupAudio;
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("VR Input References")]
    public InputActionReference throttleAction; // Map to Left Grip
    public InputActionReference shootAction;    // Map to Right Trigger

    [Header("VR Movement Settings")]
    public float maxSpeed = 10f;
    public float drag = 1f;

    [Header("Aiming & Projectiles (PRESERVED)")]
    [SerializeField] private Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 4f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        rb.linearDamping = drag;

        // STABILITY: Keeps your eyes from spinning with the ball
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        // NEW VR LOCOMOTION: No shooting required to move!
        float throttle = throttleAction.action.ReadValue<float>();
        Vector3 moveDir = mainCamera.transform.forward;
        moveDir.y = 0; // The "Flat Rail" rule
        moveDir.Normalize();

        rb.linearVelocity = moveDir * (throttle * maxSpeed);

        if (throttle > 0.1f) transform.forward = moveDir;

        // DEBUG: Helps verify the simulator 'G' key is working
        // Debug.Log("Throttle: " + throttle);
    }

    void Update()
    {
        if (!gameManager || !gameManager.IsGameStarted()) return;

        // VR SHOOTING: Purely for bricks/gameplay
        if (shootAction.action.WasPressedThisFrame())
        {
            Shoot(mainCamera.transform.forward);
        }

        if (transform.localScale.x <= 0.5f && !gameManager.IsGameOver())
        {
            gameManager.GameOver();
        }
    }

    void Shoot(Vector3 dir)
    {
        dir.y = 0;
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(dir));

        // AUDIO (PRESERVED)
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
        // RECOIL REMOVED: To keep the VR camera stable.
    }

    // ------------------ INTERACTION LOGIC (PRESERVED) ------------------
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