using System.Collections.Generic;
using UnityEngine;

public class RunnerMagnetPowerUp : MonoBehaviour
{
    public static RunnerMagnetPowerUp Instance { get; private set; }

    [Header("Magnet Duration")]
    [SerializeField] private float duration = 8f;

    [Header("Magnet Range")]
    [SerializeField] private float attractionRadius = 12f;

    [Header("Coin Attraction")]
    [SerializeField] private float startingAttractionSpeed = 25f;
    [SerializeField] private float maximumAttractionSpeed = 55f;
    [SerializeField] private float attractionAcceleration = 80f;

    [Header("Optional Visual")]
    [SerializeField] private GameObject magnetEffectVisual;

    private readonly List<RunnerCoin> activeCoins = new List<RunnerCoin>();

    private Transform playerTransform;

    private float remainingTime;
    private bool magnetActive;

    public bool IsActive
    {
        get { return magnetActive; }
    }

    public float RemainingTime
    {
        get { return remainingTime; }
    }

    public float Duration
    {
        get { return duration; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerTransform = transform;

        if (magnetEffectVisual != null)
        {
            magnetEffectVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (!magnetActive)
        {
            return;
        }

        if (playerTransform == null)
        {
            StopMagnet();
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            StopMagnet();
            return;
        }

        AttractCoins();
    }

    public void ActivateMagnet()
    {
        duration = Mathf.Max(0.1f, duration);
        attractionRadius = Mathf.Max(0.1f, attractionRadius);
        startingAttractionSpeed = Mathf.Max(0f, startingAttractionSpeed);
        maximumAttractionSpeed = Mathf.Max(
            startingAttractionSpeed,
            maximumAttractionSpeed
        );
        attractionAcceleration = Mathf.Max(
            0f,
            attractionAcceleration
        );

        remainingTime = duration;
        magnetActive = true;

        if (magnetEffectVisual != null)
        {
            magnetEffectVisual.SetActive(true);
        }

        Debug.Log(
            "MAGNET ACTIVATED | " +
            "Duration: " + duration +
            "s | Radius: " + attractionRadius +
            " | Speed: " + startingAttractionSpeed +
            "-" + maximumAttractionSpeed
        );
    }

    private void AttractCoins()
    {
        RunnerCoin.GetActiveCoins(activeCoins);

        float radiusSqr = attractionRadius * attractionRadius;

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            RunnerCoin coin = activeCoins[i];

            if (coin == null || !coin.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 direction =
                playerTransform.position -
                coin.transform.position;

            float distanceSqr = direction.sqrMagnitude;

            if (distanceSqr > radiusSqr)
            {
                continue;
            }

            float distance = Mathf.Sqrt(distanceSqr);

            if (distance <= 0.05f)
            {
                continue;
            }

            Vector3 normalizedDirection =
                direction / distance;

            /*
             * 0 = coin is very close
             * 1 = coin is at the outer edge
             */
            float distanceRatio =
                Mathf.Clamp01(distance / attractionRadius);

            /*
             * Stronger pull at greater distances.
             *
             * This makes coins immediately react when
             * they enter the magnet field.
             */
            float pullStrength =
                Mathf.Lerp(1.5f, 1f, distanceRatio);

            /*
             * Close coins receive the strongest final
             * acceleration toward the player.
             */
            float proximityStrength =
                1f - distanceRatio;

            float targetSpeed =
                Mathf.Lerp(
                    maximumAttractionSpeed,
                    startingAttractionSpeed,
                    distanceRatio
                );

            targetSpeed *= pullStrength;

            /*
             * Move directly toward the player with strong
             * acceleration.
             */
            float currentSpeed = Mathf.MoveTowards(
                startingAttractionSpeed,
                targetSpeed,
                attractionAcceleration *
                Time.deltaTime
            );

            Vector3 movement =
                normalizedDirection *
                currentSpeed *
                Time.deltaTime;

            coin.transform.position += movement;
        }
    }

    private void StopMagnet()
    {
        magnetActive = false;
        remainingTime = 0f;

        if (magnetEffectVisual != null)
        {
            magnetEffectVisual.SetActive(false);
        }

        Debug.Log("MAGNET ENDED");
    }

    public void CancelMagnet()
    {
        if (!magnetActive)
        {
            return;
        }

        StopMagnet();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}