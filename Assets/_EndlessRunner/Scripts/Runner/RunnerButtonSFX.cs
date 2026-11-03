using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RunnerButtonSFX : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(PlayButtonSound);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayButtonSound);
        }
    }

    private void PlayButtonSound()
    {
        if (GottaGoAudioManager.Instance == null)
        {
            Debug.LogWarning(
                "RunnerButtonSFX: GottaGoAudioManager.Instance is missing."
            );

            return;
        }

        GottaGoAudioManager.Instance.PlayButtonClick();
    }
}