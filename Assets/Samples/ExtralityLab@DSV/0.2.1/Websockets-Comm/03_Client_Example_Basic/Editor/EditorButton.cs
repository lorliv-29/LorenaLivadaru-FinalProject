using UnityEngine;
using UnityEditor;

// This script draws the buttons in the Inspector for WebSocketClientExample
[CustomEditor(typeof(WebSocketClientExample))]
public class EditorButton : Editor
{
    public override void OnInspectorGUI()
    {
        // Draws the default variables (IP, Port, Player Script, etc.)
        DrawDefaultInspector();

        WebSocketClientExample myScript = (WebSocketClientExample)target;

        GUILayout.Space(15);
        GUILayout.Label("SHIP MOVEMENT TESTS", EditorStyles.boldLabel);

        // Use RepeatButton so the ship keeps moving while you hold the click
        if (GUILayout.RepeatButton("FORWARD ↑")) myScript.TestForward();
        if (GUILayout.RepeatButton("BACKWARD ↓")) myScript.TestBackward();
        if (GUILayout.RepeatButton("LEFT ←")) myScript.TestLeft();
        if (GUILayout.RepeatButton("RIGHT →")) myScript.TestRight();

        if (GUILayout.Button("STOP / CENTER")) myScript.TestStop();

        GUILayout.Space(15);
        GUILayout.Label("ESP32 HARDWARE TESTS", EditorStyles.boldLabel);

        if (GUILayout.Button("Send Test Message")) myScript.SendHello();
        if (GUILayout.Button("Send LED ON")) myScript.SendLedON();
        if (GUILayout.Button("Send LED OFF")) myScript.SendLedOFF();

        // Add a small slider for the LED intensity test
        if (GUILayout.Button("Send LED Intensity")) myScript.SendLedIntensity();
    }
}