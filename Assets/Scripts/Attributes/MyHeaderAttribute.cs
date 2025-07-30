using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class MyHeaderAttribute : PropertyAttribute
{
    public string header;
    public string color;

    public MyHeaderAttribute(string header)
    {
        this.header = header;
        this.color = "#A8D9FF";
    }

    public MyHeaderAttribute(string header, string color)
    {
        this.header = header;
        this.color = color;
    }

    public MyHeaderAttribute(string header, Color color)
    {
        this.header = header;
        this.color = ColorUtility.ToHtmlStringRGB(color);
    }
}
