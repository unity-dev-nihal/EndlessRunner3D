using System.Collections;
using UnityEngine;

public class StoreNotification : MonoBehaviour
{
    public static StoreNotification Instance { get; private set; }

    [Header("Notification")]
    [SerializeField] private GameObject notificationPopup;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 2f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }
    }

    public void ShowNotification()
    {
        if (notificationPopup == null)
        {
            Debug.LogWarning(
                "StoreNotification: Notification Popup is not assigned."
            );

            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        notificationPopup.SetActive(true);

        hideCoroutine = StartCoroutine(
            HideNotificationAfterDelay()
        );
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayDuration);

        if (notificationPopup != null)
        {
            notificationPopup.SetActive(false);
        }

        hideCoroutine = null;
    }

    private void OnDisable()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
}