using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonPropertyDrawer : DecoratorDrawer
{
    private ButtonAttribute buttonAttribute;

    private float propertyHeight;

    public override float GetHeight()
    {
        return propertyHeight;
    }

    public override void OnGUI(Rect position)
    {
        buttonAttribute = attribute as ButtonAttribute;
        propertyHeight = buttonAttribute.height;

        Object unityObject = Selection.activeGameObject.GetComponent(buttonAttribute.unityClass) as Object;

        GUIContent buttonLabel = new GUIContent(buttonAttribute.label);
        buttonLabel.tooltip = buttonAttribute.tooltip;

        // Create a button with position, label, tooltip.
        if (GUI.Button(position, buttonLabel))
        {
            MethodInfo method = unityObject.GetType().GetMethod(buttonAttribute.functionName);

            // If there's a method/function with that name, execute method/function.
            if (method != null)
            {
                method.Invoke(unityObject, null);
            }
        }
    }
}