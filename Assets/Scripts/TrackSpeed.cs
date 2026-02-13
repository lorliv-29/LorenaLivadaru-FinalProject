using UnityEngine;

public class SurfaceSpeedController : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask trackLayer;      // Select the "Track" layer in Inspector
    public float rayDistance = 2.0f;
    public float offRoadSpeedMult = 0.3f; // 30% speed when off-track

    [Header("References")]
    public PlayerController playerScript; // Drag XR Origin here

    private float originalMaxSpeed;
    private bool isOffRoad = false;

    void Start()
    {
        if (playerScript != null)
        {
            // Capture the 7f speed you set in the PlayerController
            originalMaxSpeed = playerScript.maxSpeed;
        }
        else
        {
            Debug.LogError("Please drag the PlayerController into the SurfaceSpeedController slot!");
        }
    }

    void FixedUpdate()
    {
        if (playerScript == null) return;

        RaycastHit hit;
        // Shoot ray down from center of tank
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, rayDistance))
        {
            // If we hit something that is NOT on the track layer
            if (((1 << hit.collider.gameObject.layer) & trackLayer) == 0)
            {
                if (!isOffRoad) SetOffRoad();
            }
            else
            {
                if (isOffRoad) SetOnRoad();
            }
        }
        else
        {
            // If hitting nothing (falling), assume off-road
            if (!isOffRoad) SetOffRoad();
        }
    }

    void SetOffRoad()
    {
        isOffRoad = true;
        playerScript.maxSpeed = originalMaxSpeed * offRoadSpeedMult;
        Debug.Log("<color=orange>OFF TRACK: Speed Reduced!</color>");
    }

    void SetOnRoad()
    {
        isOffRoad = false;
        playerScript.maxSpeed = originalMaxSpeed;
        Debug.Log("<color=green>ON TRACK: Full Speed!</color>");
    }

    void OnDrawGizmos()
    {
        // Visual aid for the Scene View
        Gizmos.color = isOffRoad ? Color.red : Color.green;
        Vector3 start = transform.position + Vector3.up;
        Gizmos.DrawLine(start, start + (Vector3.down * rayDistance));
    }
}