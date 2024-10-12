using System;
using System.Linq;
using BepInEx.Configuration;
using MoMEssentials.UI;
using UnityEngine;

namespace MoMEssentials;

public class AdvancedCollectionManagerUi
{
    private static AdvancedCollectionManagerUi _instance;
    public static AdvancedCollectionManagerUi Instance => _instance ??= new AdvancedCollectionManagerUi();

    private AdvancedUserCollection _collection = new AdvancedUserCollection();
    private static Color[] _colors = [Color.red, Color.yellow, Color.green];

    private Lazy<GUIStyle[]> _headerStyles = new(() => _colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        style.margin.top = 10;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _toggleStyles = new(() => _colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.toggle);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _buttonStyles = new(() => _colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        return style;
    }).ToArray());

    private void DrawConfigurationImpl(ConfigEntryBase entry)
    {
        _collection.LoadFromString(entry.BoxedValue as string);
        GUILayout.BeginVertical();
        foreach (var item in _collection.Items)
        {
            int styleIndex = item.IsEverythingSelected ? 2 : item.IsAnythingSelected ? 1 : 0;
            var productTitle = Utilities.GetProductIcons(item.ProductModel) + " " + item.ProductModel.ProductName;
            GUILayout.Label(productTitle, _headerStyles.Value[styleIndex]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select all", _buttonStyles.Value[styleIndex]))
            {
                item.SetEverything(true);
            }
            else if (GUILayout.Button("Select none", _buttonStyles.Value[styleIndex]))
            {
                item.SetEverything(false);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            item.HasInvestigators =
                GUILayout.Toggle(item.HasInvestigators, "Investigators", _toggleStyles.Value[styleIndex]);
            item.HasItems = GUILayout.Toggle(item.HasItems, "Items", _toggleStyles.Value[styleIndex]);
            item.HasMonsters = GUILayout.Toggle(item.HasMonsters, "Monsters", _toggleStyles.Value[styleIndex]);
            item.HasOtherContent =
                GUILayout.Toggle(item.HasOtherContent, "Other content", _toggleStyles.Value[styleIndex]);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        entry.BoxedValue = _collection.SaveToString();
    }

    public static void DrawConfiguration(ConfigEntryBase entry)
    {
        Instance.DrawConfigurationImpl(entry);
    }
}