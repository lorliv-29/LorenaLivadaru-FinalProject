using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;

    // OffseT position above the player
    private Vector3 offset; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculate the offset
        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void LateUpdate()
    {
        // Follow the player with an offset
        transform.position = player.transform.position + offset;
    }
}
