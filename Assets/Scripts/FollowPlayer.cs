using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;

    // OffseT position above the player
    private Vector3 offset = new Vector3(0f, 15f, 0f); 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
