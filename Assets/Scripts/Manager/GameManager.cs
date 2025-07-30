using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [MyHeader("Game Data")]
    [Tooltip("Name of your game data file")]
    [SerializeField] private string _fileName = "Game Data";
    [Tooltip("Determine if this scene isn't count for tutorial")]
    [SerializeField] private bool _isTutorial = false;

    [Button("Clear Data", "Clear game data", typeof(GameManager), "ClearGameData")]

    public GameManager_Component component;

    private GameData gameData; // Current game data.
    private FileDataHandler fileDataHandler; // File data handler.
    private List<IGameData> gameDataObjects = new List<IGameData>(); // All game object that has certain game data.

    private int goldCoin = 0; // Amount of gold coin.

    public static GameManager instance;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // If there's no audio manager in the scene, generate one.
        if (!AudioManager.instance && component.audioManagerPrefab)
        {
            Instantiate(component.audioManagerPrefab);
        }

        fileDataHandler = new FileDataHandler(Application.persistentDataPath, _fileName + ".Grotto");
        gameDataObjects = FindAllGameDataObjects();

        LoadGameData();
    }

    private void Update()
    {
        Debugging();
    }

    // Function to use debugging keys.
    private void Debugging()
    {
        // [C] - Add Gold Coin.
        if (Input.GetKey(KeyCode.C))
        {
            AddGoldCoin(100);
        }

        // [I] - Add all items by 1.
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryManager invM = InventoryManager.instance;

            for (int i = 0; i < invM.GetAllSlots().Count; i++)
            {
                InventorySlot slot = invM.GetAllSlots()[i];

                if (slot.GetItem())
                {
                    invM.AddItem(slot.GetItem(), 1);
                }
            }
        }

        // [T] - Unlock tutorial.
        if (Input.GetKeyDown(KeyCode.T))
        {
            gameData.isTutorialize = true;
        }
    }

    // Function to find all game data objects.
    private List<IGameData> FindAllGameDataObjects()
    {
        IEnumerable<IGameData> gameDataObjects = FindObjectsOfType<MonoBehaviour>().OfType<IGameData>();
        return new List<IGameData>(gameDataObjects);
    }

    // Function to get gold coin.
    public int GetGoldCoin() => goldCoin;

    // Function to add gold coin.
    public void AddGoldCoin(int amount) => goldCoin += amount;

    // Function to remove gold coin.
    public void RemoveGoldCoin(int amount)
    {
        goldCoin -= amount;

        if (goldCoin < 0)
        {
            goldCoin = 0;
        }
    }

    // Function to fetch game data.
    public GameData GetGameData() => gameData;

    // Function to set gamedata.
    public void SetGameData(GameData gameData) => this.gameData = gameData;

    // Function to load game data.
    public void LoadGameData()
    {
        gameData = fileDataHandler.Load();

        // If there's no game data, create a new one.
        if (gameData == null)
        {
            gameData = new GameData();
            Debug.LogWarning("[Game Data] Unable to find game data, initalizing new data...");
        }
        
        // Send game datas through all game object with game data.
        foreach (IGameData gameDataObject in gameDataObjects)
        {
            gameDataObject.LoadData(gameData);
        }

        goldCoin = gameData.goldCoin;
        Debug.Log("[Game Data] Loaded all game datas");
    }

    // Function to save game data.
    public void SaveGameData()
    {
        // Load game datas through all game object with game data.
        foreach (IGameData gameDataObject in gameDataObjects)
        {
            gameDataObject.SaveData(ref gameData);
        }

        gameData.goldCoin = goldCoin;

        fileDataHandler.Save(gameData);
        Debug.Log("[Game Data] Saved all game datas");
    }

    // Function to clear game data
    public void ClearGameData()
    {
        GameData oldGameData = gameData;
        gameData = new GameData();

        if (oldGameData != null)
        {
            gameData.musicVolume = oldGameData.musicVolume;
            gameData.soundVolume = oldGameData.soundVolume;
        }

        if (fileDataHandler == null)
        {
            fileDataHandler = new FileDataHandler(Application.persistentDataPath, _fileName + ".Grotto");
        }
        
        fileDataHandler.Delete();

        Debug.Log("[Game Data] Cleared game datas");
    }

    // Function to update audio volume. (Can't reference with AudioManager)
    public void AudioVolumeUpdater() => AudioManager.instance.VolumeUpdater();

    // Function to play music. (Can't reference with AudioManager)
    public void PlayMusic(string audioName) => AudioManager.instance.PlayMusic(audioName);

    // Function to execute when quit the game.
    private void OnApplicationQuit()
    {
        if (gameData.isTutorialize)
        {
            SaveGameData();
        }
        else
        {
            if (_isTutorial)
            {
                ClearGameData();
            }
            else
            {
                SaveGameData();
            }
        }
    }
}

[System.Serializable]
public class GameManager_Component
{
    [Header("References")]
    [Tooltip("Slider for Audio SFX")]
    public Slider audioSFXSlider;
    [Tooltip("Slider for Audio BGM")]
    public Slider audioBGMSlider;
    
    [Space(5f)]
    
    [Tooltip("Volume text for Audio SFX")]
    public TextMeshProUGUI audioSFXAmount;
    [Tooltip("Volume text for Audio BGM")]
    public TextMeshProUGUI audioBGMAmount;

    [Header("Prefabs")]
    [Tooltip("Audio Manager to instantiate, if there's none in the scene")]
    public GameObject audioManagerPrefab;
}