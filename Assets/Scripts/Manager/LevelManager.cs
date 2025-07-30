using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Tooltip("Determine to give & show reward in this level or not")]
    [SerializeField] private bool _hasReward = true;

    [Header("Reward Settings")]

    [Tooltip("Amount of reward for complete level")]
    [SerializeField] private int _completeGoldReward = 10;
    [Tooltip("Amount of reward for complete level with minimum requirement")]
    [SerializeField] private int _completeMinimumReward = 30;

    [Header("References")]

    [Tooltip("Reward UI Panel")]
    [SerializeField] private GameObject _rewardPanel;
    [Tooltip("Parent of stars UI")]
    [SerializeField] private Transform _starsParent;
    [Tooltip("Text to display coin")]
    [SerializeField] private TextMeshProUGUI _coinText;

    private LevelPlantRequirement[] currentMinimumList;
    private string coinTextFormat;

    private DungeonPoolManager poolM;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        poolM = DungeonPoolManager.instance;
        currentMinimumList = new LevelPlantRequirement[0];

        coinTextFormat = _coinText.text;

        if (poolM.CurrentDungeonPool)
        {
            List<LevelPlantRequirement> plantRequirement = poolM.CurrentDungeonPool.CurrentLevel.PlantRequirements;
            currentMinimumList = new LevelPlantRequirement[plantRequirement.Count];

            for (int i = 0; i < plantRequirement.Count; i++)
            {
                currentMinimumList[i] = new LevelPlantRequirement(plantRequirement[i].PlantItem, 0);
            }
        }
    }

    // Function to execute when player complete the level.
    public void Complete()
    {
        if (!_hasReward)
        {
            Continue();
            return;
        }

        int starAmount = 1;
        int sum = _completeGoldReward;

        if (IsMinimumComplete())
        {
            starAmount++;
            sum += _completeMinimumReward;
        }

        PauseManager.instance.PauseGame(_rewardPanel, false);
        GameManager.instance.AddGoldCoin(sum);

        for (int i = 0; i < starAmount; i++)
        {
            if (i <= _starsParent.childCount)
            {
                _starsParent.GetChild(i).Find("Icon/Fill").gameObject.SetActive(true);
            }
        }

        _coinText.text = coinTextFormat.Replace("<coin>", sum.ToString());
    }

    // Function to continue to next level.
    public void Continue()
    {
        PauseManager.instance.ResumeGame();
        FindObjectOfType<DungeonDoor>().LoadScene();
    }

    // Function to determine if player complete level with minimum plant requirement or not.
    public bool IsMinimumComplete()
    {
        // If there's no dungeon pool, return true.
        if (!poolM.CurrentDungeonPool)
        {
            return true;
        }

        List<LevelPlantRequirement> plantRequirement = poolM.CurrentDungeonPool.CurrentLevel.PlantRequirements;

        for (int i = 0; i < currentMinimumList.Length; i++)
        {
            LevelPlantRequirement current = currentMinimumList[i];
            LevelPlantRequirement minimum = plantRequirement[i];

            if (!current.PlantItem.Equals(minimum.PlantItem) || current.MinimumAmount > minimum.MinimumAmount)
            {
                return false;
            }
        }

        return true;
    }

    // Function to add plant minimum limit.
    public void AddPlantLimit(Item plantItem)
    {
        for (int i = 0; i < currentMinimumList.Length; i++)
        {
            if (currentMinimumList[i].PlantItem.Equals(plantItem))
            {
                currentMinimumList[i].Increase();
                break;
            }
        }
    }
}