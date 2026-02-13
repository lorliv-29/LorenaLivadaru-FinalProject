using UnityEngine;

public class CockpitSlider : MonoBehaviour
{
    [Header("Bounds")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Output")]
    public float speedPercentage;

    void Update()
    {
        // 1. Get the direction of the track
        Vector3 lineVec = endPoint.position - startPoint.position;
        float lineLength = lineVec.magnitude;
        Vector3 lineUnitVec = lineVec.normalized;

        // 2. Project the handle's current position onto that line
        // This 'snaps' the handle back to the track
        Vector3 participantVec = transform.position - startPoint.position;
        float dotProduct = Vector3.Dot(participantVec, lineUnitVec);
        dotProduct = Mathf.Clamp(dotProduct, 0f, lineLength);

        // 3. Apply the locked position
        transform.position = startPoint.position + (lineUnitVec * dotProduct);

        // 4. Calculate 0-1 speed
        speedPercentage = dotProduct / lineLength;
    }
}