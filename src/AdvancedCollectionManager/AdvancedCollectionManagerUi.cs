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
        // Form a note about restricted components by ScenarioRestrictedComponentTypes
        var restrictedComponentTypesNote = "";
        if (Plugin.ConfigScenarioRestrictedComponentTypes.Value != 0)
        {
            var variant = CurrentScenarioVariantPatch.CurrentScenarioVariant;
            var requiredProductIcons = Utilities.GetProductIconsForScenarioVariant(variant);
            var restrictedComponentTypes = Plugin.ConfigScenarioRestrictedComponentTypes.Value;
            var componentTypesStr = restrictedComponentTypes.ToCommaSeparatedString();
            restrictedComponentTypesNote =
                $" Due to ScenarioRestrictedComponentTypes the following components are taken from {requiredProductIcons} only: {componentTypesStr}";
        }

        GUILayout.BeginVertical();
        GUILayout.Label(
            $"The following components are enabled in this scenario.{restrictedComponentTypesNote}",
            Common.CreateSmallLabelStyle());
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

        bool hasInvestigators = product.HasInvestigators;
        bool hasItems = product.HasItems;
        bool hasMonsters = product.HasMonsters;
        bool hasMythos = product.HasMythosEvents;
        bool hasTiles = product.HasTiles;

        GUILayout.BeginHorizontal();
        if ((editable || hasInvestigators) && product.CanHaveInvestigators)
        {
            hasInvestigators = GUILayout.Toggle(product.HasInvestigators,
                $"Investigators ({product.InvestigatorCount})",
                _toggleStyles.Value[styleIndex]);
        }

        if ((editable || hasItems) && product.CanHaveItems)
        {
            hasItems = GUILayout.Toggle(product.HasItems, $"Items ({product.ItemCount})",
                _toggleStyles.Value[styleIndex]);
        }

        if ((editable || hasMonsters) && product.CanHaveMonsters)
        {
            hasMonsters =
                GUILayout.Toggle(product.HasMonsters, $"Monsters ({product.MonsterCount})",
                    _toggleStyles.Value[styleIndex]);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if ((editable || hasMythos) && product.CanHaveMythosEvents)
        {
            hasMythos = GUILayout.Toggle(product.HasMythosEvents, $"Mythos events ({product.MythosCount})",
                _toggleStyles.Value[styleIndex]);
        }

        if ((editable || hasTiles) && product.CanHaveTiles)
        {
            hasTiles = GUILayout.Toggle(product.HasTiles, $"Tiles ({product.TileCount})",
                _toggleStyles.Value[styleIndex]);
        }

        GUILayout.EndHorizontal();

        if (!editable) return;
        product.HasInvestigators = hasInvestigators;
        product.HasItems = hasItems;
        product.HasMonsters = hasMonsters;
        product.HasMythosEvents = hasMythos;
        product.HasTiles = hasTiles;
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