using UnityEngine;

public class SliderThrottle : MonoBehaviour
{
    [Header("Track Reference Points")]
    public Transform startPoint; // Move this to the TOP of the track
    public Transform endPoint;   // Move this to the BOTTOM of the track

    [Header("Output Value")]
    [Range(0f, 1f)]
    public float speedPercentage; // This is what the Tank reads

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        // 1. Calculate the total physical length of the track
        float trackLength = Vector3.Distance(startPoint.position, endPoint.position);

        // 2. Calculate how far the handle is from the BOTTOM (endPoint)
        float distanceToBottom = Vector3.Distance(transform.position, endPoint.position);

        // 3. Map it: 
        // Handle at the Bottom (distance 0) -> speed 0.0
        // Handle at the Top (distance = trackLength) -> speed 1.0
        speedPercentage = Mathf.Clamp01(distanceToBottom / trackLength);
    }
}