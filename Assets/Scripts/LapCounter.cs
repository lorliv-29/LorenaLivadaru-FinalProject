using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public ObstacleMapManager mapManager; // Assign in the Inspector
    public GameManager gameManager; // Assign in the Inspector
    public GameObject confettiPrefab; // Assign in the Inspector


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered by: " + other.name);

        if (other.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                gameManager.OnLapCompleted(); // Tell GameManager to count lap
            }
            if (mapManager != null)
            {
                mapManager.SwitchToNextLayout(); // Switch to the next layout
            }

        }

        if (confettiPrefab != null)
        {
            Instantiate(confettiPrefab, other.transform.position, Quaternion.identity);
            //Destroy(confettiPrefab, 2f); // destroy after 2 seconds
        }
    }
}