using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour, IGameData
{
    public static UpgradeManager instance;

    [Tooltip("List of all upgrades as base")]
    [SerializeField] private List<BaseUpgrade> baseUpgrades = new List<BaseUpgrade>();

    private List<BaseUpgrade> upgrades = new List<BaseUpgrade>(); // List of current upgrades.

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    // Function to get upgrade by name.
    public BaseUpgrade GetUpgrade(string upgradeName)
    {
        if (upgrades.Count == 0)
        {
            LoadData(GameManager.instance.GetGameData());
        }

        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].upgradeName.Equals(upgradeName))
            {
                return upgrades[i];
            }
        }

        return null;
    }

    // Function to create upgrade by name.
    public BaseUpgrade CreateUpgrade(string upgradeName)
    {
        for (int i = 0; i < baseUpgrades.Count; i++)
        {
            if (baseUpgrades[i].upgradeName.Equals(upgradeName))
            {
                return Instantiate(baseUpgrades[i]);
            }
        }

        return null;
    }

    public void LoadData(GameData gameData)
    {
        if (upgrades.Count > 0)
        {
            return;
        }

        // If there's no data for game upgrade, create new one.
        if (gameData.upgrades == null || gameData.upgrades.Count == 0)
        {
            for (int i = 0; i < baseUpgrades.Count; i++)
            {
                BaseUpgrade baseUpgrade = Instantiate(baseUpgrades[i]);
                upgrades.Add(baseUpgrade);
            }

            return;
        }

        foreach (KeyValuePair<string, int> upgrades in gameData.upgrades)
        {
            BaseUpgrade upgrade = CreateUpgrade(upgrades.Key);
            upgrade.SetLevel(upgrades.Value);
            this.upgrades.Add(upgrade);
        }
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.upgrades.Clear();

        for (int i = 0; i < upgrades.Count; i++)
        {
            gameData.upgrades.Add(upgrades[i].upgradeName, upgrades[i].GetLevel());
        }
    }
}