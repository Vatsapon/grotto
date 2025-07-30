using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneTransitionController;

[Serializable]
[CreateAssetMenu(fileName = "New Dungeon Pool", menuName = "Dungeon Pool")]
public class DungeonPool : ScriptableObject
{
    [Tooltip("Name of this dungeon pool")]
    [SerializeField] private string _poolName;

    [Space(10f)]

    [Tooltip("Next scene to load after complete all level in this pool")]
    [SerializeField] private DungeonLevel _nextLevelAfter;

    [Space(10f)]

    [ReadOnly]
    [Tooltip("Determine if player has complete this pool or not")]
    [SerializeField] private bool _isComplete = false;
    [ReadOnly]
    [Tooltip("Current level of this dungeon pool")]
    [SerializeField] private int _currentLevel = 0;

    [Space(10f)]

    [Tooltip("List of all level in this pool")]
    [SerializeField] private DungeonLevel[] _levelPools;

    public string PoolName => _poolName;
    public DungeonLevel NextLevelAfter => _nextLevelAfter;
    public DungeonLevel[] LevelPools => _levelPools;
    public DungeonLevel CurrentLevel => _levelPools[_currentLevel];
    public bool IsComplete => _isComplete;

    // Function to reset progress.
    public void Resets() => _currentLevel = 0;

    // Function to go to next level
    public void NextLevel() => _currentLevel++;
    
    // Function to determine if this pool is reaches the end or not.
    public bool ReachEnd() => _currentLevel >= _levelPools.Length;

    // Function to marked this pool as complete.
    public void Complete() => _isComplete = true;
}

[Serializable]
public class DungeonLevel
{
    [Tooltip("Name of this scene")]
    [SerializeField] private string _sceneName;
    [Tooltip("Type of this scene")]
    [SerializeField] private SceneType _sceneType;

    [Space(5f)]

    [Tooltip("List of all plant requirements")]
    [SerializeField] private List<LevelPlantRequirement> _plantRequirements;

    public string SceneName => _sceneName;
    public SceneType SceneType => _sceneType;
    public List<LevelPlantRequirement> PlantRequirements => _plantRequirements;
}

[Serializable]
public class LevelPlantRequirement
{
    [Tooltip("Item of this plant")]
    [SerializeField] private Item _plantItem;
    [Tooltip("Minimum amount of this plant in this level")]
    [SerializeField] private int _minimumAmount;

    public Item PlantItem => _plantItem;
    public int MinimumAmount => _minimumAmount;

    // Function to increase amount of plant.
    public void Increase() => _minimumAmount++;

    public LevelPlantRequirement(Item plantItem, int minimumAmount)
    {
        _plantItem = plantItem;
        _minimumAmount = minimumAmount;
    }
}