using UnityEngine;

public class WindTunnelZone : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector3 windDirection = Vector3.forward; // The direction of the push
    public float windStrength = 5f;               // How hard it pushes

    // This runs while the player is standing inside the trigger
    private void OnTriggerStay(Collider other)
    {
        // Check if the object entering is the Player
        if (other.CompareTag("Player"))
        {
            // Try to find the PlayerController script on that object
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Calculate the push vector
                Vector3 force = windDirection.normalized * windStrength;

                // Send this force to the PlayerController's new ApplyExternalForce function
                player.ApplyExternalForce(force);
            }
        }
    }
}