using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameUIPanel;

    private bool isGameStarted = false;
    private bool isGameOver = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowStart();
    }

    public void StartGame()
    {
        isGameStarted = true;
        startPanel.SetActive(false);
        gameUIPanel.SetActive(true);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        gameOverPanel.SetActive(true);
        gameUIPanel.SetActive(false);
        Debug.Log("Game Over triggered!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowStart()
    {
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        gameUIPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}


