using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    private float maxDistance = 100f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate distance from projectile to camera
        Vector3 howFar = transform.position - Camera.main.transform.position;

        // Get lenght of vector ( magnitude )
        float distance = howFar.magnitude;

        if (distance > maxDistance)
        {
            Destroy(gameObject);
        }

        Debug.Log("Projectile destroyed at: " + transform.position);

    }
}
