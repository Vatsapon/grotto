using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum PlaceablePlants { Lilypad, Whirlybird, VenusLauncher, none }
public class PlacePlantInteractive : MonoBehaviour, IConditional
{
    public PlaceablePlants PlantToPlace = PlaceablePlants.Lilypad;
    public List<Item> PlantItems;

    [HideInInspector]
    public bool PlantPlaced = false;
    public BoxCollider InteractionCollider;

    [HideInInspector]
    public InventoryManager IM;

    private void Start()
    {
        IM = InventoryManager.instance;
    }

    private void Update()
    {
        if(!PlantPlaced)
        {
            InteractionCollider.enabled = true;
        }
    }
    public void Activate()
    {
        if(!PlantPlaced)
        {
            SpawnPlant();
            InteractionCollider.enabled = false;
        }
    }

    //Ent does not need approval to plant, will simply plant
    public void EntSpawnPlant()
    {
        if (!PlantPlaced)
        {
            GameObject plant = Instantiate(PlantItems[((int)PlantToPlace)].itemPrefab, transform.position, transform.rotation);
            plant.transform.parent = this.transform;
            PlantPlaced = true;
        }
    }

    public void SpawnPlant()
    {
        GameObject plant = null;

        PlayerHolder holder = FindObjectOfType<Player>().Holder;
        Item plantItem = PlantItems[(int)PlantToPlace];

        if (holder.IsVisualHolding())
        {
            // If there's item to plant, plant. Or else, play SFX.
            Item item = PlantItems[((int)PlantToPlace)];
            InventorySlot slot = IM.GetHotbarSlot();

            if (slot.GetItem() && slot.GetItem().Equals(item) && slot.GetAmount() > 0)
            {
                LevelManager.instance.AddPlantLimit(item);

                IM.RemoveItem(item, 1);
                plant = Instantiate(PlantItems[((int)PlantToPlace)].itemPrefab, transform.position, transform.rotation);
                plant.transform.parent = this.transform;
                PlantPlaced = true;
            }
            else
            {
                AudioManager.instance.PlaySound("NoPlant");
            }

            return;
        }

        // If player is holding a plant, use plant that player is holding to check.
        if (holder.IsHolding())
        {
            // If plant that holding can't be place here, cancel.
            if (plantItem.Equals(holder.GetHolding()))
            {
                plant = Instantiate(holder.PlacePlant().itemPrefab, transform.position, transform.rotation);
                plant.transform.parent = this.transform;
                PlantPlaced = true;
            }
            else
            {
                AudioManager.instance.PlaySound("NoPlant");
            }
        }
    }

    public bool IsActivate()
    {
        return PlantPlaced;
    }

    public void SetActivate(bool value)
    {
        // NOTE: It shouldn't be able to force to place plant.
    }
}
