using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Inventory Size Upgrade", menuName = "Upgrade/Inventory Size")]
public class InventorySizeUpgrade : BaseUpgrade
{
    [Tooltip("Upgrades for each level")]
    public List<InventorySizeUpgrade_Level> upgrades = new List<InventorySizeUpgrade_Level>();

    public override List<BaseUpgrade_Level> GetUpgradeLevels() { return upgrades.Cast<BaseUpgrade_Level>().ToList(); }

    public override string GetUpgradeLabel(int level)
    {
        return level == 0 ? InventoryManager.instance.GetStackAmount().ToString() : upgrades[level - 1].upgradeLabel;
    }

    // Function to get amount of stack based on current upgrade level.
    public int GetStackAmount()
    {
        if (level == 0)
        {
            return InventoryManager.instance.GetStackAmount();
        }

        return upgrades[level - 1].stackAmount;
    }
}

[System.Serializable]
public class InventorySizeUpgrade_Level : BaseUpgrade_Level
{
    [Tooltip("Amount of item that can stack for this level")]
    public int stackAmount;
}