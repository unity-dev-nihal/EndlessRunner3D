using System.Collections;
using TMPro;
using UnityEngine;

public class RunnerStartSequence : MonoBehaviour
{
    [Header("Countdown UI")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Timing")]
    [SerializeField] private float readyDuration = 0.8f;
    [SerializeField] private float countdownStepDuration = 1f;
    [SerializeField] private float runDuration = 0.8f;

    [Header("Animation")]
    [SerializeField] private float startScale = 0.65f;
    [SerializeField] private float peakScale = 1.15f;
    [SerializeField] private float endScale = 1f;

    private Vector3 originalScale;
    private Coroutine countdownRoutine;
    private bool sequenceStarted;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (countdownText != null)
        {
            originalScale = countdownText.transform.localScale;
            countdownText.transform.localScale = originalScale;
            countdownText.text = string.Empty;
        }
    }

    private void Start()
    {
        if (countdownText == null)
        {
            Debug.LogError(
                "RunnerStartSequence: Countdown Text is not assigned."
            );

            enabled = false;
            return;
        }

        if (sequenceStarted)
        {
            return;
        }

        sequenceStarted = true;

        countdownRoutine = StartCoroutine(
            RunStartSequence()
        );
    }

    private IEnumerator RunStartSequence()
    {
        Time.timeScale = 0f;

        yield return ShowMessage(
            "READY",
            readyDuration
        );

        yield return ShowMessage(
            "3",
            countdownStepDuration
        );

        yield return ShowMessage(
            "2",
            countdownStepDuration
        );

        yield return ShowMessage(
            "1",
            countdownStepDuration
        );

        yield return ShowMessage(
            "RUN!",
            runDuration
        );

        HideCountdown();

        Time.timeScale = 1f;

        Debug.Log("RUNNER START SEQUENCE COMPLETE");
    }

    private IEnumerator ShowMessage(
        string message,
        float duration
    )
    {
        countdownText.text = message;
        countdownText.transform.localScale =
            originalScale * startScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float scale;

            if (normalizedTime < 0.5f)
            {
                float firstHalf =
                    normalizedTime / 0.5f;

                scale = Mathf.Lerp(
                    startScale,
                    peakScale,
                    EaseOutBack(firstHalf)
                );
            }
            else
            {
                float secondHalf =
                    (normalizedTime - 0.5f) / 0.5f;

                scale = Mathf.Lerp(
                    peakScale,
                    endScale,
                    EaseInOut(secondHalf)
                );
            }

            countdownText.transform.localScale =
                originalScale * scale;

            yield return null;
        }

        countdownText.transform.localScale =
            originalScale * endScale;
    }

    private void HideCountdown()
    {
        if (countdownText == null)
        {
            return;
        }

        countdownText.text = string.Empty;
        countdownText.transform.localScale =
            originalScale;
    }

    private float EaseOutBack(float value)
    {
        const float overshoot = 1.70158f;

        float adjustedValue =
            value - 1f;

        return
            1f +
            (overshoot + 1f) *
            adjustedValue *
            adjustedValue *
            adjustedValue +
            overshoot *
            adjustedValue *
            adjustedValue;
    }

    private float EaseInOut(float value)
    {
        return value * value *
            (3f - 2f * value);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}