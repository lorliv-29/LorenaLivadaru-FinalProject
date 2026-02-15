using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Debug 1: See exactly what object and layer is hitting the line
        Debug.Log($"<color=yellow>PHYSICS HIT:</color> {other.name} on Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        // Use a more robust check: CompareTag or check if the layer index matches
        // Ensure "Player" layer exists in your Tags & Layers settings!
        int playerLayer = LayerMask.NameToLayer("Player");

        if (other.gameObject.layer == playerLayer || other.CompareTag("MainCamera"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                if (!gm.IsGameOver())
                {
                    Debug.Log("<color=green>SUCCESS:</color> Finish Line crossed. Ending Race.");
                    gm.GameOver();
                }
            }
            else
            {
                Debug.LogError("CRITICAL: GameManager not found in scene!");
            }
        }
    }
}