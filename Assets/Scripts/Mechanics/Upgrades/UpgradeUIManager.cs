using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    [Tooltip("Parent of all upgrades")]
    [SerializeField] private Transform upgradeParent;

    [Header("References")]

    [Tooltip("Icon of selected upgrade")]
    [SerializeField] private Image upgradeIcon;
    [Tooltip("Name of selected upgrade")]
    [SerializeField] private TextMeshProUGUI upgradeName;
    [Tooltip("Description of selected upgrade")]
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [Tooltip("Text to show amount of coin")]
    [SerializeField] private TextMeshProUGUI coinText;
    [Tooltip("Button to upgrade")]
    [SerializeField] private Button upgradeButton;

    private BaseUpgrade currentSelectUpgrade;
    private UpgradeManager upgradeManager;

    private void Start()
    {
        upgradeManager = UpgradeManager.instance;

        Setup();
    }

    private void Update()
    {
        int goldCoin = GameManager.instance.GetGoldCoin();

        coinText.text = "<sprite=0> " + goldCoin;

        upgradeIcon.enabled = currentSelectUpgrade;
        upgradeIcon.sprite = currentSelectUpgrade ? currentSelectUpgrade.upgradeSprite : null;
        upgradeName.text = currentSelectUpgrade ? currentSelectUpgrade.upgradeName : "";
        upgradeDescription.text = currentSelectUpgrade ? currentSelectUpgrade.upgradeDescription : "";

        string buttonLabel = "Upgrade    <sprite=0>";

        if (currentSelectUpgrade)
        {
            buttonLabel = currentSelectUpgrade.IsMaxLevel() ? "Max" : "Upgrade    <sprite=0>" + currentSelectUpgrade.GetPrice().ToString();
        }
        else
        {
            buttonLabel = "Not selected yet";
        }

        upgradeButton.gameObject.SetActive(currentSelectUpgrade);
        upgradeButton.interactable = currentSelectUpgrade ? !currentSelectUpgrade.IsMaxLevel() : false;
        upgradeButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = buttonLabel;
    }

    // Function to select the upgprade.
    private void Select(UpgradeUI ui, BaseUpgrade upgrade)
    {
        currentSelectUpgrade = upgrade;

        for (int i = 0; i < upgradeParent.childCount; i++)
        {
            Transform upgradeObj = upgradeParent.GetChild(i);
            UpgradeUI upgradeUI = upgradeObj.GetComponent<UpgradeUI>();

            if (upgradeUI.Equals(ui))
            {
                upgradeUI.Select();
            }
            else
            {
                upgradeUI.Unselect();
            }
        }
    }

    // Function to increase selected upgrade.
    public void Upgrade()
    {
        if (!currentSelectUpgrade)
        {
            return;
        }

        GameManager gameM = GameManager.instance;
        
        // If player has enough coin to upgrade and not reach maximum level yet...
        if (!currentSelectUpgrade.IsMaxLevel() && gameM.GetGoldCoin() >= currentSelectUpgrade.GetPrice())
        {
            gameM.RemoveGoldCoin(currentSelectUpgrade.GetPrice());

            currentSelectUpgrade.Upgrade();
        }
    }

    // Function to setup UI.
    private void Setup()
    {
        for (int i = 0; i < upgradeParent.childCount; i++)
        {
            Transform upgrade = upgradeParent.GetChild(i);
            UpgradeUI upgradeUI = upgrade.GetComponent<UpgradeUI>();

            BaseUpgrade baseUpgrade = upgradeManager.GetUpgrade(upgradeUI.GetUpgradeName());

            Button button = upgrade.transform.Find("Upgrade Icon").GetComponent<Button>();
            button.onClick.AddListener(delegate
            {
                Select(upgradeUI, baseUpgrade);
            });
        }
    }
}
