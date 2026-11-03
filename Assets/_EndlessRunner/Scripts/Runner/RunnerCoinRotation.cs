using UnityEngine;

public class RunnerCoinRotation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Rotation Axis")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(
            rotationAxis,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}