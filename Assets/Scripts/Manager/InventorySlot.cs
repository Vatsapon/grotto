using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Tooltip("Current item data of this slot")]
    [SerializeField] private Item item;
    [Tooltip("Amount of this item")]
    [SerializeField] private int amount = 0;
    [Tooltip("Template of amount text")]
    [SerializeField] private string amountTemplate = "{amount} / {max_amount}";
    [Tooltip("Determine to remove item from slot when amount reaches 0 or not")]
    [SerializeField] private bool removeWhenEmpty = true;

    [Header("References")]
    [Tooltip("Selected object frame for this item")]
    [SerializeField] private GameObject selectedObject;
    [Tooltip("Text for display item's name")]
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("Image of this slot")]
    [SerializeField] private Image slotImage;
    [Tooltip("Amount GameObject")]
    [SerializeField] private GameObject amountObject;
    [Tooltip("Text of item's amount")]
    [SerializeField] private TextMeshProUGUI amountText;

    public event Action<InventorySlot> OnSelect;

    private InventoryManager invM;

    private void Start()
    {
        invM = InventoryManager.instance;
        UpdateSlot();
    }

    // Function to update slot data.
    private void UpdateSlot()
    {
        slotImage.sprite = item ? item.itemTexture : null;
        slotImage.enabled = item;

        if (amountObject)
        {
            amountObject.SetActive(item);
        }

        if (nameText)
        {
            nameText.text = item ? item.itemName : "";
        }

        amountText.text = amountTemplate.Replace("{amount}", amount.ToString()).Replace("{max_amount}", InventoryManager.instance.GetStackAmount().ToString());
    }

    // Function to select this slot.
    public void Selected()
    {
        invM.SetSelectSlot(this);

        if (selectedObject)
        {
            selectedObject.SetActive(true);
        }

        OnSelect?.Invoke(this);
    }

    // Function to unselect this slot.
    public void Unselected()
    {
        if (selectedObject)
        {
            selectedObject.SetActive(false);
        }
    }

    // Function to fetch item data.
    public Item GetItem()
    {
        return item; 
    }

    // Function to set item data.
    public void SetItem(Item item)
    {
        this.item = item;
        UpdateSlot();
    }
    
    // Function to fetch amount of item.
    public int GetAmount()
    {
        return item ? amount : 0;
    }

    // Function to set amount of item.
    public void SetAmount(int amount)
    {
        if (!item)
        {
            return;
        }

        int maxAmount = item.stackable ? InventoryManager.instance.GetStackAmount() : 1;

        amount = Mathf.Clamp(amount, 0, maxAmount);
        this.amount = amount;

        UpdateSlot();
    }

    // Function to increase amount.
    public void AddAmount(int amount)
    {
        if (!item)
        {
            return;
        }

        int maxAmount = item.stackable ? InventoryManager.instance.GetStackAmount() : 1;

        amount = Mathf.Clamp(amount, 0, maxAmount - this.amount);
        this.amount += amount;

        UpdateSlot();
    }

    // Function to decrease amount.
    public void RemoveAmount(int amount)
    {
        if (!item)
        {
            return;
        }

        amount = Mathf.Clamp(amount, 0, this.amount);
        this.amount -= amount;

        UpdateSlot();
    }

    // Function to determine if this slot is full or not.
    public bool IsFull()
    {
        if (!item || amount == 0)
        {
            return false;
        }

        if (item)
        {
            if (!item.stackable && amount >= 1)
            {
                return true;
            }

            if (amount == invM.GetStackAmount())
            {
                return true;
            }    
        }

        return false;
    }

    // Function to determine to remove this slot when empty (amount is 0) or not.
    public bool IsRemoveWhenEmpty() => removeWhenEmpty;
}
