using UnityEngine;

public class VRLever : MonoBehaviour
{
    private HingeJoint hinge;
    public float speedPercentage;

    void Awake() // Awake runs before Start, making it ready sooner
    {
        hinge = GetComponent<HingeJoint>();
    }

    void Update()
    {
        if (hinge == null) return;

        // Force the hinge to stay within your -60/60 limits
        float angle = hinge.angle;
        float min = hinge.limits.min;
        float max = hinge.limits.max;

        // If for some reason min/max are the same, avoid dividing by zero
        if (Mathf.Abs(max - min) < 0.01f)
        {
            speedPercentage = 0;
            return;
        }

        speedPercentage = Mathf.InverseLerp(min, max, angle);
    }
}