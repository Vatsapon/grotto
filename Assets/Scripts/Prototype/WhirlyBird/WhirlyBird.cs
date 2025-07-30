using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlyBird : MonoBehaviour
{
    public float PushForce = 5f;

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Boulder"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //rb.velocity = transform.forward * PushForce;
                rb.AddForce(transform.forward * PushForce);
            }
        }
    }
}
