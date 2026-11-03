using System.Collections.Generic;
using UnityEngine;

public class EndlessRoadManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject roadSegmentPrefab;

    [Header("Road Settings")]
    [SerializeField] private int segmentCount = 6;
    [SerializeField] private float segmentLength = 20f;

    [Header("Spawn Settings")]
    [SerializeField] private float recycleBehindDistance = 30f;

    private readonly List<GameObject> roadSegments =
        new List<GameObject>();

    private float nextSpawnZ;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "EndlessRoadManager: " +
                "Player is not assigned."
            );

            return;
        }

        if (roadSegmentPrefab == null)
        {
            Debug.LogError(
                "EndlessRoadManager: " +
                "Road Segment Prefab is not assigned."
            );

            return;
        }

        CreateInitialRoad();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        RecycleRoadSegments();
    }

    private void CreateInitialRoad()
    {
        roadSegments.Clear();

        nextSpawnZ = 0f;

        for (
            int i = 0;
            i < segmentCount;
            i++
        )
        {
            SpawnRoadSegment();
        }
    }

    private void SpawnRoadSegment()
    {
        GameObject segment =
            Instantiate(
                roadSegmentPrefab,
                new Vector3(
                    0f,
                    0f,
                    nextSpawnZ
                ),
                Quaternion.identity
            );

        roadSegments.Add(
            segment
        );

        nextSpawnZ +=
            segmentLength;
    }

    private void RecycleRoadSegments()
    {
        if (roadSegments.Count == 0)
        {
            return;
        }

        GameObject firstSegment =
            roadSegments[0];

        if (
            firstSegment.transform.position.z +
            segmentLength <
            player.position.z -
            recycleBehindDistance
        )
        {
            MoveSegmentToFront(
                firstSegment
            );
        }
    }

    private void MoveSegmentToFront(
        GameObject segment
    )
    {
        segment.transform.position =
            new Vector3(
                0f,
                0f,
                nextSpawnZ
            );

        nextSpawnZ +=
            segmentLength;

        roadSegments.Remove(
            segment
        );

        roadSegments.Add(
            segment
        );
    }
}