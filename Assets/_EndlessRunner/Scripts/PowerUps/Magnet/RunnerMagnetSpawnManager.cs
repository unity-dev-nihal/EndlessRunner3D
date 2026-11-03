using UnityEngine;

public class RunnerMagnetSpawnManager : MonoBehaviour
{
    public static RunnerMagnetSpawnManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RunnerPlayerController player;
    [SerializeField] private RunnerMagnetPickup magnetPrefab;

    [Header("Spawn Distance")]
    [SerializeField] private float firstSpawnMinimumDistance = 500f;
    [SerializeField] private float firstSpawnMaximumDistance = 700f;

    [SerializeField] private float nextSpawnMinimumDistance = 500f;
    [SerializeField] private float nextSpawnMaximumDistance = 800f;

    [Header("Spawn Position")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private float spawnHeight = 1.2f;

    [Header("Obstacle Safety")]
    [SerializeField] private float obstacleCheckRadius = 2.2f;
    [SerializeField] private float minimumObstacleForwardClearance = 5f;
    [SerializeField] private float minimumObstacleSideClearance = 2.5f;

    [Header("Spawn Attempts")]
    [SerializeField] private int maximumSpawnAttempts = 20;
    [SerializeField] private float spawnZRandomOffset = 10f;

    [Header("Cleanup")]
    [SerializeField] private float removeBehindPlayerDistance = 25f;

    private float nextSpawnDistance;
    private RunnerMagnetPickup activeMagnet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        FindPlayer();

        if (magnetPrefab == null)
        {
            Debug.LogError(
                "RunnerMagnetSpawnManager: Magnet Prefab is NOT assigned."
            );
        }

        ScheduleFirstSpawn();
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

        if (RunnerGameManager.Instance != null &&
            RunnerGameManager.Instance.IsGameOver)
        {
            return;
        }

        if (RunnerScoreManager.Instance == null)
        {
            return;
        }

        HandleActiveMagnet();

        if (activeMagnet != null)
        {
            return;
        }

        float currentDistance =
            RunnerScoreManager.Instance.Distance;

        if (currentDistance < nextSpawnDistance)
        {
            return;
        }

        TrySpawnMagnet();
    }

    private void FindPlayer()
    {
        if (player != null)
        {
            return;
        }

        player =
            FindFirstObjectByType<RunnerPlayerController>();

        if (player == null)
        {
            Debug.LogWarning(
                "RunnerMagnetSpawnManager: " +
                "RunnerPlayerController not found."
            );
        }
    }

    private void ScheduleFirstSpawn()
    {
        float minimum =
            Mathf.Max(1f, firstSpawnMinimumDistance);

        float maximum =
            Mathf.Max(
                minimum,
                firstSpawnMaximumDistance
            );

        nextSpawnDistance =
            Random.Range(minimum, maximum);

        Debug.Log(
            "MAGNET FIRST SPAWN TARGET: " +
            nextSpawnDistance.ToString("F0") +
            "m"
        );
    }

    private void ScheduleNextSpawn()
    {
        if (RunnerScoreManager.Instance == null)
        {
            return;
        }

        float minimum =
            Mathf.Max(1f, nextSpawnMinimumDistance);

        float maximum =
            Mathf.Max(
                minimum,
                nextSpawnMaximumDistance
            );

        float distanceGap =
            Random.Range(minimum, maximum);

        float currentDistance =
            RunnerScoreManager.Instance.Distance;

        nextSpawnDistance =
            currentDistance + distanceGap;

        Debug.Log(
            "NEXT MAGNET TARGET: " +
            nextSpawnDistance.ToString("F0") +
            "m"
        );
    }

    private void TrySpawnMagnet()
    {
        if (magnetPrefab == null)
        {
            Debug.LogError(
                "RunnerMagnetSpawnManager: " +
                "Magnet Prefab is missing."
            );

            ScheduleNextSpawn();
            return;
        }

        if (FindSafeSpawnPosition(
                out Vector3 spawnPosition))
        {
            activeMagnet =
                Instantiate(
                    magnetPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            activeMagnet.name =
                "Magnet_PowerUp_Active";

            Debug.Log(
                "MAGNET SPAWNED SAFELY | Position: " +
                spawnPosition
            );

            ScheduleNextSpawn();

            return;
        }

        /*
         * The current road section is unsuitable.
         * Retry shortly instead of losing the power-up.
         */
        float currentDistance =
            RunnerScoreManager.Instance != null
                ? RunnerScoreManager.Instance.Distance
                : 0f;

        nextSpawnDistance =
            currentDistance + 50f;

        Debug.LogWarning(
            "MAGNET: No safe position found. " +
            "Retrying in 50m."
        );
    }

    private bool FindSafeSpawnPosition(
        out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        if (player == null)
        {
            return false;
        }

        Vector3 playerPosition =
            player.transform.position;

        float targetZ =
            playerPosition.z +
            spawnAheadDistance;

        for (int attempt = 0;
             attempt < maximumSpawnAttempts;
             attempt++)
        {
            int lane =
                Random.Range(0, 3);

            float x =
                (lane - 1) * laneDistance;

            float zOffset =
                Random.Range(
                    -spawnZRandomOffset,
                    spawnZRandomOffset
                );

            float z =
                targetZ + zOffset;

            Vector3 candidate =
                new Vector3(
                    x,
                    spawnHeight,
                    z
                );

            /*
             * Never spawn too close to the player.
             */
            if (z <
                playerPosition.z + 30f)
            {
                continue;
            }

            /*
             * Check for actual obstacles around
             * the Magnet.
             */
            if (IsObstacleTooClose(candidate))
            {
                continue;
            }

            spawnPosition = candidate;

            return true;
        }

        return false;
    }

    private bool IsObstacleTooClose(
        Vector3 position)
    {
        Collider[] colliders =
            Physics.OverlapSphere(
                position,
                obstacleCheckRadius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            Collider collider =
                colliders[i];

            if (collider == null)
            {
                continue;
            }

            RunnerObstacle obstacle =
                collider.GetComponentInParent<RunnerObstacle>();

            if (obstacle == null)
            {
                continue;
            }

            Bounds obstacleBounds =
                collider.bounds;

            float horizontalDistance =
                Vector2.Distance(
                    new Vector2(
                        position.x,
                        position.z
                    ),
                    new Vector2(
                        obstacleBounds.center.x,
                        obstacleBounds.center.z
                    )
                );

            float forwardDistance =
                Mathf.Abs(
                    position.z -
                    obstacleBounds.center.z
                );

            float sideDistance =
                Mathf.Abs(
                    position.x -
                    obstacleBounds.center.x
                );

            /*
             * Strong forward safety check.
             */
            if (forwardDistance <
                minimumObstacleForwardClearance)
            {
                return true;
            }

            /*
             * Strong side clearance check.
             */
            if (sideDistance <
                minimumObstacleSideClearance &&
                horizontalDistance <
                obstacleCheckRadius + 1f)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleActiveMagnet()
    {
        if (activeMagnet == null)
        {
            return;
        }

        /*
         * Pickup disables itself after collection.
         */
        if (!activeMagnet.gameObject.activeInHierarchy)
        {
            Destroy(activeMagnet.gameObject);

            activeMagnet = null;

            Debug.Log(
                "MAGNET COLLECTED"
            );

            ScheduleNextSpawn();

            return;
        }

        float playerZ =
            player.transform.position.z;

        float magnetZ =
            activeMagnet.transform.position.z;

        if (magnetZ <
            playerZ - removeBehindPlayerDistance)
        {
            Debug.Log(
                "MAGNET MISSED - REMOVING"
            );

            Destroy(activeMagnet.gameObject);

            activeMagnet = null;

            ScheduleNextSpawn();
        }
    }

    public void ForceSpawnMagnet()
    {
        if (activeMagnet != null)
        {
            return;
        }

        if (magnetPrefab == null)
        {
            Debug.LogError(
                "RunnerMagnetSpawnManager: " +
                "Magnet Prefab is missing."
            );

            return;
        }

        if (!FindSafeSpawnPosition(
                out Vector3 spawnPosition))
        {
            Debug.LogWarning(
                "MAGNET FORCE SPAWN FAILED - " +
                "NO SAFE POSITION."
            );

            return;
        }

        activeMagnet =
            Instantiate(
                magnetPrefab,
                spawnPosition,
                Quaternion.identity
            );

        activeMagnet.name =
            "Magnet_PowerUp_Active";

        Debug.Log(
            "MAGNET FORCE SPAWNED SAFELY"
        );
    }

    public float GetNextSpawnDistance()
    {
        return nextSpawnDistance;
    }

    public bool HasActiveMagnet()
    {
        return activeMagnet != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }

        Vector3 previewPosition =
            player.transform.position +
            Vector3.forward *
            spawnAheadDistance;

        Gizmos.DrawWireSphere(
            previewPosition,
            obstacleCheckRadius
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}