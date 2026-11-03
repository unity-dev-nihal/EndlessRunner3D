using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerCameraController : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform target;
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset =
        new Vector3(0f, 4.5f, -8f);

    [Header("Follow")]
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    [Header("Look")]
    [SerializeField] private float lookHeight = 1.5f;

    [Header("Field of View")]
    [SerializeField] private float startingFOV = 60f;
    [SerializeField] private float maximumFOV = 72f;

    [Header("Speed Range")]
    [SerializeField] private float startingSpeed = 8f;
    [SerializeField] private float maximumSpeed = 12f;

    [Header("Camera Shake")]
    [SerializeField] private bool enableSpeedShake = true;
    [SerializeField] private float maximumShakeAmount = 0.035f;
    [SerializeField] private float shakeSpeed = 18f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Camera cameraComponent;

    private Vector3 positionVelocity;
    private Vector3 baseCameraPosition;

    private bool isInitialized;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        ResetCameraState();

        FindPlayer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetCameraState();

        FindPlayer();

        if (target != null)
        {
            InitializeCameraForPlayer();
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log(
                    "RunnerCameraController: " +
                    "Waiting for Player..."
                );
            }
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (showDebugLogs)
        {
            Debug.Log(
                "RunnerCameraController: " +
                "Scene loaded. Reacquiring Player."
            );
        }

        ResetCameraState();

        target = null;
        playerController = null;

        isInitialized = false;

        FindPlayer();

        if (target != null)
        {
            InitializeCameraForPlayer();
        }
    }

    private void LateUpdate()
    {
        // Always make sure we have a valid player.
        if (target == null ||
            playerController == null)
        {
            FindPlayer();

            if (target != null &&
                playerController != null)
            {
                InitializeCameraForPlayer();
            }
            else
            {
                return;
            }
        }

        FollowPlayer();

        RotateTowardsPlayer();

        UpdateFieldOfView();

        UpdateSpeedShake();
    }

    private void FindPlayer()
    {
        RunnerPlayerController foundPlayer =
            FindFirstObjectByType<RunnerPlayerController>();

        if (foundPlayer == null)
        {
            return;
        }

        playerController =
            foundPlayer;

        target =
            foundPlayer.transform;

        if (showDebugLogs)
        {
            Debug.Log(
                "RunnerCameraController: " +
                "Player found: " +
                target.name
            );
        }
    }

    private void InitializeCameraForPlayer()
    {
        if (target == null)
        {
            return;
        }

        baseCameraPosition =
            target.position + offset;

        positionVelocity =
            Vector3.zero;

        transform.position =
            baseCameraPosition;

        Vector3 lookPosition =
            target.position +
            Vector3.up * lookHeight;

        Vector3 direction =
            lookPosition -
            transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        if (cameraComponent != null)
        {
            cameraComponent.fieldOfView =
                startingFOV;
        }

        isInitialized = true;

        if (showDebugLogs)
        {
            Debug.Log(
                "RunnerCameraController: " +
                "Camera initialized for Player."
            );
        }
    }

    private void FollowPlayer()
    {
        if (!isInitialized)
        {
            InitializeCameraForPlayer();

            if (!isInitialized)
            {
                return;
            }
        }

        Vector3 desiredPosition =
            target.position + offset;

        baseCameraPosition =
            Vector3.SmoothDamp(
                baseCameraPosition,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );

        transform.position =
            baseCameraPosition;
    }

    private void RotateTowardsPlayer()
    {
        if (target == null)
        {
            return;
        }

        Vector3 lookPosition =
            target.position +
            Vector3.up * lookHeight;

        Vector3 direction =
            lookPosition -
            transform.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion desiredRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmoothSpeed *
                Time.deltaTime
            );
    }

    private void UpdateFieldOfView()
    {
        if (cameraComponent == null)
        {
            return;
        }

        if (playerController == null)
        {
            return;
        }

        float currentSpeed =
            playerController.ForwardSpeed;

        float speedPercentage =
            Mathf.InverseLerp(
                startingSpeed,
                maximumSpeed,
                currentSpeed
            );

        float targetFOV =
            Mathf.Lerp(
                startingFOV,
                maximumFOV,
                speedPercentage
            );

        cameraComponent.fieldOfView =
            Mathf.Lerp(
                cameraComponent.fieldOfView,
                targetFOV,
                5f *
                Time.deltaTime
            );
    }

    private void UpdateSpeedShake()
    {
        if (!enableSpeedShake)
        {
            transform.position =
                baseCameraPosition;

            return;
        }

        if (playerController == null)
        {
            return;
        }

        float currentSpeed =
            playerController.ForwardSpeed;

        float speedPercentage =
            Mathf.InverseLerp(
                startingSpeed,
                maximumSpeed,
                currentSpeed
            );

        float shakeAmount =
            Mathf.Lerp(
                0f,
                maximumShakeAmount,
                speedPercentage
            );

        float shakeX =
            Mathf.Sin(
                Time.time *
                shakeSpeed
            ) *
            shakeAmount;

        float shakeY =
            Mathf.Cos(
                Time.time *
                shakeSpeed *
                1.37f
            ) *
            shakeAmount;

        Vector3 shakeOffset =
            new Vector3(
                shakeX,
                shakeY,
                0f
            );

        transform.position =
            baseCameraPosition +
            shakeOffset;
    }

    private void ResetCameraState()
    {
        positionVelocity =
            Vector3.zero;

        baseCameraPosition =
            transform.position;

        isInitialized = false;

        if (cameraComponent != null)
        {
            cameraComponent.fieldOfView =
                startingFOV;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning(
                "RunnerCameraController: " +
                "SetTarget received a null target."
            );

            return;
        }

        target =
            newTarget;

        playerController =
            target.GetComponent<
                RunnerPlayerController
            >();

        InitializeCameraForPlayer();
    }

    public Transform GetTarget()
    {
        return target;
    }

    public bool HasTarget()
    {
        return target != null &&
               playerController != null &&
               isInitialized;
    }
}