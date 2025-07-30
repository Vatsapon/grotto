using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("The speed of camera to lerp")]
    [Range(1f, 20f)]
    [SerializeField] private float _lerpSpeed = 5f;
    [Space(10)]
    [Tooltip("Target to make camera follow (None = won't move)")]
    [SerializeField] private Transform target;

    private Vector3 position; // Position to let camera follow to.

    private void Start()
    {
        position = transform.position;
    }

    private void FixedUpdate()
    {
        // If there's target, set position to target position.
        if (target)
        {
            position = target.position;
        }

        transform.position = Vector3.Lerp(transform.position, position, _lerpSpeed * Time.fixedDeltaTime);
    }
}
