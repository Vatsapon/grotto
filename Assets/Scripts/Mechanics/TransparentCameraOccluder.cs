using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentCameraOccluder : MonoBehaviour
{
    //object mesh rendered
    private MeshRenderer MR;
    //base transparency to apply to a object
    public float BaseTransparency = 0.5f;

    //multiplier for multiple object
    private float OpacityMultiplier;

    [HideInInspector]
    public bool IsOccluding = false;

    private void Start()
    {
        //get renderer
        MR = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (IsOccluding)
        {
            //set material to be transparent by the multiplier
            MR.material.color = new Color(MR.material.color.r, 
                MR.material.color.g, MR.material.color.b, OpacityMultiplier);
        }
        else
        {
            //set back to normal
            MR.material.color = new Color(MR.material.color.r, 
                MR.material.color.g, MR.material.color.b, 1f);
        }
        IsOccluding = false;
    }
    public void SetToTransparent(float divideBy)
    {
        //depending on the location of the object, make it more transparent
        OpacityMultiplier = BaseTransparency / divideBy;
        IsOccluding = true;
    }
}
