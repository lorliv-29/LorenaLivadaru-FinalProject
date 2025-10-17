using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject BrickHitFBXPrefab; // Assign in Inspector
    void OnCollisionEnter(Collision col)
    {
        // Check if we hit a brick
        if (col.collider.CompareTag("Brick"))
        {
            if (BrickHitFBXPrefab != null)
            {

                Vector3 vfxPosition = col.transform.position;
                vfxPosition.y = 1f;
                // spawn it at the brick's position
                Instantiate(BrickHitFBXPrefab, vfxPosition, Quaternion.Euler(90f, 0f, 0f));
            }

            Destroy(col.collider.gameObject); // Destroy the brick

            // Always destroy the projectile on collision
            Destroy(gameObject);
        }

    
    }
}
