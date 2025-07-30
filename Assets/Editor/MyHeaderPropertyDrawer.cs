using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MyHeaderAttribute))]
public class MyHeaderPropertyDrawer : DecoratorDrawer
{
    MyHeaderAttribute myHeader;

    public override float GetHeight()
    {
        return base.GetHeight() + 10f;
    }

    public override void OnGUI(Rect position)
    {
        myHeader = attribute as MyHeaderAttribute;

        string colorHTML = myHeader.color;
        string color = "<color=" + colorHTML + ">";
        float offset = 20f;

        // Draw property of default one first, then create label field above it.
        GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontSize = 15;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.richText = true;

        GUIContent headerLabel = new GUIContent(myHeader.header);
        EditorGUI.LabelField(position, color + headerLabel.text + "</color>", headerStyle);

        float headerWidth = GUI.skin.label.CalcSize(headerLabel).x;
        float lineWidth = ((position.width - headerWidth) / 2f);
        Color lineColor = Color.white;
        ColorUtility.TryParseHtmlString(colorHTML, out lineColor);

        // Left Horizontal
        Rect horizontalLine = position;
        horizontalLine.height = 2f;
        horizontalLine.y += (EditorGUIUtility.singleLineHeight * 1.5f) / 2f;
        horizontalLine.width = lineWidth - offset;
        EditorGUI.DrawRect(horizontalLine, lineColor);

        // Right Horizontal
        horizontalLine.x += horizontalLine.width + headerWidth + (offset * 2f);
        EditorGUI.DrawRect(horizontalLine, lineColor);
    }
}
