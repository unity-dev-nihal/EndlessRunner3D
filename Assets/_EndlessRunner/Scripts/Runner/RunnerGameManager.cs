using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerGameManager : MonoBehaviour
{
    public static RunnerGameManager Instance { get; private set; }

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over Timing")]
    [SerializeField] private float gameOverDelay = 1.5f;

    public bool IsGameOver { get; private set; }
    public bool IsGameOverUIReady { get; private set; }

    private Coroutine gameOverRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        IsGameOver = false;
        IsGameOverUIReady = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        IsGameOverUIReady = false;

        Debug.Log("RUNNER GAME OVER");

        if (gameOverRoutine != null)
        {
            StopCoroutine(gameOverRoutine);
        }

        gameOverRoutine = StartCoroutine(PrepareGameOverUI());
    }

    private IEnumerator PrepareGameOverUI()
    {
        yield return new WaitForSeconds(gameOverDelay);

        IsGameOverUIReady = true;

        Debug.Log("GAME OVER UI READY");
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.buildIndex);
    }
}