using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUI_FaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        if (player)
        {
          //  transform.LookAt(player.transform);

            transform.rotation = Quaternion.LookRotation(transform.position - player.transform.position);
        }
    }
}
