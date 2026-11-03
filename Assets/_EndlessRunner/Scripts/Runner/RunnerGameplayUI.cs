using TMPro;
using UnityEngine;

public class RunnerGameplayUI : MonoBehaviour
{
    [Header("Gameplay HUD")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Formatting")]
    [SerializeField] private bool useThousandsSeparator = true;

    private void Update()
    {
        if (RunnerScoreManager.Instance == null)
        {
            return;
        }

        UpdateCoins();
        UpdateScore();
        UpdateDistance();
        UpdateHighScore();
    }

    private void UpdateCoins()
    {
        if (coinText == null)
        {
            return;
        }

        int coins =
            RunnerScoreManager.Instance.Coins;

        coinText.text =
            FormatCoinAmount(coins);
    }

    private void UpdateScore()
    {
        if (scoreText == null)
        {
            return;
        }

        int score =
            RunnerScoreManager.Instance.Score;

        coinTextSafeSet(
            scoreText,
            FormatNumber(score)
        );
    }

    private void UpdateDistance()
    {
        if (distanceText == null)
        {
            return;
        }

        int distance =
            Mathf.FloorToInt(
                RunnerScoreManager.Instance.Distance
            );

        distanceText.text =
            distance.ToString() + " m";
    }

    private void UpdateHighScore()
    {
        if (highScoreText == null)
        {
            return;
        }

        int highScore =
            RunnerScoreManager.Instance.HighScore;

        highScoreText.text =
            FormatNumber(highScore);
    }

    private string FormatCoinAmount(int amount)
    {
        if (amount < 10000)
        {
            return amount.ToString("N0");
        }

        if (amount < 1000000)
        {
            return FormatCompactAmount(
                amount,
                1000f,
                "K"
            );
        }

        if (amount < 1000000000)
        {
            return FormatCompactAmount(
                amount,
                1000000f,
                "M"
            );
        }

        return FormatCompactAmount(
            amount,
            1000000000f,
            "B"
        );
    }

    private string FormatCompactAmount(
        int amount,
        float divisor,
        string suffix
    )
    {
        float value = amount / divisor;

        if (value >= 100f)
        {
            return Mathf.RoundToInt(value) + suffix;
        }

        if (value >= 10f)
        {
            return value.ToString("0.#") + suffix;
        }

        return value.ToString("0.##") + suffix;
    }

    private string FormatNumber(int value)
    {
        if (useThousandsSeparator)
        {
            return value.ToString("N0");
        }

        return value.ToString();
    }

    private void coinTextSafeSet(
        TMP_Text target,
        string value
    )
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
    }
}