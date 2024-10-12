using System;
using UnityEngine;

namespace MoMEssentials.UI;

public static class Common
{
    public static readonly Lazy<GUIStyle> LabelStyle = new(CreateLabelStyle);
    public static readonly Lazy<GUIStyle> HighlightLabelStyle = new(CreateHighlightLabelStyle);
    public static readonly Lazy<GUIStyle> HighlightOnHoverLabelStyle = new(CreateHighlightOnHoverLabelStyle);

    private static GUIStyle CreateLabelStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.font = IconFontLocator.IconFont;
        style.fontSize = 30;
        return style;
    }

    private static GUIStyle CreateHighlightLabelStyle()
    {
        var style = CreateLabelStyle();
        style.normal.textColor = Color.green;
        return style;
    }

    private static GUIStyle CreateHighlightOnHoverLabelStyle()
    {
        var style = CreateLabelStyle();
        style.hover.textColor = Color.green;
        return style;
    }
}