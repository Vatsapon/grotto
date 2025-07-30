using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDataDrawer
{
    [MenuItem("GameData/Clear Data")]
    private static void ClearGameData()
    {
        if (GameManager.instance)
        {
            GameManager.instance.ClearGameData();
        }
        else
        {
            FileDataHandler fileHandler = new FileDataHandler(Application.persistentDataPath, "Grotto.Grotto");
            fileHandler.Delete();
        }

        Debug.Log("Cleared game datas");
    }
}
