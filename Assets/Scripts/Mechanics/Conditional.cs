using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Conditional : MonoBehaviour
{
    [Tooltip("Determine to activate condition only once or not")]
    [SerializeField] private bool conditionOnce = true;
    [Tooltip("List of all conditions to fulfill")]
    [SerializeField] private List<GameObject> conditionList = new List<GameObject>();

    [Tooltip("Event when all condition isn't met")]
    [SerializeField] private UnityEvent onConditionUnmet;
    [Tooltip("Event when all condition is met")]
    [SerializeField] private UnityEvent onConditionMet;

    private bool isConditioned = false; // Determine if condition is met or not.

    // Function to check if condition is met.
    private void Update()
    {
        // If condition already met and only once, return.
        if (isConditioned && conditionOnce)
        {
            return;
        }

        bool conditionMet = true;

        for (int i = 0; i < conditionList.Count; i++)
        {
            IConditional conditional;

            if (conditionList[i].TryGetComponent(out conditional))
            {
                if (!conditional.IsActivate())
                {
                    conditionMet = false;
                    break;
                }
            }
        }

        if (conditionMet)
        {
            if (!isConditioned)
            {
                isConditioned = true;
                onConditionMet.Invoke();
            }
        }
        else
        {
            if (isConditioned)
            {
                isConditioned = false;
                onConditionUnmet.Invoke();
            }
        }
    }
}

public interface IConditional
{
    public bool IsActivate();
    public void SetActivate(bool value);
}
