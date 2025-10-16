using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject[] obstacleInstances; // Array to obstacle prefabs
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

        // Create a copy of the spawn positions array to shuffle
        Transform[] shuffled = (Transform[])spawnPositions.Clone();

        // Shuffle the spawn positions array
        ShuffleSpawnPoints(shuffled);

        // Assign obstacles to the shuffled spawn positions
        for (int i = 0; i < obstacleInstances.Length && i < shuffled.Length; i++)
        {
            obstacleInstances[i].transform.SetPositionAndRotation(shuffled[i].position,shuffled[i].rotation);
        }
    }

    // Randomize the spawn positions
    void ShuffleSpawnPoints(Transform[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            Transform temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
