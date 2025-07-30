using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class ButtonAttribute : PropertyAttribute
{
    public string label; // Label on the button.
    public string tooltip; // Tooltip that will shown when hover the button.
    public float height = 25f; // Height of button.
    public Type unityClass; // Class that will execute function.
    public string functionName; // Function that will execute when press the button.

    public ButtonAttribute(string label, string tooltip, Type unityClass, string functionName)
    {
        this.label = label;
        this.tooltip = tooltip;
        this.unityClass = unityClass;
        this.functionName = functionName;
    }

    public ButtonAttribute(string label, Type unityClass, string functionName)
    {
        this.label = label;
        this.tooltip = "";
        this.unityClass = unityClass;
        this.functionName = functionName;
    }

    public ButtonAttribute(string label, string tooltip, float height, Type unityClass, string functionName)
    {
        this.label = label;
        this.tooltip = tooltip;
        this.height = height;
        this.unityClass = unityClass;
        this.functionName = functionName;
    }

    public ButtonAttribute(string label, float height, Type unityClass, string functionName)
    {
        this.label = label;
        this.tooltip = "";
        this.height = height;
        this.unityClass = unityClass;
        this.functionName = functionName;
    }
}