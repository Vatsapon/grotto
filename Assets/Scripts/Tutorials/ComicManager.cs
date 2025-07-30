using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicManager : MonoBehaviour
{
    private int comicIndex = -1;
    private Coroutine coroutine;

    // Function to start the comic.
    public void StartComic()
    {
        comicIndex = -1;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<CanvasGroup>().alpha = i == 0 ? 1f : 0.05f;
        }
    }

    // Function to continue the comic.
    public void ContinueComic()
    {
        comicIndex++;

        if (comicIndex < transform.childCount)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);

                CanvasGroup canvasGroup = transform.GetChild(comicIndex - 1).GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
            }

             coroutine = StartCoroutine(Display(comicIndex));
        }
    }

    // Function to reveal panel by index.
    private IEnumerator Display(int index)
    {
        CanvasGroup canvasGroup = transform.GetChild(index).GetComponent<CanvasGroup>();
        float alpha = canvasGroup.alpha;

        while (alpha < 1f)
        {
            alpha += 0.5f * Time.unscaledDeltaTime;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        ContinueComic();
    }
}
