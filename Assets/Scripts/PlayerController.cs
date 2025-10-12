using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject projectilePrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player based on input
        if (Input.GetMouseButtonDown(0))
        {
            // When clicked spawn a projectile at the player's position with no rotation.
            Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Reduce the player's size by 5%
            transform.localScale *= 0.95f;

        }
    }
}
