using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyPropertyDrawer : PropertyDrawer
{
    private bool isPositionHeight;
    private float propertyHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return propertyHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PrefixLabel(position, label);

        if (position.height == 0f)
        {
            isPositionHeight = false;
        }

        propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);

        if (!isPositionHeight)
        {
            isPositionHeight = true;
            propertyHeight += position.height;
        }

        // Disable and draw property, then re-enable back.
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
