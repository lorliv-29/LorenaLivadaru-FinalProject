using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject projectilePrefab;
    public float speed = 10f;


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

    void FixedUpdate()
    {
        // Get input axes
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        // Create a movement vector
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        // Apply the movement to the Rigidbody
        rb.AddForce(movement * speed);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            // Increase the player's size by 10%
            transform.localScale *= 1.1f;
            // Destroy the power-up object
            Destroy(other.gameObject);
        }
    }

}

