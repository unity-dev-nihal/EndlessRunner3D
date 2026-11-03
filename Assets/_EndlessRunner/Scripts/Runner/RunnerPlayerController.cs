using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class RunnerPlayerController : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 8f;

    [Header("Lane Movement")]
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private float laneChangeSpeed = 18f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float gravity = -28f;

    [Header("Mobile Input")]
    [SerializeField] private RunnerMobileInput mobileInput;

    [Header("Shield")]
    [SerializeField] private RunnerShieldPowerUp shieldPowerUp;
    [SerializeField] private float shieldHitProtectionDuration = 0.75f;

    [Header("Phase")]
    [SerializeField] private RunnerPhasePowerUp phasePowerUp;

    [Header("Vibration")]
    [SerializeField] private bool enableVibration = true;
    [SerializeField] private bool vibrateOnPlayerCollision = true;
    [SerializeField] private bool vibrateOnInvalidMove = true;
    [SerializeField] private float invalidMoveVibrationCooldown = 0.12f;
    [SerializeField] private string vibrationPlayerPrefsKey = "GottaGo_Vibration";

    [Header("Collision")]
    [SerializeField] private bool showCollisionDebug = false;

    private CharacterController characterController;

    private int currentLane = 1;
    private float verticalVelocity;

    private bool gameOver;
    private bool isDead;

    private float shieldHitProtectionTimer;
    private float vibrationCooldownTimer;

    public float ForwardSpeed => forwardSpeed;

    public bool IsJumping =>
        characterController != null &&
        !characterController.isGrounded;

    public bool IsDead => isDead;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (mobileInput == null)
        {
            mobileInput = GetComponent<RunnerMobileInput>();
        }

        if (mobileInput == null)
        {
            mobileInput = FindFirstObjectByType<RunnerMobileInput>();
        }

        if (shieldPowerUp == null)
        {
            shieldPowerUp = GetComponent<RunnerShieldPowerUp>();
        }

        if (shieldPowerUp == null)
        {
            shieldPowerUp = FindFirstObjectByType<RunnerShieldPowerUp>();
        }

        if (phasePowerUp == null)
        {
            phasePowerUp = GetComponent<RunnerPhasePowerUp>();
        }

        if (phasePowerUp == null)
        {
            phasePowerUp = FindFirstObjectByType<RunnerPhasePowerUp>();
        }

        shieldHitProtectionDuration =
            Mathf.Max(0.1f, shieldHitProtectionDuration);

        invalidMoveVibrationCooldown =
            Mathf.Max(0f, invalidMoveVibrationCooldown);
    }

    private void OnEnable()
    {
        SubscribeToMobileInput();
    }

    private void OnDisable()
    {
        UnsubscribeFromMobileInput();
    }

    private void Update()
    {
        UpdateShieldHitProtection();
        UpdateVibrationCooldown();

        if (gameOver || isDead)
        {
            return;
        }

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            return;
        }

        HandleKeyboardInput();
        HandleVerticalMovement();
        HandleForwardAndLaneMovement();
    }

    private void SubscribeToMobileInput()
    {
        if (mobileInput == null)
        {
            return;
        }

        mobileInput.OnSwipeLeft += HandleMobileSwipeLeft;
        mobileInput.OnSwipeRight += HandleMobileSwipeRight;
        mobileInput.OnSwipeUp += HandleMobileSwipeUp;
    }

    private void UnsubscribeFromMobileInput()
    {
        if (mobileInput == null)
        {
            return;
        }

        mobileInput.OnSwipeLeft -= HandleMobileSwipeLeft;
        mobileInput.OnSwipeRight -= HandleMobileSwipeRight;
        mobileInput.OnSwipeUp -= HandleMobileSwipeUp;
    }

    private void HandleMobileSwipeLeft()
    {
        if (CanProcessInput())
        {
            MoveLeft();
        }
    }

    private void HandleMobileSwipeRight()
    {
        if (CanProcessInput())
        {
            MoveRight();
        }
    }

    private void HandleMobileSwipeUp()
    {
        if (CanProcessInput())
        {
            Jump();
        }
    }

    private bool CanProcessInput()
    {
        if (gameOver || isDead)
        {
            return false;
        }

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            return false;
        }

        return true;
    }

    private void UpdateShieldHitProtection()
    {
        if (shieldHitProtectionTimer <= 0f)
        {
            return;
        }

        shieldHitProtectionTimer -= Time.deltaTime;

        if (shieldHitProtectionTimer < 0f)
        {
            shieldHitProtectionTimer = 0f;
        }
    }

    private void UpdateVibrationCooldown()
    {
        if (vibrationCooldownTimer <= 0f)
        {
            return;
        }

        vibrationCooldownTimer -= Time.unscaledDeltaTime;

        if (vibrationCooldownTimer < 0f)
        {
            vibrationCooldownTimer = 0f;
        }
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveLeft();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveRight();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }

    private void MoveLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            return;
        }

        VibrateForInvalidMove();
    }

    private void MoveRight()
    {
        if (currentLane < 2)
        {
            currentLane++;
            return;
        }

        VibrateForInvalidMove();
    }

    private void Jump()
    {
        if (!characterController.isGrounded)
        {
            VibrateForInvalidMove();
            return;
        }

        verticalVelocity = jumpForce;
    }

    private void HandleVerticalMovement()
    {
        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleForwardAndLaneMovement()
    {
        float targetX =
            (currentLane - 1) * laneDistance;

        float newX =
            Mathf.MoveTowards(
                transform.position.x,
                targetX,
                laneChangeSpeed * Time.deltaTime
            );

        float horizontalMovement =
            newX - transform.position.x;

        float forwardMovement =
            forwardSpeed * Time.deltaTime;

        float verticalMovement =
            verticalVelocity * Time.deltaTime;

        characterController.Move(
            new Vector3(
                horizontalMovement,
                verticalMovement,
                forwardMovement
            )
        );
    }

    public void SetForwardSpeed(float newSpeed)
    {
        forwardSpeed =
            Mathf.Max(0f, newSpeed);
    }

    private void OnControllerColliderHit(
        ControllerColliderHit hit)
    {
        RunnerObstacle obstacle =
            hit.collider.GetComponentInParent<RunnerObstacle>();

        if (obstacle == null)
        {
            return;
        }

        if (gameOver || isDead)
        {
            return;
        }

        if (phasePowerUp == null)
        {
            phasePowerUp =
                FindFirstObjectByType<RunnerPhasePowerUp>();
        }

        if (phasePowerUp != null &&
            phasePowerUp.IsActive)
        {
            if (showCollisionDebug)
            {
                Debug.Log(
                    "PHASE ACTIVE - " +
                    "PLAYER PASSED THROUGH OBSTACLE."
                );
            }

            return;
        }

        if (shieldHitProtectionTimer > 0f)
        {
            if (showCollisionDebug)
            {
                Debug.Log(
                    "SHIELD COLLISION PROTECTION ACTIVE - " +
                    "IGNORING OBSTACLE HIT."
                );
            }

            return;
        }

        HandleObstacleCollision(obstacle);
    }

    private void HandleObstacleCollision(
        RunnerObstacle obstacle)
    {
        if (shieldPowerUp == null)
        {
            shieldPowerUp =
                FindFirstObjectByType<RunnerShieldPowerUp>();
        }

        VibrateForCollision();

        if (shieldPowerUp != null &&
            shieldPowerUp.TryAbsorbHit())
        {
            shieldHitProtectionTimer =
                shieldHitProtectionDuration;

            if (obstacle != null)
            {
                DestroyObstacle(obstacle);
            }

            if (showCollisionDebug)
            {
                Debug.Log(
                    "SHIELD SAVED PLAYER! " +
                    "Obstacle destroyed."
                );
            }

            return;
        }

        isDead = true;
        gameOver = true;

        if (GottaGoAudioManager.Instance != null)
        {
            GottaGoAudioManager.Instance
                .PlayObjectHitSound();
        }

        if (showCollisionDebug)
        {
            Debug.Log(
                "PLAYER HIT OBSTACLE! " +
                "DEATH ANIMATION TRIGGERED."
            );
        }

        if (RunnerGameManager.Instance != null)
        {
            RunnerGameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError(
                "RunnerPlayerController: " +
                "RunnerGameManager is missing!"
            );
        }
    }

    private void DestroyObstacle(
        RunnerObstacle obstacle)
    {
        if (obstacle == null)
        {
            return;
        }

        GameObject obstacleObject =
            obstacle.gameObject;

        if (showCollisionDebug)
        {
            Debug.Log(
                "SHIELD DESTROYING OBSTACLE: " +
                obstacleObject.name
            );
        }

        obstacleObject.SetActive(false);

        Destroy(obstacleObject);
    }

    private bool IsVibrationEnabled()
    {
        if (!enableVibration)
        {
            return false;
        }

        return PlayerPrefs.GetInt(
            vibrationPlayerPrefsKey,
            1
        ) == 1;
    }

    private void VibrateForCollision()
    {
        if (!vibrateOnPlayerCollision)
        {
            return;
        }

        TriggerVibration();
    }

    private void VibrateForInvalidMove()
    {
        if (!vibrateOnInvalidMove)
        {
            return;
        }

        if (vibrationCooldownTimer > 0f)
        {
            return;
        }

        TriggerVibration();

        vibrationCooldownTimer =
            invalidMoveVibrationCooldown;
    }

    private void TriggerVibration()
    {
        if (!IsVibrationEnabled())
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}