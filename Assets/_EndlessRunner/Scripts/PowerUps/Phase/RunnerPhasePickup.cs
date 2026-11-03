using UnityEngine;

public class RunnerPhasePickup : MonoBehaviour
{
    [Header("Phase")]
    [SerializeField] private RunnerPhasePowerUp phasePowerUp;

    [Header("Pickup")]
    [SerializeField] private bool disableAfterPickup = true;

    private bool collected;

    private void Awake()
    {
        if (phasePowerUp == null)
        {
            phasePowerUp =
                FindFirstObjectByType<RunnerPhasePowerUp>();
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

        if (phasePowerUp == null)
        {
            Debug.LogError(
                "RunnerPhasePickup: " +
                "RunnerPhasePowerUp reference is missing."
            );

            return;
        }

        collected = true;

        phasePowerUp.ActivatePhase();

        Debug.Log(
            "PHASE POWER-UP COLLECTED!"
        );

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