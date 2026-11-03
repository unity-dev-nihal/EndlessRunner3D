using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunnerGameOverUI : MonoBehaviour
{
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Result Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Display Settings")]
    [SerializeField] private bool useThousandsSeparator = true;

    [Header("Game Over Timing")]
    [SerializeField] private float gameOverDelay = 1.0f;

    private RunnerPlayerController playerController;

    private bool gameOverShown;
    private bool gameOverSequenceStarted;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(
                RestartGame
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }

        gameOverDelay =
            Mathf.Max(
                0f,
                gameOverDelay
            );
    }

    private void Start()
    {
        FindPlayer();

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (gameOverShown ||
            gameOverSequenceStarted)
        {
            return;
        }

        if (playerController == null)
        {
            FindPlayer();
        }

        if (playerController == null)
        {
            return;
        }

        if (playerController.IsDead)
        {
            StartGameOverSequence();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.Find("Player");

        if (playerObject == null)
        {
            return;
        }

        playerController =
            playerObject.GetComponent<RunnerPlayerController>();
    }

    private void StartGameOverSequence()
    {
        if (gameOverShown ||
            gameOverSequenceStarted)
        {
            return;
        }

        gameOverSequenceStarted = true;

        StartCoroutine(
            GameOverSequence()
        );
    }

    private IEnumerator GameOverSequence()
    {
        /*
         * Make sure the game continues running
         * while the player's death animation plays.
         */
        Time.timeScale = 1f;

        Debug.Log(
            "RunnerGameOverUI: Death animation playing. " +
            "Waiting " + gameOverDelay + " seconds."
        );

        /*
         * Use realtime so the delay still works
         * even if another system changes Time.timeScale.
         */
        float elapsed = 0f;

        while (elapsed < gameOverDelay)
        {
            elapsed += Time.unscaledDeltaTime;

            /*
             * Keep gameplay time running during
             * the death animation.
             */
            Time.timeScale = 1f;

            yield return null;
        }

        ShowGameOver();
    }

    private void ShowGameOver()
    {
        if (gameOverShown)
        {
            return;
        }

        gameOverShown = true;

        if (RunnerScoreManager.Instance != null)
        {
            RunnerScoreManager.Instance.FinalizeRun();
        }

        UpdateResultValues();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        /*
         * Pause gameplay and show the result screen.
         *
         * IMPORTANT:
         * We intentionally do NOT pause the music here.
         * Audio continues playing even though Time.timeScale is 0.
         */
        Time.timeScale = 0f;

        Debug.Log(
            "RunnerGameOverUI: Game Over panel opened. " +
            "Music continues playing."
        );
    }

    private void UpdateResultValues()
    {
        if (RunnerScoreManager.Instance == null)
        {
            return;
        }

        RunnerScoreManager scoreManager =
            RunnerScoreManager.Instance;

        if (scoreText != null)
        {
            scoreText.text =
                FormatNumber(scoreManager.Score);
        }

        if (distanceText != null)
        {
            int distance =
                Mathf.FloorToInt(
                    scoreManager.Distance
                );

            distanceText.text =
                distance.ToString() + " m";
        }

        if (coinsText != null)
        {
            coinsText.text =
                FormatNumber(scoreManager.Coins);
        }

        if (highScoreText != null)
        {
            highScoreText.text =
                FormatNumber(scoreManager.HighScore);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private string FormatNumber(int value)
    {
        if (useThousandsSeparator)
        {
            return value.ToString("N0");
        }

        return value.ToString();
    }

    private void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(
                RestartGame
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                ReturnToMainMenu
            );
        }

        Time.timeScale = 1f;
    }
}