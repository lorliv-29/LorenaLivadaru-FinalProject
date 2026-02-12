using UnityEngine;
using NativeWebSocket;
using System;
using System.Threading.Tasks;

public class WebSocketClientExample : MonoBehaviour
{
    private WebSocket websocket;

    [Header("Network Settings")]
    public string serverIP = "10.204.0.105"; // Updated to your server IP
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
        string uri = $"ws://{serverIP}:{serverPort}";
        websocket = new WebSocket(uri);

        websocket.OnOpen += async () =>
        {
            Debug.Log("<color=green>Connected to WebSocket server</color>");
            await websocket.SendText("Device (Unity): Tank Cockpit Online");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            IncomingMessageParser(message);
        };

        websocket.OnError += (e) => Debug.LogError($"WebSocket Error: {e}");
        websocket.OnClose += (c) => Debug.Log("WebSocket closed");

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    private async void OnDestroy()
    {
        if (websocket != null) await websocket.Close();
    }

    public void IncomingMessageParser(string msg)
    {
        if (string.IsNullOrEmpty(msg) || !msg.Contains(":")) return;

        string[] parts = msg.Split(':');
        if (parts.Length < 2) return;

        string type = parts[0].Trim();
        string valueStr = parts[1].Trim();

        if (float.TryParse(valueStr, out float rawValue))
        {
            float normalized = (rawValue - 2048f) / 2048f;

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
                if (valueStr == "1" && playerScript != null) playerScript.Shoot();
                return;
            }

            // SAFETY: Only update if the playerScript exists and numbers are valid
            if (playerScript != null && !float.IsNaN(currentJoyX) && !float.IsNaN(currentJoyY))
            {
                playerScript.UpdateExternalInput(currentJoyX, currentJoyY);
            }
        }
    }

    // --- TEST BUTTON FUNCTIONS (Call these from UI Buttons in Unity) ---
    public void TestForward() { if (playerScript != null) playerScript.UpdateExternalInput(0, 1f); }
    public void TestBackward() { if (playerScript != null) playerScript.UpdateExternalInput(0, -1f); }
    public void TestLeft() { if (playerScript != null) playerScript.UpdateExternalInput(-1f, 0); }
    public void TestRight() { if (playerScript != null) playerScript.UpdateExternalInput(1f, 0); }
    public void TestStop() { if (playerScript != null) playerScript.UpdateExternalInput(0, 0); }

    // --- ESP32 COMMUNICATION FUNCTIONS ---
    public async void SendHello() { await SendToESP32("MSG", "Hello from Unity"); }
    public async void SendLedON() { await SendToESP32("LED_INTENSITY", "255"); }
    public async void SendLedOFF() { await SendToESP32("LED_INTENSITY", "0"); }
    public async void SendLedIntensity() { await SendToESP32("LED_INTENSITY", ledIntensity.ToString()); }

    public async Task SendToESP32(string type, string value)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText($"{type}:{value}");
            Debug.Log($"Sent to ESP32: {type}:{value}");
        }
    }
}