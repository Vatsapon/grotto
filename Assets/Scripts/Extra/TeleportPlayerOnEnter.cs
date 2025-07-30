using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerOnEnter : MonoBehaviour
{
    public Transform TeleportDestination;

    private Transform Player;

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Activate();
        }
    }

    public void Activate()
    {
        Player.transform.position = TeleportDestination.position;
        Camera.main.GetComponent<TDCameraFollow>().UpdatePosition();
    }
}
