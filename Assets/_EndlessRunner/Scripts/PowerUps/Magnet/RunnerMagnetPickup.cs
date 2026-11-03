using UnityEngine;

public class RunnerMagnetPickup : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private RunnerMagnetPowerUp magnetPowerUp;

    [Header("Pickup")]
    [SerializeField] private bool disableAfterPickup = true;

    private bool collected;

    private void Awake()
    {
        if (magnetPowerUp == null)
        {
            magnetPowerUp = FindFirstObjectByType<RunnerMagnetPowerUp>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (magnetPowerUp == null)
        {
            Debug.LogError(
                "RunnerMagnetPickup: RunnerMagnetPowerUp reference is missing."
            );

            return;
        }

        collected = true;

        magnetPowerUp.ActivateMagnet();

        Debug.Log("MAGNET POWER-UP COLLECTED!");

        if (disableAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetPickup()
    {
        collected = false;
        gameObject.SetActive(true);
    }
}