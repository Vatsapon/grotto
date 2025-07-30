using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileDataHandler
{
    private string directoryPath;
    private string filePath;

    public FileDataHandler(string directoryPath, string filePath)
    {
        this.directoryPath = directoryPath;
        this.filePath = filePath;
    }

    // Function to load game data from file.
    public GameData Load()
    {
        string path = Path.Combine(directoryPath, filePath);

        GameData gameData = null;

        if (File.Exists(path))
        {
            try
            {
                string jsonData = "";

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }

                gameData = JsonUtility.FromJson<GameData>(jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to load game data from file! (" + e + ")");
            }
        }

        return gameData;
    }

    // Function to save game data to file.
    public void Save(GameData gameData)
    {
        string path = Path.Combine(directoryPath, filePath);
        
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            string jsonData = JsonUtility.ToJson(gameData, true);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(jsonData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to save game data to file! (" + e + ")");
        }
    }

    // Function to delete game data file.
    public void Delete()
    {
        string path = Path.Combine(directoryPath, filePath);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
