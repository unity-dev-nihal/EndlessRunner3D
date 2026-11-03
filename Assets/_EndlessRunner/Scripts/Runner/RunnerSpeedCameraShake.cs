using UnityEngine;

public class RunnerSpeedCameraShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Speed Range")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float maximumSpeed = 12f;

    [Header("Shake Settings")]
    [SerializeField] private float minimumShakeAmount = 0f;
    [SerializeField] private float maximumShakeAmount = 0.035f;

    [SerializeField] private float minimumShakeSpeed = 8f;
    [SerializeField] private float maximumShakeSpeed = 18f;

    [Header("Smoothing")]
    [SerializeField] private float shakeSmoothSpeed = 8f;

    private Vector3 originalLocalPosition;
    private float currentShakeAmount;

    private void Start()
    {
        originalLocalPosition =
            transform.localPosition;

        if (playerController == null)
        {
            Debug.LogError(
                "RunnerSpeedCameraShake: " +
                "Player Controller is not assigned."
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (playerController == null)
            return;

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            currentShakeAmount =
                Mathf.Lerp(
                    currentShakeAmount,
                    0f,
                    shakeSmoothSpeed *
                    Time.deltaTime
                );

            transform.localPosition =
                Vector3.Lerp(
                    transform.localPosition,
                    originalLocalPosition,
                    shakeSmoothSpeed *
                    Time.deltaTime
                );

            return;
        }

        UpdateShake();
    }

    private void UpdateShake()
    {
        float currentSpeed =
            playerController.ForwardSpeed;

        float speedProgress =
            Mathf.InverseLerp(
                startingSpeed,
                maximumSpeed,
                currentSpeed
            );

        float targetShakeAmount =
            Mathf.Lerp(
                minimumShakeAmount,
                maximumShakeAmount,
                speedProgress
            );

        currentShakeAmount =
            Mathf.Lerp(
                currentShakeAmount,
                targetShakeAmount,
                shakeSmoothSpeed *
                Time.deltaTime
            );

        if (currentShakeAmount <= 0.001f)
        {
            transform.localPosition =
                originalLocalPosition;

            return;
        }

        float shakeSpeed =
            Mathf.Lerp(
                minimumShakeSpeed,
                maximumShakeSpeed,
                speedProgress
            );

        float offsetX =
            (
                Mathf.PerlinNoise(
                    Time.time *
                    shakeSpeed,
                    0f
                ) -
                0.5f
            ) *
            currentShakeAmount;

        float offsetY =
            (
                Mathf.PerlinNoise(
                    0f,
                    Time.time *
                    shakeSpeed
                ) -
                0.5f
            ) *
            currentShakeAmount;

        Vector3 targetOffset =
            new Vector3(
                offsetX,
                offsetY,
                0f
            );

        transform.localPosition =
            originalLocalPosition +
            targetOffset;
    }
}