using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlock_Collision : MonoBehaviour
{
    [Tooltip("Amount of damages")]
    [SerializeField] private float _damage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Player player;

        if (other.TryGetComponent(out player))
        {
            player.OnTakeDamage(_damage);

            if (player.Health > 0)
            {
                player.ResetPlayerPos();
            }

            //also respawn the ent if one exists
            Ent entObj = FindObjectOfType<Ent>();
            if (entObj)
            {
                Destroy(entObj.gameObject);
                FindObjectOfType<EntManager>().SpawnNewEntWithoutStartingDialogue();
            }
        }

        //look for ent collision with water
        Ent ent;

        //ent dies if it hits the water
        if (other.TryGetComponent(out ent))
        {
            Player.instance.ResetPlayerPos();

            Destroy(ent.gameObject);
            FindObjectOfType<EntManager>().SpawnNewEntWithoutStartingDialogue();
        }
    }
}
