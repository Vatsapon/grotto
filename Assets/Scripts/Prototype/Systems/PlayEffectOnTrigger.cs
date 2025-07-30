using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayEffectOnTrigger : MonoBehaviour
{
    public ParticleSystem Effect;
    private bool Activated = false;
    private void OnTriggerEnter(Collider other)
    {
        if(!Activated)
        {
            Effect.Play();
            Activated = true;
        }
    }
}
