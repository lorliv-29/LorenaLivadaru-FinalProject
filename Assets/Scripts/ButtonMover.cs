using UnityEngine;

public class ButtonMover : MonoBehaviour
{
    public Transform buttonCap;
    public Vector3 pushedPosition;
    private Vector3 startPosition;

    void Start()
    {
        // Remember where the button starts
        startPosition = buttonCap.localPosition;
    }

    // This is the function we will call from the Inspector
    public void MoveButtonDown()
    {
        buttonCap.localPosition = pushedPosition;
    }

    public void MoveButtonUp()
    {
        buttonCap.localPosition = startPosition;
    }
}