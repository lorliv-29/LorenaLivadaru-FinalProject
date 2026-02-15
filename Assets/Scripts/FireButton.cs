using UnityEngine;
using UnityEngine.Events;

public class FireButtonSmash : MonoBehaviour
{
    public Transform buttonCap;
    public float pushDistance = 0.02f;
    public PlayerController tank;

    [Header("Smash Settings")]
    public string handTag = "GameController"; // Ensure your hands/controllers have this tag
    public float resetDelay = 0.2f; // How fast the button pops back up

    private Vector3 startPos;
    private Vector3 pushedPos;
    private bool isPressed = false;

    void Start()
    {
        startPos = buttonCap.localPosition;
        // Pushes down on Y. Adjust to Z if your button is on a wall!
        pushedPos = startPos + new Vector3(0, -pushDistance, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing hitting the button is a hand/controller
        if (!isPressed && (other.CompareTag(handTag) || other.name.Contains("Hand")))
        {
            Smash();
        }
    }

    void Smash()
    {
        isPressed = true;
        buttonCap.localPosition = pushedPos;

        if (tank != null) tank.Shoot();

        // Automatically pop back up after a short delay
        Invoke("ResetButton", resetDelay);
    }

    void ResetButton()
    {
        buttonCap.localPosition = startPos;
        isPressed = false;
    }
}