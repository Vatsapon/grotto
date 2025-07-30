using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBoulder : MonoBehaviour
{
    public GameObject Boulder;
    private GameObject BoulderObj = null;

    private void Update()
    {
        if(BoulderObj == null)
        {
            BoulderSpawn();
        }
    }

    public void BoulderSpawn()
    {
        BoulderObj = Instantiate(Boulder, transform.position, transform.rotation);
    }
}
