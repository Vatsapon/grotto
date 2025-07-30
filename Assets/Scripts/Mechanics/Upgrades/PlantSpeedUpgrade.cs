using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Plant Speed Upgrade", menuName = "Upgrade/Plant Speed")]
public class PlantSpeedUpgrade : BaseUpgrade
{
    [Tooltip("Upgrades for each level")]
    public List<PlantSpeedUpgrade_Level> upgrades = new List<PlantSpeedUpgrade_Level>();

    public override List<BaseUpgrade_Level> GetUpgradeLevels() => upgrades.Cast<BaseUpgrade_Level>().ToList();

    public override string GetUpgradeLabel(int level)
    {
        return level == 0 ? "0%" : upgrades[level - 1].upgradeLabel;
    }

    // Function to get amount of speed that increase based on current upgrade level.
    public float GetSpeedIncrease()
    {
        if (level == 0)
        {
            return 0f;
        }

        return upgrades[level - 1].speedIncrease;
    }
}

[System.Serializable]
public class PlantSpeedUpgrade_Level : BaseUpgrade_Level
{
    [Range(0, 100)]
    [Tooltip("Amount of speed to increase (%)")]
    public float speedIncrease;
}
