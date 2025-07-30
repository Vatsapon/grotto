using System;
using System.Collections;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class PlaceholderAttribute : Attribute
{
    public string displayName;

    public PlaceholderAttribute(string displayName)
    {
        this.displayName = displayName;
    }
}