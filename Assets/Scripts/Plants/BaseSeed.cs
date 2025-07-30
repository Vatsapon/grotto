using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSeed : MonoBehaviour
{
    [Tooltip("Item for this seed")]
    public Item seedItem;

    [Tooltip("Number of seeds to give when this seed is picked up")]
    public int SeedsToGive;

    public void PickupSeed()
    {
       //add to the inventory
       InventoryManager.instance.AddItem(seedItem, SeedsToGive);

       //destroy the seed
       Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
