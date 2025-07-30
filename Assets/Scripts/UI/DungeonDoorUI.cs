using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonDoorUI : MonoBehaviour
{
    [Tooltip("Text to display current dungeon name")]
    [SerializeField] private TMP_Text _dungeonNameText;
    [Tooltip("Parent of all plant requirement list")]
    [SerializeField] private Transform _plantRequirementParent;
    [Tooltip("Button to enter the dungeon")]
    [SerializeField] private Button _enterButton;

    [Space(10f)]

    [Tooltip("Prefab of Plant Requirement UI")]
    [SerializeField] private GameObject _plantRequirementPrefab;

    private DungeonDoor selectedDoor; // Selected door.

    private void OnDisable()
    {
        Clear();
    }

    // Function to display the pool data.
    public void Refresh()
    {
        // Clear all plant requirements.
        for (int i = 0; i < _plantRequirementParent.childCount; i++)
        {
            Destroy(_plantRequirementParent.GetChild(i).gameObject);
        }

        _dungeonNameText.text = selectedDoor.DoorPool.PoolName;

        // Load all plant requirements.
        Dictionary<Item, int> plantItems = new Dictionary<Item, int>();

        for (int i = 0; i < selectedDoor.DoorPool.LevelPools.Length; i++)
        {
            DungeonLevel dungeonLevel = selectedDoor.DoorPool.LevelPools[i];
            
            for (int j = 0; j < dungeonLevel.PlantRequirements.Count; j++)
            {
                LevelPlantRequirement plantRequirement = dungeonLevel.PlantRequirements[j];
                
                if (!plantItems.ContainsKey(plantRequirement.PlantItem))
                {
                    plantItems.Add(plantRequirement.PlantItem, plantRequirement.MinimumAmount);
                }
                else
                {
                    plantItems[plantRequirement.PlantItem] += plantRequirement.MinimumAmount;
                }
            }
        }

        DungeonLevel lastLevel = selectedDoor.DoorPool.NextLevelAfter;

        for (int i = 0; i < lastLevel.PlantRequirements.Count; i++)
        {
            LevelPlantRequirement plantRequirement = lastLevel.PlantRequirements[i];

            if (!plantItems.ContainsKey(plantRequirement.PlantItem))
            {
                plantItems.Add(plantRequirement.PlantItem, plantRequirement.MinimumAmount);
            }
            else
            {
                plantItems[plantRequirement.PlantItem] += plantRequirement.MinimumAmount;
            }
        }

        bool reachMinimum = true;

        foreach (KeyValuePair<Item, int> plant in plantItems)
        {
            GameObject plantItem = Instantiate(_plantRequirementPrefab, _plantRequirementParent);
            plantItem.transform.Find("Icon Panel/Icon").GetComponent<Image>().sprite = plant.Key.itemTexture;
            plantItem.transform.Find("Name").GetComponent<TMP_Text>().text = plant.Key.itemName;

            int amount = InventoryManager.instance.GetItemAmount(plant.Key);
            plantItem.transform.Find("Requirement").GetComponent<TMP_Text>().text = amount + " / " + plant.Value;

            plantItem.GetComponent<CanvasGroup>().alpha = amount < plant.Value ? 1f : 0.5f;

            if (amount < plant.Value)
            {
                reachMinimum = false;
            }
        }

        _enterButton.interactable = reachMinimum;
    }

    // Function to set selected pool.
    public void SetDoor(DungeonDoor door) => selectedDoor = door;

    // Function to enter the dungeon.
    public void Enter()
    {
        selectedDoor.LoadScene();

        PauseManager.instance.ResumeGame();
    }

    // Function to clear pool.
    private void Clear()
    {
        selectedDoor = null;

        for (int i = 0; i < _plantRequirementParent.childCount; i++)
        {
            Destroy(_plantRequirementParent.GetChild(i).gameObject);
        }
    }
}
