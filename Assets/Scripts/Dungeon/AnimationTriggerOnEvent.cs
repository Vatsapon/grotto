using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTriggerOnEvent : MonoBehaviour
{
    public Animator Anim;
    public void TriggerAnimation(string parameter)
    {
        Anim.SetTrigger(parameter);
    }

}
