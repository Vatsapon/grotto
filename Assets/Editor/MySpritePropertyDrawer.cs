using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MySpriteAttribute))]
public class MySpritePropertyDrawer : PropertyDrawer
{
    MySpriteAttribute mySpriteAttribute;
    float imageHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Check if property is reference object and it's a sprite object. 
        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue as Sprite)
        {
            // Extend property height by imageHeight with a little space.
            return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight + 20;
        }

        // Else, use the default height.
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        mySpriteAttribute = attribute as MySpriteAttribute;
        imageHeight = mySpriteAttribute.size;

        // Draw default sprite property.
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(position, property, label, true);

        // Check if property is reference object and it's a sprite object. (Similar with the one in GetPropertyHeight() function)
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            var sprite = property.objectReferenceValue as Sprite;

            if (sprite)
            {
                // Extend space by property and image height. Then draw sprite texture on the blank area.
                position.y += EditorGUI.GetPropertyHeight(property, label, true) + 10;
                position.height = imageHeight;
                GUI.DrawTexture(position, sprite.texture, ScaleMode.ScaleToFit);
            }
        }

        EditorGUI.EndProperty();
    }
}
