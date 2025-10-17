using UnityEngine;

public class Projectile : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        // Check if we hit a brick
        if (col.collider.CompareTag("Brick"))
        {
            Destroy(col.collider.gameObject); // Destroy the brick
        }

        // Always destroy the projectile on collision
        Destroy(gameObject);
    }
}
