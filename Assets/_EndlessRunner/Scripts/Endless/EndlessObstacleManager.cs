using System.Collections.Generic;
using UnityEngine;

public class EndlessObstacleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Obstacle Count")]
    [Tooltip("Number of obstacle patterns kept active ahead of the player.")]
    [SerializeField] private int obstacleCount = 6;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 3f;

    [Header("Reaction-Friendly Spacing")]
    [Tooltip("Distance between normal obstacle patterns at the beginning.")]
    [SerializeField] private float startingSpacing = 34f;

    [Tooltip("Smallest allowed distance between normal obstacle patterns.")]
    [SerializeField] private float minimumSpacing = 30f;

    [Tooltip("Distance at which the minimum spacing is reached.")]
    [SerializeField] private float distanceForMinimumSpacing = 1000f;

    [Header("Two Obstacle Difficulty")]
    [SerializeField] private float twoObstacleStartDistance = 250f;

    [SerializeField] private float twoObstacleFullDistance = 900f;

    [SerializeField] private float maximumTwoObstacleChance = 0.50f;

    [Header("Lane Switching Difficulty")]
    [SerializeField] private float laneSwitchStartDistance = 500f;

    [SerializeField] private float laneSwitchFullDistance = 1200f;

    [SerializeField] private float maximumLaneSwitchChance = 0.20f;

    [Header("Lane Switch Pattern")]
    [Tooltip("Percentage of normal spacing used between lane-switch groups.")]
    [SerializeField] private float laneSwitchGapMultiplier = 0.85f;

    [Tooltip("Absolute minimum distance between lane-switch groups.")]
    [SerializeField] private float minimumLaneSwitchGap = 26f;

    [Header("Position Settings")]
    [SerializeField] private float obstacleY = 1f;

    [Tooltip("Distance of the first obstacle pattern in front of the player.")]
    [SerializeField] private float startZ = 35f;

    [Tooltip("Distance behind the player before an old pattern is recycled.")]
    [SerializeField] private float recycleDistance = 35f;

    [Header("Pool Settings")]
    [Tooltip("Extra distance behind the player used when hiding unused pooled obstacles.")]
    [SerializeField] private float hiddenObstacleDistance = 250f;

    private const int MaximumObjectsPerPattern = 4;

    private readonly List<ObstaclePattern> activePatterns =
        new List<ObstaclePattern>();

    private readonly Queue<ObstaclePattern> availablePatterns =
        new Queue<ObstaclePattern>();

    private float nextSpawnZ;

    private int lastSafeLane = -1;

    private class ObstaclePattern
    {
        public readonly GameObject[] Objects =
            new GameObject[MaximumObjectsPerPattern];

        public int ActiveObjectCount;

        public float Z;

        public bool IsActive;
    }

    public static EndlessObstacleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ValidateSettings();

        CreatePool();
    }

    private void Start()
    {
        FindPlayer();

        if (player == null)
        {
            Debug.LogError(
                "EndlessObstacleManager: " +
                "Player is not assigned."
            );

            return;
        }

        if (!HasValidObstaclePrefabs())
        {
            Debug.LogError(
                "EndlessObstacleManager: " +
                "No valid obstacle prefabs are assigned."
            );

            return;
        }

        CreateInitialPatterns();
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

        RecycleOldPatterns();
    }

    private void ValidateSettings()
    {
        obstacleCount =
            Mathf.Max(
                1,
                obstacleCount
            );

        laneDistance =
            Mathf.Max(
                0.1f,
                laneDistance
            );

        startingSpacing =
            Mathf.Max(
                1f,
                startingSpacing
            );

        minimumSpacing =
            Mathf.Max(
                1f,
                minimumSpacing
            );

        if (minimumSpacing > startingSpacing)
        {
            minimumSpacing =
                startingSpacing;
        }

        distanceForMinimumSpacing =
            Mathf.Max(
                1f,
                distanceForMinimumSpacing
            );

        startZ =
            Mathf.Max(
                1f,
                startZ
            );

        recycleDistance =
            Mathf.Max(
                1f,
                recycleDistance
            );

        hiddenObstacleDistance =
            Mathf.Max(
                50f,
                hiddenObstacleDistance
            );

        maximumTwoObstacleChance =
            Mathf.Clamp01(
                maximumTwoObstacleChance
            );

        maximumLaneSwitchChance =
            Mathf.Clamp01(
                maximumLaneSwitchChance
            );

        laneSwitchGapMultiplier =
            Mathf.Clamp(
                laneSwitchGapMultiplier,
                0.1f,
                1f
            );

        minimumLaneSwitchGap =
            Mathf.Max(
                1f,
                minimumLaneSwitchGap
            );
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

    private bool HasValidObstaclePrefabs()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return false;
        }

        for (int i = 0;
             i < obstaclePrefabs.Length;
             i++)
        {
            if (obstaclePrefabs[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private void CreatePool()
    {
        if (!HasValidObstaclePrefabs())
        {
            return;
        }

        for (int patternIndex = 0;
             patternIndex < obstacleCount;
             patternIndex++)
        {
            ObstaclePattern pattern =
                new ObstaclePattern();

            for (int objectIndex = 0;
                 objectIndex < MaximumObjectsPerPattern;
                 objectIndex++)
            {
                GameObject prefab =
                    GetRandomObstaclePrefab();

                if (prefab == null)
                {
                    continue;
                }

                GameObject obstacle =
                    Instantiate(
                        prefab,
                        transform
                    );

                obstacle.name =
                    "Obstacle_Pooled_" +
                    patternIndex +
                    "_" +
                    objectIndex;

                obstacle.SetActive(false);

                pattern.Objects[objectIndex] =
                    obstacle;
            }

            pattern.ActiveObjectCount = 0;
            pattern.Z = 0f;
            pattern.IsActive = false;

            availablePatterns.Enqueue(
                pattern
            );
        }

        Debug.Log(
            "EndlessObstacleManager: " +
            "Obstacle pool created. Patterns: " +
            obstacleCount
        );
    }

    private void CreateInitialPatterns()
    {
        activePatterns.Clear();

        nextSpawnZ =
            player.position.z +
            startZ;

        for (int i = 0;
             i < obstacleCount;
             i++)
        {
            ObstaclePattern pattern =
                GetAvailablePattern();

            if (pattern == null)
            {
                Debug.LogError(
                    "EndlessObstacleManager: " +
                    "Obstacle pattern pool is exhausted."
                );

                break;
            }

            CreatePattern(
                pattern
            );
        }
    }

    private ObstaclePattern GetAvailablePattern()
    {
        if (availablePatterns.Count == 0)
        {
            return null;
        }

        return availablePatterns.Dequeue();
    }

    private void ReturnPatternToPool(
        ObstaclePattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        for (int i = 0;
             i < pattern.Objects.Length;
             i++)
        {
            GameObject obstacle =
                pattern.Objects[i];

            if (obstacle == null)
            {
                continue;
            }

            obstacle.SetActive(false);
        }

        pattern.ActiveObjectCount = 0;
        pattern.Z = 0f;
        pattern.IsActive = false;

        availablePatterns.Enqueue(
            pattern
        );
    }

    private void CreatePattern(
        ObstaclePattern pattern)
    {
        float roll =
            Random.value;

        float laneSwitchChance =
            GetLaneSwitchChance();

        float twoObstacleChance =
            GetTwoObstacleChance();

        if (roll < laneSwitchChance)
        {
            CreateLaneSwitchPattern(
                pattern
            );
        }
        else if (roll <
                 laneSwitchChance +
                 twoObstacleChance)
        {
            CreateTwoObstaclePattern(
                pattern
            );
        }
        else
        {
            CreateSingleObstaclePattern(
                pattern
            );
        }
    }

    private void CreateSingleObstaclePattern(
        ObstaclePattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        int blockedLane =
            Random.Range(
                0,
                3
            );

        SetObstacle(
            pattern,
            0,
            blockedLane,
            nextSpawnZ
        );

        HideUnusedObjects(
            pattern,
            1
        );

        pattern.ActiveObjectCount = 1;
        pattern.Z = nextSpawnZ;
        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            GetSafeLaneFromBlockedLane(
                blockedLane
            );

        nextSpawnZ +=
            GetCurrentSpacing();
    }

    private void CreateTwoObstaclePattern(
        ObstaclePattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        int openLane =
            GetDifferentSafeLane();

        int objectIndex = 0;

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == openLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                nextSpawnZ
            );

            objectIndex++;
        }

        HideUnusedObjects(
            pattern,
            objectIndex
        );

        pattern.ActiveObjectCount =
            objectIndex;

        pattern.Z =
            nextSpawnZ;

        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            openLane;

        nextSpawnZ +=
            GetCurrentSpacing();
    }

    private void CreateLaneSwitchPattern(
        ObstaclePattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        int firstSafeLane =
            GetDifferentSafeLane();

        int secondSafeLane =
            GetDifferentLane(
                firstSafeLane
            );

        float secondPatternZ =
            nextSpawnZ +
            GetLaneSwitchGap();

        int objectIndex = 0;

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == firstSafeLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                nextSpawnZ
            );

            objectIndex++;
        }

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == secondSafeLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                secondPatternZ
            );

            objectIndex++;
        }

        HideUnusedObjects(
            pattern,
            objectIndex
        );

        pattern.ActiveObjectCount =
            objectIndex;

        pattern.Z =
            nextSpawnZ;

        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            secondSafeLane;

        nextSpawnZ =
            secondPatternZ +
            GetCurrentSpacing();
    }

    private void SetObstacle(
        ObstaclePattern pattern,
        int objectIndex,
        int lane,
        float zPosition)
    {
        if (pattern == null)
        {
            return;
        }

        if (objectIndex < 0 ||
            objectIndex >= pattern.Objects.Length)
        {
            return;
        }

        GameObject obstacle =
            pattern.Objects[objectIndex];

        if (obstacle == null)
        {
            return;
        }

        float laneX =
            (lane - 1) *
            laneDistance;

        obstacle.transform.SetPositionAndRotation(
            new Vector3(
                laneX,
                obstacleY,
                zPosition
            ),
            Quaternion.identity
        );

        obstacle.SetActive(true);
    }

    private void HideUnusedObjects(
        ObstaclePattern pattern,
        int startingIndex)
    {
        if (pattern == null)
        {
            return;
        }

        for (int i = startingIndex;
             i < pattern.Objects.Length;
             i++)
        {
            GameObject obstacle =
                pattern.Objects[i];

            if (obstacle == null)
            {
                continue;
            }

            obstacle.transform.position =
                new Vector3(
                    0f,
                    obstacleY,
                    player.position.z -
                    hiddenObstacleDistance
                );

            obstacle.SetActive(false);
        }
    }

    private void RecycleOldPatterns()
    {
        if (activePatterns.Count == 0)
        {
            return;
        }

        while (activePatterns.Count > 0)
        {
            ObstaclePattern oldestPattern =
                activePatterns[0];

            if (oldestPattern == null)
            {
                activePatterns.RemoveAt(0);
                continue;
            }

            float recycleZ =
                player.position.z -
                recycleDistance;

            if (oldestPattern.Z >= recycleZ)
            {
                break;
            }

            activePatterns.RemoveAt(0);

            RecyclePattern(
                oldestPattern
            );
        }
    }

    private void RecyclePattern(
        ObstaclePattern pattern)
    {
        if (pattern == null)
        {
            return;
        }

        float roll =
            Random.value;

        float laneSwitchChance =
            GetLaneSwitchChance();

        float twoObstacleChance =
            GetTwoObstacleChance();

        if (roll < laneSwitchChance)
        {
            RecycleAsLaneSwitchPattern(
                pattern
            );
        }
        else if (roll <
                 laneSwitchChance +
                 twoObstacleChance)
        {
            RecycleAsTwoObstaclePattern(
                pattern
            );
        }
        else
        {
            RecycleAsSingleObstaclePattern(
                pattern
            );
        }
    }

    private void RecycleAsSingleObstaclePattern(
        ObstaclePattern pattern)
    {
        int blockedLane =
            Random.Range(
                0,
                3
            );

        SetObstacle(
            pattern,
            0,
            blockedLane,
            nextSpawnZ
        );

        HideUnusedObjects(
            pattern,
            1
        );

        pattern.ActiveObjectCount = 1;
        pattern.Z = nextSpawnZ;
        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            GetSafeLaneFromBlockedLane(
                blockedLane
            );

        nextSpawnZ +=
            GetCurrentSpacing();
    }

    private void RecycleAsTwoObstaclePattern(
        ObstaclePattern pattern)
    {
        int openLane =
            GetDifferentSafeLane();

        int objectIndex = 0;

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == openLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                nextSpawnZ
            );

            objectIndex++;
        }

        HideUnusedObjects(
            pattern,
            objectIndex
        );

        pattern.ActiveObjectCount =
            objectIndex;

        pattern.Z =
            nextSpawnZ;

        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            openLane;

        nextSpawnZ +=
            GetCurrentSpacing();
    }

    private void RecycleAsLaneSwitchPattern(
        ObstaclePattern pattern)
    {
        int firstSafeLane =
            GetDifferentSafeLane();

        int secondSafeLane =
            GetDifferentLane(
                firstSafeLane
            );

        float secondPatternZ =
            nextSpawnZ +
            GetLaneSwitchGap();

        int objectIndex = 0;

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == firstSafeLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                nextSpawnZ
            );

            objectIndex++;
        }

        for (int lane = 0;
             lane < 3;
             lane++)
        {
            if (lane == secondSafeLane)
            {
                continue;
            }

            SetObstacle(
                pattern,
                objectIndex,
                lane,
                secondPatternZ
            );

            objectIndex++;
        }

        HideUnusedObjects(
            pattern,
            objectIndex
        );

        pattern.ActiveObjectCount =
            objectIndex;

        pattern.Z =
            nextSpawnZ;

        pattern.IsActive = true;

        activePatterns.Add(
            pattern
        );

        lastSafeLane =
            secondSafeLane;

        nextSpawnZ =
            secondPatternZ +
            GetCurrentSpacing();
    }

    private int GetDifferentSafeLane()
    {
        int safeLane =
            Random.Range(
                0,
                3
            );

        if (lastSafeLane == -1)
        {
            return safeLane;
        }

        int attempts = 0;

        while (safeLane == lastSafeLane &&
               attempts < 10)
        {
            safeLane =
                Random.Range(
                    0,
                    3
                );

            attempts++;
        }

        return safeLane;
    }

    private int GetDifferentLane(
        int previousLane)
    {
        int lane =
            Random.Range(
                0,
                3
            );

        while (lane == previousLane)
        {
            lane =
                Random.Range(
                    0,
                    3
                );
        }

        return lane;
    }

    private int GetSafeLaneFromBlockedLane(
        int blockedLane)
    {
        int safeLane =
            Random.Range(
                0,
                3
            );

        while (safeLane == blockedLane)
        {
            safeLane =
                Random.Range(
                    0,
                    3
                );
        }

        return safeLane;
    }

    private float GetLaneSwitchGap()
    {
        float spacing =
            GetCurrentSpacing();

        float gap =
            spacing *
            laneSwitchGapMultiplier;

        return Mathf.Max(
            gap,
            minimumLaneSwitchGap
        );
    }

    private float GetCurrentSpacing()
    {
        float distance = 0f;

        if (RunnerScoreManager.Instance != null)
        {
            distance =
                RunnerScoreManager.Instance.Distance;
        }

        float progress =
            Mathf.Clamp01(
                distance /
                distanceForMinimumSpacing
            );

        float curvedProgress =
            Mathf.Pow(
                progress,
                0.75f
            );

        return Mathf.Lerp(
            startingSpacing,
            minimumSpacing,
            curvedProgress
        );
    }

    private float GetTwoObstacleChance()
    {
        float distance = 0f;

        if (RunnerScoreManager.Instance != null)
        {
            distance =
                RunnerScoreManager.Instance.Distance;
        }

        if (distance <=
            twoObstacleStartDistance)
        {
            return 0f;
        }

        float progress =
            Mathf.InverseLerp(
                twoObstacleStartDistance,
                twoObstacleFullDistance,
                distance
            );

        return Mathf.Lerp(
            0f,
            maximumTwoObstacleChance,
            progress
        );
    }

    private float GetLaneSwitchChance()
    {
        float distance = 0f;

        if (RunnerScoreManager.Instance != null)
        {
            distance =
                RunnerScoreManager.Instance.Distance;
        }

        if (distance <=
            laneSwitchStartDistance)
        {
            return 0f;
        }

        float progress =
            Mathf.InverseLerp(
                laneSwitchStartDistance,
                laneSwitchFullDistance,
                distance
            );

        return Mathf.Lerp(
            0f,
            maximumLaneSwitchChance,
            progress
        );
    }

    private GameObject GetRandomObstaclePrefab()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return null;
        }

        int validCount = 0;

        for (int i = 0;
             i < obstaclePrefabs.Length;
             i++)
        {
            if (obstaclePrefabs[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        int selectedIndex =
            Random.Range(
                0,
                validCount
            );

        int currentValidIndex = 0;

        for (int i = 0;
             i < obstaclePrefabs.Length;
             i++)
        {
            GameObject prefab =
                obstaclePrefabs[i];

            if (prefab == null)
            {
                continue;
            }

            if (currentValidIndex ==
                selectedIndex)
            {
                return prefab;
            }

            currentValidIndex++;
        }

        return null;
    }

    public float GetCurrentObstacleSpacing()
    {
        return GetCurrentSpacing();
    }

    public float GetNextObstacleZ()
    {
        return nextSpawnZ;
    }

    public float GetLaneSwitchSpacing()
    {
        return GetLaneSwitchGap();
    }

    public int GetActivePatternCount()
    {
        return activePatterns.Count;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}