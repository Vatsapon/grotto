using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne_BoulderSpawner : MonoBehaviour
{
    public GameObject Boulder;

    [HideInInspector]
    public GameObject Obj;
    public void SpawnBoulder()
    {
        Obj = Instantiate(Boulder, transform.position, Quaternion.identity);
    }
}
