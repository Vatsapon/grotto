using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera Cam;

    private void Start()
    {
        Cam = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(Cam.transform.position);
    }
}
