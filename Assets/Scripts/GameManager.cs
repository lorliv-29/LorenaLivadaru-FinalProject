using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameUIPanel;
    public TMP_InputField nameInputField;
    public GameObject leaderboardPanel;             // Assign in Inspector
    public Transform leaderboardContentParent;      // VerticalLayoutGroup container
    public GameObject scoreEntryPrefab;             // TMP_Text prefab (deactivated by default)
    private string playerName;
    public AudioSource backgroundMusic; // assign in Inspector


    private bool isGameStarted = false;
    private bool isGameOver = false;

    public int currentLap = 0;             // Current lap count
    public TextMeshProUGUI lapText;        // Reference to UI text that displays the lap coun

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowStart();

        CsvLogger.Instance.StartLogger();
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;

        startPanel.SetActive(false); // Hide start panel
        gameOverPanel.SetActive(false); // Hide game over panel
        gameUIPanel.SetActive(true); // Show game UI panel

        playerName = nameInputField.text;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }
        // Start background music if assigned
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();

            Debug.Log("Background music started!");

        }

        Debug.Log("Game Started");

        CsvLogger.LogEvent("Player Name", playerName);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Add score
        ScoreManager.AddScore(playerName, currentLap);

        // Hide all panels
        startPanel.SetActive(false);
        gameUIPanel.SetActive(false);

        // Show leaderboard panel and fill entries
        ShowLeaderboard();
        // Show game over panel
        gameOverPanel.SetActive(true);

        // Pause game AFTER UI finishes rendering
        StartCoroutine(PauseAfterUI());

        Debug.Log("Game Over triggered!");
    }

    void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);

        // Remove old entries
        foreach (Transform child in leaderboardContentParent)
        {
            Destroy(child.gameObject);
        }

        // Get saved scores
        var scores = ScoreManager.GetScoreStrings();

        // Add a text entry for each score
        foreach (string score in scores)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, leaderboardContentParent);
            entry.SetActive(true);

            TMP_Text text = entry.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.text = score.Replace(":", " - ");
            }
        }
    }

    public void DownloadData()
    {
        CsvLogger.Instance.StopLogger();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowStart()
    {
        startPanel.SetActive(true); // Show start panel
        gameOverPanel.SetActive(false); // Hide game over panel
        gameUIPanel.SetActive(false);  // Hide game UI panel
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnLapCompleted()
    {
        CsvLogger.LogEvent("Lap Number", currentLap.ToString());

        currentLap++;

        Debug.Log("Lap Completed! Total laps: " + currentLap);

        if (lapText != null)
            lapText.text = "Laps: " + currentLap;
    }

    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    // Coroutine to pause the game after UI has rendered
    private IEnumerator PauseAfterUI()
    {
        yield return new WaitForEndOfFrame();  // wait until UI renders
        Time.timeScale = 0f;                   // now pause the game
    }

}


