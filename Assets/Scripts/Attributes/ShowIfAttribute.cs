using System;
using UnityEngine;

public enum DisableType
{
    ReadOnly, Hide
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ShowIfAttribute : PropertyAttribute
{
    public string variable;
    public System.Object value;
    public DisableType disableType;

    public ShowIfAttribute(string variable, System.Object value, DisableType disableType = DisableType.Hide)
    {
        this.variable = variable;
        this.value = value;
        this.disableType = disableType;
    }
}
