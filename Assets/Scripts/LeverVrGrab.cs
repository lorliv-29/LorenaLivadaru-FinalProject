using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Removed the namespace to make it easy for your PlayerController to find
public class VRLever : XRBaseInteractable
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
        if (grabbingInteractor == null || handle == null) return;

        // Math to find the angle between the base and your hand
        Vector3 worldPoint = grabbingInteractor.GetAttachTransform(this).position;
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        // Calculate angle on the Z/Y plane (forward/back)
        float angle = Mathf.Atan2(localPoint.z, localPoint.y) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        // Apply rotation to the handle
        handle.localRotation = Quaternion.Euler(angle, 0, 0);

        // Convert angle to 0-1 percentage for the tank speed
        speedPercentage = Mathf.InverseLerp(minAngle, maxAngle, angle);
    }
}