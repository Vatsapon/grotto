using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public Camera NewCamera;
    public string TagToCheck;
    public string SecondTagToCheck;

    private Camera PlayerCam;

    private void Start()
    {
        NewCamera.enabled = false;
        PlayerCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TagToCheck == null || TagToCheck == "")
            return;

        if (other.CompareTag(TagToCheck))
        {
            NewCamera.enabled = !NewCamera.enabled;
            PlayerCam.enabled = !PlayerCam.enabled;
        }

        if (SecondTagToCheck == null || SecondTagToCheck == "")
            return;

        if (other.CompareTag(SecondTagToCheck))
        {
            NewCamera.enabled = !NewCamera.enabled;
            PlayerCam.enabled = !PlayerCam.enabled;
        }

    }
}
