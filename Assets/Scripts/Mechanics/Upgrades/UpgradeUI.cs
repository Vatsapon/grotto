using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Tooltip("Name of this upgrade")]
    [SerializeField] private string upgradeName;

    [Header("References")]

    [Tooltip("Icon for this upgrade")]
    [SerializeField] private Image upgradeIcon;
    [Tooltip("Upgrade Selected icon")]
    [SerializeField] private GameObject upgradeSelected;
    [Tooltip("Parent of upgrade levels")]
    [SerializeField] private Transform upgradeLevelParent;

    private bool selected = false; // Determine if this upgrade is selecting or not.

    private UpgradeManager upgradeManager;

    private void Start()
    {
        upgradeManager = UpgradeManager.instance;
    }

    private void Update()
    {
        BaseUpgrade upgrade = upgradeManager.GetUpgrade(upgradeName);
        upgradeIcon.sprite = upgrade.upgradeSprite;

        int level = upgrade.GetLevel();

        for (int i = 0; i < upgradeLevelParent.childCount; i++)
        {
            Transform upgradeLevel = upgradeLevelParent.GetChild(i);
            upgradeLevel.Find("Unlocked").gameObject.SetActive(i < level);
            upgradeLevel.Find("Selected").gameObject.SetActive(i < level && selected);
        }

        upgradeSelected.SetActive(selected);
    }

    // Function to select this upgrade.
    public void Select() => selected = true;

    // Function to unselect this upgrade.
    public void Unselect() => selected = false;

    // Function to get upgrade name.
    public string GetUpgradeName() => upgradeName;
}
