using UnityEngine;

public class AutomaticThrottle : MonoBehaviour
{
    [Header("Settings")]
    public float cycleDuration = 10f; // Time in seconds for a full speed cycle

    [Header("Output")]
    public float speedPercentage; // The PlayerController will read this

    void Update()
    {
        // This creates a smooth wave moving between 0.0 and 1.0 over time
        float wave = Mathf.Sin(Time.time * (2f * Mathf.PI / cycleDuration));

        // Map the -1 to 1 wave to a 0 to 1 range
        speedPercentage = (wave + 1f) / 2f;
    }
}