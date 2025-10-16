using UnityEngine;

public class LapCounter : MonoBehaviour
{

    public int currentLap = 0;
    public ObstacleMapManager mapManager;


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
        if (other.CompareTag("Player"))
        {
            currentLap++;

            Debug.Log("Lap Completed! Current lap: " + currentLap);

            if (mapManager != null)
            {
                mapManager.SwitchToNextLayout(); // Switch to the next layout
            }

        }
    }
}