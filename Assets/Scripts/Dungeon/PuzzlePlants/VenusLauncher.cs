using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class VenusLauncher : MonoBehaviour
{
    SplineComputer Com;

    private void Awake()
    {
        Com = GetComponent<SplineComputer>();
    }

    /*public GameObject Box2Launch;
    public Transform LaunchStartPos;
    private Rigidbody RB;
    public Transform Target;

    public float MaxLaunchHeight = 25;
    public float ObjectGravity = -18;

    private void Start()
    {
        RB = Box2Launch.GetComponent<Rigidbody>();
        RB.useGravity = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Launch();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCube();
        }
        print(Physics.gravity);
    }
    void Launch()
    {
        RB.useGravity = true;
        RB.velocity = GetLaunchVelocity();
    }

    Vector3 GetLaunchVelocity()
    {
        //Get Y displacement of target and launch object
        float displacementY = Target.position.y - RB.position.y;
        //get X displacement;
        float displacementX = Target.position.x - RB.position.x;
        //get X displacement;
        float displacementZ = Target.position.z - RB.position.z;
        //save x and z displacement together;
        Vector3 displacementXZ = new Vector3(displacementX, 0, displacementZ);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * ObjectGravity * MaxLaunchHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * MaxLaunchHeight / ObjectGravity) + Mathf.Sqrt(2 * (displacementY - MaxLaunchHeight) / ObjectGravity));

        return velocityXZ + velocityY;
    }

    public void ResetCube()
    {
        RB.useGravity = false;
        Box2Launch.transform.position = LaunchStartPos.position;
    }*/
}
