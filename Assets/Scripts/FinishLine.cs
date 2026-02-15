using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is on the "PlayerBody" layer
        // We use LayerMask.NameToLayer to avoid hardcoding numbers
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            GameManager gm = FindObjectOfType<GameManager>();

            if (gm != null && !gm.IsGameOver())
            {
                Debug.Log("<color=green>Goal Reached!</color> Detected PlayerBody layer.");
                gm.GameOver();
            }
        }
    }
}