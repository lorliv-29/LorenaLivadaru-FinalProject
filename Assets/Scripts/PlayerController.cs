using UnityEngine;
public class PlayerController : MonoBehaviour

{
    // ------------------ Core References ------------------

    private Rigidbody rb;                  // Player's Rigidbody for movement and recoil
    private Camera mainCamera;             // Main camera used for mouse aiming
    private LineRenderer lineRenderer;     // Draws aiming line from player to mouse

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

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // Only need two points for a straight line
    }

    // ------------------ Per-Frame Updates ------------------

    void Update()
    {

        //check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // When clicked spawn a projectile at the player's position with no rotation.
            Shoot(shootDirection);
        }

        // Calculate direction from player to mouse once per frame
        shootDirection = GetMouseDirection();

        // Update aim line in real-time
        UpdateLineRenderer(shootDirection);
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
            target.y = transform.position.y;

            Vector3 dir = target - transform.position;

            // If direction is nearly zero, default to forward
            if (dir.magnitude < 0.1f)
            {
                return transform.forward;
            }

            return dir.normalized;
        }

        return transform.forward; // fallback
    }

    // ------------------ Visual Aim Line ----------------------

    void UpdateLineRenderer(Vector3 dir)
    {
        //Start the line at the player's current position
        lineRenderer.SetPosition(0, transform.position);

        //End the line 3 units in the shoot direction
        lineRenderer.SetPosition(1, transform.position + dir * 3f);
    }

    // ------------------ Shooting and Recoil ------------------

    void Shoot(Vector3 dir)
    {
        // Visual debug line for shooting direction
        Debug.DrawRay(transform.position, dir * 5f, Color.red, 2f);

        // Flatten the shoot direction
        dir.y = 0;

        // Slightly offset spawn upward to prevent clipping
        Vector3 spawnPos = transform.position + Vector3.up * 0.2f;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();

        // Apply projectile force 
        if (projRb != null)
        {
            projRb.AddForce(dir.normalized * projectileForce, ForceMode.Impulse);
            Destroy(projectile, 2f);
        }

        // Apply recoil in opposite direction
        rb.AddForce(-dir.normalized * recoilForce, ForceMode.Impulse);

        //Reduce player size by 5% on each shot
        transform.localScale *= 0.95f;
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
