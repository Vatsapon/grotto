using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Determine to enable/disable this popup")]
    [SerializeField] private bool enable = true;
    [Tooltip("Determine to show gizmo a debugging mode or not")]
    [SerializeField] private bool showGizmo = false;

    [Space(5f)]

    [Tooltip("Type of popup\n- Radius - Will show only player walk within radius\n- Toggle - Use function to toggle show/hide")]
    [SerializeField] private PopupType type = PopupType.Radius;
    [ShowIf("type", PopupType.Radius, DisableType.Hide)]
    [Tooltip("Amount of radius to show when player walk in")]
    [SerializeField] private float activeRadius = 1f;
    [Tooltip("Popup's height that offset from the target object")]
    [SerializeField] private float offset = 0f;

    [Header("Popup Context")]
    [Tooltip("Sprite of this popup")]
    [SerializeField] private Sprite popupSprite;
    [Tooltip("Text of this popup")]
    [SerializeField] private string popupText;

    [Header("References")]
    [Tooltip("Popup object that will spawn above")]
    [SerializeField] private GameObject popupObject;
    
    private Transform popupUIParent; // Parent that will handle the popup.
    private GameObject currentPopup; // Current popup object.
    private bool togglePopup = false; // Toggle popup to show/hide.

    private Player player;

    public enum PopupType
    {
        Radius, Toggle
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();

        // If it's not enable, return.
        if (!enable)
        {
            return;
        }

        // If there's no popup object, disable.
        if (!popupObject)
        {
            Debug.LogWarning("Popup for game object '" + gameObject.name + "' haven't assign popup object yet! Disable popup..");

            enable = false;
            return;
        }

        Setup();
    }

    private void Update()
    {
        if (!enable)
        {
            SetPopup(false);
            return;
        }

        if (currentPopup)
        {
            ContextUpdater();
            PositionUpdater();
        }

        switch (type)
        {
            case PopupType.Radius:

            float distance = Vector3.Distance(transform.position, player.transform.position);
            SetPopup(distance <= activeRadius);

            break;

            case PopupType.Toggle:

            SetPopup(togglePopup);
            break;
        }
    }

    // Function to setup the popup object.
    private void Setup()
    {
        Transform parentUI = popupUIParent ? popupUIParent : GameObject.Find("UI Canvas").transform;
        currentPopup = Instantiate(popupObject, parentUI);
        currentPopup.transform.SetAsFirstSibling();

        // Make popup not flickering before the position is update.
        currentPopup.SetActive(false);
        ContextUpdater();
        PositionUpdater();
        currentPopup.SetActive(true);
    }
    
    // Function to update popup position.
    private void PositionUpdater()
    {
        Vector2 targetUIPosition;

        if (Camera.main)
            targetUIPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        else
        {
            Camera mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (!mainCam)
                print("ERROR: Could not find main camera in PopupManager");

            targetUIPosition = mainCam.WorldToScreenPoint(gameObject.transform.position);
        }

        targetUIPosition.y += offset * 2f;

        if (currentPopup)
        {
            currentPopup.transform.position = targetUIPosition;
        }
    }

    // Function to update popup context.
    private void ContextUpdater()
    {
        if (currentPopup)
        {
            Popup popup;

            if (currentPopup.TryGetComponent(out popup))
            {
                GameObject iconObject = popup.GetUI("Icon");

                if (iconObject)
                {
                    Image icon = iconObject.GetComponent<Image>();
                    icon.sprite = popupSprite ? popupSprite : icon.sprite;
                }

                GameObject contextObject = popup.GetUI("Context");

                if (contextObject)
                {
                    TextMeshProUGUI text = contextObject.GetComponent<TextMeshProUGUI>();
                    text.text = !popupText.Equals("") ? popupText : text.text;
                }
            }
            else
            {
                try
                {
                    Image iconRenderer = currentPopup.transform.Find("Icon").GetComponent<Image>();
                    iconRenderer.sprite = popupSprite ? popupSprite : iconRenderer.sprite;

                    TextMeshProUGUI contextText = currentPopup.transform.Find("Context").GetComponent<TextMeshProUGUI>();
                    contextText.text = !popupText.Equals("") ? popupText : contextText.text;
                }
                catch (NullReferenceException) { }
            }
        }
    }

    // Function to toggle popup to show/hide (If using Toggle mode).
    public void Toggle() => togglePopup = !togglePopup;

    // Function to show popup (If using Toggle mode).
    public void Show() => togglePopup = true;

    // Function to hide popup (If use Toggle mode).
    public void Hide() => togglePopup = false;

    // Function to determine if popup is showing or not.
    public bool IsShowing()
    {
        return currentPopup;
    }

    // Function to get popup object.
    public GameObject GetPopup() => currentPopup;

    // Function to set popup type.
    public void SetType(PopupType type) => this.type = type;

    // Function to set popup type.
    public void SetType(string type)
    {
        try
        {
            this.type = (PopupType)Enum.Parse(typeof(PopupType), type);
        }
        catch (ArgumentException) { }
    }

    // Function to set popup to create or destroy.
    private void SetPopup(bool value)
    {
        if (value)
        {
            if (!currentPopup)
            {
                Setup();
            }
        }
        else
        {
            if (currentPopup)
            {
                Animator popupAnimator;

                if (currentPopup.TryGetComponent(out popupAnimator))
                {
                    popupAnimator.Play("Hide");
                    Destroy(currentPopup, popupAnimator.GetCurrentAnimatorStateInfo(0).length);
                }
                else
                {
                    Destroy(currentPopup);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (currentPopup)
        {
            Destroy(currentPopup);
        }
    }

    private void OnDisable()
    {
        SetPopup(false);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo)
        {
            return;
        }

        if (type == PopupType.Radius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activeRadius);
        }
    }
}