using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedArea : MonoBehaviour
{
    [Range(0f, 100f)]
    [Tooltip("Chance to obtain seed from picking up")]
    [SerializeField] private float seedChance = 50f;
    [Tooltip("Duration before reset")]
    [SerializeField] private float resetDuration = 180f;

    [Header("References")]

    [Tooltip("Item to be add into inventory when retreived")]
    [SerializeField] private Item seedItem;
    [Tooltip("Particle that play when retreived")]
    [SerializeField] private ParticleSystem pickupParticle;
    [Tooltip("Popup for progress")]
    [SerializeField] private PopupManager progressPopupManager;
    [Tooltip("Popup for collect")]
    [SerializeField] private PopupManager collectPopupManager;

    private float currentResetDuration;

    private void Start()
    {
        progressPopupManager.Show();

        currentResetDuration = resetDuration;
    }

    private void Update()
    {
        progressPopupManager.Show();

        GameObject currentPopup = progressPopupManager.GetPopup();

        if (currentPopup)
        {
            currentPopup.transform.Find("Context").gameObject.SetActive(currentResetDuration > 0f);
            currentPopup.transform.Find("Slider").gameObject.SetActive(currentResetDuration > 0f);

            currentPopup.transform.Find("Context").GetComponent<TextMeshProUGUI>().text = "Resetting...";
            currentPopup.transform.Find("Slider").GetComponent<Slider>().value = 1f - (currentResetDuration / resetDuration);
            currentPopup.transform.Find("Pickable").gameObject.SetActive(currentResetDuration <= 0f);

            if (currentResetDuration > 0f)
            {
                collectPopupManager.Hide();
            }

            if (collectPopupManager.GetPopup())
            {
                int collectIndex = collectPopupManager.GetPopup().transform.GetSiblingIndex();
                int progressIndex = progressPopupManager.GetPopup().transform.GetSiblingIndex();

                if (collectIndex < progressIndex)
                {
                    collectPopupManager.GetPopup().transform.SetSiblingIndex(progressIndex + 1);
                }
            }
        }

        currentResetDuration -= Time.deltaTime;
        currentResetDuration = Mathf.Clamp(currentResetDuration, 0f, resetDuration);
    }

    // Function to execute when player pick up th seed.
    public void Pickup()
    {
        if (currentResetDuration > 0f)
        {
            return;
        }

        float chance = Random.Range(0f, 100f);

        if (seedChance > chance)
        {
            InventoryManager.instance.AddItem(seedItem, 1);
            pickupParticle.Play();
        }

        currentResetDuration = resetDuration;
    }
}
