using UnityEngine;

public class SkullZoneTrigger : MonoBehaviour
{
    public AudioClip zoneMusic;          // Music to play when inside zone
    public AudioClip defaultMusic;       // Music to revert to when leaving
    public AudioSource musicSource;      // The AudioSource playing the music

    public Light sceneLight;             // Reference to the scene's main light
    public float darkIntensity = 0.2f;   // Intensity when in the zone
    private float originalIntensity;     // Original intensity to revert to
    private bool isInZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Change music to zone music
            if (zoneMusic != null && musicSource != null)
            {
                musicSource.clip = zoneMusic;
                musicSource.Play();
            }

            // Save the light's current state the first time we enter
            if (!isInZone && sceneLight != null)
            {
                originalIntensity = sceneLight.intensity;
                isInZone = true;
            }

            // Dim the lights
            if (sceneLight != null)
            {
                sceneLight.intensity = darkIntensity;
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Return to default music
            if (defaultMusic != null && musicSource != null)
            {
                musicSource.clip = defaultMusic;
                musicSource.Play();
            }

            // Restore lighting
            if (sceneLight != null)
            {
                sceneLight.intensity = originalIntensity;
            }
        }
    }
}
