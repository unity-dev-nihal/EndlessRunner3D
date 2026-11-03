using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string runnerSceneName = "RunnerScene";

    [Header("Optional Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;

    private bool isLoadingScene;

    private void Start()
    {
        CloseOptionalPanels();
    }

    public void PlayGame()
    {
        if (isLoadingScene)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(runnerSceneName))
        {
            Debug.LogError("MainMenuManager: Runner scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(runnerSceneName))
        {
            Debug.LogError(
                "MainMenuManager: Scene '" +
                runnerSceneName +
                "' cannot be loaded. " +
                "Make sure RunnerScene is added to the Build Profile."
            );

            return;
        }

        isLoadingScene = true;

        SceneManager.LoadScene(runnerSceneName);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning(
                "MainMenuManager: Settings Panel is not assigned yet."
            );

            return;
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        settingsPanel.SetActive(true);
    }

    public void OpenHowToPlay()
    {
        if (howToPlayPanel == null)
        {
            Debug.LogWarning(
                "MainMenuManager: How To Play Panel is not assigned yet."
            );

            return;
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        howToPlayPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Debug.Log("MainMenuManager: Quit is not supported on WebGL.");
#else
        Application.Quit();
#endif
    }

    private void CloseOptionalPanels()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }
} 