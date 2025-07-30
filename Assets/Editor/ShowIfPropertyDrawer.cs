using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfPropertyDrawer : PropertyDrawer
{
    ShowIfAttribute showIf;
    SerializedProperty selectedProperty;

    private float propertyHeight;
    private Rect defaultPosition;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return propertyHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Cast attribute as ShowIf attribute.
        showIf = attribute as ShowIfAttribute;
        string path;

        defaultPosition = position;

        if (property.propertyPath.Contains("."))
        {
            path = System.IO.Path.ChangeExtension(property.propertyPath, showIf.variable);
        }
        else
        {
            path = showIf.variable;
        }

        selectedProperty = property.serializedObject.FindProperty(path);

        if (selectedProperty == null)
        {
            return;
        }

        // Get default property height.
        propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);

        bool result = GetResult(selectedProperty);

        // Check either result matches or true.
        if (result)
        {
            // Draw property field out.
            GUIContent newLabel = new GUIContent(property.displayName);
            newLabel.tooltip = property.tooltip;

            EditorGUI.PropertyField(position, property, newLabel);
        }
        else
        {
            // If not, check either to hide or set as Read-Only.
            if (showIf.disableType.Equals(DisableType.ReadOnly))
            {
                GUI.enabled = false;

                GUIContent newLabel = new GUIContent(property.displayName);
                newLabel.tooltip = property.tooltip;

                EditorGUI.PropertyField(position, property, newLabel);

                GUI.enabled = true;
            }
            else
            {
                // Hide - Don't draw any property and set height to 0.
                propertyHeight = 0f;
            }
        }
    }

    // Return label position (since property height might not in single line)
    private Rect GetLabelPosition(Rect position)
    {
        if (propertyHeight > EditorGUIUtility.singleLineHeight)
        {
            position.y -= (propertyHeight / 2f) - (EditorGUIUtility.singleLineHeight / 2f);
        }

        return position;
    }

    private bool GetResult(SerializedProperty property)
    {
        switch (selectedProperty.type)
        {
            case "bool":
            return selectedProperty.boolValue.Equals(showIf.value);

            case "string":
            return selectedProperty.stringValue.Equals(showIf.value);

            case "Enum":
            return selectedProperty.enumValueIndex.Equals((int)showIf.value);
        }

        return false;
    }
}
