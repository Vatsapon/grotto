using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Interactive : MonoBehaviour
{
    [Header("Settings")]

    [Tooltip("Determine to make this interactive become conditioned on start or not")]
    [SerializeField] private bool conditionOnStart = false;
    [Tooltip("Determine to interact as holding or not")]
    [SerializeField] private bool isHoldInteract = false;

    [Space(10f)]
    [Tooltip("Event that will execute when interacted")]
    [SerializeField] private UnityEvent InteractEvent;

    public TextMeshProUGUI TextDisplay;

    //default text for a interaction to have when looking at it
    public string TextForInteraction;


    [Header("Optional Settings")]

    [Tooltip("If set, the image will be activated when the player is within pickup range of this object")]
    public Image pickupIcon;

    [Tooltip("If true, the icon will only display when in player range AND the ActivateConditional() function is called")]
    public bool IsConditionalIcon;

    //for conditional triggering
    private bool conditional;

    //check if object is activated
    [HideInInspector]
    public string NoText = " ";

    private PopupManager popupManager;

    private void Start()
    {
        //sets current text to default
        UpdateText(NoText);

        if (pickupIcon)
        {
            pickupIcon.gameObject.SetActive(false);
        }

        conditional = conditionOnStart;

        TryGetComponent(out popupManager);
    }

    public void UpdateText(string text)
    {
        if (TextDisplay)
            TextDisplay.text = text;
    }

    public void ActivateConditional()
    {
        conditional = true;
    }

    public void DeActivateConditional()
    {
        conditional = false;
    }

    public void ActivatePickupIcon()
    {
        if (!enabled)
        {
            return;
        }

        if (pickupIcon)
        {
            //if it is conditional, only activate the icon if true
            if (IsConditionalIcon)
            {
                if (conditional)
                {
                    if (!pickupIcon.gameObject.activeSelf)
                    {
                        PlayPromptSound();
                    }

                    pickupIcon.gameObject.SetActive(true);
                }
            }

            //otherwise activate it regardless
            else
            {
                if (!pickupIcon.gameObject.activeSelf)
                {
                    PlayPromptSound();
                }

                pickupIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            if (popupManager)
            {
                //if it is conditional, only activate the icon if true
                if (IsConditionalIcon)
                {
                    if (conditional)
                    {
                        if (!popupManager.IsShowing())
                        {
                            PlayPromptSound();
                        }

                        popupManager.Show();
                    }
                }

                //otherwise activate it regardless
                else
                {
                    if (!popupManager.IsShowing())
                    {
                        PlayPromptSound();
                    }

                    popupManager.Show();
                }
            }
        }
    }
    public void DeActivatePickupIcon()
    {
        if (pickupIcon)
        {
            pickupIcon.gameObject.SetActive(false);
        }
        else
        {
            if (popupManager)
            {
                popupManager.Hide();
            }
        }
    }

    public bool IsShowingInteractKey()
    {
        if (!popupManager)
            return false;

        return popupManager.IsShowing();
    }


    public void Activate()
    {
        InteractEvent.Invoke();

    }

    void PlayPromptSound()
    {
        AudioManager.instance.RandomPlaySound("Prompt");
    }

    // Function to determine if this interactive need to hold to interact or not.
    public bool IsHoldInteract() => isHoldInteract;
}
