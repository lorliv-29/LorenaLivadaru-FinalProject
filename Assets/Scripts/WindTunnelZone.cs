using UnityEngine;

public class WindTunnelZone : MonoBehaviour
{
    public Vector3 windDirection = Vector3.up; // Direction of the wind
    public float windStrength = 10f; // Strength of the wind

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Zero Y to keep force flat if needed
                Vector3 flatDirection = new Vector3(windDirection.x, 0, windDirection.z).normalized;

                // Apply wind force to the player
                rb.AddForce(windDirection.normalized * windStrength, ForceMode.Acceleration);
            }
        }
    }

}
