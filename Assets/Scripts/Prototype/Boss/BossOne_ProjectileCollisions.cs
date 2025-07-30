using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne_ProjectileCollisions : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if(player != null)
        {
            player.OnTakeDamage(1);
        }
    }
}
