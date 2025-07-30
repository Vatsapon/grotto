using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [Tooltip("Determine to play the cinematic only once or not")]
    [SerializeField] private bool _playOnce = true;

    [Space(10f)]

    [Tooltip("Speed to transition between player's camera to this camera")]
    [SerializeField] private float _transitionSpeed = 10f;
    [Tooltip("Duration for cinematic camera before transition back to player's camera")]
    [SerializeField] private float _cinematicDuration = 3f;

    [Tooltip("Camera to use for cinematic")]
    [SerializeField] private Camera _camera;

    private bool played = false;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();

        _camera.gameObject.SetActive(false);
    }

    // Function to start cinematic camera.
    public async void StartCinematic()
    {
        if (_playOnce && played)
        {
            return;
        }

        played = true;
        player.SetControllable(false);

        defaultPosition = _camera.transform.position;
        defaultRotation = _camera.transform.rotation;

        _camera.transform.position = Camera.main.transform.position;
        _camera.transform.rotation = Camera.main.transform.rotation;

        _camera.gameObject.SetActive(true);

        while (gameObject && !_camera.transform.position.Equals(defaultPosition))
        {
            if (!gameObject)
            {
                return;
            }

            _camera.transform.position = Vector3.Lerp(_camera.transform.position, defaultPosition, _transitionSpeed * Time.deltaTime);
            _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, defaultRotation, _transitionSpeed * Time.deltaTime);

            if (Vector3.Distance(_camera.transform.position, defaultPosition) < 0.01f)
            {
                _camera.transform.position = defaultPosition;
                break;
            }

            await Task.Yield();
        }

        if (!gameObject)
        {
            return;
        }

        float duration = _cinematicDuration;

        while (duration > 0f)
        {
            if (!gameObject)
            {
                return;
            }

            duration -= Time.deltaTime;
            await Task.Yield();
        }

        player.SetControllable(true);

        while (gameObject && !_camera.transform.position.Equals(Camera.main.transform.position))
        {
            if (!gameObject)
            {
                return;
            }

            _camera.transform.position = Vector3.Lerp(_camera.transform.position, Camera.main.transform.position, _transitionSpeed * Time.deltaTime);
            _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, Camera.main.transform.rotation, _transitionSpeed * Time.deltaTime);

            if (Vector3.Distance(_camera.transform.position, Camera.main.transform.position) < 0.01f)
            {
                _camera.transform.position = Camera.main.transform.position;
                break;
                
            }

            await Task.Yield();
        }

        if (!gameObject)
        {
            return;
        }

        _camera.gameObject.SetActive(false);

        _camera.transform.position = defaultPosition;
        _camera.transform.rotation = defaultRotation;
    }
}
