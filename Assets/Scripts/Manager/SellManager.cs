using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellManager : MonoBehaviour
{
    [Tooltip("Parent of all selling panel")]
    [SerializeField] private Transform sellParent;
    [Tooltip("Parent of inventory panel")]
    [SerializeField] private Transform inventoryParent;

    [Space(5f)]

    [Tooltip("Text for total selling coin")]
    [SerializeField] private TextMeshProUGUI totalSellText;
    [Tooltip("Text for total player's coin")]
    [SerializeField] private TextMeshProUGUI totalCoinText;
    [Tooltip("Button to sell items")]
    [SerializeField] private Button sellButton;
    [Tooltip("Animator of Red Mark that mark the price that player will gain when selling")]
    [SerializeField] private Animator markAnimator;

    private InventoryManager invM;
    private List<InventorySlot> sellingItems = new List<InventorySlot>(); // List of item that selected to sell.

    private void Start()
    {
        invM = InventoryManager.instance;

        GenerateSlot();
    }

    private void OnEnable()
    {
        InventoryManager.instance.SetInventoryPanel(inventoryParent);

        for (int i = 0; i < InventoryManager.instance.GetAllSlots().Count; i++)
        {
            InventorySlot slot = InventoryManager.instance.GetAllSlots()[i];
            slot.OnSelect += AddSellItem;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < invM.GetAllSlots().Count; i++)
        {
            InventorySlot slot = invM.GetAllSlots()[i];
            slot.OnSelect -= AddSellItem;
        }

        Clear();
        invM.SetDefaultInventoryPanel();
    }

    private void Update()
    {
        totalCoinText.text = "Total Coins    <sprite=0> " + GameManager.instance.GetGoldCoin();
        totalSellText.text = "<sprite=0> " + GetTotalSell().ToString();

        bool hasItem = false;

        for (int i = 0; i < sellingItems.Count; i++)
        {
            InventorySlot slot = sellingItems[i];
            TextMeshProUGUI price = slot.transform.Find("Sell Price").GetComponent<TextMeshProUGUI>();

            if (slot.GetItem() && slot.GetAmount() > 0)
            {
                price.text = "<sprite=0> " + (slot.GetItem().sellPrice * slot.GetAmount()).ToString();
                price.enabled = true;

                hasItem = true;
            }
            else
            {
                price.enabled = false;
            }
        }

        sellButton.interactable = hasItem;
        markAnimator.SetBool("IsShow", hasItem);
    }

    // Function to generate slot.
    private void GenerateSlot()
    {
        for (int i = 0; i < sellParent.childCount; i++)
        {
            InventorySlot slot = sellParent.GetChild(i).GetComponent<InventorySlot>();

            slot.OnSelect += RemoveSellItem;
            sellingItems.Add(slot);
        }
    }

    // Function to add selling item.
    public void AddSellItem(InventorySlot inventorySlot)
    {
        Item item = inventorySlot.GetItem();

        if (!item || inventorySlot.GetAmount() <= 0)
        {
            return;
        }

        InventorySlot sellSlot = GetSlotItem(item);

        if (sellSlot)
        {
            sellSlot.AddAmount(1);
            invM.RemoveItem(item, 1);
        }
        else
        {
            for (int i = 0; i < sellingItems.Count; i++)
            {
                if (!sellingItems[i].GetItem())
                {
                    sellingItems[i].SetItem(item);
                    sellingItems[i].SetAmount(1);

                    invM.RemoveItem(item, 1);
                    break;
                }
            }
        }
    }

    // Function to remove selling item.
    public void RemoveSellItem(InventorySlot sellSlot)
    {
        if (sellSlot.GetItem() && sellSlot.GetAmount() > 0)
        {
            sellSlot.RemoveAmount(1);
            invM.AddItem(sellSlot.GetItem(), 1);

            if (sellSlot.GetAmount() == 0)
            {
                sellSlot.SetItem(null);
            }
        }
    }

    // Function to get slot that matches item with selected slot.
    public InventorySlot GetSlotItem(Item item)
    {
        for (int i = 0; i < sellingItems.Count; i++)
        {
            if (!sellingItems[i].GetItem())
            {
                continue;
            }

            if (sellingItems[i].GetItem().Equals(item))
            {
                return sellingItems[i];
            }
        }

        return null;
    }

    // Function to sell all selected items.
    public void Sell()
    {
        GameManager.instance.AddGoldCoin(GetTotalSell());

        for (int i = 0; i < sellingItems.Count; i++)
        {
            sellingItems[i].SetItem(null);
            sellingItems[i].SetAmount(0);
        }
    }

    // Function to clear data in sell slot.
    public void Clear()
    {
        for (int i = 0; i < sellingItems.Count; i++)
        {
            InventorySlot slot = sellingItems[i];

            if (slot.GetItem() && slot.GetAmount() > 0)
            {
                invM.AddItem(slot.GetItem(), slot.GetAmount());
            }
            
            sellingItems[i].SetItem(null);
            sellingItems[i].SetAmount(0);
        }
    }

    // Function to get total sell coin.
    private int GetTotalSell()
    {
        int totalPrice = 0;

        for (int i = 0; i < sellingItems.Count; i++)
        {
            Item item = sellingItems[i].GetItem();

            if (item)
            {
                totalPrice += item.sellPrice * sellingItems[i].GetAmount();
            }
        }

        return totalPrice;
    }
}
