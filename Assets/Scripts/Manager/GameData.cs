using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Farm
    public SerializableDictionary<string, PlantableData> plantLocations;

    // Inventory
    public List<InventorySlotData> inventories;

    // Day/Night Cycle
    public CycleVariableData cycleData;

    // Teleport point
    public SerializableDictionary<string, WaypointPosition> wayPoints;

    // Currency
    public int goldCoin;

    // Upgrades
    public SerializableDictionary<string, int> upgrades;

    // Dungeon Pool
    public List<string> completePools;
    public DungeonPool currentDungeonPool;

    // Tutorial
    public bool isTutorialize;
    public string tutorialName;

    // Audio
    public float musicVolume;
    public float soundVolume;

    public GameData()
    {
        this.plantLocations = new SerializableDictionary<string, PlantableData>();
        this.inventories = new List<InventorySlotData>();
        this.wayPoints = new SerializableDictionary<string, WaypointPosition>();
        this.goldCoin = 0;
        this.upgrades = new SerializableDictionary<string, int>();
        this.completePools = new List<string>();
        this.currentDungeonPool = null;
        this.isTutorialize = false;
        this.tutorialName = null;
        this.musicVolume = 100f;
        this.soundVolume = 100f;
    }
}

public interface IGameData
{
    /// <summary>
    /// Function that execute when game is loading data.
    /// </summary>
    /// <param name="gameData"></param>
    void LoadData(GameData gameData);

    /// <summary>
    /// Function that execute when game is saving data.
    /// </summary>
    /// <param name="gameData"></param>
    void SaveData(ref GameData gameData);
}