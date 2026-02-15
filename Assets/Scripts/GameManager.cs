using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameUIPanel;
    public TMP_InputField nameInputField;
    public GameObject leaderboardPanel;
    public Transform leaderboardContentParent;
    public GameObject scoreEntryPrefab;

    [Header("Race UI")]
    public TMP_Text timerText;      // Changed from TextMeshProUGUI to TMP_Text
    public TMP_Text bestTimeText;   // Changed from TextMeshProUGUI to TMP_Text

    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioSource engineStartSound;

    private float raceTimer = 0f;
    private string playerName;
    private bool isGameStarted = false;
    private bool isGameOver = false;

    void Start()
    {
        ShowStart();
        // Ensure CsvLogger exists in scene or is a Singleton
        if (CsvLogger.Instance != null) CsvLogger.Instance.StartLogger();

        if (bestTimeText != null) bestTimeText.text = "Best: --:--";
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;
        raceTimer = 0f;

        // 1. ENGINE IGNITION
        // Play the engine sound first for that "Starting the Tank" feel
        if (engineStartSound != null)
        {
            engineStartSound.Play();
            Debug.Log("<color=orange>Engine:</color> Ignition successful!");
        }

        // 2. MUSIC HANDLING
        // Using PlayDelayed lets the engine roar be heard clearly for a split second
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.PlayDelayed(0.8f);
        }

        // 3. UI STATE TRANSITION
        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        // 4. DATA & LOGGING
        playerName = string.IsNullOrWhiteSpace(nameInputField.text) ? "Player" : nameInputField.text;

        if (CsvLogger.Instance != null)
        {
            CsvLogger.LogEvent("Game Start", System.DateTime.Now.ToString("HH:mm:ss"));
            CsvLogger.LogEvent("Player Name", playerName);
        }
    }

    void Update()
    {
        if (isGameStarted && !isGameOver)
        {
            raceTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(raceTimer);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        float fraction = (time * 100f) % 100f;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 1. TURN OFF HUD IMMEDIATELY (Move this to the top!)
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(false);
            Debug.Log("<color=cyan>UI Sync:</color> Game HUD deactivated.");
        }

        // 2. HARDWARE SHUTDOWN
        WebSocketClientExample ws = Object.FindFirstObjectByType<WebSocketClientExample>();
        if (ws != null) ws.SendLedOFF();

        // 3. DATA & LEADERBOARD
        ScoreManager.AddScore(playerName, Mathf.FloorToInt(raceTimer));
        ShowLeaderboard();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        StartCoroutine(PauseAfterUI());
    }
    public void RestartGame()
    {
        WebSocketClientExample ws = UnityEngine.Object.FindFirstObjectByType<WebSocketClientExample>();
        if (ws != null) ws.SendLedOFF();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ShowLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (leaderboardContentParent == null) return;

        foreach (Transform child in leaderboardContentParent) Destroy(child.gameObject);

        var scores = ScoreManager.GetScoreStrings();

        foreach (string scoreEntry in scores)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, leaderboardContentParent);
            entry.SetActive(true);
            entry.transform.localScale = Vector3.one;

            TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                // FORCE the component to wake up
                text.enabled = true;
                text.text = scoreEntry;

                // Log to console so you can see it working
                Debug.Log($"<color=green>Leaderboard:</color> Activated text for {scoreEntry}");
            }
        }
    }

    private void ShowStart()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;

    private IEnumerator PauseAfterUI()
    {
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
    }
}