using System.Collections.Generic;
using UnityEngine;

public class EndlessCoinManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RunnerCoin coinPrefab;

    [Header("Pool")]
    [SerializeField] private int poolSize = 24;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 3f;

    [Header("Coin Pattern")]
    [SerializeField] private float coinSpacing = 4f;
    [SerializeField] private float patternSpacing = 12f;
    [SerializeField] private float coinY = 1f;

    [Header("Spawn")]
    [SerializeField] private float startZ = 25f;
    [SerializeField] private float spawnAheadDistance = 100f;
    [SerializeField] private float recycleDistance = 30f;

    [Header("Pattern Generation")]
    [SerializeField] private int maximumPatternsPerFrame = 2;

    [Header("Obstacle Safety")]
    [SerializeField] private float obstacleSafetyRadius = 0.75f;

    private readonly Queue<RunnerCoin> coinPool =
        new Queue<RunnerCoin>();

    private readonly List<RunnerCoin> activeCoins =
        new List<RunnerCoin>();

    private float nextPatternZ;

    private void Awake()
    {
        CreatePool();
    }

    private void Start()
    {
        FindPlayer();

        if (player == null)
        {
            return;
        }

        nextPatternZ =
            player.position.z +
            startZ;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                return;
            }
        }

        RecycleOldCoins();
        SpawnAhead();
    }

    private void FindPlayer()
    {
        if (player != null)
        {
            return;
        }

        RunnerPlayerController runnerPlayer =
            FindFirstObjectByType<RunnerPlayerController>();

        if (runnerPlayer != null)
        {
            player =
                runnerPlayer.transform;
        }
    }

    private void CreatePool()
    {
        if (coinPrefab == null)
        {
            Debug.LogError(
                "EndlessCoinManager: Coin Prefab is missing."
            );

            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            RunnerCoin coin =
                Instantiate(
                    coinPrefab,
                    transform
                );

            coin.gameObject.SetActive(false);

            coinPool.Enqueue(coin);
        }
    }

    private void SpawnAhead()
    {
        float targetZ =
            player.position.z +
            spawnAheadDistance;

        int patternsSpawned = 0;

        while (nextPatternZ < targetZ &&
               patternsSpawned <
               maximumPatternsPerFrame)
        {
            SpawnPattern(nextPatternZ);

            nextPatternZ +=
                patternSpacing;

            patternsSpawned++;
        }
    }

    private void SpawnPattern(float centerZ)
    {
        bool useTwoLanes =
            Random.value > 0.5f;

        if (useTwoLanes)
        {
            SpawnTwoLanePattern(centerZ);
        }
        else
        {
            SpawnSingleLanePattern(centerZ);
        }
    }

    private void SpawnSingleLanePattern(
        float centerZ)
    {
        int lane =
            Random.Range(0, 3);

        SpawnStraightLine(
            lane,
            centerZ
        );
    }

    private void SpawnTwoLanePattern(
        float centerZ)
    {
        int pair =
            Random.Range(0, 3);

        int firstLane;
        int secondLane;

        switch (pair)
        {
            case 0:
                firstLane = 0;
                secondLane = 1;
                break;

            case 1:
                firstLane = 1;
                secondLane = 2;
                break;

            default:
                firstLane = 0;
                secondLane = 2;
                break;
        }

        SpawnStraightLine(
            firstLane,
            centerZ
        );

        SpawnStraightLine(
            secondLane,
            centerZ
        );
    }

    private void SpawnStraightLine(
        int lane,
        float centerZ)
    {
        SpawnCoin(
            lane,
            centerZ
        );

        SpawnCoin(
            lane,
            centerZ + coinSpacing
        );

        SpawnCoin(
            lane,
            centerZ + coinSpacing * 2f
        );
    }

    private void SpawnCoin(
        int lane,
        float z)
    {
        if (coinPool.Count == 0)
        {
            return;
        }

        float x =
            (lane - 1) *
            laneDistance;

        Vector3 spawnPosition =
            new Vector3(
                x,
                coinY,
                z
            );

        if (IsObstacleNearby(spawnPosition))
        {
            return;
        }

        RunnerCoin coin =
            coinPool.Dequeue();

        coin.transform.SetParent(
            transform
        );

        coin.transform.position =
            spawnPosition;

        coin.transform.rotation =
            Quaternion.identity;

        coin.ResetCoin();

        activeCoins.Add(coin);
    }

    private bool IsObstacleNearby(
        Vector3 position)
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                position,
                obstacleSafetyRadius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            RunnerObstacle obstacle =
                colliders[i].GetComponentInParent<RunnerObstacle>();

            if (obstacle != null)
            {
                return true;
            }
        }

        return false;
    }

    private void RecycleOldCoins()
    {
        float recycleZ =
            player.position.z -
            recycleDistance;

        for (int i = activeCoins.Count - 1;
             i >= 0;
             i--)
        {
            RunnerCoin coin =
                activeCoins[i];

            if (coin == null)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            if (!coin.gameObject.activeInHierarchy)
            {
                ReturnCoinToPool(
                    coin,
                    i
                );

                continue;
            }

            if (coin.transform.position.z <
                recycleZ)
            {
                ReturnCoinToPool(
                    coin,
                    i
                );
            }
        }
    }

    private void ReturnCoinToPool(
        RunnerCoin coin,
        int index)
    {
        coin.gameObject.SetActive(false);

        coinPool.Enqueue(coin);

        activeCoins.RemoveAt(index);
    }
}