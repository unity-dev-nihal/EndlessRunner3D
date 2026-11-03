using UnityEngine;

public class RunnerPhaseSpawnManager : MonoBehaviour
{
    public static RunnerPhaseSpawnManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RunnerPlayerController player;
    [SerializeField] private RunnerPhasePickup phasePrefab;

    [Header("Spawn Distance")]
    [SerializeField] private float firstSpawnMinimumDistance = 300f;
    [SerializeField] private float firstSpawnMaximumDistance = 500f;

    [SerializeField] private float nextSpawnMinimumDistance = 400f;
    [SerializeField] private float nextSpawnMaximumDistance = 650f;

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
    private RunnerPhasePickup activePhase;

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

        if (phasePrefab == null)
        {
            Debug.LogError(
                "RunnerPhaseSpawnManager: " +
                "Phase Prefab is NOT assigned."
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

        HandleActivePhase();

        if (activePhase != null)
        {
            return;
        }

        float currentDistance =
            RunnerScoreManager.Instance.Distance;

        if (currentDistance < nextSpawnDistance)
        {
            return;
        }

        TrySpawnPhase();
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
                "RunnerPhaseSpawnManager: " +
                "RunnerPlayerController not found."
            );
        }
    }

    private void ScheduleFirstSpawn()
    {
        float minimum =
            Mathf.Max(
                1f,
                firstSpawnMinimumDistance
            );

        float maximum =
            Mathf.Max(
                minimum,
                firstSpawnMaximumDistance
            );

        nextSpawnDistance =
            Random.Range(
                minimum,
                maximum
            );

        Debug.Log(
            "PHASE FIRST SPAWN TARGET: " +
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
            Mathf.Max(
                1f,
                nextSpawnMinimumDistance
            );

        float maximum =
            Mathf.Max(
                minimum,
                nextSpawnMaximumDistance
            );

        float distanceGap =
            Random.Range(
                minimum,
                maximum
            );

        float currentDistance =
            RunnerScoreManager.Instance.Distance;

        nextSpawnDistance =
            currentDistance +
            distanceGap;

        Debug.Log(
            "NEXT PHASE TARGET: " +
            nextSpawnDistance.ToString("F0") +
            "m"
        );
    }

    private void TrySpawnPhase()
    {
        if (phasePrefab == null)
        {
            Debug.LogError(
                "RunnerPhaseSpawnManager: " +
                "Phase Prefab is missing."
            );

            ScheduleNextSpawn();

            return;
        }

        if (FindSafeSpawnPosition(
                out Vector3 spawnPosition))
        {
            activePhase =
                Instantiate(
                    phasePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            activePhase.name =
                "Phase_PowerUp_Active";

            Debug.Log(
                "PHASE SPAWNED SAFELY | Position: " +
                spawnPosition
            );

            ScheduleNextSpawn();

            return;
        }

        float currentDistance =
            RunnerScoreManager.Instance != null
                ? RunnerScoreManager.Instance.Distance
                : 0f;

        nextSpawnDistance =
            currentDistance + 50f;

        Debug.LogWarning(
            "PHASE: No safe position found. " +
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
                (lane - 1) *
                laneDistance;

            float zOffset =
                Random.Range(
                    -spawnZRandomOffset,
                    spawnZRandomOffset
                );

            float z =
                targetZ +
                zOffset;

            Vector3 candidate =
                new Vector3(
                    x,
                    spawnHeight,
                    z
                );

            if (z <
                playerPosition.z + 30f)
            {
                continue;
            }

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

            if (forwardDistance <
                minimumObstacleForwardClearance)
            {
                return true;
            }

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

    private void HandleActivePhase()
    {
        if (activePhase == null)
        {
            return;
        }

        if (!activePhase.gameObject.activeInHierarchy)
        {
            Destroy(
                activePhase.gameObject
            );

            activePhase = null;

            Debug.Log(
                "PHASE COLLECTED"
            );

            ScheduleNextSpawn();

            return;
        }

        float playerZ =
            player.transform.position.z;

        float phaseZ =
            activePhase.transform.position.z;

        if (phaseZ <
            playerZ -
            removeBehindPlayerDistance)
        {
            Debug.Log(
                "PHASE MISSED - REMOVING"
            );

            Destroy(
                activePhase.gameObject
            );

            activePhase = null;

            ScheduleNextSpawn();
        }
    }

    public void ForceSpawnPhase()
    {
        if (activePhase != null)
        {
            return;
        }

        if (phasePrefab == null)
        {
            Debug.LogError(
                "RunnerPhaseSpawnManager: " +
                "Phase Prefab is missing."
            );

            return;
        }

        if (!FindSafeSpawnPosition(
                out Vector3 spawnPosition))
        {
            Debug.LogWarning(
                "PHASE FORCE SPAWN FAILED - " +
                "NO SAFE POSITION."
            );

            return;
        }

        activePhase =
            Instantiate(
                phasePrefab,
                spawnPosition,
                Quaternion.identity
            );

        activePhase.name =
            "Phase_PowerUp_Active";

        Debug.Log(
            "PHASE FORCE SPAWNED SAFELY"
        );
    }

    public float GetNextSpawnDistance()
    {
        return nextSpawnDistance;
    }

    public bool HasActivePhase()
    {
        return activePhase != null;
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