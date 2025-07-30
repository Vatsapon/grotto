using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    public float TimeToDestroy = 4f;

    private float Timer = 0f;

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer >= TimeToDestroy)
        {
            Destroy(gameObject);
        }
    }
}
