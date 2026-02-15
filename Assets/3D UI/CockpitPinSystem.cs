using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class CockpitPinSystem : MonoBehaviour
{
    [Header("Cockpit Lighting")]
    public Light[] cockpitLights; // Drag all cockpit lights here in the Inspector
    public float targetIntensity = 1.0f;

    [Header("Security Settings")]
    public string correctPin = "1234";
    public TextMeshPro displayScreen;

    [Header("Events")]
    public UnityEvent OnAccessGranted;
    public UnityEvent OnAccessDenied;

    private string currentInput = "";
    private bool isPoweredOn = false; // Prevents interaction before hitting 'Start'

    void Start()
    {
        // Ensure everything starts in "Cold and Dark" mode
        foreach (Light l in cockpitLights) l.intensity = 0;
        displayScreen.gameObject.SetActive(false);
    }

    // STEP 1: Called by the UI "Start Experience" button
    public void PowerUpCockpit()
    {
        isPoweredOn = true;
        displayScreen.gameObject.SetActive(true);
        displayScreen.text = "ENTER PIN - PINCH";
        displayScreen.color = Color.white;
    }

    public void EnterNumber(string number)
    {
        if (!isPoweredOn) return; // Block input if UI hasn't been cleared

        if (currentInput.Length < 4)
        {
            currentInput += number;
            UpdateScreen();
        }

        if (currentInput.Length == 4)
        {
            CheckPin();
        }
    }

    private void CheckPin()
    {
        if (currentInput == correctPin)
        {
            displayScreen.text = "ENGINE ON";
            displayScreen.color = Color.green;

            // The "Ignition" moment - Lights on!
            foreach (Light l in cockpitLights)
            {
                l.intensity = targetIntensity;
            }

            OnAccessGranted.Invoke();

            // NEW: Clear the "ENGINE ON" text after 3 seconds
            Invoke("ClearScreenText", 3.0f);
        }
        else
        {
            displayScreen.text = "WRONG PIN";
            displayScreen.color = Color.red;
            OnAccessDenied.Invoke();
            Invoke("FullSystemReset", 1.5f);
        }
    }

    // NEW: Helper function to wipe the text
    private void ClearScreenText()
    {
        displayScreen.text = "";
        // If you want it to show something else like "READY", use:
        // displayScreen.text = "READY TO RACE";
    }

    public void DeleteNumber(string number)
    {
        int index = currentInput.IndexOf(number);
        if (index != -1)
        {
            currentInput = currentInput.Remove(index, 1);
            UpdateScreen();
        }
    }

    public void FullSystemReset()
    {
        currentInput = "";
        UpdateScreen();

        // Tell the LED to turn off during the reset
        WebSocketClientExample ws = FindObjectOfType<WebSocketClientExample>();
        if (ws != null)
        {
            ws.SendLedOFF();
        }

        // Reset the physical buttons
        PinButton[] allButtons = FindObjectsOfType<PinButton>();
        foreach (PinButton btn in allButtons)
        {
            btn.ForceReset();
        }
    }

    private void UpdateScreen()
    {
        displayScreen.text = currentInput;
        displayScreen.color = Color.white;
    }
}