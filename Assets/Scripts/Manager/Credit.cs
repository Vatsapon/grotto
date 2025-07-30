using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Speed for scrolling")]
    [SerializeField] private float scrollSpeed = 5f;
 
    [Space(5f)]

    [Header("References")]
    [Tooltip("Scroll bar")]
    [SerializeField] private Scrollbar scrollbar;

    private void Start()
    {
        ResetScroll();
    }

    private void Update()
    {
        if (scrollbar.value > 0f)
        {
            scrollbar.value -= scrollSpeed * 0.01f * Time.deltaTime;
        }
        else
        {
            ResetScroll();
        }
    }

    // Function to reset scroll back to start.
    public void ResetScroll()
    {
        scrollbar.value = 1f;
    }
}
