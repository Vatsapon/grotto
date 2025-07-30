using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne_DamageBoxCollisions : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.OnTakeDamage(1);
        }
    }
}
