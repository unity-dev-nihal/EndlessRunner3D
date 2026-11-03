using UnityEngine;

public class DistantCityBackdropFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Backdrop Distance")]
    [SerializeField] private float distanceAhead = 350f;

    [Header("Backdrop Position")]
    [SerializeField] private float backdropY = 30f;

    [Header("Backdrop Scale")]
    [SerializeField] private Vector3 backdropScale = new Vector3(150f, 80f, 1f);

    [Header("Follow Settings")]
    [SerializeField] private bool followPlayerX = true;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        transform.localScale = backdropScale;
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        float targetX = followPlayerX ? player.position.x : transform.position.x;
        float targetZ = player.position.z + distanceAhead;

        transform.position = new Vector3(
            targetX,
            backdropY,
            targetZ
        );
    }
}