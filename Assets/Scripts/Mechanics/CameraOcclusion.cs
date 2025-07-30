using UnityEngine;

public class CameraOcclusion : MonoBehaviour
{
    //Player Position
    public Transform PlayerPos;
    //What the camera can see
    public LayerMask WhatRaycastCanSee;

    // Update is called once per frame
    void Update()
    {
        //direction from the player to the camera
        Vector3 playerToCamDir = (transform.position - PlayerPos.position).normalized;
        //initialize ray from the player to the camera
        Ray occlusionRay = new Ray(PlayerPos.position, playerToCamDir);

        //Everything inbetween the player and the camera
        RaycastHit[] hits = Physics.RaycastAll(occlusionRay, WhatRaycastCanSee);

        //loop through everything hit
        for (int i = 0; i < hits.Length; i++)
        {
            //check for transparent component
            TransparentCameraOccluder tco = hits[i].collider.GetComponent<TransparentCameraOccluder>();
            if (tco != null)
            {
                //set material to transparent. Pass in the position in the array of each object + 1
                tco.SetToTransparent(i + 1);
            }
        }
    }
}
