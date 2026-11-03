using UnityEngine;

public class RunnerShieldPowerUp : MonoBehaviour
{
    public static RunnerShieldPowerUp Instance { get; private set; }

    [Header("Shield")]
    [SerializeField] private GameObject shieldEffectVisual;

    private bool shieldActive;

    public bool IsActive
    {
        get { return shieldActive; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (shieldEffectVisual != null)
        {
            shieldEffectVisual.SetActive(false);
        }
    }

    public void ActivateShield()
    {
        shieldActive = true;

        if (shieldEffectVisual != null)
        {
            shieldEffectVisual.SetActive(true);
        }

        Debug.Log("SHIELD ACTIVATED!");
    }

    public bool TryAbsorbHit()
    {
        if (!shieldActive)
        {
            return false;
        }

        shieldActive = false;

        if (shieldEffectVisual != null)
        {
            shieldEffectVisual.SetActive(false);
        }

        Debug.Log("SHIELD ABSORBED OBSTACLE HIT!");

        return true;
    }

    public void CancelShield()
    {
        shieldActive = false;

        if (shieldEffectVisual != null)
        {
            shieldEffectVisual.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}