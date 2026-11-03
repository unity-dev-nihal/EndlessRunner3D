using UnityEngine;

public class RunnerScoreManager : MonoBehaviour
{
    public static RunnerScoreManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Score Settings")]
    [SerializeField] private float scoreMultiplier = 10f;

    [Header("High Score")]
    [SerializeField] private string highScorePlayerPrefsKey = "GottaGo_HighScore";

    [Header("Total Coins")]
    [SerializeField] private string totalCoinsPlayerPrefsKey = "GottaGo_TotalCoins";

    public float Distance { get; private set; }

    public int Score { get; private set; }

    /// <summary>
    /// Coins collected during the current run.
    /// </summary>
    public int Coins { get; private set; }

    /// <summary>
    /// Total coins collected across all runs.
    /// </summary>
    public int TotalCoins { get; private set; }

    public int HighScore { get; private set; }

    public bool IsRunFinalized { get; private set; }

    private int bonusScore;

    private float startingZ;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadHighScore();
        LoadTotalCoins();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError(
                "RunnerScoreManager: Player is not assigned."
            );

            return;
        }

        startingZ = player.position.z;

        Distance = 0f;
        Score = 0;
        Coins = 0;
        bonusScore = 0;

        IsRunFinalized = false;
    }

    private void Update()
    {
        if (player == null || IsRunFinalized)
        {
            return;
        }

        UpdateDistance();
        UpdateScore();
    }

    private void UpdateDistance()
    {
        Distance = Mathf.Max(
            0f,
            player.position.z - startingZ
        );
    }

    private void UpdateScore()
    {
        int distanceScore = Mathf.FloorToInt(
            Distance * scoreMultiplier
        );

        Score = distanceScore + bonusScore;
    }

    public void AddCoin(int scoreValue)
    {
        if (IsRunFinalized)
        {
            return;
        }

        // Current run coin count.
        Coins++;

        // Lifetime coin count.
        TotalCoins++;

        // Coin score bonus.
        bonusScore += scoreValue;

        // Save lifetime total immediately.
        SaveTotalCoins();

        Debug.Log(
            "COIN COLLECTED! " +
            "Run Coins: " + Coins +
            " | Total Coins: " + TotalCoins +
            " | Bonus: +" + scoreValue +
            " | Total Score: " + Score
        );
    }

    public void AddBonusScore(int amount)
    {
        if (IsRunFinalized)
        {
            return;
        }

        bonusScore += amount;

        Debug.Log(
            "BONUS SCORE +" + amount +
            " | Total Bonus: " + bonusScore
        );
    }

    public void FinalizeRun()
    {
        if (IsRunFinalized)
        {
            return;
        }

        IsRunFinalized = true;

        UpdateDistance();
        UpdateScore();

        if (Score > HighScore)
        {
            HighScore = Score;

            SaveHighScore();

            Debug.Log(
                "NEW HIGH SCORE! " +
                HighScore
            );
        }

        Debug.Log(
            "RUN FINISHED | " +
            "Score: " + Score +
            " | Distance: " +
            Mathf.FloorToInt(Distance) +
            "m | Run Coins: " +
            Coins +
            " | Total Coins: " +
            TotalCoins +
            " | High Score: " +
            HighScore
        );
    }

    public void ResetRun()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            startingZ = player.position.z;
        }

        Distance = 0f;
        Score = 0;
        Coins = 0;
        bonusScore = 0;

        IsRunFinalized = false;
    }

    private void LoadHighScore()
    {
        HighScore = PlayerPrefs.GetInt(
            highScorePlayerPrefsKey,
            0
        );
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(
            highScorePlayerPrefsKey,
            HighScore
        );

        PlayerPrefs.Save();
    }

    private void LoadTotalCoins()
    {
        TotalCoins = PlayerPrefs.GetInt(
            totalCoinsPlayerPrefsKey,
            0
        );
    }

    private void SaveTotalCoins()
    {
        PlayerPrefs.SetInt(
            totalCoinsPlayerPrefsKey,
            TotalCoins
        );

        PlayerPrefs.Save();
    }

    public int GetTotalCoins()
    {
        return TotalCoins;
    }

    public void ResetHighScore()
    {
        HighScore = 0;

        PlayerPrefs.DeleteKey(
            highScorePlayerPrefsKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "RunnerScoreManager: High Score reset."
        );
    }

    public void ResetTotalCoins()
    {
        TotalCoins = 0;

        PlayerPrefs.DeleteKey(
            totalCoinsPlayerPrefsKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "RunnerScoreManager: Total Coins reset."
        );
    }
}