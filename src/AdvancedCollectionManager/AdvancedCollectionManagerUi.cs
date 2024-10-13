﻿using System;
using System.Linq;
using BepInEx.Configuration;
using MoMEssentials.Patch;
using MoMEssentials.UI;
using UnityEngine;

namespace MoMEssentials.AdvancedCollectionManager;

public class AdvancedCollectionManagerUi
{
    private static AdvancedCollectionManagerUi _instance;
    public static AdvancedCollectionManagerUi Instance => _instance ??= new AdvancedCollectionManagerUi();

    private readonly AdvancedUserCollection _collection = new AdvancedUserCollection();
    private static readonly Color[] Colors = [Color.red, Color.yellow, Color.green];

    private Lazy<GUIStyle[]> _headerStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        style.margin.top = 10;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _toggleStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.toggle);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _buttonStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        return style;
    }).ToArray());

    private void DrawCollectionEditorImpl(ConfigEntryBase entry)
    {
        _collection.LoadFromString(entry.BoxedValue as string);
        GUILayout.BeginVertical();
        foreach (var item in _collection.Items)
        {
            DrawItem(item, item.CanToggle);
        }

        GUILayout.EndVertical();
        entry.BoxedValue = _collection.SaveToString();
    }

    private void DrawEffectiveCollectionImpl()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(
            "The following components are enabled in this scenario. This is affected by LimitAvailableItems.");
        var effectiveCollection = AdvancedCollectionFacade.GetEffectiveCollectionForCurrentScenario();
        foreach (var item in effectiveCollection.Items)
        {
            if (!item.IsAnythingSelected) continue;
            DrawItem(item, false);
        }

        GUILayout.EndVertical();
    }

    private void DrawItem(AdvancedUserCollection.Item item, bool editable)
    {
        int styleIndex = GetStyleIndex(item);
        var productTitle = Utilities.GetProductIcons(item.ProductModel) + " " + item.ProductModel.ProductName;
        GUILayout.Label(productTitle, _headerStyles.Value[styleIndex]);
        if (editable)
        {
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
        }

        GUILayout.BeginHorizontal();
        bool hasInvestigators =
            GUILayout.Toggle(item.HasInvestigators, "Investigators", _toggleStyles.Value[styleIndex]);
        bool hasItems = GUILayout.Toggle(item.HasItems, "Items", _toggleStyles.Value[styleIndex]);
        bool hasMonsters = GUILayout.Toggle(item.HasMonsters, "Monsters", _toggleStyles.Value[styleIndex]);
        bool hasOtherContent = GUILayout.Toggle(item.HasOtherContent, "Other content", _toggleStyles.Value[styleIndex]);
        if (editable)
        {
            item.HasInvestigators = hasInvestigators;
            item.HasItems = hasItems;
            item.HasMonsters = hasMonsters;
            item.HasOtherContent = hasOtherContent;
        }

        GUILayout.EndHorizontal();
    }

    private static int GetStyleIndex(AdvancedUserCollection.Item item)
    {
        int styleIndex = item.IsEverythingSelected ? 2 : item.IsAnythingSelected ? 1 : 0;
        return styleIndex;
    }

    public static void DrawConfiguration(ConfigEntryBase entry)
    {
        if (IsEditingAllowed())
            Instance.DrawCollectionEditorImpl(entry);
        else
            Instance.DrawEffectiveCollectionImpl();
    }

    private static bool IsEditingAllowed()
    {
        return CurrentScenarioVariantPatch.CurrentScenarioVariant == null;
    }
}