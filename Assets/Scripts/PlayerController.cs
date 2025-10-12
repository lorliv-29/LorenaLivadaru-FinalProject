using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject projectilePrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player based on input
        if (Input.GetMouseButtonDown(0))
        {
            // Shoot a projectile
            Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        }
    }
}
