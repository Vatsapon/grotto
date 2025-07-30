using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PressurePlate : MonoBehaviour, IConditional
{
    [Tooltip("Determine to enable pressure plate start or not")]
    [SerializeField] private bool enableOnStart = true;
    [Tooltip("Determine to activate this pressure plate only once or not")]
    [SerializeField] private bool oneTimeTrigger = true;
    [Tooltip("If it should should deactiveate on trigger exit")]
    [SerializeField] private bool deactivateOnExit = false;
    [Tooltip("Layer to check for collision or trigger")]
    [SerializeField] private LayerMask triggerLayer;

    [Space(5f)]

    [Tooltip("Events for pressure plate")]
    [SerializeField] private PressurePlate_Event events;

    private bool isEnable = false; // Determine if this pressure plate is enable or not.
    private bool isActivate = false; // Determine if this pressure plate is activate or not.

    private void Start()
    {
        isEnable = enableOnStart;

        if (isEnable)
        {
            events.onEnable.Invoke();
        }
        else
        {
            events.onDisable.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If pressure plate is disable, don't do anything.
        if (!isEnable)
        {
            return;
        }

        // Check if triggerLayer contains object's layer.
        if ((triggerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            events.onTrigger.Invoke();

            if (!isActivate)
            {
                isActivate = true;
                events.onActivate.Invoke();
                Debug.Log("button triggered by " + other.gameObject.name);
            }
            else
            {
                // If it's one time trigger, don't deactivate.
                if (oneTimeTrigger)
                {
                    return;
                }

                isActivate = false;
                events.onDeactivate.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(deactivateOnExit && isActivate)
        {
            //make sure un-triggering object is same as triggering object
            if ((triggerLayer.value & (1 << other.gameObject.layer)) > 0)
            {
                Debug.Log("button UN-triggered by " + other.gameObject.name);
                isActivate = false;
                events.onDeactivate.Invoke();
            }
        }
    }

    // Function to play sound on trigger.
    public void PlaySound(string name)
    {
        AudioManager.instance.PlaySound(name);
    }

    // Function to set enable value.
    public void SetEnable(bool value)
    {
        isEnable = value;

        if (isEnable)
        {
            events.onEnable.Invoke();
        }
        else
        {
            events.onDisable.Invoke();
        }
    }

    // Function to determine if this pressure plate is activate or not.
    public bool IsActivate()
    {
        return isActivate;
    }

    // Function to set activate value.
    public void SetActivate(bool value)
    {
        isActivate = value;

        if (isActivate)
        {
            events.onActivate.Invoke();
        }
        else
        {
            events.onDeactivate.Invoke();
        }
    }
}

[System.Serializable]
public struct PressurePlate_Event
{
    [Tooltip("Event when pressure plate is enabled, this will execute when only when it changes to enable")]
    public UnityEvent onEnable;
    [Tooltip("Event when pressure plate is disabled, this will execute when only when it changes to disable")]
    public UnityEvent onDisable;
    [Tooltip("Event when pressure plate is triggered, this will execute everytime something trigger it")]
    public UnityEvent onTrigger;
    [Tooltip("Event when pressure plate is activated, this will execute only when it changes to activate")]
    public UnityEvent onActivate;
    [Tooltip("Event when pressure plate is deactivated, this will execute only when it changes to deactivate")]
    public UnityEvent onDeactivate;
}