using UnityEngine;
using NativeWebSocket;
using System;
using System.Threading.Tasks; // Added for Task support

public class WebSocketClientExample : MonoBehaviour
{
    private WebSocket websocket;

    [Header("Network Settings")]
    public string serverIP = "127.0.0.1";
    public int serverPort = 8081;

    [Header("Hardware Link")]
    public PlayerController playerScript;

    [Header("Joystick Tuning")]
    public float deadzone = 0.1f;
    [Range(0, 255)] public int ledIntensity = 0;
    private float currentJoyX = 0;
    private float currentJoyY = 0;

    async void Start()
    {
        websocket = new WebSocket("ws://" + serverIP + ":" + serverPort);

        websocket.OnOpen += async () =>
        {
            Debug.Log("Connected to WebSocket server");
            await websocket.SendText("Device (Unity): Ship Cockpit Online");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            IncomingMessageParser(message);
        };

        websocket.OnClose += (code) => { Debug.Log("WebSocket closed"); };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void OnDestroy()
    {
        if (websocket != null) await websocket.Close();
    }

    public void IncomingMessageParser(string msg)
    {
        if (!msg.Contains(":")) return;

        string[] parts = msg.Split(':');
        string type = parts[0].Trim(); // Trim to remove accidental spaces
        string valueStr = parts[1].Trim();

        if (float.TryParse(valueStr, out float rawValue))
        {
            float normalized = (rawValue - 2048f) / 2048f;

            // APPLY DEADZONE HERE
            if (Mathf.Abs(normalized) < deadzone) normalized = 0;

            if (type.Equals("JOY_X", StringComparison.OrdinalIgnoreCase))
            {
                currentJoyX = normalized;
            }
            else if (type.Equals("JOY_Y", StringComparison.OrdinalIgnoreCase))
            {
                currentJoyY = normalized;
            }
            else if (type.Equals("button", StringComparison.OrdinalIgnoreCase))
            {
                // Note: Use valueStr here because it's a discrete 0 or 1
                if (valueStr == "1" && playerScript != null) playerScript.Shoot();
                return; // Don't update movement on a button press
            }

            // Only send to player if we are getting JOY messages
            if (playerScript != null)
            {
                playerScript.UpdateExternalInput(currentJoyX, currentJoyY);
            }
        }
    }
    // --- TEST BUTTON FUNCTIONS ---
    public void TestForward() { playerScript.UpdateExternalInput(0, 1f); }
    public void TestBackward() { playerScript.UpdateExternalInput(0, -1f); }
    public void TestLeft() { playerScript.UpdateExternalInput(-1f, 0); }
    public void TestRight() { playerScript.UpdateExternalInput(1f, 0); }
    public void TestStop() { playerScript.UpdateExternalInput(0, 0); }

    // --- EDITOR COMPATIBILITY FUNCTIONS ---
    public async void SendHello() { await SendToESP32("MSG", "Hello from Unity"); }
    public async void SendLedON() { await SendToESP32("LED_INTENSITY", "255"); }
    public async void SendLedOFF() { await SendToESP32("LED_INTENSITY", "0"); }
    public async void SendLedIntensity() { await SendToESP32("LED_INTENSITY", ledIntensity.ToString()); }

    // Fix for CS4008: Changed return type to Task
    public async Task SendToESP32(string type, string value)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText($"{type}:{value}");
            Debug.Log($"Sent to ESP32: {type}:{value}");
        }
    }
}