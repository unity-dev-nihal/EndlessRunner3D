using System.Collections.Generic;
using UnityEngine;

public class RunnerPhasePowerUp : MonoBehaviour
{
    public static RunnerPhasePowerUp Instance { get; private set; }

    [Header("Phase Settings")]
    [SerializeField] private float duration = 5f;

    [Header("Ghost Visual")]
    [Tooltip("Optional. Leave empty to automatically detect the currently active character.")]
    [SerializeField] private Transform visualRoot;

    [Tooltip("Ghost material used while Phase is active.")]
    [SerializeField] private Material ghostMaterial;

    private CharacterController playerCharacterController;

    private readonly HashSet<Collider> ignoredObstacleColliders =
        new HashSet<Collider>();

    private readonly Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();

    private SkinnedMeshRenderer[] characterRenderers;

    private bool phaseActive;
    private float remainingTime;
    private bool ghostVisualApplied;

    public bool IsActive
    {
        get { return phaseActive; }
    }

    public float RemainingTime
    {
        get { return remainingTime; }
    }

    public float Duration
    {
        get { return duration; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        duration = Mathf.Max(0.1f, duration);

        playerCharacterController =
            GetComponent<CharacterController>();

        if (playerCharacterController == null)
        {
            playerCharacterController =
                GetComponentInParent<CharacterController>();
        }

        if (playerCharacterController == null)
        {
            Debug.LogError(
                "RunnerPhasePowerUp: CharacterController could not be found."
            );
        }

        CacheCharacterRenderers();
    }

    private void Update()
    {
        if (!phaseActive)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        IgnoreObstacleCollisions();

        if (remainingTime <= 0f)
        {
            StopPhase();
        }
    }

    private void CacheCharacterRenderers()
    {
        if (visualRoot == null)
        {
            visualRoot = FindActiveCharacterRoot();
        }

        if (visualRoot == null)
        {
            Debug.LogWarning(
                "RunnerPhasePowerUp: Could not find an active character visual root."
            );

            return;
        }

        characterRenderers =
            visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(
                true
            );

        if (characterRenderers == null ||
            characterRenderers.Length == 0)
        {
            Debug.LogWarning(
                "RunnerPhasePowerUp: No SkinnedMeshRenderer components were found " +
                "under the active character."
            );

            return;
        }

        originalMaterials.Clear();

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            SkinnedMeshRenderer renderer =
                characterRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            originalMaterials[renderer] =
                renderer.sharedMaterials;
        }

        Debug.Log(
            "RunnerPhasePowerUp: Cached " +
            characterRenderers.Length +
            " renderer(s) for character '" +
            visualRoot.name +
            "'."
        );
    }

    private Transform FindActiveCharacterRoot()
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            Transform candidate = children[i];

            if (candidate == transform)
            {
                continue;
            }

            if (!candidate.gameObject.activeInHierarchy)
            {
                continue;
            }

            SkinnedMeshRenderer[] renderers =
                candidate.GetComponentsInChildren<SkinnedMeshRenderer>(
                    true
                );

            if (renderers == null || renderers.Length == 0)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private void ApplyGhostVisual()
    {
        if (ghostVisualApplied)
        {
            return;
        }

        if (ghostMaterial == null)
        {
            Debug.LogError(
                "RunnerPhasePowerUp: Ghost Material is not assigned."
            );

            return;
        }

        if (visualRoot == null ||
            !visualRoot.gameObject.activeInHierarchy)
        {
            visualRoot = null;
            characterRenderers = null;

            CacheCharacterRenderers();
        }

        if (characterRenderers == null ||
            characterRenderers.Length == 0)
        {
            CacheCharacterRenderers();
        }

        if (characterRenderers == null ||
            characterRenderers.Length == 0)
        {
            Debug.LogWarning(
                "RunnerPhasePowerUp: No character renderers available " +
                "for Ghost Visual."
            );

            return;
        }

        originalMaterials.Clear();

        for (int i = 0; i < characterRenderers.Length; i++)
        {
            SkinnedMeshRenderer renderer =
                characterRenderers[i];

            if (renderer == null)
            {
                continue;
            }

            Material[] currentMaterials =
                renderer.sharedMaterials;

            if (currentMaterials == null ||
                currentMaterials.Length == 0)
            {
                continue;
            }

            originalMaterials[renderer] =
                currentMaterials;

            Material[] ghostMaterials =
                new Material[currentMaterials.Length];

            for (int j = 0; j < ghostMaterials.Length; j++)
            {
                ghostMaterials[j] = ghostMaterial;
            }

            renderer.sharedMaterials =
                ghostMaterials;
        }

        ghostVisualApplied = true;

        Debug.Log(
            "PHASE GHOST VISUAL ACTIVATED | Character: " +
            (visualRoot != null ? visualRoot.name : "Unknown")
        );
    }

    private void RestoreOriginalVisual()
    {
        if (!ghostVisualApplied)
        {
            return;
        }

        foreach (
            KeyValuePair<Renderer, Material[]> pair
            in originalMaterials)
        {
            Renderer renderer =
                pair.Key;

            if (renderer == null)
            {
                continue;
            }

            Material[] materials =
                pair.Value;

            if (materials == null)
            {
                continue;
            }

            renderer.sharedMaterials =
                materials;
        }

        originalMaterials.Clear();

        ghostVisualApplied = false;

        Debug.Log(
            "PHASE GHOST VISUAL RESTORED"
        );
    }

    public void ActivatePhase()
    {
        duration = Mathf.Max(0.1f, duration);

        remainingTime = duration;

        phaseActive = true;

        RefreshCharacterVisual();

        ApplyGhostVisual();

        IgnoreObstacleCollisions();

        Debug.Log(
            "PHASE ACTIVATED | Duration: " +
            duration.ToString("F1") +
            "s"
        );
    }

    private void RefreshCharacterVisual()
    {
        if (ghostVisualApplied)
        {
            return;
        }

        visualRoot = FindActiveCharacterRoot();

        characterRenderers = null;

        CacheCharacterRenderers();
    }

    private void IgnoreObstacleCollisions()
    {
        if (!phaseActive)
        {
            return;
        }

        if (playerCharacterController == null)
        {
            playerCharacterController =
                GetComponent<CharacterController>();

            if (playerCharacterController == null)
            {
                playerCharacterController =
                    GetComponentInParent<CharacterController>();
            }
        }

        if (playerCharacterController == null)
        {
            return;
        }

        RunnerObstacle[] obstacles =
            FindObjectsByType<RunnerObstacle>(
                FindObjectsSortMode.None
            );

        for (int i = 0; i < obstacles.Length; i++)
        {
            RunnerObstacle obstacle =
                obstacles[i];

            if (obstacle == null)
            {
                continue;
            }

            Collider[] obstacleColliders =
                obstacle.GetComponentsInChildren<Collider>(
                    true
                );

            for (int j = 0; j < obstacleColliders.Length; j++)
            {
                Collider obstacleCollider =
                    obstacleColliders[j];

                if (obstacleCollider == null)
                {
                    continue;
                }

                if (obstacleCollider ==
                    playerCharacterController)
                {
                    continue;
                }

                if (ignoredObstacleColliders.Contains(
                    obstacleCollider))
                {
                    continue;
                }

                Physics.IgnoreCollision(
                    playerCharacterController,
                    obstacleCollider,
                    true
                );

                ignoredObstacleColliders.Add(
                    obstacleCollider
                );
            }
        }
    }

    private void RestoreObstacleCollisions()
    {
        if (playerCharacterController == null)
        {
            ignoredObstacleColliders.Clear();
            return;
        }

        foreach (
            Collider obstacleCollider
            in ignoredObstacleColliders)
        {
            if (obstacleCollider == null)
            {
                continue;
            }

            Physics.IgnoreCollision(
                playerCharacterController,
                obstacleCollider,
                false
            );
        }

        ignoredObstacleColliders.Clear();
    }

    public void StopPhase()
    {
        phaseActive = false;
        remainingTime = 0f;

        RestoreObstacleCollisions();
        RestoreOriginalVisual();

        Debug.Log(
            "PHASE ENDED"
        );
    }

    public void CancelPhase()
    {
        if (!phaseActive)
        {
            return;
        }

        StopPhase();
    }

    private void OnDisable()
    {
        phaseActive = false;
        remainingTime = 0f;

        RestoreObstacleCollisions();
        RestoreOriginalVisual();
    }

    private void OnDestroy()
    {
        RestoreObstacleCollisions();
        RestoreOriginalVisual();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}