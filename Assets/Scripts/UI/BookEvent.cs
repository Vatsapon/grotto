using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookEvent : MonoBehaviour
{
    [Tooltip("Event to execute after page animation")]
    public UnityEvent afterPageEvent;
}
