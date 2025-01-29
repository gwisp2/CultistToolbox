using System;
using System.Linq;
using BepInEx.Configuration;
using CultistToolbox.Patch;
using CultistToolbox.UI;
using UnityEngine;

namespace CultistToolbox.AdvancedCollectionManager;

public class AdvancedCollectionManagerUi
{
    private static AdvancedCollectionManagerUi _instance;
    public static AdvancedCollectionManagerUi Instance => _instance ??= new AdvancedCollectionManagerUi();

    private static readonly Color[] Colors = [Color.red, Color.yellow, Color.green];

    private Lazy<GUIStyle[]> _headerStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = color;
        style.font = IconFontLocator.IconFont;
        style.margin.top = 20;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _toggleStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.toggle);
        style.normal.textColor = color;
        return style;
    }).ToArray());

    private Lazy<GUIStyle[]> _buttonStyles = new(() => Colors.Select(color =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.normal.textColor = color;
        return style;
    }).ToArray());

    private void DrawCollectionEditorImpl(ConfigEntryBase entry)
    {
        var typedEntry = ((ConfigEntry<AdvancedUserCollection>)entry);
        var collection = typedEntry.Value.Copy();
        GUILayout.BeginVertical();
        foreach (var item in collection.Items)
        {
            DrawItem(item, item.ProductModel.CanToggle);
        }

        DrawMargin();
        GUILayout.EndVertical();
        typedEntry.Value = collection.Freeze();
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

        DrawMargin();
        GUILayout.EndVertical();
    }

    private void DrawMargin()
    {
        GUILayout.Label("", _headerStyles.Value[0]); // just for margin
    }

    private void DrawItem(AdvancedUserCollectionProduct product, bool editable)
    {
        int styleIndex = GetStyleIndex(product);
        var productTitle = Utilities.GetProductIcons(product.ProductModel) + " " + product.ProductModel.ProductName;
        GUILayout.Label(productTitle, _headerStyles.Value[styleIndex]);
        if (editable)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select all", _buttonStyles.Value[styleIndex]))
            {
                product.SetEverything(true);
            }
            else if (GUILayout.Button("Select none", _buttonStyles.Value[styleIndex]))
            {
                product.SetEverything(false);
            }

            GUILayout.EndHorizontal();
        }

        var nInvestigators = product.ProductModel.Investigators.Count;
        var nItems = product.ProductModel.Items.Count;
        var nMonsters = product.ProductModel.Monsters.Count;
        var nTiles = product.ProductModel.TileQuantity;
        var nMythos = product.ProductModel.CanToggle
            ? MoMDBManager.DB.MythosEvents.Count(e =>
                e.RequiredProducts != null && e.RequiredProducts.Contains(product.ProductModel))
            : MoMDBManager.DB.MythosEvents.Count(e =>
                e.RequiredProducts == null || !e.RequiredProducts.Any() ||
                (e.RequiredProducts != null && e.RequiredProducts.Contains(product.ProductModel)));
        GUILayout.BeginHorizontal();
        bool hasInvestigators = GUILayout.Toggle(product.HasInvestigators, $"Investigators ({nInvestigators})",
            _toggleStyles.Value[styleIndex]);
        bool hasItems = GUILayout.Toggle(product.HasItems, $"Items ({nItems})", _toggleStyles.Value[styleIndex]);
        bool hasMonsters =
            GUILayout.Toggle(product.HasMonsters, $"Monsters ({nMonsters})", _toggleStyles.Value[styleIndex]);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool hasMythos = GUILayout.Toggle(product.HasMythosEvents, $"Mythos events ({nMythos})",
            _toggleStyles.Value[styleIndex]);
        bool hasTiles = GUILayout.Toggle(product.HasTiles, $"Tiles ({nTiles})", _toggleStyles.Value[styleIndex]);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool isShared = GUILayout.Toggle(product.IsShared, $"Shared with other table");
        GUILayout.EndHorizontal();
        if (editable)
        {
            product.HasInvestigators = hasInvestigators;
            product.HasItems = hasItems;
            product.HasMonsters = hasMonsters;
            product.HasMythosEvents = hasMythos;
            product.HasTiles = hasTiles;
        }

        product.IsShared = isShared;
    }

    private static int GetStyleIndex(AdvancedUserCollectionProduct product)
    {
        int styleIndex = product.IsEverythingSelected ? 2 : product.IsAnythingSelected ? 1 : 0;
        return styleIndex;
    }

    public static void DrawCollectionEditor(ConfigEntryBase entry)
    {
        if (IsEditingAllowed())
            Instance.DrawCollectionEditorImpl(entry);
        else
            Instance.DrawEffectiveCollectionImpl();
    }

    public static void DrawScenarioRestrictedComponents(ConfigEntryBase entry)
    {
        var typedEntry = ((ConfigEntry<ItemComponentTypes>)entry);
        ItemComponentTypes types = typedEntry.Value;
        GUILayout.BeginVertical();
        if (entry.Description != null && !string.IsNullOrEmpty(entry.Description.Description))
        {
            GUILayout.Label(entry.Description.Description);
        }

        GUILayout.BeginHorizontal();
        types = ComponentTypeToggle("Items", types, ItemComponentTypes.Items);
        types = ComponentTypeToggle("Monsters", types, ItemComponentTypes.Monsters);
        types = ComponentTypeToggle("Mythos", types, ItemComponentTypes.MythosEvents);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        if (IsEditingAllowed())
        {
            typedEntry.Value = types;
        }
    }

    private static ItemComponentTypes ComponentTypeToggle(string text, ItemComponentTypes types,
        ItemComponentTypes flag)
    {
        var oldValue = types.HasFlag(flag);
        var newValue = GUILayout.Toggle(oldValue, text);
        return (types & ~flag) | (newValue ? flag : 0);
    }

    private static bool IsEditingAllowed()
    {
        return CurrentScenarioVariantPatch.CurrentScenarioVariant == null;
    }
}