#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TilemapCreatePalette : PopupWindowContent
{
    public static TilemapCreatePalette instance { get; private set; }

    private string newPaletteName = ""; // Name of palette that will going to create.

    public override Vector2 GetWindowSize() => new Vector2(350f, EditorGUIUtility.singleLineHeight * 5f);

    public override void OnOpen()
    {
        base.OnOpen();

        instance = this;
    }

    public override void OnClose()
    {
        instance = null;

        base.OnClose();
    }

    public override void OnGUI(Rect rect)
    {
        Rect newFolderRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 5f));
        GUI.Box(newFolderRect, "", GUI.skin.window);

        float margin = 5f;

        newFolderRect.x += margin;
        newFolderRect.y += margin;
        newFolderRect.width -= margin * 2f;

        // Header
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        Rect headerRect = new Rect(newFolderRect.x, newFolderRect.y, newFolderRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(headerRect, "Creating new Palette", headerStyle);

        // Name
        Rect nameRect = headerRect;
        nameRect.y += EditorGUIUtility.singleLineHeight * 1.5f;
        GUIContent nameLabel = new GUIContent("Palette Name");
        nameLabel.tooltip = "Name of palette";

        newPaletteName = EditorGUI.TextField(nameRect, nameLabel, newPaletteName);

        // Create & Cancel button
        Rect buttonRect = nameRect;
        buttonRect.y += nameRect.height * 1.25f;
        buttonRect.width = (newFolderRect.width / 2f) - 5f;
        buttonRect.height = EditorGUIUtility.singleLineHeight * 1.25f;

        string path = TilemapEditor.instance.TilemapPath;

        // [Create] - Button to create palette.
        if (GUI.Button(buttonRect, "Create"))
        {
            // Check if name isn't empty, then ignore.
            if (newPaletteName.Equals(""))
            {
                Debug.LogError("The palette name is empty!");
                return;
            }

            // Check if there's folder with same name, then ignore.
            DirectoryInfo folderPath = new DirectoryInfo(path);
            string[] paletteFolders = new string[folderPath.GetDirectories().Length];

            for (int i = 0; i < paletteFolders.Length; i++)
            {
                paletteFolders[i] = folderPath.GetDirectories()[i].Name;
            }

            for (int i = 0; i < paletteFolders.Length; i++)
            {
                if (paletteFolders[i].Equals(newPaletteName))
                {
                    Debug.LogError("There's already that palette name in the path!");
                    return;
                }
            }

            AssetDatabase.CreateFolder(path, newPaletteName);
            Debug.Log("Create palette successfully!");

            editorWindow.Close();
        }

        buttonRect.x += buttonRect.width + 5f;

        // [Cancel] - Button to cancel.
        if (GUI.Button(buttonRect, "Cancel"))
        {
            editorWindow.Close();
        }
    }

    public static bool IsOpen() => instance != null; 
}
#endif