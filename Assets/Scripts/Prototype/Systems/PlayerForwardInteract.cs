using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TheraBytes.BetterUi.LocationAnimations;

public class PlayerForwardInteract : MonoBehaviour
{
    [Tooltip("Determine to show debug interaction line or not")]
    [SerializeField] private bool DebugLine = false;

    [Space(10f)]

    [Tooltip("Maximum range for player to interact")]
    [SerializeField] private float MaxInteractRange = 3f;
    [Tooltip("Duration for hold interact")]
    [SerializeField] private float HoldInteractDuration = 1f;

    [Space(10f)]

    [Tooltip("Layer that can be interact")]
    [SerializeField] private LayerMask WhatCanBeInteracted;

    private Interactive currentInteractive; // Current interactive that player is interacting.
    private float currentHoldInteractTime; // Current duration for hold interact.

    private PopupManager popupManager;

    private void Start()
    {
        popupManager = GetComponent<PopupManager>();
    }

    private void Update()
    {
        // Raycast initialization.
        Ray camRay = new Ray(transform.position, transform.forward);
        RaycastHit hit = new RaycastHit();

        // If object that interact is interactable...
        if (Physics.Raycast(camRay, out hit, MaxInteractRange, WhatCanBeInteracted))
        {
            // If component has interactive...
            Interactive inter;

            if (hit.transform.TryGetComponent(out inter))
            {
                if (inter.TextDisplay != null)
                {
                    inter.UpdateText(inter.TextForInteraction);
                }

                // If current interact object isn't the same, deactive the interact.
                if (currentInteractive && !currentInteractive.Equals(inter))
                {
                    currentInteractive.DeActivatePickupIcon();
                    currentInteractive = null;
                }

                // If player hasn't interact anything yet, interact one.
                if (!currentInteractive)
                {
                    currentInteractive = inter;
                    inter.ActivatePickupIcon();
                }
            }
        }
        else
        {
            // If there's no interacive object within range, deactive the interactive.
            if (currentInteractive)
            {
                currentInteractive.DeActivatePickupIcon();

                if (currentInteractive.TextDisplay != null)
                {
                    currentInteractive.UpdateText(currentInteractive.NoText);
                }

                currentInteractive = null;
            }
        }

        PlantSpeedUpgrade upgrade = UpgradeManager.instance.GetUpgrade("Interaction") as PlantSpeedUpgrade;
        float finalDuration = HoldInteractDuration * ((100f - upgrade.GetSpeedIncrease()) / 100f);

        Animator animator = FindObjectOfType<Player>().GetAnimator();

        if (currentInteractive)
        {
            if (currentInteractive.IsHoldInteract())
            {
                // If player holding interact key, countdown the interact time.
                if (Input.GetKey(KeyCode.E))
                {
                    currentHoldInteractTime -= Time.deltaTime;
                    popupManager.Show();

                    float length = 0f;
                    RuntimeAnimatorController controller = animator.runtimeAnimatorController;

                    for (int i = 0; i < controller.animationClips.Length; i++)
                    {
                        if (controller.animationClips[i].name.Equals("Player_Planting"))
                        {
                            length = controller.animationClips[i].length;
                            break;
                        }
                    }

                    animator.SetBool("IsPlanting", true);
                    animator.SetFloat("PlantingSpeed", length / finalDuration);

                    // Update slider value in Popup.
                    if (popupManager.GetPopup())
                    {
                        Slider slider = popupManager.GetPopup().transform.Find("Slider").GetComponent<Slider>();
                        slider.value = 1f - (currentHoldInteractTime / finalDuration);
                    }

                    // If interact time reaches 0, inteacted and reset time.
                    if (currentHoldInteractTime <= 0f)
                    {
                        currentInteractive.Activate();
                        currentInteractive.UpdateText(currentInteractive.NoText);

                        popupManager.Hide();

                        animator.SetBool("IsPlanting", false);
                        currentHoldInteractTime = finalDuration;

                        currentInteractive.DeActivatePickupIcon();
                        currentInteractive = null;
                    }
                }
                else
                {
                    popupManager.Hide();
                    animator.SetBool("IsPlanting", false);

                    currentHoldInteractTime = finalDuration;
                }
            }
            else
            {
                // If player press interact key, interacted.
                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentInteractive.Activate();
                    currentInteractive.UpdateText(currentInteractive.NoText);
                }
            }
        }
        else
        {
            popupManager.Hide();
            animator.SetBool("IsPlanting", false);
            currentHoldInteractTime = finalDuration;
        }

        DebugStuff();
    }

    // Function to handle all debugs.
    private void DebugStuff()
    {
        if (DebugLine)
        {
            Debug.DrawRay(transform.position, transform.forward * MaxInteractRange, Color.green);
        }
    }
}
