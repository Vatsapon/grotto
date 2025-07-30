using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BookManager : MonoBehaviour
{
    [Tooltip("Default page sprite")]
    [SerializeField] private Sprite defaultPageSprite;
    [Tooltip("Animator for page")]
    [SerializeField] private Animator pageAnimator;

    [Space(5f)]

    [Tooltip("List of all book events to set order")]
    [SerializeField] private List<BookEvent> bookEvents;

    private int currentBookEvent = 0;
    private bool playing = false;

    private UnityEvent afterPageEvent; // Event to execute after page animation.

    private void OnDisable()
    {
        if (playing)
        {
            AfterAnimation();
        }

        GetComponent<Image>().sprite = defaultPageSprite;
    }

    // Function to play animation based on current page.
    public void PlayAnimation(BookEvent currentBookEvent)
    {
        playing = true;
        afterPageEvent = currentBookEvent.afterPageEvent;

        int index = bookEvents.IndexOf(currentBookEvent);

        if (index > this.currentBookEvent)
        {
            pageAnimator.Play("Next");
        }
        else
        {
            pageAnimator.Play("Previous");
        }

        this.currentBookEvent = index;
    }

    // Function to execute when animation ended.
    public void AfterAnimation()
    {
        afterPageEvent.Invoke();
        playing = false;
    }
}
