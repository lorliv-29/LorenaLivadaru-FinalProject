using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PermanentButton : MonoBehaviour
{
    public Transform buttonCap;
    public float pushDistance = 0.02f;
    public UnityEvent OnFirstPressed;

    private Vector3 startPos;
    private Vector3 pushedPos;
    private bool isPermanentlyDown = false;
    private XRSimpleInteractable interactable;

    void Start()
    {
        startPos = buttonCap.localPosition;
        pushedPos = startPos + new Vector3(0, -pushDistance, 0);
        interactable = GetComponent<XRSimpleInteractable>();

        if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();
    }

    void Update()
    {
        // If it's already down, stop all logic. It stays there forever.
        if (isPermanentlyDown) return;

        if (interactable.isHovered)
        {
            // Smoothly move to pushed position
            buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, pushedPos, Time.deltaTime * 20f);

            // Check if we reached the bottom
            if (Vector3.Distance(buttonCap.localPosition, pushedPos) < 0.001f)
            {
                StayDown();
            }
        }
        else
        {
            // If not hovered and NOT yet locked down, return to start
            buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, startPos, Time.deltaTime * 5f);
        }
    }

    void StayDown()
    {
        isPermanentlyDown = true;
        buttonCap.localPosition = pushedPos; // Snap exactly to position

        // Disable the interactable so it can't be "hovered" anymore
        interactable.enabled = false;

        OnFirstPressed.Invoke();
        Debug.Log("Button is now locked down permanently.");
    }
}