using System;
using UnityEngine;

namespace CultistToolbox.UI;

public static class Common
{
    public static readonly Lazy<GUIStyle> SmallLabelStyle = new(CreateSmallLabelStyle);
    public static readonly Lazy<GUIStyle> LabelStyle = new(CreateLabelStyle);
    public static readonly Lazy<GUIStyle> HighlightLabelStyle = new(CreateHighlightLabelStyle);
    public static readonly Lazy<GUIStyle> HighlightOnHoverLabelStyle = new(CreateHighlightOnHoverLabelStyle);
    public static readonly Lazy<GUIStyle> WarningLabelStyle = new(CreateWarningLabelStyle);
    public static readonly Lazy<GUIStyle> ButtonStyle = new(CreateButtonStyle);
    public static readonly Lazy<GUIStyle> HighlightButtonStyle = new(CreateHighlightButtonStyle);

    private static GUIStyle CreateButtonStyle()
    {
        return new GUIStyle(GUI.skin.button)
        {
            font = IconFontLocator.IconFont,
        };
    }

    private static GUIStyle CreateHighlightButtonStyle()
    {
        var style = CreateButtonStyle();
        style.normal.textColor = Color.cyan;
        return style;
    }

    public static GUIStyle CreateSmallLabelStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.font = IconFontLocator.IconFont;
        return style;
    }

    private static GUIStyle CreateLabelStyle()
    {
        var style = CreateSmallLabelStyle();
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

    private static GUIStyle CreateWarningLabelStyle()
    {
        var style = CreateLabelStyle();
        style.normal.textColor = Color.yellow;
        return style;
    }

    public static void DrawTexture(Texture texture, float minSideLength)
    {
        float scale = minSideLength / Mathf.Min(texture.width, texture.height);
        float textureDw = texture.width * scale;
        float textureDh = texture.height * scale;
        GUILayout.Box(texture, GUILayout.Width(textureDw), GUILayout.Height(textureDh));
    }
}