using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject[] obstaclePrefabs; // Array to obstacle prefabs
    public Transform[] spawnPositions; // Array to spawn positions

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Call reset method to randomize obstacles at start
        ResetObstacles();
    }

    public void ResetObstacles()
    {

        Debug.Log("ResetObstacles() was called!");

        for (int i = 0; i < obstaclePrefabs.Length && i < spawnPositions.Length; i++)
        {
            Instantiate(obstaclePrefabs[i], spawnPositions[i].position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
