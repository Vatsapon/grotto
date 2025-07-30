using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnTriggerEnter : MonoBehaviour
{
    [MyHeader("Setup")]
    public Animator Anim;
    public string ParameterName;
    [Tooltip("Is parameter a bool or trigger?")]
    public ParamType ParameterType = ParamType.Bool;
    [Tooltip("What tag of the entering object should it be checking?")]
    public string TagToCheck;

    [MyHeader("OnTriggerEnter")]
    [Tooltip("On Trigger enter, if param is a bool, will the bool be set to true? Will be set to false on enter if unchecked.")]
    public bool SetBoolTrue = true;

    public enum ParamType
    {
        Trigger,
        Bool,
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(TagToCheck))
        {
            Debug.Log("Found" + other.name);
            if (ParameterType == ParamType.Trigger)
            {
                Anim.SetTrigger(ParameterName);
            }
            else
            {
                if (SetBoolTrue)
                {
                    Anim.SetBool(ParameterName, true);
                }
                else if (!SetBoolTrue)
                {
                    Anim.SetBool(ParameterName, false);
                }
            }
        }
    }
}
