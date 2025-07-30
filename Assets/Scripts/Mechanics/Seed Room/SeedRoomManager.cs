using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedRoomManager : MonoBehaviour, IGameData
{
    [Tooltip("Area that contains all seed areas")]
    [SerializeField] private List<SeedArea> seedAreas = new List<SeedArea>();

    // Function to load data.
    public void LoadData(GameData gameData)
    {

    }

    // Function to save data.
    public void SaveData(ref GameData gameData)
    {

    }
}
