using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRotateObject : MonoBehaviour
{
    public float XAnglesPerSecond;
    public float YAnglesPerSecond;
    public float ZAnglesPerSecond;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(XAnglesPerSecond, YAnglesPerSecond, ZAnglesPerSecond);
    }
}
