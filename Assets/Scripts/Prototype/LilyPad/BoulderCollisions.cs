using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class BoulderCollisions : MonoBehaviour
{
    public WhirlyBird Current = null;
    private SplineFollower SF;
    private Rigidbody RB;
    public float Timer;
    public float LerpTime = 0.5f;
    private bool Lerping;
    public Vector3 target;

    private void Start()
    {
        RB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //whirly bird stuff
        if (Lerping)
        {
            Timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, target, Timer / LerpTime);
        }
        if (Timer >= LerpTime)
        {
            Lerping = false;
            Timer = 0;
        }

        //catapult stuff
        if(SF != null)
        {
            if (SF.result.percent >= 0.99f)
            {
                Destroy(SF);
                RB.velocity = Vector3.zero;
                RB.constraints = RigidbodyConstraints.FreezeAll;
            }
        }


    }
    private void OnTriggerEnter(Collider other)
    {
        Current = other.GetComponent<WhirlyBird>();
        if(Current != null)
        {
            RB.constraints = RigidbodyConstraints.None;
            if (Current.gameObject.transform.forward.x == 0)
            {
                target = new Vector3(Current.transform.position.x, transform.position.y, transform.position.z);
                RB.constraints = RigidbodyConstraints.FreezePositionX;
            }
            else
            {
                target = new Vector3(transform.position.x, transform.position.y, Current.transform.position.z);
                RB.constraints = RigidbodyConstraints.FreezePositionZ;
            }

            Lerping = true;
            if(SF != null)
            {
                Destroy(SF);
            }
        }

        SplineComputer spline = other.GetComponent<SplineComputer>();
        if (spline != null)
        {
            SF = gameObject.AddComponent<SplineFollower>();
            SF.followMode = SplineFollower.FollowMode.Time;
            SF.followDuration = 3;
            SF.Restart();
            SF.spline = spline;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.GetComponent<Player>())
        {
            Rigidbody rigid = collision.collider.GetComponent<Rigidbody>();
            
            // Prevent player from pushing boulder and climb over.
            if (rigid.velocity.magnitude > 0f)
            {
                float velocityY = Mathf.Clamp(rigid.velocity.y, Mathf.NegativeInfinity, -0.1f);
                rigid.velocity = new Vector3(rigid.velocity.x, velocityY, rigid.velocity.z);

                velocityY = Mathf.Clamp(RB.velocity.y, Mathf.NegativeInfinity, -0.1f);
                RB.velocity = new Vector3(RB.velocity.x, velocityY, RB.velocity.z);
            }
        }
    }
}
