using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunnerPauseUI : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(
                OpenPausePanel
            );
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(
                ClosePausePanel
            );
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                RestartGame
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        IsPaused = false;
    }

    public void OpenPausePanel()
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        PauseMusic();

        Debug.Log(
            "RunnerPauseUI: Pause panel opened."
        );
    }

    public void ClosePausePanel()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        ResumeMusic();

        Debug.Log(
            "RunnerPauseUI: Pause panel closed."
        );
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        ResumeMusic();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        ResumeMusic();

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    private void PauseMusic()
    {
        if (GottaGoAudioManager.Instance != null)
        {
            GottaGoAudioManager.Instance.PauseMusic();
        }
    }

    private void ResumeMusic()
    {
        if (GottaGoAudioManager.Instance != null)
        {
            GottaGoAudioManager.Instance.ResumeMusic();
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(
                OpenPausePanel
            );
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(
                ClosePausePanel
            );
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
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