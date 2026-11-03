using TMPro;
using UnityEngine;

public class MainMenuCoinUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinText;

    private const string TotalCoinsPlayerPrefsKey =
        "GottaGo_TotalCoins";

    private void Start()
    {
        RefreshCoinText();
    }

    private void OnEnable()
    {
        RefreshCoinText();
    }

    public void RefreshCoinText()
    {
        if (coinText == null)
        {
            return;
        }

        int totalCoins = PlayerPrefs.GetInt(
            TotalCoinsPlayerPrefsKey,
            0
        );

        coinText.text =
            FormatCoinAmount(totalCoins);
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
}