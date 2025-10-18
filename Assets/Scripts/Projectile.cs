using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject BrickHitFBXPrefab; // Assign in Inspector
   // public float floatForce = 10f;
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

            // Find the "InnerBricks" group in the scene
           // GameObject innerGroup = GameObject.Find("InnerBricks");

            //if (innerGroup != null)
            //{
                // Apply upward force to each inner brick
               // foreach (Transform t in innerGroup.transform)
               // {
                    // Check if the inner brick has the correct tag
                  //  if (t.CompareTag("InnerBrick"))
                  //  {
                     //   Rigidbody rb = t.GetComponent<Rigidbody>();

                        // Apply upward force
                      //  if (rb != null)
                      //  {
                        //    rb.WakeUp(); // Ensure the Rigidbody is active
                       //     rb.useGravity = false;
                       //     Vector3 explosionForce = new Vector3(Random.Range(-1f, 1f), Random.Range(2f, 4f), Random.Range(-1f, 1f));
                       //     rb.AddForce(explosionForce * floatForce, ForceMode.Impulse);
                      //  }
                  //  }
               // }

              
           // }

            Destroy(col.collider.gameObject); // Destroy the brick

            // Always destroy the projectile on collision
            Destroy(gameObject);


        }
    }
}
