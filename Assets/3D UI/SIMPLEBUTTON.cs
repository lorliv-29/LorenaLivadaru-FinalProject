using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SimpleCockpitButton : MonoBehaviour
{
    [Header("Components")]
    public Transform buttonCap; // The moving part of the button

    [Header("Settings")]
    public float pushDistance = 0.02f; // How far it moves down (in meters)
    public float returnSpeed = 5f;     // How fast it snaps back up

    [Header("Events")]
    public UnityEvent OnPressed;

    private Vector3 startPos;
    private bool isPressed = false;
    private XRSimpleInteractable interactable;

    void Start()
    {
        startPos = buttonCap.localPosition;
        interactable = GetComponent<XRSimpleInteractable>();

        // Ensure we have an interactable
        if (interactable == null) interactable = gameObject.AddComponent<XRSimpleInteractable>();
    }

    void Update()
    {
        // 1. Check if a hand is "hovering" (touching) the button
        if (interactable.isHovered)
        {
            // Move the cap toward the pushed position
            Vector3 targetPos = startPos + new Vector3(0, -pushDistance, 0);
            buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, targetPos, Time.deltaTime * 20f);

            // 2. Trigger event if we hit the bottom
            float dist = Vector3.Distance(buttonCap.localPosition, targetPos);
            if (dist < 0.005f && !isPressed)
            {
                isPressed = true;
                OnPressed.Invoke();
                Debug.Log("Button Pushed!");
            }
        }
        else
        {
            // 3. Reset position when hand leaves
            buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, startPos, Time.deltaTime * returnSpeed);
            isPressed = false;
        }
    }
}