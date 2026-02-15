using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PinButton : MonoBehaviour
{
    public Transform buttonCap;
    public float pushDistance = 0.015f;
    public int buttonValue;
    public CockpitPinSystem brain;

    private Vector3 startPos;
    private Vector3 pushedPos;
    private Vector3 targetPos; // Tracks where the button SHOULD be
    private bool isToggledDown = false;
    private XRSimpleInteractable interactable;

    public UnityEvent OnPressedDown;
    public UnityEvent OnPoppedUp;

    void Start()
    {
        // Safety check: ensure buttonCap is assigned
        if (buttonCap == null) buttonCap = transform;

        startPos = buttonCap.localPosition;
        // Pushes along the local Y axis. 
        pushedPos = startPos + new Vector3(0, -pushDistance, 0);
        targetPos = startPos;

        interactable = GetComponent<XRSimpleInteractable>();
    }

    void Update()
    {
        // This loop ensures the button actually slides smoothly
        // Using unscaledDeltaTime bypasses the GameManager's Time.timeScale = 0
        buttonCap.localPosition = Vector3.Lerp(buttonCap.localPosition, targetPos, Time.unscaledDeltaTime * 15f);
    }

    public void ToggleButton()
    {
        if (!isToggledDown)
        {
            isToggledDown = true;
            targetPos = pushedPos; // Tell Update() to move down

            if (brain != null)
            {
                brain.EnterNumber(buttonValue.ToString());
            }
            OnPressedDown.Invoke();
        }
        else
        {
            isToggledDown = false;
            targetPos = startPos; // Tell Update() to move up

            if (brain != null)
            {
                brain.DeleteNumber(buttonValue.ToString());
            }
            OnPoppedUp.Invoke();
        }
    }

    public void ForceReset()
    {
        isToggledDown = false;
        targetPos = startPos; // This tells the Update() loop to slide the button up
    }
}