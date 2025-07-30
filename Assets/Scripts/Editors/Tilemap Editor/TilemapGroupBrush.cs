#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TilemapGroupBrush : ScriptableObject
{
    [SerializeField] public bool isExpand = false; // Determine to expand th section or not.
    [SerializeField] public bool isEnable = false; // Determine to enable this function or not.

    [SerializeField] public int density = 1; // Amount of density (prefab).
    [SerializeField] public bool randomRotation = false; // Determine to rotate randomly or not.

    [SerializeField] public bool isPrefabExpand = false; // Determine to expand prefab section or not.

    public string path = "Assets/Visuals/Tile Palette";
    [SerializeField] public List<GroupBrushObject> prefabs = new List<GroupBrushObject>(); // Prefab that will be use to draw on Painting.

    private List<string> prefabsPathList = new List<string>(); // Store current prefab's path of 'prefabs'.
    private List<string> prefabsList = new List<string>(); // Store all prefabs from directory.
    
    private int prefabIndex; // Index for changing prefab.

    public SerializedObject serializedObject; // Serialized Object of this scriptableobject.

    public void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        SerializedProperty elementProperty = prefabsProperty.GetArrayElementAtIndex(index);

        SerializedProperty nameProperty = elementProperty.FindPropertyRelative("name");
        SerializedProperty weightProperty = elementProperty.FindPropertyRelative("weight");
        SerializedProperty pathProperty = elementProperty.FindPropertyRelative("path");

        SerializedProperty folderPathProperty = serializedObject.FindProperty("path");
        string path = folderPathProperty.stringValue;

        // Weight field
        float weightWidth = 50f;

        Rect weightRect = rect;
        weightRect.x += rect.width - weightWidth;
        weightRect.width = weightWidth;
        weightRect.height -= 3f;

        GUIContent weightLabel = new GUIContent("Weight");
        weightLabel.tooltip = "Chance to paint this object";
        weightProperty.doubleValue = EditorGUI.DoubleField(weightRect, GUIContent.none, weightProperty.doubleValue);
        weightProperty.doubleValue = Math.Clamp(weightProperty.doubleValue, 0.0, 1.0);

        float weightLabelWidth = GUI.skin.label.CalcSize(weightLabel).x;

        weightRect.x -= weightLabelWidth + 5f;
        weightRect.width = weightLabelWidth;

        EditorGUI.LabelField(weightRect, weightLabel);

        // Name field
        Rect nameRect = new Rect(rect.x, rect.y, weightRect.x - rect.x - 10f, rect.height - 3f);

        prefabIndex = FindIndexFromPath(pathProperty.stringValue);
        prefabIndex = EditorGUI.Popup(nameRect, prefabIndex, prefabsList.ToArray());
        nameProperty.stringValue = prefabsList[prefabIndex];
        pathProperty.stringValue = prefabsPathList[prefabIndex];
    }

    public float DrawHeight(int index)
    {
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        return prefabsProperty.isExpanded ? EditorGUIUtility.singleLineHeight : 0f;
    }

    // Function to find name from index.
    private int FindIndexFromPath(string path)
    {
        int count = 0;
        
        for (int i = 0; i < prefabsPathList.Count; i++)
        {
            if (prefabsPathList[i].Equals(path))
            {
                return count;
            }

            count++;
        }
        
        return 0;
    }

    // Function to get random index weight from group brush.
    public int GetRandomWeight()
    {
        double totalWeight = 0.0;

        foreach (GroupBrushObject groupBrushObject in prefabs)
        {
            totalWeight += groupBrushObject.weight;
            groupBrushObject.SetAccumulatedWeight(totalWeight);
        }

        System.Random random = new System.Random();
        double r = random.NextDouble() * totalWeight;

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i].GetAccumulatedWeight() >= r)
            {
                return i;
            }
        }

        return 0;
    }
}

[System.Serializable]
public class GroupBrushObject
{
    [SerializeField] public string name; // Name of file.
    [SerializeField] public double weight = 1.0; // Weight to spawn this object.
    [SerializeField] public string path; // Path to this file. (Since file in different folder might have same name)

    private double accumulatedWeight = 0.0;

    // Function to set accumulated weight.
    public double GetAccumulatedWeight()
    {
        return accumulatedWeight;
    }

    // Function to set accumulated weight.
    public void SetAccumulatedWeight(double weight)
    {
        accumulatedWeight = weight;
    }
}
#endif