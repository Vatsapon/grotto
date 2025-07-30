using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

[System.Serializable]
[RequireComponent(typeof(Interactive))]
public class PlantableLocation : MonoBehaviour, IGameData
{
    private GameObject currentPlant;
    private Item plantItem;

    private Interactive interactive;

    // Start is called before the first frame update
    void Start()
    {
        interactive = GetComponent<Interactive>();
    }

    public void PlacePlantableSeed()
    {
        InventoryManager invM = InventoryManager.instance;

        //if no plant here, then put one
        if (!currentPlant)
        {
            // If amount of item is 0 or it's not seed item, cancel.
            if (!IsPlantable())
            {
                AudioManager.instance.PlaySound("NoPlant");
                return;
            }

            Item currentItem = invM.GetHotbarSlot().GetItem();
            invM.RemoveItem(currentItem, 1);

            AudioManager.instance.PlaySound("PlantDirt");

            //item to spawn, spawn location, spawn rotation
            GameObject newPlant = Instantiate(currentItem.itemPrefab, transform.position, Quaternion.identity);

            currentPlant = newPlant;
            plantItem = currentItem;

            // Make player unable to use interactive of this one and use plant object collider instead.
            interactive.GetComponent<Collider>().enabled = false;
        }
        else
        {
            AudioManager.instance.PlaySound("NoPlant");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //make sure you can only interact with the plantable location if there is not already a plant
        if (!currentPlant)
        {
            if (IsPlantable())
            {
                plantItem = null;
                interactive.GetComponent<Collider>().enabled = true;
                interactive.ActivateConditional();
            }
            else
            {
                interactive.GetComponent<Collider>().enabled = false;
                interactive.DeActivateConditional();
            }
        }
        else
        {
            interactive.GetComponent<Collider>().enabled = false;
            interactive.DeActivateConditional();
        }
    }
    
    // Function to determine if player can plant or not.
    private bool IsPlantable()
    {
        InventorySlot slot = InventoryManager.instance.GetHotbarSlot();
        return slot.GetItem() && slot.GetAmount() > 0 && slot.GetItem().itemType == ItemType.Seed;
    }

    // Function to load game data.
    public void LoadData(GameData gameData)
    {
        PlantableData plantableData;
        gameData.plantLocations.TryGetValue(name, out plantableData);

        if (plantableData != null)
        {
            if (plantableData.plantItem)
            {
                plantItem = plantableData.plantItem;
                currentPlant = Instantiate(plantableData.plantItem.itemPrefab, transform.position, Quaternion.identity);

                BasePlant plant = currentPlant.GetComponent<BasePlant>();
                plant.SetTimeLeft(plantableData.growthTimeLeft);
                plant.SetPlantStateObject(plantableData.currentObjectStateName);
            }
        }
    }

    // Function to save game data.
    public void SaveData(ref GameData gameData)
    {
        string objectID = name;

        if (gameData.plantLocations.ContainsKey(objectID))
        {
            gameData.plantLocations.Remove(objectID);
        }

        PlantableData plantableData = new PlantableData();
        plantableData.plantItem = plantItem;

        if (currentPlant)
        {
            BasePlant plant = currentPlant.GetComponent<BasePlant>();

            plantableData.growthTimeLeft = plant.GetTimeLeft();
            plantableData.currentObjectStateName = plant.GetPlantStateObject().name;
        }

        gameData.plantLocations.Add(objectID, plantableData);
    }
}

[System.Serializable]
public class PlantableData
{
    public Item plantItem; // Item of plant

    public float growthTimeLeft = 0f; // Time left to grow
    public string currentObjectStateName = ""; // Current plant game object state
}
