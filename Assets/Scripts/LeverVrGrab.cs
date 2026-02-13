using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Added this
using UnityEngine.XR.Interaction.Toolkit.Interactors;    // Added this

// Removed the namespace to make it easy for your PlayerController to find
public class LeverVrGrab : XRBaseInteractable
{
    [Header("Lever Settings")]
    public Transform handle;
    public float minAngle = -45f;
    public float maxAngle = 45f;

    [Header("Output")]
    public float speedPercentage; // This is what your Tank script reads

    private IXRSelectInteractor grabbingInteractor;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        grabbingInteractor = args.interactorObject;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        grabbingInteractor = null;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
        {
            UpdateLeverPosition();
        }
    }

    private void UpdateLeverPosition()
    {
        // Since you 'Assigned' the parent, we read the rotation of the handle (Child)
        if (handle == null) return;

        // 1. Get the rotation from the child (the one with the Hinge)
        float currentAngle = handle.localEulerAngles.x;

        // 2. Fix Unity's 0-360 degree wrap-around
        if (currentAngle > 180) currentAngle -= 360;

        // 3. Update the speed variable that the PlayerController reads
        // This maps your -45 to 45 degree tilt into 0.0 to 1.0 speed
        speedPercentage = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
    }
}