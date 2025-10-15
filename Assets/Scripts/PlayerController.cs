using UnityEngine;
public class PlayerController : MonoBehaviour

{
    // ------------------ Core References ------------------

    private Rigidbody rb;                  // Player's Rigidbody for movement and recoil
    private Camera mainCamera;             // Main camera used for mouse aiming
    //private LineRenderer lineRenderer;     // Draws aiming line from player to mouse

    // Pivot point for aiming (assign in Inspector) 
    [SerializeField]
    private Transform aimPivot;

    // Point from which projectiles are spawned
    [SerializeField] 
    private Transform projectileSpawnPoint;

    // ------------------ Variables ------------------------

    public GameObject projectilePrefab;    // Prefab to instantiate as a projectile
    public float projectileForce = 4f;     // Reduced force for better control
    public float recoilForce = 2f;         // Lowered recoil for smoother gameplay
    public float speed = 10f;              // Player movement speed using WASD

    private Vector3 shootDirection;        // Shared direction used for both aiming and shooting

    // ------------------ Initialization -------------------

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Ensure aimPivot is assigned in the Inspector
        if (aimPivot == null)
        {
            Debug.LogError("AimPivot is not assigned in the Inspector.");
        }

        //lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.positionCount = 2; // Only need two points for a straight line
    }

    // ------------------ Per-Frame Updates ------------------

    void Update()
    {

        // 1. Get direction from player to mouse
        shootDirection = GetMouseDirection();

        // 2. Rotate the aim pivot to face the mouse
        UpdateAimingRotation(shootDirection);

        // 3. If left mouse button is clicked, shoot
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(shootDirection);
        }

        // Update aim line in real-time
        //UpdateLineRenderer(shootDirection);
    }

    // ------------------ Mouse Aiming Logic ------------------

    Vector3 GetMouseDirection()
    {
        //cast a ray from mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        //check if ray hits something
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 target = hit.point;

            // Flatten both target and player to same height
            target.y = aimPivot.position.y;

            // Direction from HINGE to mouse
            Vector3 dir = target - aimPivot.position;

            // If direction is nearly zero, default to forward
            if (dir.sqrMagnitude < 0.1f)
            {
                return aimPivot.forward;
            }

            return dir.normalized;
        }
        
        return aimPivot.forward; // Fallback: forward direction if ray doesn't hit anything
    }

    // ------------------ Visual Aim Line ----------------------

    //void UpdateLineRenderer(Vector3 dir)
    // {
    //Start the line at the player's current position
    //lineRenderer.SetPosition(0, transform.position);

    //End the line 3 units in the shoot direction
    //lineRenderer.SetPosition(1, transform.position + dir * 3f);
    // }


    void UpdateAimingRotation(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
        {
            // Get rotation that faces the direction
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            // Only rotate around Y axis for horizontal aiming
            aimPivot.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f); // Y only
        }
    }

    // ------------------ Shooting and Recoil ------------------

    void Shoot(Vector3 dir)
    {
        // Make sure direction is horizontal
        dir.y = 0;

        dir = dir.normalized;

        // Determine spawn position at the projectile spawn point
        Vector3 spawnPos = projectileSpawnPoint.position;

        spawnPos.y = 0f; // Force Y to 0

        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        Debug.Log("Projectile instantiated at: " + spawnPos);

           // Apply force to the projectile
            Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.useGravity = false; //  keep it flat
            projRb.AddForce(dir * projectileForce, ForceMode.Impulse);
            Destroy(projectile, 2f); // Auto-destroy after 2 seconds
        }

        // Apply recoil to the player in the opposite direction
        rb.AddForce(-dir * recoilForce, ForceMode.Impulse);

        // Visual debug ray to show shooting direction
        Debug.DrawRay(spawnPos, dir * 2f, Color.red, 3f);

        //Reduce player size by 2% on each shot
        transform.localScale *= 0.98f;
    }

    // ------------------ WASD Movement ------------------------

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);

     
    }

    // ------------------ Pickup Collision -----------------------
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            // Increase the player's size by 10%
            transform.localScale *= 1.1f;

            // Remove the pickup from the scene
            Destroy(other.gameObject);
        }
    }
}
