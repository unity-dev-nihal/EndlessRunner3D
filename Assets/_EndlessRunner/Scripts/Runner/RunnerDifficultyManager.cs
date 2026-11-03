using UnityEngine;

public class RunnerDifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerPlayerController player;

    [Header("Starting Difficulty")]
    [SerializeField] private float startingSpeed = 18f;

    [Header("Maximum Difficulty")]
    [SerializeField] private float maximumSpeed = 40f;

    [Header("Speed Progression")]
    [SerializeField] private float distancePerSpeedIncrease = 250f;
    [SerializeField] private float speedIncreaseAmount = 1.5f;

    [Header("Acceleration")]
    [SerializeField] private float accelerationRate = 1.5f;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.1f;

    private float startingZ;
    private float updateTimer;
    private float currentSpeed;

    public float CurrentSpeed
    {
        get { return currentSpeed; }
    }

    public float CurrentDistance
    {
        get
        {
            if (player == null)
                return 0f;

            return Mathf.Max(
                0f,
                player.transform.position.z - startingZ
            );
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "RunnerDifficultyManager: Player is not assigned."
            );

            enabled = false;
            return;
        }

        startingZ =
            player.transform.position.z;

        currentSpeed =
            startingSpeed;

        player.SetForwardSpeed(
            currentSpeed
        );

        Debug.Log(
            "RunnerDifficultyManager started. " +
            "Starting Speed: " +
            currentSpeed
        );
    }

    private void Update()
    {
        if (player == null)
            return;

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            return;
        }

        updateTimer +=
            Time.deltaTime;

        if (updateTimer <
            updateInterval)
        {
            return;
        }

        updateTimer = 0f;

        UpdateDifficulty();
    }

    private void UpdateDifficulty()
    {
        float distance =
            CurrentDistance;

        int difficultyLevel =
            Mathf.FloorToInt(
                distance /
                distancePerSpeedIncrease
            );

        float targetSpeed =
            startingSpeed +
            (
                difficultyLevel *
                speedIncreaseAmount
            );

        targetSpeed =
            Mathf.Min(
                targetSpeed,
                maximumSpeed
            );

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                accelerationRate *
                updateInterval
            );

        player.SetForwardSpeed(
            currentSpeed
        );
    }

    public void ResetDifficulty()
    {
        if (player == null)
            return;

        startingZ =
            player.transform.position.z;

        currentSpeed =
            startingSpeed;

        player.SetForwardSpeed(
            currentSpeed
        );

        updateTimer = 0f;
    }
}