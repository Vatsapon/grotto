using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IGameData
{
    public static InventoryManager instance;

    [MyHeader("Inventory")]
    [Tooltip("Limit stack amount of each slot")]
    [SerializeField] private int stackAmount = 64;
    
    [Space(10f)]
    public InvM_Comp components;

    private List<InventorySlot> itemList = new List<InventorySlot>();
    private int selectedIndex = -1; // Slot that currently select in Inventory screen.
    private int defaultStackAmount; // Default amount of stack.

    private GameManager gameManager;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        Initialize();
        Refresh();
    }

    private void Start()
    {
        gameManager = GameManager.instance;

        defaultStackAmount = stackAmount;
    }

    private void Update()
    {
        InventorySizeUpgrade upgrade = UpgradeManager.instance.GetUpgrade("Inventory Size") as InventorySizeUpgrade;
        stackAmount = upgrade.GetLevel() == 0 ? defaultStackAmount : upgrade.GetStackAmount();

        // Set stack amount to each item equally (THIS IS FOR DEBUG/PROTOTYPE, MIGHT BE CHANGES IN THE FUTURE)
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].GetItem())
            {
                itemList[i].GetItem().maxAmount = stackAmount;
            }
        }

        // If index is out of range, then return.
        if (selectedIndex < 0 || selectedIndex >= itemList.Count)
        {
            components.displayName.enabled = false;
            components.displayImage.enabled = false;
            components.displayDescription.enabled = false;
            return;
        }

        // If pause panel isn't active (book not showing), clear selection.
        if (!components.displayImage.gameObject.activeInHierarchy)
        {
            ClearSelection();
            return;
        }

        InventorySlot slot = itemList[selectedIndex];

        // Check if there's item in that slot, then display image and description.
        if (slot.GetItem())
        {
            components.displayName.text = slot.GetItem().itemName;
            components.displayImage.sprite = slot.GetItem().itemTexture;
            components.displayDescription.text = slot.GetItem().itemDescription;
        }

        components.displayName.enabled = slot.GetItem();
        components.displayImage.enabled = slot.GetItem();
        components.displayDescription.enabled = slot.GetItem();
    }

    // Function to load all slots.
    private void Initialize()
    {
        for (int i = 0; i < components.inventoryParent.childCount; i++)
        {
            Transform slotObject = components.inventoryParent.GetChild(i);
            itemList.Add(slotObject.GetComponent<InventorySlot>());
        }
    }

    // Function to change inventory panel.
    public void SetInventoryPanel(Transform parent)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Transform invTransform = itemList[i].transform;
            invTransform.SetParent(parent);
        }
    }

    // Function to set inventory panel back to default
    public void SetDefaultInventoryPanel() => SetInventoryPanel(components.inventoryParent);

    // Function to refresh inventory.
    public void Refresh()
    {
        components.HotBarManager.ContentUpdate();
    }

    // Function to add item to inventory. (For Unity Event)
    public void AddItem(Item item)
    {
        AddItem(item, 1);
    }

    // Function to add item to slot in inventory based on Item reference. And return amount that haven't been added into inventory.
    public int AddItem(Item item, int amount)
    {
        // If item is null or number is negative or zero, return.
        if (!item || amount <= 0)
        {
            return amount;
        }

        AudioManager.instance.PlaySound("Additem");

        List<InventorySlot> slots = GetSlots(item);

        int remainAmount = amount;
        int exceedAmount = 0;

        int itemAmount = GetItemAmount(item);

        // If amount to add is exceed maxmimum amount of item, return exceed amount of item.
        if (itemAmount + amount > item.maxAmount)
        {
            remainAmount = item.maxAmount - itemAmount;
            exceedAmount = amount - remainAmount;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (remainAmount == 0)
            {
                break;
            }

            InventorySlot slot = slots[i];

            if (item.stackable)
            {
                if (slot.GetAmount() + remainAmount >= stackAmount)
                {
                    remainAmount -= stackAmount - slot.GetAmount();
                    slot.SetAmount(stackAmount);
                    Refresh();
                }
                else
                {
                    slot.AddAmount(remainAmount);
                    remainAmount = 0;
                    Refresh();
                }
            }
            else
            {
                if (remainAmount > 1)
                {
                    remainAmount--;
                }
                else
                {
                    remainAmount = 0;
                }

                slot.SetAmount(1);
                Refresh();
            }
        }

        if (remainAmount > 0)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (remainAmount == 0)
                {
                    break;
                }

                InventorySlot slot = itemList[i];

                if (slots.Contains(slot))
                {
                    continue;
                }

                if (!slot.GetItem())
                {
                    if (item.stackable)
                    {
                        slot.SetItem(item);

                        if (slot.GetAmount() + remainAmount >= stackAmount)
                        {
                            remainAmount -= stackAmount - slot.GetAmount();
                            slot.SetAmount(stackAmount);
                            Refresh();
                        }
                        else
                        {
                            slot.AddAmount(remainAmount);
                            remainAmount = 0;
                            Refresh();
                        }
                    }
                    else
                    {
                        slot.SetItem(item);

                        if (remainAmount > 1)
                        {
                            remainAmount--;
                        }
                        else
                        {
                            remainAmount = 0;
                        }

                        slot.SetAmount(1);
                        Refresh();
                    }
                }
            }
        }

        SaveData();
        return remainAmount + exceedAmount;
    }

    // Function to add item to slot in inventory based on Item name reference. And return amount that haven't been added into inventory.
    public int AddItem(string itemName, int amount)
    {
        // If item is null or number is negative or zero, return.
        if (!FindItem(itemName) || amount <= 0)
        {
            return amount;
        }

        AudioManager.instance.PlaySound("Additem");

        Item item = FindItem(itemName);
        List<InventorySlot> slots = GetSlots(itemName);

        // Remain amount of item.
        int remainAmount = amount;
        int exceedAmount = 0;

        int itemAmount = GetItemAmount(item);

        // If amount to add is exceed maxmimum amount of item, return exceed amount of item.
        if (itemAmount + amount > item.maxAmount)
        {
            remainAmount = item.maxAmount - itemAmount;
            exceedAmount = amount - remainAmount;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            // If there's no more remain amount, out of loop.
            if (remainAmount == 0)
            {
                break;
            }

            InventorySlot slot = slots[i];

            // If item is stackable, add amount by remain amount, else, add only one.
            if (slot.GetItem().stackable)
            {
                if (slot.GetAmount() + remainAmount >= stackAmount)
                {
                    remainAmount -= stackAmount - slot.GetAmount();
                    slot.SetAmount(stackAmount);
                    Refresh();
                }
                else
                {
                    slot.AddAmount(remainAmount);
                    remainAmount = 0;
                    Refresh();
                }
            }
            else
            {
                if (remainAmount > 1)
                {
                    remainAmount--;
                }
                else
                {
                    remainAmount = 0;
                }

                slot.SetAmount(1);
                Refresh();
            }
        }

        // If there's still more remain amount, do another loop again.
        if (remainAmount > 0)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (remainAmount == 0)
                {
                    break;
                }

                InventorySlot slot = itemList[i];

                if (slots.Contains(slot))
                {
                    continue;
                }

                if (!slot.GetItem())
                {
                    if (item.stackable)
                    {
                        slot.SetItem(item);

                        if (slot.GetAmount() + remainAmount >= stackAmount)
                        {
                            remainAmount -= stackAmount - slot.GetAmount();
                            slot.SetAmount(stackAmount);
                            Refresh();
                        }
                        else
                        {
                            slot.AddAmount(remainAmount);
                            remainAmount = 0;
                            Refresh();
                        }
                    }
                    else
                    {
                        slot.SetItem(item);

                        if (remainAmount > 1)
                        {
                            remainAmount--;
                        }
                        else
                        {
                            remainAmount = 0;
                        }

                        slot.SetAmount(1);
                        Refresh();
                    }
                }
            }
        }

        SaveData();
        return remainAmount + exceedAmount;
    }

    // Function to remove item from slot in inventory based on Item reference. And return amount that haven't been remove from inventory.
    public bool RemoveItem(Item item, int amount)
    {
        // If item is null or number is negative or zero, return.
        if (!item || amount <= 0)
        {
            return false;
        }

        // If there's not enough item to remove, return
        if (!HasEnoughItem(item, amount))
        {
            return false;
        }

        int remainAmount = amount;

        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (remainAmount == 0)
            {
                break;
            }

            InventorySlot slot = itemList[i];

            if (slot.GetItem() && slot.GetItem().Equals(item))
            {
                if (remainAmount >= slot.GetAmount())
                {
                    remainAmount -= slot.GetAmount();
                    slot.SetAmount(0);
                    Refresh();
                }
                else
                {
                    slot.RemoveAmount(remainAmount);
                    remainAmount = 0;
                    Refresh();
                    break;
                }
            }
        }

        SaveData();
        return true;
    }

    // Function to remove item from slot in inventory based on Item name reference. And return amount that haven't been remove from inventory.
    public bool RemoveItem(string itemName, int amount)
    {
        // If item is null or number is negative or zero, return.
        if (!FindItem(itemName) || amount <= 0)
        {
            return false;
        }

        // If there's not enough item to remove, return
        if (!HasEnoughItem(itemName, amount))
        {
            return false;
        }

        int remainAmount = amount;

        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (remainAmount == 0)
            {
                break;
            }

            InventorySlot slot = itemList[i];

            if (slot.GetItem() && slot.GetItem().itemName.Equals(itemName))
            {
                if (remainAmount >= slot.GetAmount())
                {
                    remainAmount -= slot.GetAmount();
                    slot.SetAmount(0);
                    Refresh();
                }
                else
                {
                    slot.RemoveAmount(remainAmount);
                    remainAmount = 0;
                    Refresh();
                    break;
                }
            }
        }

        SaveData();
        return true;
    }

    // Function to return amount of item in inventory based on Item reference.
    public int GetItemAmount(Item item)
    {
        if (!item)
        {
            return 0;
        }

        List<InventorySlot> items = GetSlots(item);
        int amount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetItem() && items[i].GetItem().Equals(item))
            {
                amount += items[i].GetAmount();
            }
        }

        return amount;
    }

    // Function to return amount of item in inventory based on Item name.
    public int GetItemAmount(string itemName)
    {
        if (!FindItem(itemName))
        {
            return 0;
        }

        List<InventorySlot> items = GetSlots(FindItem(itemName));
        int amount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetItem() && items[i].GetItem().itemName.Equals(itemName))
            {
                amount += items[i].GetAmount();
            }
        }

        return amount;
    }

    // Function to determine if player has enough item.
    public bool HasEnoughItem(Item item, int amount)
    {
        if (!item)
        {
            return false;
        }

        return GetItemAmount(item) >= amount;
    }

    // Function to determine if player has enough item.
    public bool HasEnoughItem(string itemName, int amount)
    {
        if (!FindItem(itemName))
        {
            return false;
        }

        return GetItemAmount(FindItem(itemName)) >= amount;
    }

    // Function to fetch all item slot based on item data reference.
    private List<InventorySlot> GetSlots(Item item)
    {
        List<InventorySlot> slots = new List<InventorySlot>();

        for (int i = 0; i < itemList.Count; i++)
        {
            Item slotItem = itemList[i].GetItem();

            if (slotItem && slotItem.Equals(item))
            {
                slots.Add(itemList[i]);
            }
        }

        return slots;
    }

    // Function to fetch all item slot based on item name reference.
    private List<InventorySlot> GetSlots(string itemName)
    {
        List<InventorySlot> slots = new List<InventorySlot>();

        for (int i = 0; i < itemList.Count; i++)
        {
            Item slotItem = itemList[i].GetItem();

            if (slotItem && slotItem.itemName.Equals(itemName))
            {
                slots.Add(itemList[i]);
            }
        }

        return slots;
    }

    // Function to fetch all slots.
    public List<InventorySlot> GetAllSlots() => itemList;

    // Function to set select slot. (Use through Button event)
    public void SetSelectSlot(InventorySlot slot)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].Equals(slot))
            {
                selectedIndex = i;
            }
            else
            {
                itemList[i].Unselected();
            }
        }
    }

    // Function to clear all item selection.
    public void ClearSelection()
    {
        selectedIndex = -1;

        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].Unselected();
        }
    }

    // Function to get select slot
    public InventorySlot GetSelectSlot()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (i == selectedIndex)
            {
                return itemList[i];
            }
        }

        return null;
    }

    // TEMPORARY FUNCTION SINCE THERE'LL BE MULTIPLE HOTBAR SLOTS IN THE FUTURE
    // Function t o get hotbar slot. []
    public InventorySlot GetHotbarSlot()
    {
        return itemList[components.HotBarManager.HotbarIndex];
    }

    // Function to fetch Item in project by item name.
    private Item FindItem(string itemName)
    {
        foreach (Item item in Resources.FindObjectsOfTypeAll(typeof(Item)))
        {
            if (item.itemName.Equals(itemName))
            {
                return item;
            }
        }

        return null;
    }

    // Function to fetch Stack amount.
    public int GetStackAmount()
    {
        InventorySizeUpgrade upgrade = UpgradeManager.instance.GetUpgrade("Inventory Size") as InventorySizeUpgrade;
        return upgrade.GetLevel() == 0 ? stackAmount : upgrade.GetStackAmount();
    }

    // Function to load game data.
    public void LoadData(GameData gameData)
    {
        List<InventorySlotData> itemDatas = gameData.inventories;

        if (itemList.Count != itemDatas.Count)
        {
            return;
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].SetItem(itemDatas[i].item);
            itemList[i].SetAmount(itemDatas[i].amount);
        }

        Refresh();
    }

    // Function to save game data.
    public void SaveData(ref GameData gameData)
    {
        List<InventorySlotData> itemDatas = new List<InventorySlotData>();

        for (int i = 0; i < itemList.Count; i++)
        {
            InventorySlotData slotData = new InventorySlotData();
            slotData.item = itemList[i].GetItem();
            slotData.amount = itemList[i].GetAmount();

            itemDatas.Add(slotData);
        }

        gameData.inventories = itemDatas;
    }

    // Function to save game data (with auto-reference to game data in game manager)
    private void SaveData()
    {
        GameData gameData = gameManager.GetGameData();
        SaveData(ref gameData);
        gameManager.SetGameData(gameData);
    }

    // Function to play Audio SFX.
    public void PlaySound(string name)
    {
        AudioManager.instance.PlaySound(name);
    }
}

[System.Serializable]
public class InvM_Comp
{
    [Header("Item Display")]
    [Tooltip("Image that will display on the left of inventory")]
    public Image displayImage;
    [Tooltip("Name text that will display on the left of inventory")]
    public TextMeshProUGUI displayName;
    [Tooltip("Description text that will display on the left of inventory")]
    public TextMeshProUGUI displayDescription;

    [Header("Prefabs")]
    [Tooltip("Prefab of InventorySlot")]
    public GameObject inventorySlotPrefab;

    [Header("References")]
    [Tooltip("Current inventory category parent")]
    public Transform inventoryParent;
    [Tooltip("Hotbar Manager")]
    [SerializeField] private HotbarManager _hotbarManager;

    public HotbarManager HotBarManager => _hotbarManager;
}

[System.Serializable]
public class InventorySlotData
{
    public Item item; // Item in this slot.
    public int amount = 0; // Amount of item.
}