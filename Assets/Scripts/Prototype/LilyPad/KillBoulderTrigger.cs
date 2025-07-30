using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBoulderTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boulder"))
        {
            Destroy(other.gameObject);
        }
    }
}
