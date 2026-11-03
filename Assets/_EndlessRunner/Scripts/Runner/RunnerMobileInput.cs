using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RunnerMobileInput : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float minimumSwipeDistance = 100f;

    private Vector2 touchStartPosition;
    private bool trackingTouch;

    public bool SwipeLeft { get; private set; }
    public bool SwipeRight { get; private set; }
    public bool SwipeUp { get; private set; }

    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;
    public event Action OnSwipeUp;

    private void Awake()
    {
        minimumSwipeDistance =
            Mathf.Max(1f, minimumSwipeDistance);
    }

    private void Update()
    {
        ResetSwipeStates();

        if (Touchscreen.current == null)
        {
            return;
        }

        var touch =
            Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            touchStartPosition =
                touch.position.ReadValue();

            trackingTouch = true;
        }

        if (trackingTouch &&
            touch.press.wasReleasedThisFrame)
        {
            Vector2 touchEndPosition =
                touch.position.ReadValue();

            ProcessSwipe(touchEndPosition);

            trackingTouch = false;
        }
    }

    private void ProcessSwipe(
        Vector2 touchEndPosition)
    {
        Vector2 swipeDelta =
            touchEndPosition -
            touchStartPosition;

        if (swipeDelta.magnitude <
            minimumSwipeDistance)
        {
            return;
        }

        if (Mathf.Abs(swipeDelta.x) >
            Mathf.Abs(swipeDelta.y))
        {
            if (swipeDelta.x > 0f)
            {
                SwipeRight = true;
                OnSwipeRight?.Invoke();
            }
            else
            {
                SwipeLeft = true;
                OnSwipeLeft?.Invoke();
            }
        }
        else
        {
            if (swipeDelta.y > 0f)
            {
                SwipeUp = true;
                OnSwipeUp?.Invoke();
            }
        }
    }

    private void ResetSwipeStates()
    {
        SwipeLeft = false;
        SwipeRight = false;
        SwipeUp = false;
    }
}