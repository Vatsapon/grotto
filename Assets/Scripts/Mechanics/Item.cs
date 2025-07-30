using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        Seed, Plant
    }

    [MyHeader("Item")]

    [Tooltip("Type of this item")]
    public ItemType itemType = ItemType.Seed;

    [Space(5f)]

    [Tooltip("Item's texture/sprite")]
    [MySprite(50)]
    public Sprite itemTexture;
    [Tooltip("Item's name")]
    public string itemName;
    [Tooltip("Item's description")]
    [TextArea(5, 10)]
    public string itemDescription;

    [Space(10f)]

    [Tooltip("Determine if this item is able to sell or not")]
    public bool sellable = false;
    [Tooltip("Price for this item to sell")]
    public int sellPrice = 10;

    [MyHeader("Settings")]
    [Tooltip("Either this item is stackable or not")]
    public bool stackable = true;
    [Tooltip("Limitation of this item in inventory")]
    public int maxAmount = 10;

    [Space(10f)]
    [Header("References")]
    [Tooltip("Prefab of this item")]
    public GameObject itemPrefab;
    [Tooltip("Prefab of plant item when holding")]
    public GameObject holdingPrefab;
}
