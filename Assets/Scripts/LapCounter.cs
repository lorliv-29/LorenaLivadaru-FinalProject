using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class LapCounter : MonoBehaviour
{
    public ObstacleMapManager mapManager;         // Assign in Inspector
    public GameManager gameManager;               // Assign in Inspector
    public GameObject confettiPrefab;             // Assign in Inspector
    public GameObject lapBarrier;                 // Single reusable barrier
    public float barrierDelay = 5f;               // Delay before re-enabling the barrier

    private bool isTriggered = false;
    public AudioClip confettiSound;
    public AudioSource audioSource; 

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isTriggered) return;

        isTriggered = true;

        Debug.Log("Trigger Entered by: " + other.name);

        gameManager.OnLapCompleted();

        if (confettiPrefab != null)
        { 
            Instantiate(confettiPrefab, other.transform.position, Quaternion.identity);
        if (audioSource != null && confettiSound != null)
        {
            audioSource.PlayOneShot(confettiSound, 0.7f); 
        }
        }

        // Deactivate the barrier temporarily
        if (lapBarrier != null)
        {
            lapBarrier.SetActive(false);
        }

        // Switch layout
        mapManager.SwitchToNextLayout();

        // Reactivate the barrier after delay
        StartCoroutine(ReenableBarrier());
    }

    IEnumerator ReenableBarrier()
    {
        yield return new WaitForSeconds(barrierDelay);

        if (lapBarrier != null)
        {
            lapBarrier.SetActive(true);
        }

        isTriggered = false; // Allow next lap
    }
}