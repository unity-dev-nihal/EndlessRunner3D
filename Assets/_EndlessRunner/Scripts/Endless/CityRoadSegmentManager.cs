using System.Collections.Generic;
using UnityEngine;

public class CityRoadSegmentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Road Segment Prefabs")]
    [SerializeField] private GameObject[] roadSegmentPrefabs;

    [Header("Segment Settings")]
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int initialSegmentCount = 7;

    [Header("Recycling")]
    [SerializeField] private float recycleDistance = 30f;

    [Header("Performance")]
    [SerializeField] private bool prewarmPool = true;

    private readonly List<GameObject> activeSegments =
        new List<GameObject>();

    private readonly Queue<GameObject> availableSegments =
        new Queue<GameObject>();

    private float nextSpawnZ;

    private int lastSegmentIndex = -1;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "CityRoadSegmentManager: Player is not assigned."
            );

            enabled = false;
            return;
        }

        if (
            roadSegmentPrefabs == null ||
            roadSegmentPrefabs.Length == 0
        )
        {
            Debug.LogError(
                "CityRoadSegmentManager: " +
                "No road segment prefabs assigned."
            );

            enabled = false;
            return;
        }

        if (segmentLength <= 0f)
        {
            Debug.LogError(
                "CityRoadSegmentManager: " +
                "Segment Length must be greater than zero."
            );

            enabled = false;
            return;
        }

        if (initialSegmentCount < 2)
        {
            initialSegmentCount = 2;
        }

        BuildPool();

        SpawnInitialSegments();
    }

    private void Update()
    {
        RecycleOldSegments();
    }

    private void BuildPool()
    {
        if (!prewarmPool)
        {
            return;
        }

        int requiredPoolSize =
            initialSegmentCount + 2;

        for (int i = 0; i < requiredPoolSize; i++)
        {
            int prefabIndex =
                GetRandomSegmentIndex();

            GameObject segment =
                CreateSegmentFromPrefab(
                    prefabIndex
                );

            if (segment == null)
            {
                continue;
            }

            segment.SetActive(false);

            availableSegments.Enqueue(
                segment
            );
        }

        lastSegmentIndex = -1;
    }

    private void SpawnInitialSegments()
    {
        activeSegments.Clear();

        nextSpawnZ = 0f;

        lastSegmentIndex = -1;

        for (
            int i = 0;
            i < initialSegmentCount;
            i++
        )
        {
            SpawnNextSegment();
        }
    }

    private void SpawnNextSegment()
    {
        int segmentIndex =
            GetRandomSegmentIndex();

        GameObject segment =
            GetSegmentFromPool(
                segmentIndex
            );

        if (segment == null)
        {
            Debug.LogError(
                "CityRoadSegmentManager: " +
                "Unable to obtain a road segment from the pool."
            );

            return;
        }

        segment.transform.SetPositionAndRotation(
            new Vector3(
                0f,
                0f,
                nextSpawnZ
            ),
            Quaternion.identity
        );

        segment.SetActive(true);

        activeSegments.Add(
            segment
        );

        nextSpawnZ += segmentLength;

        lastSegmentIndex =
            segmentIndex;
    }

    private GameObject GetSegmentFromPool(
        int preferredPrefabIndex
    )
    {
        GameObject selectedSegment = null;

        int poolCount =
            availableSegments.Count;

        for (int i = 0; i < poolCount; i++)
        {
            GameObject candidate =
                availableSegments.Dequeue();

            if (candidate == null)
            {
                continue;
            }

            CityRoadSegmentIdentity identity =
                candidate.GetComponent<
                    CityRoadSegmentIdentity
                >();

            if (
                selectedSegment == null &&
                identity != null &&
                identity.PrefabIndex ==
                preferredPrefabIndex
            )
            {
                selectedSegment =
                    candidate;

                continue;
            }

            availableSegments.Enqueue(
                candidate
            );
        }

        if (selectedSegment != null)
        {
            return selectedSegment;
        }

        // Fallback to any available segment.
        if (availableSegments.Count > 0)
        {
            selectedSegment =
                availableSegments.Dequeue();
        }

        // If the pool is unexpectedly exhausted,
        // create one only as a safety fallback.
        if (selectedSegment == null)
        {
            selectedSegment =
                CreateSegmentFromPrefab(
                    preferredPrefabIndex
                );
        }

        return selectedSegment;
    }

    private GameObject CreateSegmentFromPrefab(
        int prefabIndex
    )
    {
        if (
            prefabIndex < 0 ||
            prefabIndex >= roadSegmentPrefabs.Length
        )
        {
            return null;
        }

        GameObject prefab =
            roadSegmentPrefabs[prefabIndex];

        if (prefab == null)
        {
            return null;
        }

        GameObject segment =
            Instantiate(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                transform
            );

        CityRoadSegmentIdentity identity =
            segment.GetComponent<
                CityRoadSegmentIdentity
            >();

        if (identity == null)
        {
            identity =
                segment.AddComponent<
                    CityRoadSegmentIdentity
                >();
        }

        identity.SetPrefabIndex(
            prefabIndex
        );

        return segment;
    }

    private int GetRandomSegmentIndex()
    {
        if (roadSegmentPrefabs.Length == 1)
        {
            return 0;
        }

        int selectedIndex =
            Random.Range(
                0,
                roadSegmentPrefabs.Length
            );

        int safetyCounter = 0;

        while (
            selectedIndex == lastSegmentIndex &&
            safetyCounter < 20
        )
        {
            selectedIndex =
                Random.Range(
                    0,
                    roadSegmentPrefabs.Length
                );

            safetyCounter++;
        }

        return selectedIndex;
    }

    private void RecycleOldSegments()
    {
        if (activeSegments.Count == 0)
        {
            return;
        }

        GameObject oldestSegment =
            activeSegments[0];

        if (oldestSegment == null)
        {
            activeSegments.RemoveAt(0);
            return;
        }

        if (
            oldestSegment.transform.position.z <
            player.position.z -
            recycleDistance
        )
        {
            RecycleSegment(
                oldestSegment
            );
        }
    }

    private void RecycleSegment(
        GameObject segment
    )
    {
        if (segment == null)
        {
            return;
        }

        activeSegments.RemoveAt(0);

        segment.SetActive(false);

        availableSegments.Enqueue(
            segment
        );

        SpawnNextSegment();
    }
}

public class CityRoadSegmentIdentity :
    MonoBehaviour
{
    public int PrefabIndex
    {
        get;
        private set;
    }

    public void SetPrefabIndex(
        int index
    )
    {
        PrefabIndex = index;
    }
}