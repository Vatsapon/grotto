using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseUpgrade : ScriptableObject
{
    [Tooltip("Name of this upgrade")]
    public string upgradeName;
    [MySprite(50)]
    [Tooltip("Sprite of this upgrade")]
    public Sprite upgradeSprite;
    [TextArea(10, 20)]
    [Tooltip("Description of this upgrade")]
    public string upgradeDescription;

    [Space(5f)]

    [Tooltip("Label to show for this upgrade")]
    public string upgradeLabel;

    protected int level = 0; // Current level of this upgrade.

    // Function to set level of this upgrade.
    public void SetLevel(int level) => this.level = level;

    // Function to increase level of this upgrade.
    public void Upgrade() => level++;

    // Function to get current level.
    public int GetLevel() => level;

    // Function to determine if this upgrade reaches maximum level or not.
    public bool IsMaxLevel() => level >= GetUpgradeLevels().Count;

    // Funciton to get all upgrade levels.
    public virtual List<BaseUpgrade_Level> GetUpgradeLevels() => new List<BaseUpgrade_Level>();

    // Function to get price based on current level.
    public virtual int GetPrice()
    {
        List<BaseUpgrade_Level> upgrades = GetUpgradeLevels();
        return upgrades[level].price;
    }

    // Function to get label of current upgrade.
    public string GetUpgradeLabel() => GetUpgradeLabel(level);

    // Function to get label of current upgrade.
    public virtual string GetUpgradeLabel(int level) => "";
}

[System.Serializable]
public class BaseUpgrade_Level
{
    [Tooltip("Price to upgrade to this level")]
    public int price;
    [Tooltip("Label for this upgrade")]
    public string upgradeLabel;
}