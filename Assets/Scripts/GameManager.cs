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
    public TextMeshProUGUI timerText;       // Displays current race time
    public TextMeshProUGUI lapText;         // Displays current lap number
    public TextMeshProUGUI bestTimeText;    // Displays best lap time

    [Header("Audio")]
    public AudioSource backgroundMusic;

    // --- RACE DATA ---
    private float raceTimer = 0f;
    private float bestLapTime = float.MaxValue;
    private int currentLap = 0;
    private string playerName;
    private bool isGameStarted = false;
    private bool isGameOver = false;

    void Start()
    {
        ShowStart();
        CsvLogger.Instance.StartLogger();

        if (bestTimeText != null) bestTimeText.text = "Best: --:--";
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;
        raceTimer = 0f;
        currentLap = 1; // Start on Lap 1

        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        playerName = string.IsNullOrWhiteSpace(nameInputField.text) ? "Anonymous" : nameInputField.text;

        if (backgroundMusic != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();

        if (lapText != null) lapText.text = "Lap: " + currentLap;

        CsvLogger.LogEvent("Game Start", System.DateTime.Now.ToString("HH:mm:ss"));
        CsvLogger.LogEvent("Player Name", playerName);
    }

    void Update()
    {
        if (isGameStarted && !isGameOver)
        {
            // Update the race timer
            raceTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Formats time to 00:00.00
            int minutes = Mathf.FloorToInt(raceTimer / 60f);
            int seconds = Mathf.FloorToInt(raceTimer % 60f);
            float fraction = (raceTimer * 100f) % 100f;
            timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
        }
    }

    public void OnLapCompleted()
    {
        // Check if this was the fastest lap
        if (raceTimer < bestLapTime)
        {
            bestLapTime = raceTimer;
            UpdateBestTimeUI();
        }

        CsvLogger.LogEvent("Lap Finished", currentLap.ToString());
        CsvLogger.LogEvent("Lap Time", timerText.text);

        currentLap++;
        raceTimer = 0f; // Reset timer for the next lap

        if (lapText != null) lapText.text = "Lap: " + currentLap;
    }

    private void UpdateBestTimeUI()
    {
        if (bestTimeText != null)
        {
            int minutes = Mathf.FloorToInt(bestLapTime / 60f);
            int seconds = Mathf.FloorToInt(bestLapTime % 60f);
            float fraction = (bestLapTime * 100f) % 100f;
            bestTimeText.text = "Best: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Add the BEST lap time to the leaderboard instead of total laps
        ScoreManager.AddScore(playerName, Mathf.FloorToInt(bestLapTime));

        gameUIPanel.SetActive(false);
        ShowLeaderboard();
        gameOverPanel.SetActive(true);

        StartCoroutine(PauseAfterUI());
        CsvLogger.LogEvent("Race Over", System.DateTime.Now.ToString("HH:mm:ss"));
    }

    // Leaderboard and logic remains mostly the same, but uses Best Time
    void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        foreach (Transform child in leaderboardContentParent) Destroy(child.gameObject);

        var scores = ScoreManager.GetScoreStrings();
        foreach (string score in scores)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, leaderboardContentParent);
            entry.SetActive(true);
            TMP_Text text = entry.GetComponent<TMP_Text>();
            if (text != null) text.text = score;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowStart()
    {
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        gameUIPanel.SetActive(false);
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;

    private IEnumerator PauseAfterUI()
    {
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
    }
}