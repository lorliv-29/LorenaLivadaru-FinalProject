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
        // Find the hand
        GameObject hand = GameObject.Find("RightHand");
        if (hand == null) return;

        // 1. Convert the hand's World position into the Slider's Local space
        // This makes the math 'relative' to the tank's current position
        Vector3 localHandPos = transform.parent.InverseTransformPoint(hand.transform.position);

        // 2. Do the math using Local coordinates
        Vector3 localStart = startPoint.localPosition;
        Vector3 localEnd = endPoint.localPosition;

        Vector3 line = localEnd - localStart;
        float length = line.magnitude;
        Vector3 dir = line.normalized;

        Vector3 v = localHandPos - localStart;
        float d = Vector3.Dot(v, dir);
        d = Mathf.Clamp(d, 0f, length);

        // 3. Apply to LocalPosition so it stays attached to the tank
        transform.localPosition = localStart + (dir * d);

        speedPercentage = d / length;
    }
}
