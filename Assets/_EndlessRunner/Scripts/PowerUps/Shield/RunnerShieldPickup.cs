using UnityEngine;

public class RunnerShieldPickup : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private RunnerShieldPowerUp shieldPowerUp;

    [Header("Pickup")]
    [SerializeField] private bool disableAfterPickup = true;

    private bool collected;

    private void Awake()
    {
        if (shieldPowerUp == null)
        {
            shieldPowerUp =
                FindFirstObjectByType<RunnerShieldPowerUp>();
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

        if (shieldPowerUp == null)
        {
            Debug.LogError(
                "RunnerShieldPickup: " +
                "RunnerShieldPowerUp reference is missing."
            );

            return;
        }

        collected = true;

        shieldPowerUp.ActivateShield();

        Debug.Log("SHIELD POWER-UP COLLECTED!");

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