using UnityEngine;

public class RunnerSpeedCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera runnerCamera;
    [SerializeField] private RunnerPlayerController playerController;

    [Header("FOV Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float maximumFOV = 72f;

    [Header("Speed Range")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float maximumSpeed = 12f;

    [Header("FOV Smoothness")]
    [SerializeField] private float fovSmoothSpeed = 5f;

    private void Start()
    {
        if (runnerCamera == null)
        {
            runnerCamera =
                GetComponent<Camera>();
        }

        if (runnerCamera == null)
        {
            Debug.LogError(
                "RunnerSpeedCamera: " +
                "Camera reference is missing."
            );

            enabled = false;
            return;
        }

        if (playerController == null)
        {
            Debug.LogError(
                "RunnerSpeedCamera: " +
                "Player Controller is not assigned."
            );

            enabled = false;
            return;
        }

        runnerCamera.fieldOfView =
            normalFOV;
    }

    private void Update()
    {
        if (runnerCamera == null)
            return;

        if (playerController == null)
            return;

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            return;
        }

        UpdateFieldOfView();
    }

    private void UpdateFieldOfView()
    {
        float currentSpeed =
            playerController.ForwardSpeed;

        float speedProgress =
            Mathf.InverseLerp(
                startingSpeed,
                maximumSpeed,
                currentSpeed
            );

        float targetFOV =
            Mathf.Lerp(
                normalFOV,
                maximumFOV,
                speedProgress
            );

        runnerCamera.fieldOfView =
            Mathf.Lerp(
                runnerCamera.fieldOfView,
                targetFOV,
                fovSmoothSpeed *
                Time.deltaTime
            );
    }
}