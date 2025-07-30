using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySpriteAttribute : PropertyAttribute
{
    public int size;

    public MySpriteAttribute(int size)
    {
        this.size = size;
    }
}
