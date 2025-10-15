using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float pushSpeed = 2f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            
                // Push in the wall's up direction
                Vector3 pushDirection = transform.right;
                rb.AddForce(pushDirection * pushSpeed, ForceMode.Force);          
        }
    }
}
