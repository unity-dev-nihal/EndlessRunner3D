using System.Collections.Generic;
using UnityEngine;

public class RunnerCoin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int scoreValue = 50;

    private bool collected;

    private static readonly HashSet<RunnerCoin> activeCoins = new HashSet<RunnerCoin>();

    private void OnEnable()
    {
        collected = false;
        activeCoins.Add(this);
    }

    private void OnDisable()
    {
        activeCoins.Remove(this);
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

        collected = true;

        Collect();
    }

    private void Collect()
    {
        if (RunnerScoreManager.Instance != null)
        {
            RunnerScoreManager.Instance.AddCoin(scoreValue);
        }

        if (GottaGoAudioManager.Instance != null)
        {
            GottaGoAudioManager.Instance.PlayCoinSound();
        }

        Debug.Log(
            "COIN COLLECTED! +" + scoreValue
        );

        gameObject.SetActive(false);
    }

    public void ResetCoin()
    {
        collected = false;
        gameObject.SetActive(true);
    }

    public static void GetActiveCoins(List<RunnerCoin> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        foreach (RunnerCoin coin in activeCoins)
        {
            if (coin != null && coin.gameObject.activeInHierarchy)
            {
                results.Add(coin);
            }
        }
    }
}