using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPoolManager : MonoBehaviour, IGameData
{
    public static DungeonPoolManager instance;

    [ReadOnly]
    [SerializeField] private DungeonPool _currentDungeonPool; // Current dungeon pool that will be use.

    public DungeonPool CurrentDungeonPool => _currentDungeonPool;

    private List<string> _completePools = new List<string>(); // Pool that player has completed.

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // Function to set current dungeon pool.
    public void SetPool(DungeonPool pool) => _currentDungeonPool = pool ? Instantiate(pool) : null;
    
    // Function to determine if dungeon pool is complete or not.
    public bool IsPoolComplete(DungeonPool pool)
    {
        for (int i = 0; i < _completePools.Count; i++)
        {
            if (_completePools[i].Equals(pool.PoolName))
            {
                return true;
            }
        }

        return false;
    }

    // Function to load next level.
    public void LoadNextLevel()
    {
        _currentDungeonPool.NextLevel();

        if (!_currentDungeonPool.ReachEnd())
        {
            GameManager.instance.SaveGameData();
            SceneManager.LoadScene(_currentDungeonPool.CurrentLevel.SceneName, LoadSceneMode.Single);
        }
        else
        {
            // Mark the dungeon as complete and add to complete list.
            _currentDungeonPool.Complete();
            _completePools.Add(_currentDungeonPool.PoolName);

            string nextLevelAfter = _currentDungeonPool.NextLevelAfter.SceneName;
            SetPool(null);

            GameManager.instance.SaveGameData();
            SceneManager.LoadScene(nextLevelAfter, LoadSceneMode.Single);
        }
    }

    public void LoadData(GameData gameData)
    {
        _completePools = gameData.completePools;
        SetPool(gameData.currentDungeonPool);
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.completePools = _completePools;

        // If there's current dungeon pool and save to game data.
        if (_currentDungeonPool)
        {
            gameData.currentDungeonPool = _currentDungeonPool.IsComplete ? null : _currentDungeonPool;
        }
        else
        {
            gameData.currentDungeonPool = _currentDungeonPool;
        }
    }
}