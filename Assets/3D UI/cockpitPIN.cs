using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class CockpitPinSystem : MonoBehaviour
{
    [Header("Security Settings")]
    public string correctPin = "1234"; // Set your secret code here
    public TextMeshPro displayScreen; // Drag your cockpit screen text here

    [Header("Events")]
    public UnityEvent OnAccessGranted; // Link this to "Start Game" or "Enable Engine"
    public UnityEvent OnAccessDenied;  // Link this to a "Red Light" or Alarm sound

    private string currentInput = "";

    public void EnterNumber(string number)
    {
        if (currentInput.Length < 4)
        {
            currentInput += number;
            UpdateScreen();
        }

        // Auto-check once 4 digits are entered
        if (currentInput.Length == 4)
        {
            CheckPin();
        }
    }

    private void CheckPin()
    {
        if (currentInput == correctPin)
        {
            displayScreen.text = "ACCESS GRANTED";
            displayScreen.color = Color.green;
            OnAccessGranted.Invoke();
        }
        else
        {
            displayScreen.text = "WRONG PIN";
            displayScreen.color = Color.red;
            OnAccessDenied.Invoke();
            Invoke("ClearInput", 1.5f); // Clear the screen after a short delay
        }
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateScreen();
    }

    private void UpdateScreen()
    {
        // Shows asterisks (****) or the actual numbers
        displayScreen.text = currentInput;
        displayScreen.color = Color.white;
    }
}