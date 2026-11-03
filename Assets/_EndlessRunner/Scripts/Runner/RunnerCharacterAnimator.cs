using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RunnerCharacterAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerPlayerController playerController;

    [Header("Run Animation Speed")]
    [SerializeField] private float referenceSpeed = 8f;
    [SerializeField] private float minimumRunAnimationSpeed = 0.9f;
    [SerializeField] private float maximumRunAnimationSpeed = 1.35f;

    [Header("Smoothing")]
    [SerializeField] private float animationSpeedSmoothness = 8f;

    private Animator animator;

    private float currentRunAnimationSpeed = 1f;

    private static readonly int RunSpeedHash =
        Animator.StringToHash("RunSpeed");

    private static readonly int IsJumpingHash =
        Animator.StringToHash("IsJumping");

    private static readonly int IsDeadHash =
        Animator.StringToHash("IsDead");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerController == null)
        {
            playerController =
                GetComponentInParent<RunnerPlayerController>();
        }
    }

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError(
                "RunnerCharacterAnimator: " +
                "RunnerPlayerController reference is missing."
            );

            enabled = false;
            return;
        }

        animator.speed = 1f;

        animator.SetFloat(
            RunSpeedHash,
            1f
        );

        animator.SetBool(
            IsJumpingHash,
            false
        );

        animator.SetBool(
            IsDeadHash,
            false
        );
    }

    private void Update()
    {
        if (playerController == null)
        {
            return;
        }

        UpdateRunAnimationSpeed();
        UpdateJumpState();
        UpdateDeathState();
    }

    private void UpdateRunAnimationSpeed()
    {
        // Don't change animation speed after death.
        if (playerController.IsDead)
        {
            return;
        }

        float currentGameSpeed =
            playerController.ForwardSpeed;

        if (referenceSpeed <= 0f)
        {
            referenceSpeed = 8f;
        }

        float targetAnimationSpeed =
            currentGameSpeed /
            referenceSpeed;

        targetAnimationSpeed =
            Mathf.Clamp(
                targetAnimationSpeed,
                minimumRunAnimationSpeed,
                maximumRunAnimationSpeed
            );

        currentRunAnimationSpeed =
            Mathf.Lerp(
                currentRunAnimationSpeed,
                targetAnimationSpeed,
                animationSpeedSmoothness *
                Time.deltaTime
            );

        animator.SetFloat(
            RunSpeedHash,
            currentRunAnimationSpeed
        );
    }

    private void UpdateJumpState()
    {
        if (playerController.IsDead)
        {
            animator.SetBool(
                IsJumpingHash,
                false
            );

            return;
        }

        animator.SetBool(
            IsJumpingHash,
            playerController.IsJumping
        );
    }

    private void UpdateDeathState()
    {
        animator.SetBool(
            IsDeadHash,
            playerController.IsDead
        );
    }

    public void ResetAnimationState()
    {
        currentRunAnimationSpeed = 1f;

        if (animator == null)
        {
            return;
        }

        animator.speed = 1f;

        animator.SetFloat(
            RunSpeedHash,
            1f
        );

        animator.SetBool(
            IsJumpingHash,
            false
        );

        animator.SetBool(
            IsDeadHash,
            false
        );
    }
}