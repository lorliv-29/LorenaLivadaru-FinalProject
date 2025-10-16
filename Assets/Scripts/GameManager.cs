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

    private bool isGameStarted = false;
    private bool isGameOver = false;

    public int currentLap = 0;             // Current lap count
    public TextMeshProUGUI lapText;        // Reference to UI text that displays the lap coun

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowStart();
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;

        startPanel.SetActive(false); // Hide start panel
        gameOverPanel.SetActive(false); // Hide game over panel
        gameUIPanel.SetActive(true); // Show game UI panel

        Debug.Log("Game Started");
    }

    public void GameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;

        gameOverPanel.SetActive(true); // Show game over panel
        gameUIPanel.SetActive(false); // Hide game UI panel
        startPanel.SetActive(false); // Hide start panel

        Time.timeScale = 0f; // Pause the game

        Debug.Log("Game Over triggered!");
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
}


